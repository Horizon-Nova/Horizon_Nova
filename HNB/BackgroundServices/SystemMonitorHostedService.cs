// SystemMonitorHostedService.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Models.HnbHnbBackoffice;
using HNB.Repositories;

namespace HNB.BackgroundServices;

#region Options
public sealed class SystemMonitorOptions
{
    public int SystemConfigId { get; set; } = 60;
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(1);
}
#endregion

public sealed class SystemMonitorHostedService : BackgroundService
{
    #region Fields & Ctor
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SystemMonitorOptions _options;

    public SystemMonitorHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<SystemMonitorOptions> options)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }
    #endregion

    #region Execute Loop
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var t0 = Stopwatch.GetTimestamp();
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<SystemMonitorHostedRepositories>();

                var snapshot = await CollectSnapshotAsync(stoppingToken);
                snapshot.id = _options.SystemConfigId;

                await repo.UpdateSystemInfoAsync(snapshot, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch
            {
            }

            var elapsed = ElapsedMs(t0);
            var rest = _options.Interval - TimeSpan.FromMilliseconds(elapsed);
            if (rest < TimeSpan.Zero) rest = TimeSpan.Zero;

            try { await Task.Delay(rest, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }
    #endregion

    #region Snapshot
    private static async Task<system_config> CollectSnapshotAsync(CancellationToken ct = default)
    {
        var host_name = Environment.MachineName;
        var operating_system = RuntimeInformation.OSDescription ?? string.Empty;
        var kernel_version = Environment.OSVersion.VersionString ?? operating_system;

        var up = TimeSpan.FromMilliseconds(Environment.TickCount64);
        var uptime = $"{(int)up.TotalDays}天 {up.Hours}小時 {up.Minutes}分 {up.Seconds}秒";

        long inbound = 0, outbound = 0;
        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                nic.OperationalStatus != OperationalStatus.Up)
                continue;

            try
            {
                var stats = nic.GetIPStatistics();
                inbound += stats.BytesReceived;
                outbound += stats.BytesSent;
            }
            catch { }
        }

        var cpuPercent = await GetCpuUsagePercentAsync(ct);
        var coreCount = Environment.ProcessorCount;
        var (memUsed, memTotal) = GetMemoryBytes();
        var (diskUsed, diskTotal) = GetDiskBytes();

        return new system_config
        {
            id = 0,
            host_name = host_name,
            operating_system = operating_system,
            kernel_version = kernel_version,
            uptime = uptime,
            inbound_traffic = inbound.ToString(),
            outbound_traffic = outbound.ToString(),

            cpu_usage = new List<string> { coreCount.ToString(), cpuPercent.ToString("F2") },
            memory_usage = new List<string> { memUsed.ToString(), memTotal.ToString() },
            disk_usage = new List<string> { diskUsed.ToString(), diskTotal.ToString() }
        };
    }
    #endregion

    #region CPU
    private static async Task<double> GetCpuUsagePercentAsync(CancellationToken ct)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            static bool GetTimes(out ulong idle, out ulong kernel, out ulong user)
            {
                GetSystemTimes(out var idleFt, out var kernelFt, out var userFt);
                idle = ToUInt64(idleFt);
                kernel = ToUInt64(kernelFt);
                user = ToUInt64(userFt);
                return true;
            }

            GetTimes(out var idle1, out var kernel1, out var user1);
            await Task.Delay(500, ct);
            GetTimes(out var idle2, out var kernel2, out var user2);

            var idle = idle2 - idle1;
            var kernel = kernel2 - kernel1;
            var user = user2 - user1;

            var sys = kernel + user;
            if (sys == 0) return 0;
            var busy = sys - idle;
            return busy * 100.0 / sys;
        }
        else
        {
            static (ulong idle, ulong total) ReadStat()
            {
                var line = File.ReadLines("/proc/stat").First(l => l.StartsWith("cpu "));
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1)
                                .Select(ulong.Parse).ToArray();
                var idle = parts[3] + (parts.Length > 4 ? parts[4] : 0);
                var total = parts.Aggregate(0UL, (a, b) => a + b);
                return (idle, total);
            }

            var (idle1, total1) = ReadStat();
            await Task.Delay(500, ct);
            var (idle2, total2) = ReadStat();

            var idle = idle2 - idle1;
            var total = total2 - total1;
            if (total == 0) return 0;
            return (total - idle) * 100.0 / total;
        }
    }
    #endregion

    #region Memory
    private static (long used, long total) GetMemoryBytes()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var ms = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(ms))
            {
                long total = (long)ms.ullTotalPhys;
                long avail = (long)ms.ullAvailPhys;
                return (total - avail, total);
            }
            return (0, 0);
        }
        else
        {
            var dict = File.ReadAllLines("/proc/meminfo")
                .Select(l => l.Split(':'))
                .Where(a => a.Length >= 2)
                .ToDictionary(a => a[0].Trim(), a => a[1].Trim());

            long kb(string key) => dict.TryGetValue(key, out var v)
                ? long.Parse(new string(v.Where(char.IsDigit).ToArray()))
                : 0;

            var totalKb = kb("MemTotal");
            var availKb = kb("MemAvailable");
            long total = totalKb * 1024;
            long used = (totalKb - availKb) * 1024;
            return (used, total);
        }
    }
    #endregion

    #region Disk
    private static (long used, long total) GetDiskBytes()
    {
        long total = 0, free = 0;
        foreach (var d in DriveInfo.GetDrives())
        {
            if (!d.IsReady) continue;
            if (d.DriveType != DriveType.Fixed && d.DriveType != DriveType.Network) continue;
            try
            {
                total += d.TotalSize;
                free += d.AvailableFreeSpace;
            }
            catch { }
        }
        long used = total - free;
        return (used, total);
    }
    #endregion

    #region Utils & Native
    private static long ElapsedMs(long startTimestamp)
    {
        var freq = (double)Stopwatch.Frequency;
        return (long)((Stopwatch.GetTimestamp() - startTimestamp) * 1000.0 / freq);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FILETIME { public int dwLowDateTime; public int dwHighDateTime; }
    private static ulong ToUInt64(FILETIME ft)
        => ((ulong)(uint)ft.dwHighDateTime << 32) + (uint)ft.dwLowDateTime;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetSystemTimes(out FILETIME idleTime, out FILETIME kernelTime, out FILETIME userTime);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private sealed class MEMORYSTATUSEX
    {
        public uint dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
    #endregion
}

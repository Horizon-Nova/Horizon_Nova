using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

/// <summary>
/// 硬體監控資料表 - 統一儲存伺服器硬體監控資訊
/// </summary>
[Table("hardware_monitoring", Schema = "dbo")]
public partial class hardware_monitoring
{
    /// <summary>
    /// 主鍵ID
    /// </summary>
    [Key]
    public int id { get; set; }

    /// <summary>
    /// 伺服器IP位址
    /// </summary>
    [StringLength(45)]
    public string? server_ip { get; set; }

    /// <summary>
    /// 伺服器位置
    /// </summary>
    [StringLength(100)]
    public string? server_location { get; set; }

    /// <summary>
    /// 伺服器提供商
    /// </summary>
    [StringLength(100)]
    public string? server_provider { get; set; }

    /// <summary>
    /// 環境類型 (Production/Development/Test)
    /// </summary>
    [StringLength(50)]
    public string? environment_type { get; set; }

    /// <summary>
    /// 主機名稱
    /// </summary>
    [StringLength(100)]
    public string? host_name { get; set; }

    /// <summary>
    /// 作業系統
    /// </summary>
    [StringLength(100)]
    public string? operating_system { get; set; }

    /// <summary>
    /// 核心版本
    /// </summary>
    [StringLength(100)]
    public string? kernel_version { get; set; }

    /// <summary>
    /// 系統運行時間
    /// </summary>
    [StringLength(100)]
    public string? uptime { get; set; }

    /// <summary>
    /// CPU名稱陣列
    /// </summary>
    public List<string>? cpu_names { get; set; }

    /// <summary>
    /// CPU製造商陣列
    /// </summary>
    public List<string>? cpu_manufacturers { get; set; }

    /// <summary>
    /// CPU型號陣列
    /// </summary>
    public List<string>? cpu_models { get; set; }

    /// <summary>
    /// CPU核心數陣列
    /// </summary>
    public List<int>? cpu_cores { get; set; }

    /// <summary>
    /// CPU執行緒數陣列
    /// </summary>
    public List<int>? cpu_threads { get; set; }

    /// <summary>
    /// CPU基礎時脈陣列 (GHz)
    /// </summary>
    public List<decimal>? cpu_base_clocks { get; set; }

    /// <summary>
    /// CPU加速時脈陣列 (GHz)
    /// </summary>
    public List<decimal>? cpu_boost_clocks { get; set; }

    /// <summary>
    /// CPU溫度陣列 (°C)
    /// </summary>
    public List<int>? cpu_temperatures { get; set; }

    /// <summary>
    /// CPU使用率陣列 (%)
    /// </summary>
    public List<decimal>? cpu_usages { get; set; }

    /// <summary>
    /// GPU名稱陣列
    /// </summary>
    public List<string>? gpu_names { get; set; }

    /// <summary>
    /// GPU製造商陣列
    /// </summary>
    public List<string>? gpu_manufacturers { get; set; }

    /// <summary>
    /// GPU型號陣列
    /// </summary>
    public List<string>? gpu_models { get; set; }

    /// <summary>
    /// GPU記憶體大小陣列
    /// </summary>
    public List<string>? gpu_memory_sizes { get; set; }

    /// <summary>
    /// GPU溫度陣列 (°C)
    /// </summary>
    public List<int>? gpu_temperatures { get; set; }

    /// <summary>
    /// GPU使用率陣列 (%)
    /// </summary>
    public List<decimal>? gpu_usages { get; set; }

    /// <summary>
    /// 記憶體名稱陣列
    /// </summary>
    public List<string>? memory_names { get; set; }

    /// <summary>
    /// 記憶體類型陣列
    /// </summary>
    public List<string>? memory_types { get; set; }

    /// <summary>
    /// 記憶體容量陣列
    /// </summary>
    public List<string>? memory_capacities { get; set; }

    /// <summary>
    /// 記憶體速度陣列
    /// </summary>
    public List<string>? memory_speeds { get; set; }

    /// <summary>
    /// 記憶體使用率陣列 (%)
    /// </summary>
    public List<decimal>? memory_usages { get; set; }

    /// <summary>
    /// 儲存裝置名稱陣列
    /// </summary>
    public List<string>? storage_names { get; set; }

    /// <summary>
    /// 儲存類型陣列
    /// </summary>
    public List<string>? storage_types { get; set; }

    /// <summary>
    /// 儲存容量陣列
    /// </summary>
    public List<string>? storage_capacities { get; set; }

    /// <summary>
    /// 儲存介面陣列
    /// </summary>
    public List<string>? storage_interfaces { get; set; }

    /// <summary>
    /// 讀取速度陣列 (MB/s)
    /// </summary>
    public List<int>? storage_read_speeds { get; set; }

    /// <summary>
    /// 寫入速度陣列 (MB/s)
    /// </summary>
    public List<int>? storage_write_speeds { get; set; }

    /// <summary>
    /// 網路介面名稱陣列
    /// </summary>
    public List<string>? network_interfaces { get; set; }

    /// <summary>
    /// 網路類型陣列
    /// </summary>
    public List<string>? network_types { get; set; }

    /// <summary>
    /// 網路速度陣列
    /// </summary>
    public List<string>? network_speeds { get; set; }

    /// <summary>
    /// 網路狀態陣列
    /// </summary>
    public List<string>? network_statuses { get; set; }

    /// <summary>
    /// 接收位元組數陣列
    /// </summary>
    public List<long>? network_rx_bytes { get; set; }

    /// <summary>
    /// 傳輸位元組數陣列
    /// </summary>
    public List<long>? network_tx_bytes { get; set; }

    /// <summary>
    /// 系統負載平均值陣列
    /// </summary>
    public List<decimal>? system_load_avg { get; set; }

    /// <summary>
    /// 系統總記憶體 (bytes)
    /// </summary>
    public long? system_memory_total { get; set; }

    /// <summary>
    /// 系統已用記憶體 (bytes)
    /// </summary>
    public long? system_memory_used { get; set; }

    /// <summary>
    /// 系統可用記憶體 (bytes)
    /// </summary>
    public long? system_memory_free { get; set; }

    /// <summary>
    /// 系統總交換空間 (bytes)
    /// </summary>
    public long? system_swap_total { get; set; }

    /// <summary>
    /// 系統已用交換空間 (bytes)
    /// </summary>
    public long? system_swap_used { get; set; }

    /// <summary>
    /// 系統總磁碟空間 (bytes)
    /// </summary>
    public long? system_disk_total { get; set; }

    /// <summary>
    /// 系統已用磁碟空間 (bytes)
    /// </summary>
    public long? system_disk_used { get; set; }

    /// <summary>
    /// 系統可用磁碟空間 (bytes)
    /// </summary>
    public long? system_disk_free { get; set; }

    /// <summary>
    /// 系統程序數
    /// </summary>
    public int? system_processes { get; set; }

    /// <summary>
    /// 系統使用者數
    /// </summary>
    public int? system_users { get; set; }

    /// <summary>
    /// 最後檢查時間
    /// </summary>
    public DateTime? last_check_time { get; set; }

    /// <summary>
    /// 檢查方式 (agent/api/snmp)
    /// </summary>
    [StringLength(50)]
    public string? check_method { get; set; }

    /// <summary>
    /// 檢查間隔 (秒)
    /// </summary>
    public int? check_interval { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool? is_active { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime? created_at { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime? updated_at { get; set; }

    /// <summary>
    /// CPU健康狀態陣列
    /// </summary>
    public List<string>? cpu_health_statuses { get; set; }

    /// <summary>
    /// CPU健康百分比陣列
    /// </summary>
    public List<int>? cpu_health_percentages { get; set; }

    /// <summary>
    /// GPU健康狀態陣列
    /// </summary>
    public List<string>? gpu_health_statuses { get; set; }

    /// <summary>
    /// GPU健康百分比陣列
    /// </summary>
    public List<int>? gpu_health_percentages { get; set; }

    /// <summary>
    /// 記憶體健康狀態陣列
    /// </summary>
    public List<string>? memory_health_statuses { get; set; }

    /// <summary>
    /// 記憶體健康百分比陣列
    /// </summary>
    public List<int>? memory_health_percentages { get; set; }

    /// <summary>
    /// 電池電量 (%)
    /// </summary>
    public int? battery_level { get; set; }

    /// <summary>
    /// 電源效率
    /// </summary>
    [StringLength(20)]
    public string? power_efficiency { get; set; }

    /// <summary>
    /// 電源供應器資訊
    /// </summary>
    public string? power_supply_info { get; set; }
}

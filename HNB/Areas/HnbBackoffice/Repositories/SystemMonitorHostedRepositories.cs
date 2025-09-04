using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.HnbBackoffice.Repositories;

public class SystemMonitorHostedRepositories(HnbHnbBackofficeDbContext db)
{
    public async Task UpdateSystemInfoAsync(system_config model, CancellationToken ct = default)
    {
        var entity = await db.system_configs.FirstOrDefaultAsync(x => x.id == model.id, ct);
        if (entity is null) return;

        entity.host_name = model.host_name;
        entity.operating_system = model.operating_system;
        entity.kernel_version = model.kernel_version;
        entity.uptime = model.uptime;
        entity.inbound_traffic = model.inbound_traffic;
        entity.outbound_traffic = model.outbound_traffic;

        entity.cpu_usage = model.cpu_usage;
        entity.memory_usage = model.memory_usage;
        entity.disk_usage = model.disk_usage;

        await db.SaveChangesAsync(ct);
    }

}


using AutoDealerCRM.Data;
using AutoDealerCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoDealerCRM.Services;

public class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _db;

    public AuditLogService(AppDbContext db) => _db = db;

    /// <summary>
    /// Додає запис до журналу безпеки.
    /// Заборонено записувати: паролі, ключі, розшифровані значення VIN/Phone.
    /// </summary>
    public async Task LogAsync(AuditEventType eventType, string actor,
                               string target, string ip, string details = "")
    {
        _db.AuditLogs.Add(new AuditLogEntry
        {
            EventType = eventType,
            ActorName = actor,
            TargetName = target,
            IpAddress  = ip,
            Details    = details,
            Timestamp  = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    public async Task<List<AuditLogEntry>> GetLogsAsync(AuditEventType? filter, int take = 200)
    {
        var query = _db.AuditLogs.AsQueryable();

        if (filter.HasValue)
            query = query.Where(e => e.EventType == filter.Value);

        return await query
            .OrderByDescending(e => e.Timestamp)
            .Take(take)
            .ToListAsync();
    }
}

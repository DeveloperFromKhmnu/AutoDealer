using AutoDealerCRM.Models;

namespace AutoDealerCRM.Services;

/// <summary>Append-only журнал безпеки. ЛР №4, ЛР №7.</summary>
public interface IAuditLogService
{
    Task LogAsync(AuditEventType eventType, string actor, string target,
                  string ip, string details = "");

    Task<List<AuditLogEntry>> GetLogsAsync(AuditEventType? filter, int take = 200);
}

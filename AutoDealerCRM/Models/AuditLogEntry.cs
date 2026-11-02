using System.ComponentModel.DataAnnotations;

namespace AutoDealerCRM.Models;

/// <summary>
/// Запис журналу безпеки (Security Audit Log).
/// Append-only: методи Update/Delete відсутні на рівні сервісу. ЛР №4, ЛР №7.
/// </summary>
public class AuditLogEntry
{
    public int Id { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public AuditEventType EventType { get; set; }

    /// <summary>Логін користувача, що виконав дію (або "Anonymous").</summary>
    [MaxLength(256)]
    public string ActorName { get; set; } = "Anonymous";

    /// <summary>Об'єкт дії (логін менеджера, що заблокований, тощо).</summary>
    [MaxLength(256)]
    public string TargetName { get; set; } = string.Empty;

    /// <summary>IP-адреса джерела запиту.</summary>
    [MaxLength(45)]
    public string IpAddress { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Details { get; set; } = string.Empty;
}

namespace AutoDealerCRM.Services;

/// <summary>Результат перевірки ліцензійного файлу.</summary>
public record LicenseCheckResult(
    bool IsValid,
    string? Owner,
    DateTime? ExpiresAt,
    string ErrorMessage = "");

/// <summary>
/// Інтерфейс сервісу ліцензування.
/// Реалізація: HMAC-SHA256 підпис dealer.lic. ЛР №7.
/// </summary>
public interface ILicenseService
{
    /// <summary>Поточний статус ліцензії (читається при старті або після завантаження).</summary>
    LicenseCheckResult CurrentStatus { get; }

    /// <summary>
    /// Завантажує та перевіряє ліцензійний файл dealer.lic.
    /// Верифікує HMAC-SHA256 підпис. Якщо підпис хибний — повертає IsValid=false.
    /// </summary>
    Task<LicenseCheckResult> LoadLicenseAsync(Stream fileStream);
}

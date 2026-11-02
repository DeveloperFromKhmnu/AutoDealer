using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AutoDealerCRM.Services;

/// <summary>
/// Сервіс перевірки ліцензійного файлу dealer.lic.
///
/// ЛР №7: Контроль цілісності та автентичності через HMAC-SHA256.
///
/// Формат dealer.lic (JSON):
/// {
///   "Owner":     "ТОВ Автоділер Плюс",
///   "ExpiresAt": "2027-01-01T00:00:00Z",
///   "Signature": "Base64(HMAC-SHA256(Owner + "|" + ExpiresAt, LicenseKey))"
/// }
///
/// HMAC-SHA256 забезпечує:
/// - Цілісність: будь-яка зміна Owner або ExpiresAt робить підпис невалідним
/// - Автентичність: лише той хто знає LicenseKey може сформувати валідний підпис
/// - На відміну від простого хешу, HMAC неможливо підробити без знання ключа
/// </summary>
public class LicenseService : ILicenseService
{
    private readonly byte[] _licenseKey;

    public LicenseCheckResult CurrentStatus { get; private set; }
        = new(false, null, null, "Ліцензія не завантажена. Режим Demo.");

    public LicenseService(IConfiguration configuration)
    {
        var keyBase64 = configuration["License:Key"]
            ?? throw new InvalidOperationException(
                "Ключ ліцензії не знайдено в конфігурації (License:Key).");

        _licenseKey = Convert.FromBase64String(keyBase64);
    }

    public async Task<LicenseCheckResult> LoadLicenseAsync(Stream fileStream)
    {
        try
        {
            using var reader = new StreamReader(fileStream, Encoding.UTF8);
            var json = await reader.ReadToEndAsync();

            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("Owner",     out var ownerEl)    ||
                !root.TryGetProperty("ExpiresAt",  out var expiresEl)  ||
                !root.TryGetProperty("Signature",  out var sigEl))
            {
                CurrentStatus = new(false, null, null, "Невірний формат файлу ліцензії.");
                return CurrentStatus;
            }

            var owner     = ownerEl.GetString()   ?? "";
            var expiresAt = expiresEl.GetString()  ?? "";
            var signature = sigEl.GetString()      ?? "";

            // Перевірка терміну дії
            if (!DateTime.TryParse(expiresAt, out var expiresDate))
            {
                CurrentStatus = new(false, null, null, "Невірний формат дати ExpiresAt.");
                return CurrentStatus;
            }

            if (expiresDate.ToUniversalTime() < DateTime.UtcNow)
            {
                CurrentStatus = new(false, owner, expiresDate,
                    $"Ліцензія прострочена ({expiresDate:dd.MM.yyyy}).");
                return CurrentStatus;
            }

            // Криптографічна перевірка HMAC-SHA256
            var message       = $"{owner}|{expiresAt}";
            var messageBytes  = Encoding.UTF8.GetBytes(message);
            var expectedHmac  = HMACSHA256.HashData(_licenseKey, messageBytes);
            var expectedBase64 = Convert.ToBase64String(expectedHmac);

            // Порівняння у постійний час (захист від timing attack)
            var signatureBytes = Convert.FromBase64String(signature);
            var expectedBytes  = Convert.FromBase64String(expectedBase64);

            if (!CryptographicOperations.FixedTimeEquals(signatureBytes, expectedBytes))
            {
                CurrentStatus = new(false, owner, expiresDate,
                    "Невалідний криптографічний підпис. Файл ліцензії пошкоджений або підроблений.");
                return CurrentStatus;
            }

            // Ліцензія валідна
            CurrentStatus = new(true, owner, expiresDate);
            return CurrentStatus;
        }
        catch (JsonException)
        {
            CurrentStatus = new(false, null, null, "Помилка розбору JSON файлу ліцензії.");
            return CurrentStatus;
        }
        catch (FormatException)
        {
            CurrentStatus = new(false, null, null, "Помилка декодування підпису (невірний Base64).");
            return CurrentStatus;
        }
        catch (Exception ex)
        {
            CurrentStatus = new(false, null, null, $"Внутрішня помилка: {ex.Message}");
            return CurrentStatus;
        }
    }
}

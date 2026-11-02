using System.Security.Cryptography;
using System.Text;

namespace AutoDealerCRM.Services;

/// <summary>
/// Реалізація AES-256-GCM шифрування/розшифрування. ЛР №6 (симетричне шифрування),
/// ЛР №7 (автентифіковане шифрування — GCM-тег забезпечує цілісність та автентичність).
///
/// Формат зашифрованого значення (Base64):
///   [ Nonce (12 bytes) | Ciphertext (N bytes) | Tag (16 bytes) ]
///
/// Чому AES-256-GCM:
/// - Забезпечує конфіденційність (шифрування) — ЛР №6
/// - Забезпечує цілісність і автентичність через GCM-тег — ЛР №7
/// - При спробі розшифрувати модифіковані дані — кидає виняток CryptographicException
/// - 12-байтний Nonce генерується випадково для кожної операції (RFC 5116)
/// </summary>
public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    private const int NonceSize = 12; // 96 bits — рекомендований RFC 5116 для AES-GCM
    private const int TagSize   = 16; // 128 bits — максимальний GCM tag

    public EncryptionService(IConfiguration configuration)
    {
        var keyBase64 = configuration["Encryption:Key"]
            ?? throw new InvalidOperationException(
                "Ключ шифрування не знайдено в конфігурації (Encryption:Key). " +
                "Додайте його до appsettings.Development.json.");

        _key = Convert.FromBase64String(keyBase64);

        if (_key.Length != 32)
            throw new InvalidOperationException(
                "Ключ шифрування повинен бути 256 бітів (32 байти, Base64).");
    }

    /// <inheritdoc/>
    public string Encrypt(string plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext);

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

        // Генерація криптографічно стійкого Nonce для кожної операції
        var nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        var ciphertext = new byte[plaintextBytes.Length];
        var tag        = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        // Компонуємо: nonce || ciphertext || tag
        var result = new byte[NonceSize + ciphertext.Length + TagSize];
        Buffer.BlockCopy(nonce,      0, result, 0,                              NonceSize);
        Buffer.BlockCopy(ciphertext, 0, result, NonceSize,                      ciphertext.Length);
        Buffer.BlockCopy(tag,        0, result, NonceSize + ciphertext.Length,  TagSize);

        return Convert.ToBase64String(result);
    }

    /// <inheritdoc/>
    public string Decrypt(string ciphertextBase64)
    {
        ArgumentNullException.ThrowIfNull(ciphertextBase64);

        var data = Convert.FromBase64String(ciphertextBase64);

        if (data.Length < NonceSize + TagSize)
            throw new CryptographicException("Пошкоджений шифртекст: недостатня довжина.");

        var nonce          = data[..NonceSize];
        var tag            = data[^TagSize..];
        var ciphertext     = data[NonceSize..^TagSize];
        var plaintextBytes = new byte[ciphertext.Length];

        // AesGcm.Decrypt кидає CryptographicException якщо тег не збігається —
        // це означає, що дані були модифіковані (ЛР №7: виявлення порушення цілісності)
        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintextBytes);

        return Encoding.UTF8.GetString(plaintextBytes);
    }
}

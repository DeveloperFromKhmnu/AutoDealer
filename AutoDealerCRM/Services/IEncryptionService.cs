namespace AutoDealerCRM.Services;

/// <summary>
/// Інтерфейс симетричного шифрування полів бази даних.
/// Реалізація: AES-256-GCM (автентифіковане шифрування). ЛР №6, ЛР №7.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Шифрує рядок відкритого тексту.
    /// Повертає Base64-рядок формату: Base64(nonce || ciphertext || tag)
    /// </summary>
    string Encrypt(string plaintext);

    /// <summary>
    /// Розшифровує рядок, зашифрований методом Encrypt.
    /// Кидає виняток якщо перевірка автентифікаційної мітки (GCM tag) провалилась —
    /// це означає модифікацію даних у БД. ЛР №7.
    /// </summary>
    string Decrypt(string ciphertext);
}

using System.ComponentModel.DataAnnotations;

namespace AutoDealerCRM.Models;

/// <summary>
/// Заявка покупця на підбір автомобіля.
/// Поле Phone зберігається у зашифрованому вигляді (AES-256-GCM). ЛР №6.
/// </summary>
public class BuyerRequest
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Зашифрований номер телефону (AES-256-GCM).</summary>
    [MaxLength(512)]
    public string PhoneEncrypted { get; set; } = string.Empty;

    [MaxLength(300)]
    public string DesiredModel { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsProcessed { get; set; } = false;
}

using System.ComponentModel.DataAnnotations;

namespace AutoDealerCRM.Models;

/// <summary>
/// Лот автомобіля. Конфіденційні поля (VinCode, AuctionCost) зберігаються
/// у зашифрованому вигляді (AES-256-GCM). ЛР №6, ЛР №7.
/// </summary>
public class CarLot
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Make { get; set; } = string.Empty;       // Марка

    [Required, MaxLength(50)]
    public string Model { get; set; } = string.Empty;      // Модель

    [Range(1900, 2100)]
    public int Year { get; set; }                          // Рік

    [Range(0, int.MaxValue)]
    public int Mileage { get; set; }                       // Пробіг (милі)

    [Range(0, double.MaxValue)]
    public decimal PublicPrice { get; set; }               // Публічна ціна (USD)

    public DeliveryStatus Status { get; set; } = DeliveryStatus.AtAuction;

    // === КОНФІДЕНЦІЙНІ ПОЛЯ (зберігаються в БД у зашифрованому Base64-вигляді) ===

    /// <summary>
    /// Зашифрований VIN-код (AES-256-GCM).
    /// У БД: Base64-рядок. Розшифрування лише на рівні сервісу.
    /// </summary>
    [MaxLength(512)]
    public string VinCodeEncrypted { get; set; } = string.Empty;

    /// <summary>
    /// Зашифрована собівартість закупівлі (AES-256-GCM).
    /// </summary>
    [MaxLength(512)]
    public string AuctionCostEncrypted { get; set; } = string.Empty;

    [MaxLength(20)]
    public string AuctionName { get; set; } = string.Empty; // Copart / IAAI

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}

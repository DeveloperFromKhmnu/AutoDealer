using AutoDealerCRM.Models;
using System.ComponentModel.DataAnnotations;

namespace AutoDealerCRM.ViewModels;

/// <summary>ViewModel для публічного каталогу (Гість): VIN замаскований.</summary>
public class CarLotPublicViewModel
{
    public int    Id          { get; set; }
    public string Make        { get; set; } = string.Empty;
    public string Model       { get; set; } = string.Empty;
    public int    Year        { get; set; }
    public int    MileageKm   { get; set; }     // конвертовано в км
    public decimal PublicPrice { get; set; }
    public DeliveryStatus Status { get; set; }

    /// <summary>Маскований VIN: перші 11 символів + ******</summary>
    public string MaskedVin { get; set; } = string.Empty;

    public string StatusDisplay => Status switch
    {
        DeliveryStatus.AtAuction => "На аукціоні",
        DeliveryStatus.InTransit => "В дорозі",
        DeliveryStatus.AtCustoms => "На митниці",
        DeliveryStatus.Available => "В наявності",
        _ => "Невідомо"
    };
}

/// <summary>ViewModel для менеджера: повний VIN та собівартість розшифровані.</summary>
public class CarLotManagerViewModel : CarLotPublicViewModel
{
    public string FullVin     { get; set; } = string.Empty;
    public decimal AuctionCost { get; set; }
    public string AuctionName { get; set; } = string.Empty;
    public DateTime AddedAt   { get; set; }
}

/// <summary>ViewModel для форми створення/редагування лоту.</summary>
public class CarLotEditViewModel
{
    public int    Id          { get; set; }

    [Required(ErrorMessage = "Обов'язкове поле")]
    [MaxLength(50)] public string Make  { get; set; } = string.Empty;

    [Required(ErrorMessage = "Обов'язкове поле")]
    [MaxLength(50)] public string Model { get; set; } = string.Empty;

    [Range(1900, 2100)] public int Year     { get; set; } = DateTime.Now.Year;
    [Range(0, 999999)]  public int Mileage  { get; set; }

    [Range(0.01, 9999999)]
    [Display(Name = "Публічна ціна (USD)")]
    public decimal PublicPrice  { get; set; }

    [Required, RegularExpression(@"^[A-HJ-NPR-Z0-9]{17}$",
        ErrorMessage = "VIN повинен містити 17 символів (латиниця A-H,J-N,P-R,Z, цифри 0-9, без I,O,Q)")]
    [Display(Name = "VIN-код")]
    public string VinCode       { get; set; } = string.Empty;

    [Range(0.01, 9999999)]
    [Display(Name = "Собівартість (USD)")]
    public decimal AuctionCost  { get; set; }

    [MaxLength(20)]
    [Display(Name = "Аукціон")]
    public string AuctionName   { get; set; } = string.Empty;

    public DeliveryStatus Status { get; set; } = DeliveryStatus.AtAuction;
}

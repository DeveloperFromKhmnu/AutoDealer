using System.ComponentModel.DataAnnotations;

namespace AutoDealerCRM.ViewModels;

/// <summary>Форма заявки покупця (публічна сторінка).</summary>
public class BuyerRequestFormViewModel
{
    [Required(ErrorMessage = "Введіть ПІБ")]
    [MaxLength(150)]
    [Display(Name = "ПІБ")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть номер телефону")]
    [Phone]
    [MaxLength(20)]
    [Display(Name = "Телефон")]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(300)]
    [Display(Name = "Бажана модель / побажання")]
    public string DesiredModel { get; set; } = string.Empty;
}

/// <summary>ViewModel для менеджера: розшифровані контактні дані.</summary>
public class BuyerRequestManagerViewModel
{
    public int      Id           { get; set; }
    public string   FullName     { get; set; } = string.Empty;
    public string   Phone        { get; set; } = string.Empty;   // розшифровано
    public string   DesiredModel { get; set; } = string.Empty;
    public DateTime CreatedAt    { get; set; }
    public bool     IsProcessed  { get; set; }
}

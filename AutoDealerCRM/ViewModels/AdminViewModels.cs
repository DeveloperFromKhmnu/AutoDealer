using System.ComponentModel.DataAnnotations;

namespace AutoDealerCRM.ViewModels;

public class CreateManagerViewModel
{
    [Required(ErrorMessage = "Обов'язкове поле")]
    [EmailAddress]
    [Display(Name = "Email менеджера")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Обов'язкове поле")]
    [MinLength(6, ErrorMessage = "Мінімум 6 символів")]
    [DataType(DataType.Password)]
    [Display(Name = "Початковий пароль")]
    public string Password { get; set; } = string.Empty;
}

public class UserListItemViewModel
{
    public string   Id        { get; set; } = string.Empty;
    public string   Email     { get; set; } = string.Empty;
    public string   Role      { get; set; } = string.Empty;
    public bool     IsBlocked { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LicenseViewModel
{
    public bool      IsValid    { get; set; }
    public string?   Owner      { get; set; }
    public DateTime? ExpiresAt  { get; set; }
    public string    Message    { get; set; } = string.Empty;
}

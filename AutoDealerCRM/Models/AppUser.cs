using Microsoft.AspNetCore.Identity;

namespace AutoDealerCRM.Models;

/// <summary>
/// Обліковий запис користувача системи (розширює IdentityUser).
/// ЛР №4: зберігає автентифікаційні дані через ASP.NET Core Identity (PBKDF2).
/// ЛР №5: поле Role використовується для RBAC-авторизації.
/// </summary>
public class AppUser : IdentityUser
{
    /// <summary>Дата створення облікового запису.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Ознака блокування. Заблокований користувач не може увійти
    /// навіть при вірному паролі (ЛР №4, бізнес-правило).
    /// </summary>
    public bool IsBlocked { get; set; } = false;
}

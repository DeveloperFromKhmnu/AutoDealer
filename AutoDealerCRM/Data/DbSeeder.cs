using AutoDealerCRM.Models;
using Microsoft.AspNetCore.Identity;

namespace AutoDealerCRM.Data;

/// <summary>
/// Ініціалізація бази даних початковими даними.
/// Створює ролі Admin/Manager та seed-акаунт адміністратора.
/// DEMO-ONLY: seed-пароль admin123! — лише для навчального проєкту.
/// </summary>
public static class DbSeeder
{
    public const string AdminRole   = "Admin";
    public const string ManagerRole = "Manager";

    // DEMO-ONLY: фіксований seed-пароль лише для навчального проєкту
    private const string AdminEmail    = "admin@dealercrm.local";
    private const string AdminPassword = "Admin123!";

    private const string DemoManagerEmail    = "manager@dealercrm.local";
    private const string DemoManagerPassword = "Manager123!";

    public static async Task SeedAsync(
        UserManager<AppUser>  userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // 1. Створити ролі
        await EnsureRoleAsync(roleManager, AdminRole);
        await EnsureRoleAsync(roleManager, ManagerRole);

        // 2. Seed адміністратора
        await EnsureUserAsync(userManager, AdminEmail, AdminPassword, AdminRole);

        // 3. Seed тестового менеджера (для демонстрації)
        await EnsureUserAsync(userManager, DemoManagerEmail, DemoManagerPassword, ManagerRole);
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> rm, string roleName)
    {
        if (!await rm.RoleExistsAsync(roleName))
            await rm.CreateAsync(new IdentityRole(roleName));
    }

    private static async Task EnsureUserAsync(
        UserManager<AppUser> um,
        string email, string password, string role)
    {
        if (await um.FindByEmailAsync(email) is not null) return;

        var user = new AppUser
        {
            UserName  = email,
            Email     = email,
            CreatedAt = DateTime.UtcNow
        };

        var result = await um.CreateAsync(user, password);
        if (result.Succeeded)
            await um.AddToRoleAsync(user, role);
    }
}

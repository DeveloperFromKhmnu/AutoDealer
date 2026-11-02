using AutoDealerCRM.Models;
using AutoDealerCRM.Services;
using AutoDealerCRM.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AutoDealerCRM.Controllers;

/// <summary>
/// Автентифікація: вхід та вихід. ЛР №4.
/// Реалізація: ASP.NET Core Identity + Cookie Auth (PBKDF2 хешування паролів).
/// Журналювання: LoginSuccess, LoginFailed, Logout → AuditLog.
/// Захист: загальне повідомлення про помилку (не розкриває чи існує акаунт).
/// </summary>
public class AccountController : Controller
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser>  _userManager;
    private readonly IAuditLogService      _audit;

    public AccountController(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser>  userManager,
        IAuditLogService      audit)
    {
        _signInManager = signInManager;
        _userManager   = userManager;
        _audit         = audit;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (!ModelState.IsValid)
            return View(model);

        // Ідентифікація: пошук користувача за email
        var user = await _userManager.FindByEmailAsync(model.Email);

        // Якщо користувач не знайдений — повертаємо загальне повідомлення
        // (не розкриваємо чи існує акаунт — ЛР №4, теорія)
        if (user is null)
        {
            await _audit.LogAsync(AuditEventType.LoginFailed, model.Email,
                "", ip, "Користувача не знайдено");
            ModelState.AddModelError("", "Невірний email або пароль.");
            return View(model);
        }

        // Перевірка блокування (до перевірки пароля — ЛР №4, бізнес-правило)
        if (user.IsBlocked)
        {
            await _audit.LogAsync(AuditEventType.LoginFailed, model.Email,
                "", ip, "Акаунт заблоковано");
            ModelState.AddModelError("", "Ваш обліковий запис заблоковано. Зверніться до адміністратора.");
            return View(model);
        }

        // Автентифікація: перевірка пароля (PBKDF2 через Identity)
        var result = await _signInManager.PasswordSignInAsync(
            user, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            await _audit.LogAsync(AuditEventType.LoginSuccess, model.Email,
                "", ip);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // Невірний пароль — загальне повідомлення
        await _audit.LogAsync(AuditEventType.LoginFailed, model.Email,
            "", ip, "Невірний пароль");
        ModelState.AddModelError("", "Невірний email або пароль.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var actor = User.Identity?.Name ?? "Unknown";
        var ip    = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        await _signInManager.SignOutAsync();
        await _audit.LogAsync(AuditEventType.Logout, actor, "", ip);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();
}

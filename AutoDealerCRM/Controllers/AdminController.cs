using AutoDealerCRM.Data;
using AutoDealerCRM.Models;
using AutoDealerCRM.Services;
using AutoDealerCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoDealerCRM.Controllers;

/// <summary>
/// Панель адміністратора. ЛР №5 (RBAC: Тільки роль Admin).
/// Керування обліковими записами, перевірка ліцензії (ЛР №7), журнал аудиту (ЛР №4).
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILicenseService _licenseService;
    private readonly IAuditLogService _audit;
    private readonly AppDbContext _db;

    public AdminController(
        UserManager<AppUser> userManager,
        ILicenseService licenseService,
        IAuditLogService audit,
        AppDbContext db)
    {
        _userManager = userManager;
        _licenseService = licenseService;
        _audit = audit;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        var userList = new List<UserListItemViewModel>();

        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            userList.Add(new UserListItemViewModel
            {
                Id = u.Id,
                Email = u.Email ?? u.UserName ?? "",
                Role = string.Join(", ", roles),
                IsBlocked = u.IsBlocked,
                CreatedAt = u.CreatedAt
            });
        }

        return View(userList);
    }

    public IActionResult CreateManager() => View(new CreateManagerViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateManager(CreateManagerViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var actor = User.Identity?.Name ?? "Admin";
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var user = new AppUser
        {
            UserName = model.Email,
            Email = model.Email,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, DbSeeder.ManagerRole);
            await _audit.LogAsync(AuditEventType.UserCreated, actor, model.Email, ip, "Створено менеджера");
            TempData["Success"] = $"Менеджера {model.Email} успішно створено!";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBlockUser(string id)
    {
        var actor = User.Identity?.Name ?? "Admin";
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        // Захист: адмін не може заблокувати самого себе
        if (user.UserName == actor)
        {
            TempData["Error"] = "Адміністратор не може заблокувати сам себе!";
            return RedirectToAction(nameof(Index));
        }

        user.IsBlocked = !user.IsBlocked;
        await _userManager.UpdateAsync(user);

        var eventType = user.IsBlocked ? AuditEventType.UserBlocked : AuditEventType.UserUnblocked;
        await _audit.LogAsync(eventType, actor, user.UserName ?? user.Id, ip);

        TempData["Success"] = $"Статус користувача {user.UserName} змінено на: {(user.IsBlocked ? "Заблоковано" : "Активний")}";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult License()
    {
        var status = _licenseService.CurrentStatus;
        var vm = new LicenseViewModel
        {
            IsValid = status.IsValid,
            Owner = status.Owner,
            ExpiresAt = status.ExpiresAt,
            Message = status.ErrorMessage
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadLicense(IFormFile? licenseFile)
    {
        var actor = User.Identity?.Name ?? "Admin";
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (licenseFile == null || licenseFile.Length == 0)
        {
            TempData["Error"] = "Оберіть файл dealer.lic для завантаження.";
            return RedirectToAction(nameof(License));
        }

        using var stream = licenseFile.OpenReadStream();
        var result = await _licenseService.LoadLicenseAsync(stream);

        if (result.IsValid)
        {
            await _audit.LogAsync(AuditEventType.LicenseUploaded, actor, result.Owner ?? "", ip,
                $"Ліцензію активовано до {result.ExpiresAt:dd.MM.yyyy}");
            TempData["Success"] = $"Ліцензію успішно верифіковано! Власник: {result.Owner}, діє до: {result.ExpiresAt:dd.MM.yyyy}";
        }
        else
        {
            await _audit.LogAsync(AuditEventType.LicenseUploadFailed, actor, "dealer.lic", ip, result.ErrorMessage);
            TempData["Error"] = $"Помилка перевірки ліцензії: {result.ErrorMessage}";
        }

        return RedirectToAction(nameof(License));
    }

    public async Task<IActionResult> AuditLogs(AuditEventType? filter)
    {
        ViewBag.Filter = filter;
        var logs = await _audit.GetLogsAsync(filter, 300);
        return View(logs);
    }
}

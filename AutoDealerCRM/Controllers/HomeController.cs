using AutoDealerCRM.Data;
using AutoDealerCRM.Models;
using AutoDealerCRM.Services;
using AutoDealerCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoDealerCRM.Controllers;

/// <summary>
/// Публічний каталог (доступно всім) та форма заявок покупців.
/// ЛР №5: публічні ендпоінти позначені [AllowAnonymous].
/// ЛР №6: VIN читається з БД у зашифрованому вигляді, маскується для гостей.
/// </summary>
public class HomeController : Controller
{
    private readonly AppDbContext _db;
    private readonly IEncryptionService _enc;

    public HomeController(AppDbContext db, IEncryptionService enc)
    {
        _db = db;
        _enc = enc;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var lots = await _db.CarLots.OrderByDescending(c => c.AddedAt).ToListAsync();

        var vms = lots.Select(lot =>
        {
            // Розшифровуємо VIN лише для маскування; повний VIN не передається в View
            string maskedVin;
            try
            {
                var fullVin = _enc.Decrypt(lot.VinCodeEncrypted);
                maskedVin = fullVin.Length >= 11
                    ? fullVin[..11] + new string('*', fullVin.Length - 11)
                    : fullVin + "******";
            }
            catch
            {
                maskedVin = "XXXXXXXXXXX******";
            }

            return new CarLotPublicViewModel
            {
                Id = lot.Id,
                Make = lot.Make,
                Model = lot.Model,
                Year = lot.Year,
                MileageKm = (int)(lot.Mileage * 1.60934),
                PublicPrice = lot.PublicPrice,
                Status = lot.Status,
                MaskedVin = maskedVin
            };
        }).ToList();

        return View(vms);
    }

    [AllowAnonymous]
    public IActionResult SubmitRequest() => View(new BuyerRequestFormViewModel());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitRequest(BuyerRequestFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // ЛР №6: шифруємо телефон перед збереженням
        var request = new BuyerRequest
        {
            FullName = model.FullName,
            PhoneEncrypted = _enc.Encrypt(model.Phone),
            DesiredModel = model.DesiredModel
        };

        _db.BuyerRequests.Add(request);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Дякуємо! Ваша заявка прийнята. Менеджер зв'яжеться з вами найближчим часом.";
        return RedirectToAction(nameof(SubmitRequest));
    }

    [AllowAnonymous]
    public IActionResult Privacy() => View();
}

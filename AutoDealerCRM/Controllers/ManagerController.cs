using AutoDealerCRM.Data;
using AutoDealerCRM.Models;
using AutoDealerCRM.Services;
using AutoDealerCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoDealerCRM.Controllers;

/// <summary>
/// Кабінет менеджера. ЛР №5 (RBAC: доступ для Manager та Admin).
/// ЛР №6: доступ до розшифрованого VIN та собівартості, шифрування при збереженні.
/// </summary>
[Authorize(Roles = "Manager,Admin")]
public class ManagerController : Controller
{
    private readonly AppDbContext _db;
    private readonly IEncryptionService _enc;

    public ManagerController(AppDbContext db, IEncryptionService enc)
    {
        _db = db;
        _enc = enc;
    }

    public async Task<IActionResult> Index()
    {
        var lots = await _db.CarLots.OrderByDescending(c => c.AddedAt).ToListAsync();

        var vms = lots.Select(lot =>
        {
            string vin;
            decimal cost = 0;
            try
            {
                vin = _enc.Decrypt(lot.VinCodeEncrypted);
            }
            catch
            {
                vin = "ПОМИЛКА ЦІЛІСНОСТІ";
            }

            try
            {
                var costStr = _enc.Decrypt(lot.AuctionCostEncrypted);
                decimal.TryParse(costStr, out cost);
            }
            catch
            {
                cost = -1;
            }

            return new CarLotManagerViewModel
            {
                Id = lot.Id,
                Make = lot.Make,
                Model = lot.Model,
                Year = lot.Year,
                MileageKm = (int)(lot.Mileage * 1.60934),
                PublicPrice = lot.PublicPrice,
                Status = lot.Status,
                MaskedVin = vin.Length >= 11 ? vin[..11] + "******" : vin,
                FullVin = vin,
                AuctionCost = cost,
                AuctionName = lot.AuctionName,
                AddedAt = lot.AddedAt
            };
        }).ToList();

        return View(vms);
    }

    public IActionResult Create() => View(new CarLotEditViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CarLotEditViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var lot = new CarLot
        {
            Make = vm.Make,
            Model = vm.Model,
            Year = vm.Year,
            Mileage = vm.Mileage,
            PublicPrice = vm.PublicPrice,
            Status = vm.Status,
            AuctionName = vm.AuctionName ?? "",
            VinCodeEncrypted = _enc.Encrypt(vm.VinCode.Trim().ToUpperInvariant()),
            AuctionCostEncrypted = _enc.Encrypt(vm.AuctionCost.ToString("F2")),
            AddedAt = DateTime.UtcNow
        };

        _db.CarLots.Add(lot);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Лот успішно додано!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var lot = await _db.CarLots.FindAsync(id);
        if (lot == null) return NotFound();

        string vin = "";
        decimal cost = 0;
        try { vin = _enc.Decrypt(lot.VinCodeEncrypted); } catch { vin = "ERROR"; }
        try { decimal.TryParse(_enc.Decrypt(lot.AuctionCostEncrypted), out cost); } catch { }

        var vm = new CarLotEditViewModel
        {
            Id = lot.Id,
            Make = lot.Make,
            Model = lot.Model,
            Year = lot.Year,
            Mileage = lot.Mileage,
            PublicPrice = lot.PublicPrice,
            Status = lot.Status,
            AuctionName = lot.AuctionName,
            VinCode = vin,
            AuctionCost = cost
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CarLotEditViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var lot = await _db.CarLots.FindAsync(vm.Id);
        if (lot == null) return NotFound();

        lot.Make = vm.Make;
        lot.Model = vm.Model;
        lot.Year = vm.Year;
        lot.Mileage = vm.Mileage;
        lot.PublicPrice = vm.PublicPrice;
        lot.Status = vm.Status;
        lot.AuctionName = vm.AuctionName ?? "";
        lot.VinCodeEncrypted = _enc.Encrypt(vm.VinCode.Trim().ToUpperInvariant());
        lot.AuctionCostEncrypted = _enc.Encrypt(vm.AuctionCost.ToString("F2"));

        await _db.SaveChangesAsync();

        TempData["Success"] = "Дані лота оновлено!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Requests()
    {
        var requests = await _db.BuyerRequests.OrderByDescending(r => r.CreatedAt).ToListAsync();

        var vms = requests.Select(r =>
        {
            string phone;
            try
            {
                phone = _enc.Decrypt(r.PhoneEncrypted);
            }
            catch
            {
                phone = "ПОМИЛКА ЦІЛІСНОСТІ";
            }

            return new BuyerRequestManagerViewModel
            {
                Id = r.Id,
                FullName = r.FullName,
                Phone = phone,
                DesiredModel = r.DesiredModel,
                CreatedAt = r.CreatedAt,
                IsProcessed = r.IsProcessed
            };
        }).ToList();

        return View(vms);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleRequestStatus(int id)
    {
        var req = await _db.BuyerRequests.FindAsync(id);
        if (req != null)
        {
            req.IsProcessed = !req.IsProcessed;
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Requests));
    }
}

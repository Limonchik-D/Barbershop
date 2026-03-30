using Barbershop.Data;
using Barbershop.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Barbershop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Manager")]
public class ServicesController : Controller
{
    private readonly ApplicationDbContext _db;

    public ServicesController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var services = await _db.Services
            .Include(s => s.Category)
            .OrderBy(s => s.Category.SortOrder)
            .ThenBy(s => s.Name)
            .ToListAsync();
        return View(services);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulateCategories();
        return View(new Service());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Service model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategories();
            return View(model);
        }
        model.CreatedAt = DateTime.UtcNow;
        model.UpdatedAt = DateTime.UtcNow;
        _db.Services.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Услуга добавлена";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var service = await _db.Services.FindAsync(id);
        if (service == null) return NotFound();
        await PopulateCategories(service.CategoryId);
        return View(service);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Service model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            await PopulateCategories(model.CategoryId);
            return View(model);
        }
        model.UpdatedAt = DateTime.UtcNow;
        _db.Services.Update(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Услуга обновлена";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var service = await _db.Services.FindAsync(id);
        if (service == null) return NotFound();
        service.IsActive = false;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Услуга деактивирована";
        return RedirectToAction(nameof(Index));
    }

    // Категории услуг
    public async Task<IActionResult> Categories()
    {
        var cats = await _db.ServiceCategories.OrderBy(c => c.SortOrder).ToListAsync();
        return View(cats);
    }

    [HttpGet]
    public IActionResult CreateCategory() => View(new ServiceCategory());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(ServiceCategory model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.ServiceCategories.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Категория добавлена";
        return RedirectToAction(nameof(Categories));
    }

    [HttpGet]
    public async Task<IActionResult> EditCategory(int id)
    {
        var cat = await _db.ServiceCategories.FindAsync(id);
        if (cat == null) return NotFound();
        return View(cat);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(int id, ServiceCategory model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        _db.ServiceCategories.Update(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Категория обновлена";
        return RedirectToAction(nameof(Categories));
    }

    private async Task PopulateCategories(int? selectedId = null)
    {
        var cats = await _db.ServiceCategories.Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToListAsync();
        ViewBag.Categories = new SelectList(cats, "Id", "Name", selectedId);
    }
}

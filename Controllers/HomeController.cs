using Barbershop.Data;
using Barbershop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Barbershop.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _db.ServiceCategories
            .Where(c => c.IsActive)
            .Include(c => c.Services.Where(s => s.IsActive))
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        var barbers = await _db.Barbers
            .Where(b => b.IsActive)
            .Take(6)
            .ToListAsync();

        var reviews = await _db.Reviews
            .Where(r => r.IsApproved)
            .Include(r => r.Client)
            .Include(r => r.Barber)
            .OrderByDescending(r => r.CreatedAt)
            .Take(6)
            .ToListAsync();

        ViewBag.Categories = categories;
        ViewBag.Barbers = barbers;
        ViewBag.Reviews = reviews;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

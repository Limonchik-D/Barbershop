using Barbershop.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Barbershop.Controllers;

public class BarbersController : Controller
{
    private readonly ApplicationDbContext _db;

    public BarbersController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var barbers = await _db.Barbers
            .Where(b => b.IsActive)
            .Include(b => b.BarberServices)
            .ThenInclude(bs => bs.Service)
            .ToListAsync();

        return View(barbers);
    }

    public async Task<IActionResult> Details(int id)
    {
        var barber = await _db.Barbers
            .Include(b => b.BarberServices)
            .ThenInclude(bs => bs.Service)
            .ThenInclude(s => s.Category)
            .Include(b => b.Reviews.Where(r => r.IsApproved))
            .ThenInclude(r => r.Client)
            .FirstOrDefaultAsync(b => b.Id == id && b.IsActive);

        if (barber == null) return NotFound();

        ViewBag.AverageRating = barber.Reviews.Any()
            ? barber.Reviews.Average(r => r.Rating)
            : 0.0;

        return View(barber);
    }
}

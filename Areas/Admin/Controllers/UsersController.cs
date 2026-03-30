using Barbershop.Data;
using Barbershop.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Barbershop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search = null, string? role = null)
    {
        var users = await _db.Users.ToListAsync();
        var result = new List<(ApplicationUser User, IList<string> Roles)>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (!string.IsNullOrEmpty(role) && !roles.Contains(role)) continue;
            if (!string.IsNullOrEmpty(search))
            {
                var q = search.ToLower();
                if (!user.FullName.ToLower().Contains(q) && !(user.Email ?? "").ToLower().Contains(q))
                    continue;
            }
            result.Add((user, roles));
        }

        ViewBag.AllRoles = new[] { "Admin", "Manager", "Barber", "Client" };
        ViewBag.CurrentSearch = search;
        ViewBag.CurrentRole = role;
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var userRoles = await _userManager.GetRolesAsync(user);
        ViewBag.UserRoles = userRoles;
        ViewBag.AllRoles = new[] { "Admin", "Manager", "Barber", "Client" };
        return View(user);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, string[] roles, bool isActive)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        user.IsActive = isActive;
        await _userManager.UpdateAsync(user);

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRolesAsync(user, roles);

        TempData["Success"] = "Пользователь обновлён";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        user.IsActive = false;
        await _userManager.UpdateAsync(user);
        TempData["Success"] = "Пользователь деактивирован";
        return RedirectToAction(nameof(Index));
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using praesentationsanmeldung.Auth;
using praesentationsanmeldung.Data;

namespace praesentationsanmeldung.Controllers;

[Authorize(Roles = Rollen.Admin)]
public class UebersichtController : Controller
{
    private readonly AppDbContext _db;

    public UebersichtController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var praesentationen = await _db.Praesentationen
            .Include(p => p.Raum)
            .Include(p => p.Eintragungen)
            .ThenInclude(e => e.G3Sus)
            .OrderBy(p => p.Beginn)
            .ThenBy(p => p.Titel)
            .ToListAsync();
        return View(praesentationen);
    }
}

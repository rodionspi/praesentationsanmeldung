
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using praesentationsanmeldung.Auth;
using praesentationsanmeldung.Data;
using praesentationsanmeldung.Models;

namespace praesentationsanmeldung.Controllers;

[Authorize(Roles = Rollen.Admin)]
public class RaemeController : Controller
{ 
    private readonly AppDbContext _db;

    public RaemeController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var raeume = await _db.Raeume.OrderBy(r => r.Bezeichnung).ToListAsync();
        return View(raeume);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken] // Ohne das könnte eine fremde Webseite, auf der man zufällig gerade eingeloggt ist, heimlich ein Formular an seine App schicken und sich als ihn ausgeben.
    public async Task<IActionResult> Create([Bind("Bezeichnung,Kapazitaet")] Raum raum)
    // Das schränkt ein, welche Felder aus dem Formular überhaupt ins raum-Objekt übernommen werden.
    {
        if (await _db.Raeume.AnyAsync(r => r.Bezeichnung == raum.Bezeichnung))
        {
            ModelState.AddModelError(nameof(Raum.Bezeichnung), "Diese Bezeichnung gibt es schon.");
        }
    }
}

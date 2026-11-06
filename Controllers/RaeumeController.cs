using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using praesentationsanmeldung.Auth;
using praesentationsanmeldung.Data;
using praesentationsanmeldung.Models;

namespace praesentationsanmeldung.Controllers;

[Authorize(Roles = Rollen.Admin)]
public class RaeumeController : Controller
{
    private readonly AppDbContext _db;

    public RaeumeController(AppDbContext db)
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Bezeichnung,Kapazitaet")] Raum raum)
    {
        if (await _db.Raeume.AnyAsync(r => r.Bezeichnung == raum.Bezeichnung))
        {
            ModelState.AddModelError(nameof(Raum.Bezeichnung), "Diese Bezeichnung gibt es schon.");
        }

        if (!ModelState.IsValid)
        {
            return View(raum);
        }

        _db.Raeume.Add(raum);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var raum = await _db.Raeume.FindAsync(id);
        if (raum == null)
        {
            return NotFound();
        }

        return View(raum);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Bezeichnung,Kapazitaet")] Raum eingabe)
    {
        var raum = await _db.Raeume.FindAsync(id);
        if (raum == null)
        {
            return NotFound();
        }

        if (await _db.Raeume.AnyAsync(r => r.Bezeichnung == eingabe.Bezeichnung && r.Id != id))
        {
            ModelState.AddModelError(nameof(Raum.Bezeichnung), "Diese Bezeichnung gibt es schon.");
        }

        if (!ModelState.IsValid)
        {
            return View(eingabe);
        }

        raum.Bezeichnung = eingabe.Bezeichnung;
        raum.Kapazitaet = eingabe.Kapazitaet;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (await _db.Praesentationen.AnyAsync(p => p.RaumId == id))
        {
            TempData["Fehler"] = "Dieser Raum hat noch Präsentationen und kann nicht gelöscht werden.";
            return RedirectToAction(nameof(Index));
        }

        var raum = await _db.Raeume.FindAsync(id);
        if (raum != null)
        {
            _db.Raeume.Remove(raum);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
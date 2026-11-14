using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using praesentationsanmeldung.Auth;
using praesentationsanmeldung.Data;
using praesentationsanmeldung.Models;

namespace praesentationsanmeldung.Controllers;

[Authorize(Roles = Rollen.Admin)]
public class PraesentationenController : Controller
{
    private readonly AppDbContext _db;

    public PraesentationenController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var praesentationen = await _db.Praesentationen
            .Include(p => p.Raum)
            .OrderBy(p => p.Beginn)
            .ThenBy(p => p.Titel)
            .ToListAsync();
        return View(praesentationen);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await RaeumeLaden();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Titel,Beschreibung,Beginn,RaumId")] Praesentation praesentation)
    {
        if (!await _db.Raeume.AnyAsync(r => r.Id == praesentation.RaumId))
        {
            ModelState.AddModelError(nameof(Praesentation.RaumId), "Bitte einen Raum auswählen.");
        }

        if (!ModelState.IsValid)
        {
            await RaeumeLaden();
            return View(praesentation);
        }

        if (praesentation.Beginn <= DateTime.Now)
        {
            ModelState.AddModelError(nameof(Praesentation.Beginn), "Der Beginn muss in der Zukunft liegen.");

        }

        _db.Praesentationen.Add(praesentation);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var praesentation = await _db.Praesentationen.FindAsync(id);
        if (praesentation == null)
        {
            return NotFound();
        }

        await RaeumeLaden();
        return View(praesentation);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Titel,Beschreibung,Beginn,RaumId")] Praesentation eingabe)
    {
        var praesentation = await _db.Praesentationen.FindAsync(id);
        if (praesentation == null)
        {
            return NotFound();
        }

        if (!await _db.Raeume.AnyAsync(r => r.Id == eingabe.RaumId))
        {
            ModelState.AddModelError(nameof(Praesentation.RaumId), "Bitte einen Raum auswählen.");
        }

        if (!ModelState.IsValid)
        {
            await RaeumeLaden();
            return View(eingabe);
        }

        praesentation.Titel = eingabe.Titel;
        praesentation.Beschreibung = eingabe.Beschreibung;
        praesentation.Beginn = eingabe.Beginn;
        praesentation.RaumId = eingabe.RaumId;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var praesentation = await _db.Praesentationen.FindAsync(id);
        if (praesentation != null)
        {
            _db.Praesentationen.Remove(praesentation);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task RaeumeLaden()
    {
        var raeume = await _db.Raeume.OrderBy(r => r.Bezeichnung).ToListAsync();
        ViewBag.Raeume = new SelectList(raeume, nameof(Raum.Id), nameof(Raum.Bezeichnung));
    }
}

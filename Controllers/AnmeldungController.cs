using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using praesentationsanmeldung.Auth;
using praesentationsanmeldung.Data;
using praesentationsanmeldung.Models;

namespace praesentationsanmeldung.Controllers;

[Authorize(Roles = Rollen.G3SuS)]
public class AnmeldungController : Controller
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly int _maxEintragungen;

    public AnmeldungController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
        _maxEintragungen = config.GetValue<int>("MaxEintragungen");
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var g3SusId = AngemeldeteId();

        var praesentationen = await _db.Praesentationen
            .OrderBy(p => p.Beginn)
            .ThenBy(p => p.Titel)
            .Select(p => new PraesentationMitPlaetzen
            {
                Id = p.Id,
                Titel = p.Titel,
                Beschreibung = p.Beschreibung,
                Beginn = p.Beginn,
                Raum = p.Raum.Bezeichnung,
                FreiePlaetze = p.Raum.Kapazitaet - p.Eintragungen.Count,
                Eingetragen = p.Eintragungen.Any(e => e.G3SusId == g3SusId)
            })
            .ToListAsync();

        ViewBag.Anzahl = await _db.Eintragungen.CountAsync(e => e.G3SusId == g3SusId);
        ViewBag.Max = _maxEintragungen;
        return View(praesentationen);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eintragen(int id)
    {
        var g3SusId = AngemeldeteId();

        await using var transaktion = await _db.Database.BeginTransactionAsync();

        var praesentation = await _db.Praesentationen
            .FromSql($"SELECT * FROM Praesentationen WITH (UPDLOCK) WHERE Id = {id}")
            .Include(p => p.Raum)
            .SingleOrDefaultAsync();

        if (praesentation == null)
        {
            return NotFound();
        }

        if (praesentation.Beginn <= DateTime.Now)
        {
            TempData["Fehler"] = "Diese Präsentation hat schon begonnen.";
            return RedirectToAction(nameof(Index));
        }

        var g3Sus = await _db.G3Sus
            .FromSql($"SELECT * FROM G3Sus WITH (UPDLOCK) WHERE Id = {g3SusId}")
            .SingleAsync();

        if (await _db.Eintragungen.AnyAsync(e => e.G3SusId == g3SusId && e.PraesentationId == id))
        {
            TempData["Fehler"] = "Du bist für diese Präsentation schon eingetragen.";
            return RedirectToAction(nameof(Index));
        }

        if (await _db.Eintragungen.AnyAsync(e => e.G3SusId == g3SusId && e.Praesentation.Beginn == praesentation.Beginn))
        {
            TempData["Fehler"] = "Du bist zur gleichen Zeit schon für eine andere Präsentation eingetragen.";
            return RedirectToAction(nameof(Index));
        }

        var belegt = await _db.Eintragungen.CountAsync(e => e.PraesentationId == id);
        if (belegt >= praesentation.Raum.Kapazitaet)
        {
            TempData["Fehler"] = "Diese Präsentation ist leider schon voll.";
            return RedirectToAction(nameof(Index));
        }

        var meine = await _db.Eintragungen.CountAsync(e => e.G3SusId == g3SusId);
        if (meine >= _maxEintragungen)
        {
            TempData["Fehler"] = $"Du hast schon die maximale Anzahl von {_maxEintragungen} Eintragungen erreicht.";
            return RedirectToAction(nameof(Index));
        }

        _db.Eintragungen.Add(new Eintragung
        {
            G3SusId = g3SusId,
            PraesentationId = id,
            EingetragenAm = DateTime.Now
        });
        await _db.SaveChangesAsync();
        await transaktion.CommitAsync();

        TempData["Meldung"] = $"Du bist für «{praesentation.Titel}» eingetragen.";

        if (string.IsNullOrEmpty(_config["Smtp:Host"]))
        {
            TempData["Fehler"] = "Der Mailversand ist nicht eingerichtet, darum kommt keine Bestätigungsmail.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await BestaetigungSenden(g3Sus, praesentation);
        }
        catch (SmtpException)
        {
            TempData["Fehler"] = "Die Bestätigungsmail konnte nicht gesendet werden.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Meine()
    {
        var g3SusId = AngemeldeteId();

        var eintragungen = await _db.Eintragungen
            .Where(e => e.G3SusId == g3SusId)
            .Include(e => e.Praesentation)
            .ThenInclude(p => p.Raum)
            .OrderBy(e => e.Praesentation.Beginn)
            .ToListAsync();

        ViewBag.Max = _maxEintragungen;
        return View(eintragungen);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Austragen(int id)
    {
        var g3SusId = AngemeldeteId();

        var eintragung = await _db.Eintragungen.SingleOrDefaultAsync(e => e.Id == id && e.G3SusId == g3SusId);
        if (eintragung != null)
        {
            _db.Eintragungen.Remove(eintragung);
            await _db.SaveChangesAsync();
            TempData["Meldung"] = "Du wurdest ausgetragen.";
        }

        return RedirectToAction(nameof(Meine));
    }

    private async Task BestaetigungSenden(G3Sus g3Sus, Praesentation praesentation)
    {
        var text = $"Hallo {g3Sus.Vorname}\n\n" +
                   $"Du bist für die Präsentation «{praesentation.Titel}» eingetragen.\n" +
                   $"Beginn: {praesentation.Beginn:dd.MM.yyyy HH:mm} Uhr\n" +
                   $"Raum: {praesentation.Raum.Bezeichnung}\n\n" +
                   "Freundliche Grüsse";

        using var client = new SmtpClient(_config["Smtp:Host"], _config.GetValue<int>("Smtp:Port"));
        client.EnableSsl = true;
        client.Credentials = new NetworkCredential(_config["Smtp:Benutzer"], _config["Smtp:Passwort"]);

        using var mail = new MailMessage(_config["Smtp:Absender"]!, g3Sus.Email, "Bestätigung deiner Eintragung", text);
        await client.SendMailAsync(mail);
    }

    private int AngemeldeteId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}

using System.Globalization;
// .NET-Standardbibliothek für kultur-/sprachabhängige Formatierung — z. B. Datum, Zahlen, Gross-/Kleinschreibung je nach Land/Sprache (CultureInfo). Wird gebraucht, wenn z. B. ein Datum aus einem String geparst oder ein deutsches Zahlenformat (Komma statt Punkt) korrekt verarbeitet werden soll.
using Microsoft.AspNetCore.Authorization;
//Stellt [Authorize] (und verwandte Attribute wie [AllowAnonymous]) bereit
using Microsoft.AspNetCore.Mvc;
// 	Das MVC-Kernframework — liefert Controller, IActionResult, [HttpGet]/[HttpPost], View(), RedirectToAction() usw. Praktisch das Herzstück jedes Controllers.
using Microsoft.EntityFrameworkCore;
// 	Der ORM (ADO-Alternative) für Datenbankzugriff — stellt Erweiterungsmethoden wie .Any(), .Include(), .ToListAsync(), .Where() auf DbSet<T> bereit
using praesentationsanmeldung.Auth;
using praesentationsanmeldung.Data;
using praesentationsanmeldung.Models;

namespace praesentationsanmeldung.Controllers;

[Authorize(Roles = Rollen.Admin)]
//Nur Nutzer mit der Rolle Admin dürfen hier reinkommen.
public class ImportController : Controller
{
    private readonly AppDbContext _db;

    public ImportController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> G3SusImportieren(IFormFile? datei)
    // Weil die Methode nicht sofort ein Ergebnis hat — sie muss erst auf Datei-Lesen und Datenbank-Speichern warten. Also sagt der Rückgabetyp ehrlich: “du bekommst nicht direkt ein IActionResult, du bekommst eine Wartemarke dafür, die sich später in ein IActionResult einlöst.”
    {
        var zeilen = await DateiLesen(datei, "Vorname;Nachname;Klasse");
        var neue = new List<G3Sus>();

        for (var i = 1; i < zeilen.Count; i++)
        {
            var nr = i + 1;
            if (string.IsNullOrWhiteSpace(zeilen[i]))
            {
                continue;
            }

            var spalten = zeilen[i].Split(";");
            if (spalten.Length != 3)
            {
                Fehler(nr, $"3 Spalten erwartet, aber {spalten.Length} gefunden");
                continue;
            }

            var vorname = spalten[0].Trim();
            var nachname = spalten[1].Trim();
            var klasse = spalten[2].Trim();


        }
    }

    private void Fehler(int nr, string text)
    {
        ModelState.AddModelError("", $"Zeile {nr}: {text}");
    }
}
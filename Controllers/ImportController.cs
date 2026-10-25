using System.Globalization;
using Humanizer;

// .NET-Standardbibliothek für kultur-/sprachabhängige Formatierung — z. B. Datum, Zahlen, Gross-/Kleinschreibung je nach Land/Sprache (CultureInfo). Wird gebraucht, wenn z. B. ein Datum aus einem String geparst oder ein deutsches Zahlenformat (Komma statt Punkt) korrekt verarbeitet werden soll.
using Microsoft.AspNetCore.Authorization;
//Stellt [Authorize] (und verwandte Attribute wie [AllowAnonymous]) bereit
using Microsoft.AspNetCore.Mvc;
// 	Das MVC-Kernframework — liefert Controller, IActionResult, [HttpGet]/[HttpPost], View(), RedirectToAction() usw. Praktisch das Herzstück jedes Controllers.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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

    private async Task<List<string>> DateiLesen (IFormFile? datei, string kopfzeile)
    {
        var zeilen = new List<string>();        
        if (datei == null || datei.Length == 0)
        {
            ModelState.AddModelError("", "Bitte CSV-Datei auswählen.");
            return zeilen;
        }
        if (Path.GetExtension(datei.FileName).ToLower() != ".csv")
        {
            ModelState.AddModelError("", "Es sind nur CSV-Dateien erlaubt");
            return zeilen;
        }
        using var reader = new StreamReader(datei.OpenReadStream());
        // using sorgt dafür, dass die Datei am Ende automatisch geschlossen wird, sobald die Methode fertig ist, auch bei einem Fehler. Ohne using bleibe die Datei offen und würde unnötig Ressourcen belegen.
        // datei.OpenReadStream(): öffnet die Datei als Stream, also als Strom von rohen Bytes, die man der Reihe nach lesen kann.
        // StreamReader(): legt einen "Übersetzer" um diesen Stream. Rohe Bytes kann man schlecht verarbeiten. Der StreamReader macht daraus Text und bietet Methoden wie ReadLineAsync(), die eine ganze Zeile auf einmal liefern.
        string? zeile;
        while ((zeile = await reader.ReadLineAsync()) != null)
        {
            zeilen.Add(zeile);
            if (zeile.Contains('\uFFFD'))
            // Prüft, ob die aktuelle Zeile dieses Ersatzzeichen irgendwo enthält. Wenn ja, ist die Datei falsch kodiert.
            {
                Fehler(zeilen.Count, "Ungültige Zeichen. Bitte die Datei als \"CSV UTF-8\" speichern.");
            }
        }

        if (zeilen.Count == 0 || zeilen[0].Trim() != kopfzeile)
        {
            Fehler(1, $"Die Kopfzeile muss '{kopfzeile}' sein.");
            return new List<string>();
        }
        
        return zeilen;
    }

    private void Fehler(int nr, string text)
    {
        ModelState.AddModelError("", $"Zeile {nr}: {text}");
    }
}
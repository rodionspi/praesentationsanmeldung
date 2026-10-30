using System.Globalization;
using System.Security.Principal;
using Humanizer;

// .NET-Standardbibliothek für kultur-/sprachabhängige Formatierung — z. B. Datum, Zahlen, Gross-/Kleinschreibung je nach Land/Sprache (CultureInfo). Wird gebraucht, wenn z. B. ein Datum aus einem String geparst oder ein deutsches Zahlenformat (Komma statt Punkt) korrekt verarbeitet werden soll.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web;

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

            TextPruefen(nr, "Vorname", vorname, 100);
            TextPruefen(nr, "Nachname", nachname, 100);
            TextPruefen(nr, "Klasse", klasse, 20);
            
            neue.Add(new G3Sus{ Vorname = vorname, Nachname = nachname, Klasse = klasse });
        }

        if (!ModelState.IsValid)
        // Ist eine Eigenschaft mit dem Wert true oder false:

        // true: Die Fehlerliste ist leer, alles in Ordnung
        // false: Es gibt mindestens einen Fehler
        {
            return View("Index");
        }

        await using var transaktion = await _db.Database.BeginTransactionAsync(); // Wir öffnen den Paket.
        try
        {
            _db.G3Sus.AddRange(neue); // EF Core merkt sich, dass es n neue gibt.
            await _db.SaveChangesAsync(); // jetzt gehen 3 INSERTs an die Datenbank.
            await transaktion.CommitAsync(); // alles dauerhaft machen.
        }
        catch(DbUpdateException) // Wenn beim Speichern in die Daten ein Fehler auftritt, fangen wir es hier ab.
        {
            await transaktion.RollbackAsync(); // Es macht alle Änderungen dieser Transaktion wieder ungeschehen.
            ModelState.AddModelError("", "Fehler beim Speichern. Es wurde nichts importiert");
            return View("Index");
        }

        TempData["Meldung"] = $"{neue.Count} G3Sus importiert.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> PraesentationenImportieren(IFormFile? datei)
    {
        var zeilen = await DateiLesen(datei, "Titel;Beschreibung;Beginn;Raum");
        var raume = await _db.Raeume.ToListAsync();
        // _db.Raeume ist die Tabelle Raeume in der Datenbank
        // ToListAsync() hollt alle Zeilen dieser Tabelle und lädt sie als List<Raum> in den Arbeitsspeicher
        var neue = new List<Praesentation>();

        for (var i = 1; i < zeilen.Count; i++)
        {
            var nr = i + 1;
            if (string.IsNullOrWhiteSpace(zeilen[i]))
            {
                continue;
            }

            var spalten = zeilen[i].Split(";");
            if (spalten.Length != 4)
            {
                Fehler(nr, $"4 Spalten erwartet, aber {spalten.Length} gefunden.");
                continue;
            }

            var titel = spalten[0].Trim();
            var beschreibung = spalten[1].Trim();
            var beginnText = spalten[2].Trim();
            var raumBezeichnung = spalten[3].Trim();

            TextPruefen(nr, "Titel", titel, 200);

            if (beschreibung.Length > 2000)
            {
                Fehler(nr, "Beschreibung darf höchstens 2000 Zeichen lang sein.");
            }

            if (!DateTime.TryParseExact(beginnText, "d.M.yyyy H:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var beginn))
            // Das versucht, einen Text wie "12.11.2026 08:00" in ein echtes DateTime-Objekt umzuwandeln, und meldet dabei zurück, ob es geklappt hat.
            {
                Fehler(nr, $"Beginn '{beginnText}' ist ungültig. Beispiel: 12.11.2026 08:00");
            }

            var raum = raume.FirstOrDefault(r => r.Bezeichnung == raumBezeichnung);
            // FirstOrDefault durchsucht die Liste und gibt das erste Element zurück
            if (raum == null)
            {
                Fehler(nr, "Raum {raumBezeichnung gibt es nicht.}");
                continue;
            }

            neue.Add(new Praesentation
            {
                Titel = titel,
                Beschreibung = beschreibung == "" ? null : beschreibung,
                Beginn = beginn,
                RaumId = raum.Id
            });
        }
        if (!ModelState.IsValid)
        {
            return View("Index");
        }

        await using var transaktion = await _db.Database.BeginTransactionAsync(); // Wir öffnen den Paket.
        try
        {
            _db.Praesentationen.AddRange(neue); // EF Core merkt sich, dass es n neue gibt.
            await _db.SaveChangesAsync(); // jetzt gehen 3 INSERTs an die Datenbank.
            await transaktion.CommitAsync(); // alles dauerhaft machen.
        }
        catch(DbUpdateException) // Wenn beim Speichern in die Daten ein Fehler auftritt, fangen wir es hier ab.
        {
            await transaktion.RollbackAsync(); // Es macht alle Änderungen dieser Transaktion wieder ungeschehen.
            ModelState.AddModelError("", "Fehler beim Speichern. Es wurde nichts importiert");
            return View("Index");
        }

        TempData["Meldung"] = $"{neue.Count} Präsentation importiert.";
        return RedirectToAction(nameof(Index));
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
        // await reader.ReadLineAsync(); liest eine Zeile aus 
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

    private void TextPruefen(int nr, string feld, string wert, int maxLength)
    {
        if (wert == "")
        {
            Fehler(nr, $"{feld} fehlt");
        } 
        else if (wert.Length > maxLength)
        {
            Fehler(nr, $"{feld} darf höchstens {maxLength} Zeichen lang sein.");
        }
    }

    private void Fehler(int nr, string text)
    {
        ModelState.AddModelError("", $"Zeile {nr}: {text}");
    }
}
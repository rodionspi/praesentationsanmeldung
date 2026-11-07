using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using praesentationsanmeldung.Auth;
using praesentationsanmeldung.Data;
using praesentationsanmeldung.Models;

namespace praesentationsanmeldung.Controllers;

[Authorize(Roles = Rollen.Admin)]
public class ImportController : Controller
{
    private const string G3SusKopfzeile = "Vorname;Nachname;Klasse";
    private const char UngueltigesZeichen = '�';

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
    {
        var zeilen = await CsvDateiLesen(datei, G3SusKopfzeile);
        var neueG3Sus = new List<G3Sus>();

        for (var index = 1; index < zeilen.Count; index++)
        {
            var zeile = zeilen[index];
            var zeilenNummer = index + 1;

            if (string.IsNullOrWhiteSpace(zeile))
            {
                continue;
            }

            var spalten = zeile.Split(';');

            if (spalten.Length != 3)
            {
                ZeilenFehlerMelden(zeilenNummer, $"3 Spalten erwartet, aber {spalten.Length} gefunden");
                continue;
            }

            var vorname = spalten[0].Trim();
            var nachname = spalten[1].Trim();
            var klasse = spalten[2].Trim();
        }
    }

    private async Task<List<string>> CsvDateiLesen(IFormFile? datei, string erwarteteKopfzeile)
    {
        if (datei == null || datei.Length == 0)
        {
            FehlerMelden("Bitte CSV-Datei auswählen.");
            return new List<string>();
        }

        if (Path.GetExtension(datei.FileName).ToLower() != ".csv")
        {
            FehlerMelden("Es sind nur CSV-Dateien erlaubt");
            return new List<string>();
        }

        var zeilen = await AlleZeilenLesen(datei);

        if (zeilen.Count == 0 || zeilen[0].Trim() != erwarteteKopfzeile)
        {
            ZeilenFehlerMelden(1, $"Die Kopfzeile muss '{erwarteteKopfzeile}' sein.");
            return new List<string>();
        }

        return zeilen;
    }

    private async Task<List<string>> AlleZeilenLesen(IFormFile datei)
    {
        var zeilen = new List<string>();
        using var reader = new StreamReader(datei.OpenReadStream());

        string? zeile;
        while ((zeile = await reader.ReadLineAsync()) != null)
        {
            zeilen.Add(zeile);

            if (zeile.Contains(UngueltigesZeichen))
            {
                ZeilenFehlerMelden(zeilen.Count, "Ungültige Zeichen. Bitte die Datei als \"CSV UTF-8\" speichern.");
            }
        }

        return zeilen;
    }

    private void FehlerMelden(string text)
    {
        ModelState.AddModelError("", text);
    }

    private void ZeilenFehlerMelden(int zeilenNummer, string text)
    {
        FehlerMelden($"Zeile {zeilenNummer}: {text}");
    }
}

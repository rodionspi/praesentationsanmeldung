using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using praesentationsanmeldung.Auth;
using praesentationsanmeldung.Data;
using praesentationsanmeldung.Models;

namespace praesentationsanmeldung.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<Admin> _adminHasher;
    private readonly IPasswordHasher<G3Sus> _g3SusHasher;

    public AccountController(AppDbContext db, IPasswordHasher<Admin> adminHasher, IPasswordHasher<G3Sus> g3SusHasher)
    {
        _db = db;
        _adminHasher = adminHasher;
        _g3SusHasher = g3SusHasher;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    //Kurz erklärt: Ohne das könnte eine fremde Webseite, auf der ein Benutzer zufällig gerade eingeloggt ist, heimlich ein Formular an meine App schicken und sich als ihn ausgeben — weil Cookies vom Browser automatisch mitgeschickt werden.
    public async Task<IActionResult> Login(string? benutzername, string? passwort)
    // die Methode ist asynchron (wichtig, weil Datenbank-Zugriffe — z. B. await db.Admins... — Zeit brauchen und den Server-Thread währenddessen nicht blockieren sollen). IActionResult wird hier zu Task<IActionResult>, weil man das Ergebnis erst awaiten muss.
    {
        if (string.IsNullOrEmpty(benutzername) || string.IsNullOrEmpty(passwort))
        {
            ModelState.AddModelError("", "Bitte Benutzername und Passwort eingeben.");
            return View();
            // ModelState (von Controller geerbt) ist das „Fehlerprotokoll“ der aktuellen Anfrage.AddModelError(Schlüssel, Text) fügt diesem einen Fehler hinzu. Der Schlüssel „“ bedeutet, dass sich der Fehler auf das gesamte Formular bezieht und nicht auf ein bestimmtes Feld.
        }

        var admin = await _db.Admins.SingleOrDefaultAsync(a => a.Benutzername == benutzername);
        // 
        if (admin != null && _adminHasher.VerifyHashedPassword(admin, admin.PasswortHash, passwort) != PasswordVerificationResult.Failed)
        {
            await Anmelden(admin.Id, admin.Benutzername, Rollen.Admin);
            return LocalRedirect("/");
        }

        var g3Sus = await _db.G3Sus.SingleOrDefaultAsync(s => s.Email == benutzername);
        if (g3Sus != null && _g3SusHasher.VerifyHashedPassword(g3Sus, g3Sus.PasswortHash, passwort) != PasswordVerificationResult.Failed)
        {
            await Anmelden(g3Sus.Id, $"{g3Sus.Vorname} {g3Sus.Nachname}", Rollen.G3SuS);
            return LocalRedirect("/");
        }

        ModelState.AddModelError("", "Benutzername oder Passwort falsch.");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task Anmelden(int id, string name, string rolle)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, rolle)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }
}

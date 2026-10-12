using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using praesentationsanmeldung.Auth;
using praesentationsanmeldung.Data;
using Microsoft.AspNetCore.Authorization;
using praesentationsanmeldung.Migrations;

namespace praesentationsanmeldung.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<Admin> _hasher;

    public AccountController(AppDbContext db, IPasswordHasher<Admin> hasher)
    {
        _db = db;
        _hasher = hasher;
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

        if (admin == null || _hasher.VerifyHashedPassword(admin, admin.PasswortHash, passwort) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Benutzername oder Passwort falsch.");
            return View();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, admin.Benutzername),
            new Claim(ClaimTypes.Role, Rollen.Admin)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return LocalRedirect("/");
    }
}
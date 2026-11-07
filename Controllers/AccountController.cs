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
    public async Task<IActionResult> Login(string? benutzername, string? passwort)
    {
        if (string.IsNullOrEmpty(benutzername) || string.IsNullOrEmpty(passwort))
        {
            ModelState.AddModelError("", "Bitte Benutzername und Passwort eingeben.");
            return View();
        }

        var admin = await _db.Admins.SingleOrDefaultAsync(a => a.Benutzername == benutzername);

        if (admin == null || !PasswortIstKorrekt(admin, passwort))
        {
            ModelState.AddModelError("", "Benutzername oder Passwort falsch.");
            return View();
        }

        await AlsAdminAnmelden(admin);

        return LocalRedirect("/");
    }

    private bool PasswortIstKorrekt(Admin admin, string passwort)
    {
        var ergebnis = _hasher.VerifyHashedPassword(admin, admin.PasswortHash, passwort);

        return ergebnis != PasswordVerificationResult.Failed;
    }

    private async Task AlsAdminAnmelden(Admin admin)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, admin.Benutzername),
            new Claim(ClaimTypes.Role, Rollen.Admin)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var benutzer = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, benutzer);
    }
}

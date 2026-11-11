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
    private readonly PasswordHasher<Admin> _adminHasher = new PasswordHasher<Admin>();
    private readonly PasswordHasher<G3Sus> _g3SusHasher = new PasswordHasher<G3Sus>();

    public AccountController(AppDbContext db)
    {
        _db = db;
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
        var benutzer = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, benutzer);
    }
}

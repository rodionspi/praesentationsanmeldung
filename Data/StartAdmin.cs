using Microsoft.AspNetCore.Identity;
using praesentationsanmeldung.Models;

namespace praesentationsanmeldung.Data;

public static class StartAdmin
{
    public static void AnlegenFallsNoetig(WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Admin>>();
        var startPasswort = app.Configuration["StartAdmin:Passwort"];

        if (string.IsNullOrEmpty(startPasswort) || db.Admins.Any())
        {
            return;
        }

        var admin = new Admin { Benutzername = "admin" };
        admin.PasswortHash = hasher.HashPassword(admin, startPasswort);

        db.Admins.Add(admin);
        db.SaveChanges();
    }
}

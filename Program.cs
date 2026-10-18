using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using praesentationsanmeldung.Auth;
using praesentationsanmeldung.Data;
using praesentationsanmeldung.Migrations;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication
//Cookies werden zum Standardverfahren für die Anmeldung.
(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        //Hierhin wird umgeleitet, wenn jemand nicht eingeloggt ist.
        options.LogoutPath = "/Account/Logout";
        //Die Adresse für das Abmelden.
        options.AccessDeniedPath = "/Account/AccessDenied";
        //	Hierhin wird umgeleitet, wenn jemand eingeloggt ist, aber die falsche Rolle hat (z. B. ein G3SuS auf einer Admin-Seite).
        options.Cookie.Name = "Praesentationsanmeldung.Auth";
        options.Cookie.HttpOnly = true;
        //JavaScript im Browser kann den Cookie nicht lesen. Das schützt gegen XSS.
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        //Der Cookie wird nur über HTTPS gesendet.
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        //Der Login gilt 8 Stunden. Bei Aktivität verlängert sich die Zeit automatisch.
    });

builder.Services.AddAuthorization(options =>
{
        options.AddPolicy(Rollen.Admin, policy => policy.RequireRole(Rollen.Admin));
        options.AddPolicy(Rollen.G3SuS, policy => policy.RequireRole(Rollen.G3SuS));

        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
});

builder.Services.AddScoped<IPasswordHasher<Admin>, PasswordHasher<Admin>>();
//Diese Zeile registriert einen Dependency-Injection-Service zum sicheren Hashen von Passwörtern

var app = builder.Build();

using (var scope = app.Services.CreateScope())
//AppDbContext gibt es nur innerhalb eines „Scopes“. Beim Start läuft noch keine Anfrage, deshalb öffne ich selbst einen. app.Services ist der Root-Service-Provider (der ganzen App). Daraus direkt einen Scoped-Service zu holen wäre ein Fehler/Warning (“captive dependency”) — stattdessen erzeugt man manuell einen neuen, temporären Scope, der sich wie ein “virtueller Request” verhält. using sorgt dafür, dass der Scope am Ende des Blocks korrekt disposed wird
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    //Holt eine Instanz von AppDbContext aus diesem neuen Scope — genau so, wie es sonst automatisch per Constructor Injection in einem Controller passieren würde, nur hier manuell angefordert.GetRequiredService wirft eine Exception, falls der Service nicht registriert ist(im Gegensatz zu GetService, das null zurückgeben würde)
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Admin>>();
    //Gleiches Prinzip — holt den PasswordHasher<Admin>, den ich vorher mit AddScoped<IPasswordHasher<Admin>, PasswordHasher<Admin>>() registriert habe.
    var startPasswort = app.Configuration["StartAdmin:Passwort"];
    //Liest das Passwort aus den User Secrets
    
    if (!string.IsNullOrEmpty(startPasswort) && !db.Admins.Any())
    //!db.Admins.Any() — es gibt noch keinen Admin in der Datenbank-Tabelle (EF Core übersetzt .Any() in ein effizientes SQL EXISTS, nicht SELECT *)
    {
        var admin = new Admin { Benutzername = "admin" };
        //Erstellt ein neues Admin-Objekt (Entity) im Speicher — noch nicht in der DB gespeichert, nur ein C#-Objekt mit Benutzername = "admin".
        admin.PasswortHash = hasher.HashPassword(admin, startPasswort);
        db.Admins.Add(admin);
        //Markiert das admin - Objekt in EF Core als “neu hinzuzufügen” — das DbSet<Admin> ist noch im Tracking-Zustand, es ist noch kein SQL - Befehl an die Datenbank gegangen.
        db.SaveChanges();
        //Erst hier wird tatsächlich ein INSERT-SQL-Statement an die Datenbank gesendet und ausgeführt. Ohne diesen Aufruf würde gar nichts gespeichert werden.
    }


}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets().AllowAnonymous();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Import}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
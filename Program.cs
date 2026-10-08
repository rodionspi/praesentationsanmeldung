using Microsoft.EntityFrameworkCore;
using praesentationsanmeldung.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using praesentationsanmeldung.Auth;


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
        options.AddPolicy(Rollen.Admin, policy =>
    policy.RequireRole(Rollen.Admin));
        options.AddPolicy(Rollen.G3SuS, policy =>
    policy.RequireRole(Rollen.G3SuS));
});

var app = builder.Build();

app.UseHttpsRedirection();
//Wenn eine Anfrage über HTTP (unverschlüsselt) kommt, wird sie automatisch auf HTTPS umgeleitet (301/307 Redirect)
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
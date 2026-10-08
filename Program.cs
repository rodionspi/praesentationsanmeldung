using Microsoft.EntityFrameworkCore;
using praesentationsanmeldung.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using praesentationsanmeldung.Auth;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddControllersWithViews();

//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie

var app = builder.Build();

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
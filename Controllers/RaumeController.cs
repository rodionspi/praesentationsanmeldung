
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using praesentationsanmeldung.Auth;
using praesentationsanmeldung.Data;
using praesentationsanmeldung.Models;

namespace praesentationsanmeldung.Controllers;

[Authorize(Roles = Rollen.Admin)]
public class RaemeController : Controller
{ 
    private readonly AppDbContext _db;

    public RaemeController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var raeume = await _db.Raeume.OrderBy(r => r.Bezeichnung).ToListAsync();
        return View(raeume);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Bezeichnung,Kapazitaet")] Raum raum)
    {
        
    }
}

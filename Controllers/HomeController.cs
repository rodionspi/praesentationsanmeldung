using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using praesentationsanmeldung.Auth;

namespace praesentationsanmeldung.Controllers;

public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        if (User.IsInRole(Rollen.Admin))
        {
            return RedirectToAction("Index", "Uebersicht");
        }

        return RedirectToAction("Index", "Anmeldung");
    }

    [AllowAnonymous]
    public IActionResult Error()
    {
        return View();
    }
}

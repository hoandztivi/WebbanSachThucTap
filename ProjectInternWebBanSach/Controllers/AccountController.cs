using Microsoft.AspNetCore.Mvc;

namespace ProjectInternWebBanSash.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}

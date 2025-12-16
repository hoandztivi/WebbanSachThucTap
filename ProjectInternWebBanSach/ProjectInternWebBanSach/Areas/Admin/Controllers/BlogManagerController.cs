using Microsoft.AspNetCore.Mvc;

namespace ProjectInternWebBanSach.Areas.Admin.Controllers
{
    public class BlogManagerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProjectInternWebBanSach.Models;

namespace ProjectInternWebBanSash.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly QuanLyBanSachContext _context;

    public HomeController(ILogger<HomeController> logger, QuanLyBanSachContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        {
            //1. Lấy danh sách banner đang hoạt động
            var banners = _context.Banners
                .Where(b => b.TrangThai == true)
                .OrderByDescending(b => b.NgayTao)
                .ToList();

            //2. Lấy danh sách thể loại kèm 5 quyển sách đầu tiên
            var categories = _context.TheLoaiSaches
                .Select(c => new
                {
                    TheLoai = c,
                    Sach = _context.Saches
                        .Where(s => s.MaTheLoai == c.MaTheLoai)
                        .Take(5)
                        .ToList()
                })
                .ToList();

            //3. Truyền dữ liệu ra View
            ViewBag.Banners = banners;
            return View(categories);
        }
    }
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

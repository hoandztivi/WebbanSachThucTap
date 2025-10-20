using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectInternWebBanSach.Models;
using System.Linq;

namespace ProjectInternWebBanSach.Controllers
{
    public class ProductsController : Controller
    {
        private readonly QuanLyBanSachContext _context;

        public ProductsController(QuanLyBanSachContext context)
        {
            _context = context;
        }

        // Lấy danh sách thể loại để hiển thị menu
        public PartialViewResult TheLoaiMenu()
        {
            var dsTheLoai = _context.TheLoaiSaches.ToList();
            return PartialView("_TheLoaiMenu", dsTheLoai);
        }

        // ====== HIỂN THỊ SÁCH THEO THỂ LOẠI ======
        [HttpGet]
        public IActionResult TheoTheLoai(int id)
        {
            var theLoai = _context.TheLoaiSaches
                .FirstOrDefault(t => t.MaTheLoai == id);

            if (theLoai == null)
                return NotFound();

            var sachTheoLoai = _context.Saches
                .Where(s => s.MaTheLoai == id)
                .OrderByDescending(s => s.NgayTao)
                .Take(5)
                .ToList();

            ViewBag.CategoryName = theLoai.TenTheLoai;
            return View(sachTheoLoai);
        }

    }
}

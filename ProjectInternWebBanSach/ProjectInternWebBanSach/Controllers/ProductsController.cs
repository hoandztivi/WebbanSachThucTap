using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        public IActionResult TheoTheLoai(int id, int page = 1)
        {
            const int pageSize = 10;

            var theLoai = _context.TheLoaiSaches.FirstOrDefault(t => t.MaTheLoai == id);
            if (theLoai == null)
                return NotFound();

            var totalBooks = _context.Saches.Count(s => s.MaTheLoai == id);
            var totalPages = (int)Math.Ceiling(totalBooks / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var sachTheoLoai = _context.Saches
                .Where(s => s.MaTheLoai == id)
                .OrderByDescending(s => s.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CategoryName = theLoai.TenTheLoai;
            ViewBag.CategoryId = id;
            ViewBag.Pagination = new
            {
                Current = page,
                TotalPages = totalPages,
                HasPrev = page > 1,
                HasNext = page < totalPages
            };
            return View(sachTheoLoai);
        }
        //===CHI TIẾT SẢN PHẨM (DETAILS)===
        [HttpGet]
        public IActionResult Details(int id)
        {
            var product = _context.Saches
                .Include(s => s.MaTheLoaiNavigation)
                .FirstOrDefault(s => s.MaSach == id);

            if (product == null) return NotFound();

            decimal price = product.Gia ?? 0;
            decimal discount = product.GiamGia ?? 0;
            decimal final = Math.Max(0, price - discount);
            int percent = price > 0 ? (int)Math.Round(discount / price * 100) : 0;

            ViewBag.Price = final;
            ViewBag.OriginalPrice = price;
            ViewBag.SavedAmount = discount;
            ViewBag.DiscountPercent = percent;

            ViewBag.CategoryName = product.MaTheLoaiNavigation?.TenTheLoai ?? "Thể loại";
            ViewBag.CategoryId = product.MaTheLoai ?? 0;

            //Tính số lượng đánh giá và xếp hạng trung bình
            var reviews = _context.DanhGiaSaches
                .Where(r => r.MaSach == id && r.Diem.HasValue)
                .ToList();

            int reviewCount = reviews.Count;
            double avgRating = 0;

            if (reviewCount > 0)
            {
                avgRating = reviews.Average(r => r.Diem ?? 0);
            }

            ViewBag.ReviewCount = reviewCount;
            ViewBag.Rating = avgRating;

            return View(product);
        }
    }
}

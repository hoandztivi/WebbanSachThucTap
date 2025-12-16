using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectInternWebBanSach.Models;

namespace ProjectInternWebBanSach.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly QuanLyBanSachContext _context;

        public HomeController(ILogger<HomeController> logger, QuanLyBanSachContext context)
        {
            _logger = logger;
            _context = context;
        }
        // Trang chủ: phân trang theo THỂ LOẠI (3 thể loại / trang)
        public IActionResult Index(int page = 1)
        {
            const int pageSize = 3;

            // Banners
            ViewBag.Banners = _context.Banners
                .AsNoTracking()
                .Where(b => b.TrangThai == true)
                .OrderByDescending(b => b.NgayTao)
                .ToList();

            // 5 sách mới nhất
            ViewBag.NewBooks = _context.Saches
                .AsNoTracking()
                .OrderByDescending(s => s.NgayTao)
                .Take(5)
                .Select(s => new { s.MaSach, s.TieuDe, s.HinhAnh, s.Gia, s.GiamGia })
                .ToList();

            // Tính trang
            var totalCategories = _context.TheLoaiSaches.Count();
            var totalPages = (int)Math.Ceiling(totalCategories / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            // Lấy 3 thể loại / trang + 5 sách/loại
            var categories = _context.TheLoaiSaches
                .AsNoTracking()
                .OrderBy(t => t.MaTheLoai)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    TheLoai = t,
                    Sach = _context.Saches
                        .Where(s => s.MaTheLoai == t.MaTheLoai)
                        .OrderByDescending(s => s.NgayTao)
                        .Take(5)
                        .ToList()
                })
                .ToList();

            ViewBag.Pagination = new
            {
                Current = page,
                TotalPages = totalPages,
                HasPrev = page > 1,
                HasNext = page < totalPages
            };

            return View(categories); // @model IEnumerable<dynamic>
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        public IActionResult Introduction() => View();
        public IActionResult Buycomics() => View();
        public IActionResult Bookpurchasing() => View();
        public IActionResult ConTact() => View();
        public IActionResult Hoanle() => View();

        // ============ TÌM KIẾM TRANG KẾT QUẢ ============
        [HttpGet]
        public async Task<IActionResult> Search(string q, int page = 1, int pageSize = 12)
        {
            ViewBag.Keyword = q;

            var query = _context.Saches
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(s =>
                    (s.TieuDe != null && s.TieuDe.Contains(q)) ||
                    (s.TacGia != null && s.TacGia.Contains(q)) ||
                    (s.MoTa != null && s.MoTa.Contains(q))
                );
            }

            var totalItems = await query.CountAsync();
            if (page < 1) page = 1;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var items = await query
                .OrderByDescending(s => s.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(items);
        }

        // ============ API GỢI Ý AUTOCOMPLETE ============
        [HttpGet]
        public async Task<IActionResult> SearchSuggest(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Json(new List<object>());

            q = q.Trim();

            var suggestions = await _context.Saches
                .AsNoTracking()
                .Where(s => s.TieuDe != null && s.TieuDe.Contains(q))
                .OrderBy(s => s.TieuDe)
                .Take(3)
                .Select(s => new
                {
                    id = s.MaSach,
                    name = s.TieuDe,
                    image = string.IsNullOrEmpty(s.HinhAnh)
                        ? "/img/sach/no-image.png"
                        : s.HinhAnh,
                    price = s.Gia ?? 0m
                })
                .ToListAsync();
            return Json(suggestions);
        }
    }
}

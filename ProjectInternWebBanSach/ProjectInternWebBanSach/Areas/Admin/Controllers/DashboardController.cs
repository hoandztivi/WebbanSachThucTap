using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectInternWebBanSach.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ProjectInternWebBanSach.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")] // BẮT BUỘC PHẢI LOGIN VÀ LÀ ADMIN
    public class DashboardController : Controller
    {
        private readonly QuanLyBanSachContext _ctx;

        public DashboardController(QuanLyBanSachContext ctx)
        {
            _ctx = ctx;
        }      
        public IActionResult Index()
        {
            // ====== TÍNH NGÀY HÔM NAY / HÔM QUA ======
            var today = DateTime.Today;          // 00:00 hôm nay
            var tomorrow = today.AddDays(1);     // 00:00 ngày mai
            var yesterday = today.AddDays(-1);   // 00:00 hôm qua

            // ====== DOANH THU HÔM NAY / HÔM QUA ======
            var todayRevenue = _ctx.DonHangs
                .Where(d => d.NgayDat >= today
                         && d.NgayDat < tomorrow
                         && d.TrangThai == "Hoàn thành")
                .Sum(d => (decimal?)d.TongTien) ?? 0m;

            var yesterdayRevenue = _ctx.DonHangs
                .Where(d => d.NgayDat >= yesterday
                         && d.NgayDat < today
                         && d.TrangThai == "Hoàn thành")
                .Sum(d => (decimal?)d.TongTien) ?? 0m;

            decimal todayRevenuePercent = 0;
            if (yesterdayRevenue > 0)
            {
                todayRevenuePercent = Math.Round(
                    (todayRevenue - yesterdayRevenue) / yesterdayRevenue * 100, 1);
            }

            // ====== ĐƠN HÀNG ======
            var todayOrders = _ctx.DonHangs
                .Count(d => d.NgayDat >= today && d.NgayDat < tomorrow);

            var pendingOrders = _ctx.DonHangs
                .Count(d => d.TrangThai == "Đang xử lý"
                         || d.TrangThai == "Chờ xác nhận");

            // ====== NGƯỜI DÙNG ======
            var todayUsers = _ctx.NguoiDungs
                .Count(u => u.NgayTao >= today && u.NgayTao < tomorrow);

            var totalUsers = _ctx.NguoiDungs.Count();

            // ====== SÁCH ======
            var totalBooks = _ctx.Saches.Count();
            var outOfStockBooks = _ctx.Saches.Count(s => (s.SoLuong ?? 0) == 0);

            // ====== ĐƠN HÀNG GẦN ĐÂY ======
            var recentOrders = _ctx.DonHangs
                .Include(d => d.MaNguoiDungNavigation)
                .OrderByDescending(d => d.NgayDat)
                .Take(10)
                .Select(d => new
                {
                    MaDon = d.MaDonHang,
                    TenKhachHang = d.MaNguoiDungNavigation != null
                        ? d.MaNguoiDungNavigation.HoTen
                        : "Khách hàng",
                    NgayTao = d.NgayDat,
                    TongTien = d.TongTien,
                    TrangThai = d.TrangThai
                })
                .ToList();

            // ====== SÁCH BÁN CHẠY ======
            var topBooks = _ctx.ChiTietDonHangs
                .Include(ct => ct.MaSachNavigation)
                .Where(ct => ct.MaSachNavigation != null)
                .GroupBy(ct => new
                {
                    ct.MaSach,
                    TenSach = ct.MaSachNavigation.TieuDe,
                    GiaBan = ct.MaSachNavigation.Gia
                })
                .Select(g => new
                {
                    TenSach = g.Key.TenSach,
                    SoLuongDaBan = g.Sum(x => x.SoLuong ?? 0),
                    GiaBan = g.Key.GiaBan ?? 0
                })
                .OrderByDescending(x => x.SoLuongDaBan)
                .Take(5)
                .ToList();

            // ====== ĐẨY DỮ LIỆU RA VIEWBAG ======
            ViewBag.TodayRevenue = todayRevenue;
            ViewBag.TodayRevenuePercent = todayRevenuePercent;

            ViewBag.TodayOrders = todayOrders;
            ViewBag.PendingOrders = pendingOrders;

            ViewBag.TodayUsers = todayUsers;
            ViewBag.TotalUsers = totalUsers;

            ViewBag.TotalBooks = totalBooks;
            ViewBag.OutOfStockBooks = outOfStockBooks;

            ViewBag.RecentOrders = recentOrders;
            ViewBag.TopBooks = topBooks;

            return View();
        }
        // =========================================================
        // PROFILE
        // =========================================================
        [Authorize]
        public IActionResult Profile()
        {
            if (Request.Cookies.TryGetValue("AccessToken", out var token))
                ViewBag.AccessToken = token;
            else
                ViewBag.AccessToken = "***";

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            NguoiDung? user = null;

            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
            {
                user = _ctx.NguoiDungs
                    .FirstOrDefault(x => x.MaNguoiDung == userId && x.TrangThai == true);
            }

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.FullName = user.HoTen ?? user.Email;
            ViewBag.Email = user.Email;
            ViewBag.SoDu = user.SoDu ?? 0;
            ViewBag.Phone = user.SoDienThoai ?? "Chưa có số điện thoại";
            ViewBag.NgayTao = (user.NgayTao ?? DateTime.Now).ToString("dd/MM/yyyy HH:mm");
            ViewBag.Avatar = user.AnhDaiDien ?? "default.png";

            return View();
        }
    }
}

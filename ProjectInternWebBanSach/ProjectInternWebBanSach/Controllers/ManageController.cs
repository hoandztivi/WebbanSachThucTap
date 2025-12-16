using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectInternWebBanSach.Models;
using ProjectInternWebBanSach.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

[Authorize]
public class ManageController : Controller
{
    private readonly QuanLyBanSachContext _context;

    public ManageController(QuanLyBanSachContext context)
    {
        _context = context;
    }

    public IActionResult Manage()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login", "Account");
        }

        // ==== THÔNG TIN USER TRÊN HEADER ====
        var fullName = User.Identity?.Name ?? "Người dùng";
        var avatarClaim = User.FindFirst("Avatar")?.Value ?? "default.png";

        ViewBag.HoTen = fullName;
        ViewBag.AnhDaiDien = avatarClaim;

        // ==== LỊCH SỬ ĐĂNG NHẬP ====
        var loginHistory = _context.LichSuDangNhaps
            .Where(x => x.MaNguoiDung == userId)
            .OrderByDescending(x => x.NgayDangNhap)
            .Take(10)
            .ToList();

        // ==== LỊCH SỬ NẠP TIỀN (bao gồm cả hoàn tiền) ====
        var rechargeHistory = _context.LichSuNapTiens
            .Where(x => x.MaNguoiDung == userId)
            .OrderByDescending(x => x.NgayTao)
            .ToList();

        ViewBag.RechargeHistory = rechargeHistory;

        // Tổng nạp: chỉ tính giao dịch nạp tiền Hoàn thành
        ViewBag.TotalRecharge = rechargeHistory
            .Where(x => x.TrangThai == "Hoàn thành")
            .Sum(x => x.SoTien);

        // ==== ĐƠN HÀNG ====
        var orders = _context.DonHangs
            .Where(o => o.MaNguoiDung == userId)
            .OrderByDescending(o => o.NgayDat)
            .ToList();

        ViewBag.Orders = orders;

        var shippingOrders = orders
            .Where(o => o.TrangThai == "Đang giao" || o.TrangThai == "Đang vận chuyển")
            .ToList();

        ViewBag.ShippingOrders = shippingOrders;

        ViewBag.OrderSuccess = orders.Count(o => o.TrangThai == "Hoàn thành");
        ViewBag.OrderShipping = shippingOrders.Count;
        ViewBag.OrderTotal = orders.Count;

        // ==============================
        // ====  BIẾN ĐỘNG SỐ DƯ    ====
        // ==============================

        var balanceHistory = new List<BalanceChangeDto>();

        //NẠP TIỀN HOÀN THÀNH (TrangThai = "Hoàn thành")  -> tiền +
        var napHoanThanh = rechargeHistory
            .Where(x => x.TrangThai == "Hoàn thành")
            .ToList();

        foreach (var r in napHoanThanh)
        {
            balanceHistory.Add(new BalanceChangeDto
            {
                Time = r.NgayTao ?? DateTime.MinValue,
                Type = "Nạp tiền",
                Amount = r.SoTien, // luôn dương
                Note = $"Mã GD: {r.MaGiaoDich}"
            });
        }

        //HOÀN TIỀN HỦY ĐƠN (TrangThai = "Hoàn tiền hủy đơn") -> tiền +
        var hoanTienHuyDon = rechargeHistory
            .Where(x => x.TrangThai == "Hoàn tiền hủy đơn")
            .ToList();

        foreach (var r in hoanTienHuyDon)
        {
            balanceHistory.Add(new BalanceChangeDto
            {
                Time = r.NgayTao ?? DateTime.MinValue,
                Type = "Hoàn tiền hủy đơn",
                Amount = r.SoTien,
                Note = $"Mã GD: {r.MaGiaoDich}"
            });
        }

        //THANH TOÁN WALLET (ChiTietDonHang.ThanhToan = 'wallet') -> tiền -
        var chiTietWallet = (
            from ct in _context.ChiTietDonHangs
            join dh in _context.DonHangs on ct.MaDonHang equals dh.MaDonHang
            where dh.MaNguoiDung == userId
                  && ct.ThanhToan != null
                  && ct.ThanhToan.ToLower() == "wallet"
            group ct by new { dh.MaDonHang, dh.NgayDat } into g
            select new
            {
                MaDonHang = g.Key.MaDonHang,
                NgayDat = g.Key.NgayDat,
                TongTien = g.Sum(x => (x.DonGia ?? 0m) * (x.SoLuong ?? 0))
            }
        ).ToList();

        foreach (var ct in chiTietWallet)
        {
            balanceHistory.Add(new BalanceChangeDto
            {
                Time = ct.NgayDat ?? DateTime.MinValue,
                Type = "Thanh toán đơn hàng",
                Amount = ct.TongTien,
                Note = $"Thanh toán đơn #{ct.MaDonHang} bằng ví"
            });
        }

        // Sắp xếp theo thời gian mới nhất
        ViewBag.BalanceHistory = balanceHistory
            .OrderByDescending(x => x.Time)
            .ToList();

        return View(loginHistory);
    }

    // ===== HỦY ĐƠN HÀNG =====
    [HttpPost]
    public IActionResult CancelOrder(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Json(new { success = false, message = "Bạn cần đăng nhập lại." });
        }

        var order = _context.DonHangs
            .Include(o => o.ChiTietDonHangs)
            .ThenInclude(ct => ct.MaSachNavigation)
            .Include(o => o.MaNguoiDungNavigation)
            .FirstOrDefault(o => o.MaDonHang == id && o.MaNguoiDung == userId);

        if (order == null)
            return Json(new { success = false, message = "Không tìm thấy đơn hàng." });

        if (order.TrangThai == "Đang giao" ||
            order.TrangThai == "Hoàn thành" ||
            order.TrangThai == "Đã hủy")
        {
            return Json(new
            {
                success = false,
                message = "Đơn hàng ở trạng thái hiện tại không thể hủy."
            });
        }

        // TÍNH SỐ TIỀN ĐÃ THANH TOÁN BẰNG VÍ (wallet) CHO SÁCH
        var walletAmount = order.ChiTietDonHangs
            .Where(ct => ct.ThanhToan != null && ct.ThanhToan.ToLower() == "wallet")
            .Sum(ct => (ct.DonGia ?? 0m) * (ct.SoLuong ?? 0));

        // CỘNG THÊM 30.000 PHÍ SHIP CỐ ĐỊNH
        decimal refundAmount = 0m;
        if (walletAmount > 0)
        {
            decimal shippingFee = 30000m;
            refundAmount = walletAmount + shippingFee;
        }

        if (refundAmount > 0 && order.MaNguoiDungNavigation != null)
        {
            var user = order.MaNguoiDungNavigation;

            // Cộng lại vào số dư
            user.SoDu = (user.SoDu ?? 0m) + refundAmount;

            // Ghi lịch sử HOÀN TIỀN HỦY ĐƠN
            var vietnamTime = DateTime.UtcNow.AddHours(7);
            _context.LichSuNapTiens.Add(new LichSuNapTien
            {
                MaNguoiDung = user.MaNguoiDung,
                SoTien = refundAmount,
                NgayTao = vietnamTime,
                TrangThai = "Hoàn tiền hủy đơn",
                NoiDung = $"Hoàn tiền đơn hàng #{order.MaDonHang}",
                MaGiaoDich = $"REFUND-{order.MaDonHang}-{vietnamTime:yyyyMMddHHmmss}"
            });
        }

        //HOÀN LẠI TỒN KHO SÁCH
        foreach (var ct in order.ChiTietDonHangs)
        {
            if (ct.MaSachNavigation != null)
            {
                var sach = ct.MaSachNavigation;
                int current = sach.SoLuong ?? 0;
                int qty = ct.SoLuong ?? 0;

                // cộng lại số lượng đã đặt
                sach.SoLuong = current + qty;
            }
        }

        order.TrangThai = "Đã hủy";

        _context.SaveChanges();

        return Json(new { success = true, message = "Đã hủy đơn hàng." });
    }
}

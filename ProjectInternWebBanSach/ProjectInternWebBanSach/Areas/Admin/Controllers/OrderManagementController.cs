using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectInternWebBanSach.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectInternWebBanSach.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderManagementController : Controller
    {
        private readonly QuanLyBanSachContext _ctx;

        private static readonly string[] AllowedStatuses = new[]
        {
            "Chờ xác nhận",
            "Đang xử lý",
            "Đang giao",
            "Hoàn thành",
            "Đã hủy"
        };

        public OrderManagementController(QuanLyBanSachContext ctx)
        {
            _ctx = ctx;
        }

        // VIEW
        [HttpGet]
        public IActionResult Order()
        {
            return View();
        }

        // LẤY DANH SÁCH ĐƠN HÀNG + SĐT + CHI TIẾT SÁCH
        [HttpGet]
        public async Task<IActionResult> GetOrders(string? status = "all")
        {
            var query = _ctx.DonHangs
                .Include(d => d.MaNguoiDungNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaSachNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "all")
            {
                query = query.Where(d => d.TrangThai == status);
            }

            var orders = await query
                .OrderByDescending(d => d.NgayDat)
                .Select(d => new
                {
                    d.MaDonHang,
                    d.NgayDat,
                    d.TongTien,
                    d.TrangThai,
                    d.DiaChiGiao,
                    MaNguoiDung = d.MaNguoiDung,
                    HoTen = d.MaNguoiDungNavigation != null ? d.MaNguoiDungNavigation.HoTen : null,
                    Email = d.MaNguoiDungNavigation != null ? d.MaNguoiDungNavigation.Email : null,
                    SoDienThoai = d.MaNguoiDungNavigation != null ? d.MaNguoiDungNavigation.SoDienThoai : null,

                    ChiTiet = d.ChiTietDonHangs.Select(ct => new
                    {
                        ct.MaSach,
                        TenSach = ct.MaSachNavigation != null ? ct.MaSachNavigation.TieuDe : null,
                        ct.SoLuong,
                        ct.DonGia,
                        ct.ThanhToan
                    }).ToList()
                })
                .ToListAsync();

            return Json(new
            {
                success = true,
                data = orders
            });
        }

        // DTO cập nhật trạng thái
        public class UpdateOrderStatusDto
        {
            public int MaDonHang { get; set; }
            public string TrangThai { get; set; } = "";
        }

        // DTO xóa đơn
        public class DeleteOrderDto
        {
            public int MaDonHang { get; set; }
        }

        // CẬP NHẬT TRẠNG THÁI + HOÀN TIỀN VÍ NẾU HỦY ĐƠN
        [HttpPost]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateOrderStatusDto model)
        {
            if (model.MaDonHang <= 0)
            {
                return Json(new { success = false, message = "Mã đơn hàng không hợp lệ." });
            }

            if (string.IsNullOrWhiteSpace(model.TrangThai) ||
                !AllowedStatuses.Contains(model.TrangThai))
            {
                return Json(new { success = false, message = "Trạng thái không hợp lệ." });
            }

            var order = await _ctx.DonHangs
                .Include(d => d.MaNguoiDungNavigation)
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefaultAsync(d => d.MaDonHang == model.MaDonHang);

            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
            }

            var oldStatus = order.TrangThai ?? string.Empty;
            var newStatus = model.TrangThai;

            if (string.Equals(oldStatus, newStatus, StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = true, message = "Trạng thái đơn hàng không thay đổi." });
            }

            bool willBeCanceled = string.Equals(newStatus, "Đã hủy", StringComparison.OrdinalIgnoreCase);
            bool wasCanceled = string.Equals(oldStatus, "Đã hủy", StringComparison.OrdinalIgnoreCase);

            // HOÀN TIỀN VÍ KHI HỦY ĐƠN
            if (willBeCanceled && !wasCanceled && order.MaNguoiDungNavigation != null)
            {
                var walletAmount = order.ChiTietDonHangs
                    .Where(ct => ct.ThanhToan != null &&
                                 ct.ThanhToan.ToLower() == "wallet")
                    .Sum(ct => (ct.DonGia ?? 0m) * (ct.SoLuong ?? 0));

                if (walletAmount > 0)
                {
                    decimal shippingFee = 30000m;
                    decimal refundAmount = walletAmount + shippingFee;

                    var user = order.MaNguoiDungNavigation;

                    user.SoDu = (user.SoDu ?? 0m) + refundAmount;

                    var vietnamTime = DateTime.UtcNow.AddHours(7);
                    _ctx.LichSuNapTiens.Add(new LichSuNapTien
                    {
                        MaNguoiDung = user.MaNguoiDung,
                        SoTien = refundAmount,
                        NgayTao = DateTime.Now,
                        TrangThai = "Hoàn tiền hủy đơn",
                        MaGiaoDich = $"REFUND-ADMIN-{order.MaDonHang}-{vietnamTime:yyyyMMddHHmmss}",
                        NoiDung = $"Hoàn tiền đơn hàng #{order.MaDonHang}"
                    });
                }
            }

            order.TrangThai = newStatus;
            await _ctx.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật trạng thái đơn hàng thành công." });
        }

        // XÓA ĐƠN HÀNG
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] DeleteOrderDto model)
        {
            if (model == null || model.MaDonHang <= 0)
            {
                return Json(new { success = false, message = "Mã đơn hàng không hợp lệ." });
            }

            var order = await _ctx.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefaultAsync(d => d.MaDonHang == model.MaDonHang);

            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
            }

            if (order.ChiTietDonHangs != null && order.ChiTietDonHangs.Any())
            {
                _ctx.ChiTietDonHangs.RemoveRange(order.ChiTietDonHangs);
            }

            _ctx.DonHangs.Remove(order);
            await _ctx.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa đơn hàng thành công." });
        }
    }
}

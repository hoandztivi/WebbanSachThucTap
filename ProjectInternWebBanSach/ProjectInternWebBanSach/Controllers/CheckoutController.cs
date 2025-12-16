using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectInternWebBanSach.DTO;
using ProjectInternWebBanSach.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjectInternWebBanSach.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly QuanLyBanSachContext _ctx;

        public CheckoutController(QuanLyBanSachContext ctx)
        {
            _ctx = ctx;
        }

        // ------------ Lấy userId từ JWT ------------
        private int? GetUserIdFromJwt()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim)) return null;
            return int.TryParse(idClaim, out var id) ? id : (int?)null;
        }

        // ------------ GET: /Checkout/Buy ------------
        [HttpGet]
        public async Task<IActionResult> Buy(bool isEdit = false, int? orderId = null)
        {
            var userId = GetUserIdFromJwt();
            if (userId is null)
                return RedirectToAction("Login", "Account");

            var user = await _ctx.NguoiDungs
                .FirstOrDefaultAsync(u => u.MaNguoiDung == userId);

            if (user == null)
                return RedirectToAction("Login", "Account");

            // Tạo đơn mới => lấy từ giỏ hàng
            var items = await _ctx.ChiTietGioHangs
                .Include(c => c.MaSachNavigation)
                .Include(c => c.MaGioHangNavigation)
                .Where(c => c.MaGioHangNavigation!.MaNguoiDung == userId)
                .ToListAsync();

            ViewBag.IsEdit = isEdit;
            ViewBag.OrderId = orderId;

            return View(items);
        }

        // ------------ POST: /Checkout/ApplyCoupon ------------
        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(string code)
        {
            var userId = GetUserIdFromJwt();
            if (userId is null)
                return Json(new { success = false, message = "Bạn cần đăng nhập." });

            if (string.IsNullOrWhiteSpace(code))
                return Json(new { success = false, message = "Vui lòng nhập mã giảm giá." });

            code = code.Trim();

            var coupon = await _ctx.MaGiamGia
                .FirstOrDefaultAsync(c =>
                    c.MaCode == code &&
                    c.DangHoatDong == true &&
                    (c.NgayBatDau == null || c.NgayBatDau <= DateTime.Now) &&
                    (c.NgayKetThuc == null || c.NgayKetThuc >= DateTime.Now));

            if (coupon == null)
                return Json(new { success = false, message = "Mã giảm giá không hợp lệ hoặc đã hết hạn." });

            var cartItems = await _ctx.ChiTietGioHangs
                .Include(c => c.MaGioHangNavigation)
                .Where(c => c.MaGioHangNavigation!.MaNguoiDung == userId)
                .ToListAsync();

            if (!cartItems.Any())
                return Json(new { success = false, message = "Giỏ hàng đang trống." });

            decimal subtotal = cartItems.Sum(i => (i.DonGia ?? 0m) * (i.SoLuong ?? 0));
            decimal discount = CalculateDiscountAmount(subtotal, coupon.GiaTriGiam ?? 0m);

            if (discount <= 0)
                return Json(new { success = false, message = "Đơn hàng không đủ điều kiện áp dụng mã này." });

            return Json(new
            {
                success = true,
                message = "Áp dụng mã giảm giá thành công.",
                discount,
                code = coupon.MaCode
            });
        }

        // ------------ POST: /Checkout/Process ------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(CheckoutInput input)
        {
            var userId = GetUserIdFromJwt();
            if (userId is null)
                return Json(new { success = false, message = "Bạn cần đăng nhập lại." });

            // Validate cơ bản
            if (string.IsNullOrWhiteSpace(input.HoTen) ||
                string.IsNullOrWhiteSpace(input.SoDienThoai) ||
                string.IsNullOrWhiteSpace(input.DiaChiDayDu))
            {
                return Json(new { success = false, message = "Vui lòng điền đầy đủ thông tin bắt buộc." });
            }

            var user = await _ctx.NguoiDungs
                .FirstOrDefaultAsync(u => u.MaNguoiDung == userId);

            if (user == null)
                return Json(new { success = false, message = "Không tìm thấy tài khoản người dùng." });

            // Chuẩn hoá PaymentMethod thành "wallet" hoặc "cod"
            string paymentMethod = string.Equals(input.PaymentMethod, "wallet", StringComparison.OrdinalIgnoreCase)
                ? "wallet"
                : "cod";

            // ============== CHẾ ĐỘ SỬA ĐƠN ==============
            if (input.IsEdit && input.OrderId.HasValue)
            {
                var order = await _ctx.DonHangs
                    .Include(o => o.ChiTietDonHangs)
                        .ThenInclude(ct => ct.MaSachNavigation)
                    .FirstOrDefaultAsync(o => o.MaDonHang == input.OrderId.Value
                                           && o.MaNguoiDung == userId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng để cập nhật." });
                }

                // SUBTOTAL từ ChiTietDonHang
                decimal subtotal = order.ChiTietDonHangs.Sum(i => (i.DonGia ?? 0m) * (i.SoLuong ?? 0));

                // PHÍ SHIP
                decimal shippingFee = CalculateShippingFee(input.ShippingMethod);

                // GIẢM GIÁ
                decimal discount = 0m;
                if (!string.IsNullOrWhiteSpace(input.CouponCode))
                {
                    var coupon = await _ctx.MaGiamGia
                        .FirstOrDefaultAsync(c =>
                            c.MaCode == input.CouponCode &&
                            c.DangHoatDong == true &&
                            (c.NgayBatDau == null || c.NgayBatDau <= DateTime.Now) &&
                            (c.NgayKetThuc == null || c.NgayKetThuc >= DateTime.Now));

                    if (coupon != null)
                        discount = CalculateDiscountAmount(subtotal, coupon.GiaTriGiam ?? 0m);
                }

                if (discount > subtotal) discount = subtotal;
                decimal total = subtotal + shippingFee - discount;
                if (total < 0) total = 0;

                // ĐỊA CHỈ + GHI CHÚ
                string province = string.IsNullOrWhiteSpace(input.Province)
                    ? ""
                    : input.Province.Trim();

                string fullAddress = string.IsNullOrWhiteSpace(province)
                    ? input.DiaChiDayDu
                    : $"{input.DiaChiDayDu} ({province})";

                if (!string.IsNullOrWhiteSpace(input.GhiChu))
                {
                    fullAddress += $" - Ghi chú: {input.GhiChu.Trim()}";
                }

                // Cập nhật đơn
                order.DiaChiGiao = fullAddress;
                order.TongTien = total;
                order.NgayDat = DateTime.Now;

                // Cập nhật ThanhToan cho từng chi tiết nếu form có gửi PaymentMethod
                if (!string.IsNullOrWhiteSpace(input.PaymentMethod))
                {
                    foreach (var ct in order.ChiTietDonHangs)
                    {
                        ct.ThanhToan = paymentMethod; // "wallet" hoặc "cod"
                    }
                }

                // Ghi chú từng dòng
                if (input.GhiChuChiTiet != null && input.GhiChuChiTiet.Any())
                {
                    foreach (var ct in order.ChiTietDonHangs)
                    {
                        if (input.GhiChuChiTiet.TryGetValue(ct.MaChiTiet, out var note)
                            && !string.IsNullOrWhiteSpace(note))
                        {
                            ct.GhiChu = note.Trim();
                        }
                    }
                }

                await _ctx.SaveChangesAsync();

                var redirectUrlEdit = Url.Action("Manage", "Manage", new { tab = "history" });

                return Json(new
                {
                    success = true,
                    message = "Cập nhật đơn hàng thành công.",
                    redirectUrl = redirectUrlEdit
                });
            }

            // ============== TẠO ĐƠN MỚI ==============
            var cartItems = await _ctx.ChiTietGioHangs
                .Include(c => c.MaSachNavigation)
                .Include(c => c.MaGioHangNavigation)
                .Where(c => c.MaGioHangNavigation!.MaNguoiDung == userId)
                .ToListAsync();

            if (!cartItems.Any())
                return Json(new { success = false, message = "Giỏ hàng đang trống." });

            // ===== KIỂM TRA TỒN KHO TRƯỚC KHI ĐẶT HÀNG =====
            foreach (var item in cartItems)
            {
                var sach = item.MaSachNavigation;
                int soLuongMua = item.SoLuong ?? 0;
                int tonKho = sach?.SoLuong ?? 0;

                if (soLuongMua <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Số lượng sản phẩm không hợp lệ."
                    });
                }

                if (tonKho < soLuongMua)
                {
                    string tenSach = sach?.TieuDe ?? $"Sách mã #{item.MaSach}";
                    return Json(new
                    {
                        success = false,
                        message = $"Sản phẩm \"{tenSach}\" không đủ số lượng. Còn lại: {tonKho}, bạn đang đặt: {soLuongMua}."
                    });
                }
            }

            // SUBTOTAL
            decimal subtotalNew = cartItems.Sum(i => (i.DonGia ?? 0m) * (i.SoLuong ?? 0));

            // PHÍ SHIP
            decimal shippingFeeNew = CalculateShippingFee(input.ShippingMethod);

            // GIẢM GIÁ
            decimal discountNew = 0m;
            if (!string.IsNullOrWhiteSpace(input.CouponCode))
            {
                var coupon = await _ctx.MaGiamGia
                    .FirstOrDefaultAsync(c =>
                        c.MaCode == input.CouponCode &&
                        c.DangHoatDong == true &&
                        (c.NgayBatDau == null || c.NgayBatDau <= DateTime.Now) &&
                        (c.NgayKetThuc == null || c.NgayKetThuc >= DateTime.Now));

                if (coupon != null)
                    discountNew = CalculateDiscountAmount(subtotalNew, coupon.GiaTriGiam ?? 0m);
            }

            if (discountNew > subtotalNew) discountNew = subtotalNew;

            decimal totalNew = subtotalNew + shippingFeeNew - discountNew;
            if (totalNew < 0) totalNew = 0;

            // THANH TOÁN VÍ (chỉ khi "wallet")
            if (paymentMethod == "wallet")
            {
                var soDu = user.SoDu ?? 0m;
                if (soDu < totalNew)
                    return Json(new { success = false, message = "Số dư ví không đủ để thanh toán." });

                user.SoDu = soDu - totalNew;
            }

            // ĐỊA CHỈ + GHI CHÚ
            string provinceNew = string.IsNullOrWhiteSpace(input.Province)
                ? ""
                : input.Province.Trim();

            string fullAddressNew = string.IsNullOrWhiteSpace(provinceNew)
                ? input.DiaChiDayDu
                : $"{input.DiaChiDayDu} ({provinceNew})";

            if (!string.IsNullOrWhiteSpace(input.GhiChu))
            {
                fullAddressNew += $" - Ghi chú: {input.GhiChu.Trim()}";
            }

            // TẠO ĐƠN
            var newOrder = new DonHang
            {
                MaNguoiDung = userId,
                NgayDat = DateTime.Now,
                TongTien = totalNew,
                DiaChiGiao = fullAddressNew,
                TrangThai = "Đang xử lý"
            };

            _ctx.DonHangs.Add(newOrder);
            await _ctx.SaveChangesAsync();

            // CHI TIẾT ĐƠN
            var chiTietList = new List<ChiTietDonHang>();

            foreach (var item in cartItems)
            {
                string? note = null;
                if (input.GhiChuChiTiet != null &&
                    input.GhiChuChiTiet.TryGetValue(item.MaChiTiet, out var n) &&
                    !string.IsNullOrWhiteSpace(n))
                {
                    note = n.Trim();
                }

                chiTietList.Add(new ChiTietDonHang
                {
                    MaDonHang = newOrder.MaDonHang,
                    MaSach = item.MaSach,
                    SoLuong = item.SoLuong,
                    DonGia = item.DonGia,
                    GhiChu = note ?? "",
                    ThanhToan = paymentMethod   // <-- "wallet" hoặc "cod"
                });
            }

            _ctx.ChiTietDonHangs.AddRange(chiTietList);

            // ===== TRỪ TỒN KHO SÁCH THEO SỐ LƯỢNG MUA =====
            foreach (var item in cartItems)
            {
                var sach = item.MaSachNavigation;
                if (sach != null)
                {
                    int tonKho = sach.SoLuong ?? 0;
                    int soLuongMua = item.SoLuong ?? 0;

                    // đảm bảo không âm
                    sach.SoLuong = Math.Max(0, tonKho - soLuongMua);
                }
            }

            // XOÁ GIỎ HÀNG
            _ctx.ChiTietGioHangs.RemoveRange(cartItems);
            await _ctx.SaveChangesAsync();

            var redirectUrl = Url.Action("Manage", "Manage", new { tab = "shipping" });

            return Json(new
            {
                success = true,
                message = "Đặt hàng thành công! Đơn hàng đang được xử lý.",
                redirectUrl
            });
        }

        // ========== HÀM GIẢM GIÁ ==========
        private decimal CalculateDiscountAmount(decimal subtotal, decimal giaTriGiam)
        {
            if (subtotal <= 0 || giaTriGiam <= 0) return 0m;

            // <=100 coi là %, >100 là số tiền
            if (giaTriGiam <= 100)
            {
                var discount = subtotal * giaTriGiam / 100m;
                return Math.Round(discount, 0);
            }
            else
            {
                return giaTriGiam;
            }
        }

        // ========== HÀM PHÍ SHIP ==========
        private decimal CalculateShippingFee(string shippingMethod)
        {
            if (string.Equals(shippingMethod, "express", StringComparison.OrdinalIgnoreCase))
            {
                return 50000m;
            }

            return 30000m;
        }

        // ========== GET: /Checkout/EditOrder/{id} ==========
        [HttpGet]
        public async Task<IActionResult> EditOrder(int id)
        {
            var userId = GetUserIdFromJwt();
            if (userId is null)
                return RedirectToAction("Login", "Account");

            // Lấy đơn hàng + chi tiết + sách
            var order = await _ctx.DonHangs
                .Include(o => o.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaSachNavigation)
                .FirstOrDefaultAsync(o => o.MaDonHang == id && o.MaNguoiDung == userId);

            if (order == null)
                return NotFound();

            // Map ChiTietDonHang -> ChiTietGioHang "ảo" để reuse view Buy
            var fakeItems = order.ChiTietDonHangs.Select(ct => new ChiTietGioHang
            {
                MaChiTiet = ct.MaChiTiet,
                MaSach = ct.MaSach,
                SoLuong = ct.SoLuong,
                DonGia = ct.DonGia,
                MaSachNavigation = ct.MaSachNavigation
            }).ToList();

            // Đánh dấu đang ở chế độ sửa
            ViewBag.IsEdit = true;
            ViewBag.OrderId = order.MaDonHang;

            return View("Buy", fakeItems);
        }

        // ========== POST: /Checkout/BuyNow ==========
        // Mua 1 sản phẩm ở nút mua ngay hoặc thanh toán lẻ đơn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BuyNow(int productId, int quantity = 1)
        {
            if (quantity < 1) quantity = 1;

            var userId = GetUserIdFromJwt();
            if (userId is null)
            {
                return RedirectToAction("Login", "Account");
            }

            var sach = _ctx.Saches.FirstOrDefault(s => s.MaSach == productId);
            if (sach == null)
            {
                // Không dùng TempData, chỉ cho về trang chủ
                return RedirectToAction("Index", "Home");
            }

            // Lấy hoặc tạo giỏ hàng
            var cart = _ctx.GioHangs
                .Include(g => g.ChiTietGioHangs)
                .FirstOrDefault(g => g.MaNguoiDung == userId);

            if (cart == null)
            {
                cart = new GioHang
                {
                    MaNguoiDung = userId.Value,
                    NgayTao = DateTime.Now
                };
                _ctx.GioHangs.Add(cart);
                _ctx.SaveChanges();
            }

            // Lấy tất cả item hiện có trong giỏ
            var allItems = _ctx.ChiTietGioHangs
                .Where(ct => ct.MaGioHang == cart.MaGioHang)
                .ToList();

            // Tìm item của sản phẩm đang Mua ngay
            var targetItem = allItems.FirstOrDefault(ct => ct.MaSach == productId);

            if (targetItem == null)
            {
                targetItem = new ChiTietGioHang
                {
                    MaGioHang = cart.MaGioHang,
                    MaSach = productId,
                    SoLuong = quantity,
                    DonGia = sach.Gia ?? 0m
                };
                _ctx.ChiTietGioHangs.Add(targetItem);
            }
            else
            {
                // Mua ngay: set lại đúng số lượng user chọn (không cộng dồn)
                targetItem.SoLuong = quantity;
                targetItem.DonGia = sach.Gia ?? 0m;
            }

            // XÓA các sản phẩm khác trong giỏ => chỉ để lại đúng sản phẩm Mua ngay
            var others = allItems.Where(ct => ct.MaSach != productId).ToList();
            if (others.Any())
            {
                _ctx.ChiTietGioHangs.RemoveRange(others);
            }

            _ctx.SaveChanges();

            // Sau khi chuẩn bị giỏ chỉ còn 1 sản phẩm → sang trang Buy để thanh toán
            return RedirectToAction(nameof(Buy));
        }

        // ========== GET: /Checkout/OrderDetails/{id} ==========
        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = GetUserIdFromJwt();
            if (userId is null)
                return RedirectToAction("Login", "Account");

            var order = await _ctx.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaSachNavigation)
                .FirstOrDefaultAsync(d => d.MaDonHang == id && d.MaNguoiDung == userId.Value);

            if (order == null)
                return NotFound();

            var items = order.ChiTietDonHangs?.ToList() ?? new List<ChiTietDonHang>();

            decimal subtotal = items.Sum(i => (i.DonGia ?? 0m) * (i.SoLuong ?? 0));
            decimal total = order.TongTien ?? subtotal;

            // Phi vận chuyển = phần chênh giữa tổng và subtotal
            decimal shipping = Math.Max(0, total - subtotal);

            //không lưu riêng giảm giá => để 0
            decimal discount = 0m;

            int itemCount = items.Sum(i => i.SoLuong ?? 0);

            ViewBag.Subtotal = subtotal;
            ViewBag.Shipping = shipping;
            ViewBag.Total = total;
            ViewBag.Discount = discount;
            ViewBag.ItemCount = itemCount;

            return View(order);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectInternWebBanSach.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ProjectInternWebBanSach.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly QuanLyBanSachContext _ctx;
        public CartController(QuanLyBanSachContext ctx) => _ctx = ctx;

        // ------------------- LẤY USERID TỪ JWT -------------------
        private int? GetUserIdFromJwt()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim)) return null;

            return int.Parse(idClaim);
        }

        // ========== THÊM VÀO GIỎ ==========
        [HttpPost("/Cart/Add")]
        public async Task<IActionResult> Add(int id, int qty = 1)
        {
            var userId = GetUserIdFromJwt();
            if (userId is null) return Json(new { login = false });

            if (qty <= 0) qty = 1;

            // Lấy thông tin sản phẩm + tồn kho
            var product = await _ctx.Saches
                .Where(s => s.MaSach == id)
                .Select(s => new
                {
                    s.MaSach,
                    Gia = s.Gia ?? 0m,
                    SoLuong = s.SoLuong ?? 0
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return Json(new { success = false, msg = "Sản phẩm không tồn tại." });
            }

            // Hết hàng -> không cho thêm vào giỏ
            if (product.SoLuong <= 0)
            {
                return Json(new { success = false, msg = "Sản phẩm đã hết hàng, không thể thêm vào giỏ." });
            }

            // Tìm hoặc tạo giỏ hàng cho user
            var cart = await _ctx.GioHangs
                .Include(g => g.ChiTietGioHangs)
                .FirstOrDefaultAsync(g => g.MaNguoiDung == userId);

            if (cart == null)
            {
                cart = new GioHang
                {
                    MaNguoiDung = userId.Value,
                    NgayTao = DateTime.Now
                };
                _ctx.GioHangs.Add(cart);
                await _ctx.SaveChangesAsync();
            }

            // Tìm chi tiết theo sách
            var item = cart.ChiTietGioHangs
                .FirstOrDefault(c => c.MaSach == id);

            int newQty;
            if (item == null)
            {
                // Số lượng mới không được vượt quá tồn kho
                newQty = Math.Min(qty, product.SoLuong);

                item = new ChiTietGioHang
                {
                    MaGioHang = cart.MaGioHang,
                    MaSach = id,
                    SoLuong = newQty,
                    DonGia = product.Gia
                };
                _ctx.ChiTietGioHangs.Add(item);
            }
            else
            {
                var currentQty = item.SoLuong ?? 0;
                newQty = currentQty + qty;

                // Không vượt quá tồn kho
                if (newQty > product.SoLuong)
                {
                    newQty = product.SoLuong;
                }

                item.SoLuong = newQty;
            }

            await _ctx.SaveChangesAsync();

            int count = await GetCartCount(userId.Value);
            return Json(new { success = true, count });
        }

        // ========== XOÁ 1 SẢN PHẨM ==========
        [HttpPost("/Cart/Remove")]
        public async Task<IActionResult> Remove(int id)
        {
            var userId = GetUserIdFromJwt();
            if (userId is null) return Json(new { login = false });

            var item = await _ctx.ChiTietGioHangs
                .Include(c => c.MaGioHangNavigation)
                .FirstOrDefaultAsync(c =>
                    c.MaChiTiet == id &&
                    c.MaGioHangNavigation!.MaNguoiDung == userId
                );

            if (item != null)
            {
                _ctx.ChiTietGioHangs.Remove(item);
                await _ctx.SaveChangesAsync();
            }

            int count = await GetCartCount(userId.Value);
            return Json(new { success = true, count });
        }

        // ========== XOÁ HẾT ==========
        [HttpPost("/Cart/Clear")]
        public async Task<IActionResult> Clear()
        {
            var userId = GetUserIdFromJwt();
            if (userId is null) return Json(new { login = false });

            var items = _ctx.ChiTietGioHangs
                .Include(c => c.MaGioHangNavigation)
                .Where(c => c.MaGioHangNavigation!.MaNguoiDung == userId);

            _ctx.ChiTietGioHangs.RemoveRange(items);
            await _ctx.SaveChangesAsync();

            return Json(new { success = true, count = 0 });
        }

        // ========== CẬP NHẬT SỐ LƯỢNG 1 SẢN PHẨM ==========
        [HttpPost("/Cart/UpdateQuantity")]
        public async Task<IActionResult> UpdateQuantity(int maChiTiet, int soLuong)
        {
            var userId = GetUserIdFromJwt();
            if (userId is null)
                return Json(new { success = false, login = false });

            if (soLuong <= 0)
                soLuong = 1;

            var item = await _ctx.ChiTietGioHangs
                .Include(c => c.MaGioHangNavigation)
                .Include(c => c.MaSachNavigation)
                .FirstOrDefaultAsync(c =>
                    c.MaChiTiet == maChiTiet &&
                    c.MaGioHangNavigation!.MaNguoiDung == userId);

            if (item == null)
                return Json(new { success = false, msg = "Không tìm thấy sản phẩm" });

            var stock = item.MaSachNavigation?.SoLuong ?? 0;

            // Nếu hết hàng -> không cho cập nhật, bắt user xóa khỏi giỏ
            if (stock <= 0)
            {
                return Json(new
                {
                    success = false,
                    msg = "Sản phẩm đã hết hàng, vui lòng xóa khỏi giỏ.",
                    outOfStock = true
                });
            }

            // Không cho vượt quá tồn kho
            if (soLuong > stock)
                soLuong = stock;

            item.SoLuong = soLuong;
            await _ctx.SaveChangesAsync();

            // Tính lại tổng phụ và tổng đơn (chỉ tính sản phẩm còn hàng)
            var items = await _ctx.ChiTietGioHangs
                .Include(c => c.MaGioHangNavigation)
                .Include(c => c.MaSachNavigation)
                .Where(c => c.MaGioHangNavigation!.MaNguoiDung == userId)
                .ToListAsync();

            var availableItems = items
                .Where(i => (i.MaSachNavigation?.SoLuong ?? 0) > 0)
                .ToList();

            decimal subtotal = availableItems.Sum(i => (i.DonGia ?? 0m) * (i.SoLuong ?? 0));
            int count = availableItems.Sum(i => i.SoLuong ?? 0);

            return Json(new
            {
                success = true,
                msg = "Đã cập nhật số lượng",
                lineTotal = (item.DonGia ?? 0m) * soLuong,
                subtotal,
                total = subtotal,
                count
            });
        }


        // ========== ĐẾM SỐ LƯỢNG ==========
        [HttpGet("/Cart/Count")]
        public async Task<IActionResult> Count()
        {
            var userId = GetUserIdFromJwt();
            int count = 0;

            if (userId != null)
                count = await GetCartCount(userId.Value);

            return Json(new { count });
        }

        private async Task<int> GetCartCount(int userId)
        {
            return await _ctx.ChiTietGioHangs
                .Include(c => c.MaGioHangNavigation)
                .Where(c => c.MaGioHangNavigation!.MaNguoiDung == userId)
                .SumAsync(c => c.SoLuong ?? 0);
        }

        // ========== TRANG GIỎ HÀNG ==========
        [HttpGet("/Cart/Cart")]
        public async Task<IActionResult> Cart()
        {
            var userId = GetUserIdFromJwt();
            if (userId is null) return RedirectToAction("Login", "Account");

            var items = await _ctx.ChiTietGioHangs
                .Include(c => c.MaSachNavigation)
                .Include(c => c.MaGioHangNavigation)
                .Where(c => c.MaGioHangNavigation!.MaNguoiDung == userId)
                .ToListAsync();

            return View(items);
        }
    }
}

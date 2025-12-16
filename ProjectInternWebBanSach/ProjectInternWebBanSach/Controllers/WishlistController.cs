using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectInternWebBanSach.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ProjectInternWebBanSach.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly QuanLyBanSachContext _ctx;
        public WishlistController(QuanLyBanSachContext ctx) => _ctx = ctx;

        // ------------------- LẤY USERID TỪ JWT -------------------
        private int? GetUserIdFromJwt()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim)) return null;

            return int.Parse(idClaim);
        }

        // POST: /Wishlist/Add
        [HttpPost("/Wishlist/Add")]
        public async Task<IActionResult> Add(int id)
        {
            var userId = GetUserIdFromJwt();
            if (userId is null) return Json(new { login = false });

            bool exists = await _ctx.SachYeuThiches
                .AnyAsync(x => x.MaNguoiDung == userId && x.MaSach == id);

            if (!exists)
            {
                _ctx.SachYeuThiches.Add(new SachYeuThich
                {
                    MaNguoiDung = userId.Value,
                    MaSach = id,
                    NgayTao = DateTime.Now
                });
                await _ctx.SaveChangesAsync();
            }

            int count = await _ctx.SachYeuThiches.CountAsync(x => x.MaNguoiDung == userId);
            return Json(new { success = true, count });
        }

        // POST: /Wishlist/Remove
        [HttpPost("/Wishlist/Remove")]
        public async Task<IActionResult> Remove(int id)
        {
            var userId = GetUserIdFromJwt();
            if (userId is null) return Json(new { login = false });

            var item = await _ctx.SachYeuThiches
                .FirstOrDefaultAsync(x => x.MaNguoiDung == userId && x.MaSach == id);

            if (item != null)
            {
                _ctx.SachYeuThiches.Remove(item);
                await _ctx.SaveChangesAsync();
            }

            int count = await _ctx.SachYeuThiches.CountAsync(x => x.MaNguoiDung == userId);
            return Json(new { success = true, count });
        }

        // POST: /Wishlist/Clear
        [HttpPost("/Wishlist/Clear")]
        public async Task<IActionResult> Clear()
        {
            var userId = GetUserIdFromJwt();
            if (userId is null) return Json(new { login = false });

            var items = _ctx.SachYeuThiches.Where(x => x.MaNguoiDung == userId);
            _ctx.SachYeuThiches.RemoveRange(items);
            await _ctx.SaveChangesAsync();

            return Json(new { success = true, count = 0 });
        }

        // GET: /Wishlist/Count
        [HttpGet("/Wishlist/Count")]
        public async Task<IActionResult> Count()
        {
            var userId = GetUserIdFromJwt();
            int count = userId is null
                ? 0
                : await _ctx.SachYeuThiches.CountAsync(x => x.MaNguoiDung == userId);

            return Json(new { count });
        }

        // GET: /Wishlist/Wishlist
        [HttpGet("/Wishlist/Wishlist")]
        public async Task<IActionResult> Wishlist()
        {
            var userId = GetUserIdFromJwt();
            if (userId is null) return RedirectToAction("Login", "Account");

            var items = await _ctx.SachYeuThiches
                .Include(x => x.MaSachNavigation)
                .Where(x => x.MaNguoiDung == userId)
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();

            return View(items);
        }
    }
}

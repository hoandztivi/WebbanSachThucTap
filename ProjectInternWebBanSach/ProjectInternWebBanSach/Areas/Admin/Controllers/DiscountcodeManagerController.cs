using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectInternWebBanSach.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectInternWebBanSach.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DiscountcodeManagerController : Controller
    {
        private readonly QuanLyBanSachContext _ctx;

        public DiscountcodeManagerController(QuanLyBanSachContext ctx)
        {
            _ctx = ctx;
        }

        // VIEW: /Admin/DiscountcodeManager/Index
        [HttpGet]
        public IActionResult Discountcode()
        {
            return View();
        }

        // ===== DTO dùng cho API =====
        public class DiscountCodeDto
        {
            public int MaGiamGia { get; set; }
            public string MaCode { get; set; } = "";
            public string? MoTa { get; set; }
            public decimal GiaTriGiam { get; set; }
            public DateTime NgayBatDau { get; set; }
            public DateTime NgayKetThuc { get; set; }
            public bool DangHoatDong { get; set; }
        }

        // LẤY DANH SÁCH MÃ GIẢM
        // GET: /Admin/DiscountcodeManager/GetCodes
        [HttpGet]
        public async Task<IActionResult> GetCodes()
        {
            var codes = await _ctx.MaGiamGia
                .OrderByDescending(x => x.NgayBatDau)
                .Select(x => new
                {
                    x.MaGiamGia,
                    x.MaCode,
                    x.MoTa,
                    x.GiaTriGiam,
                    x.NgayBatDau,
                    x.NgayKetThuc,
                    x.DangHoatDong
                })
                .ToListAsync();

            return Json(new { success = true, data = codes });
        }

        // TẠO / CẬP NHẬT MÃ GIẢM
        // POST: /Admin/DiscountcodeManager/Save
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] DiscountCodeDto model)
        {
            if (string.IsNullOrWhiteSpace(model.MaCode))
                return Json(new { success = false, message = "Mã code không được để trống." });

            if (model.GiaTriGiam <= 0)
                return Json(new { success = false, message = "Giá trị giảm phải lớn hơn 0." });

            if (model.NgayKetThuc < model.NgayBatDau)
                return Json(new { success = false, message = "Ngày kết thúc phải sau ngày bắt đầu." });

            var codeUpper = model.MaCode.Trim().ToUpperInvariant();

            // Check trùng code
            var exists = await _ctx.MaGiamGia
                .AnyAsync(x => x.MaCode!.ToUpper() == codeUpper && x.MaGiamGia != model.MaGiamGia);

            if (exists)
                return Json(new { success = false, message = "Mã giảm giá này đã tồn tại." });

            if (model.MaGiamGia == 0)
            {
                // ===== THÊM MỚI =====
                var entity = new MaGiamGium
                {
                    MaCode = codeUpper,
                    MoTa = model.MoTa,
                    GiaTriGiam = model.GiaTriGiam,
                    NgayBatDau = model.NgayBatDau,
                    NgayKetThuc = model.NgayKetThuc,
                    DangHoatDong = model.DangHoatDong
                };

                _ctx.MaGiamGia.Add(entity);
            }
            else
            {
                // ===== CẬP NHẬT =====
                var entity = await _ctx.MaGiamGia
                    .FirstOrDefaultAsync(x => x.MaGiamGia == model.MaGiamGia);

                if (entity == null)
                    return Json(new { success = false, message = "Không tìm thấy mã giảm giá." });

                entity.MaCode = codeUpper;
                entity.MoTa = model.MoTa;
                entity.GiaTriGiam = model.GiaTriGiam;
                entity.NgayBatDau = model.NgayBatDau;
                entity.NgayKetThuc = model.NgayKetThuc;
                entity.DangHoatDong = model.DangHoatDong;
            }

            await _ctx.SaveChangesAsync();
            return Json(new { success = true, message = "Lưu mã giảm giá thành công." });
        }

        // XÓA MÃ GIẢM
        // POST: /Admin/DiscountcodeManager/Delete
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            var entity = await _ctx.MaGiamGia
                .FirstOrDefaultAsync(x => x.MaGiamGia == id);

            if (entity == null)
                return Json(new { success = false, message = "Không tìm thấy mã giảm giá." });

            _ctx.MaGiamGia.Remove(entity);
            await _ctx.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa mã giảm giá thành công." });
        }
    }
}

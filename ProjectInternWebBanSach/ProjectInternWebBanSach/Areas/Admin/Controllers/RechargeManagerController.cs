using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectInternWebBanSach.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectInternWebBanSach.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RechargeManagerController : Controller
    {
        private readonly QuanLyBanSachContext _ctx;

        // Trạng thái cho phép
        private static readonly string[] AllowedStatuses = new[]
        {
            "Chờ thanh toán",
            "Hoàn thành",
            "Hoàn tiền hủy đơn"
        };

        public RechargeManagerController(QuanLyBanSachContext ctx)
        {
            _ctx = ctx;
        }

        // VIEW: /Admin/RechargeManager/RechargeHistory
        [HttpGet]
        public IActionResult RechargeHistory()
        {
            return View(); //
        }

        // GET: /Admin/RechargeManager/GetRecharges?status=all
        [HttpGet]
        public async Task<IActionResult> GetRecharges(string? status = "all")
        {
            var query = _ctx.LichSuNapTiens
                .Include(x => x.MaNguoiDungNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "all")
            {
                query = query.Where(x => x.TrangThai == status);
            }

            var list = await query
                .OrderByDescending(x => x.NgayTao)
                .Select(x => new
                {
                    x.MaNapTien,
                    x.MaNguoiDung,
                    HoTen = x.MaNguoiDungNavigation.HoTen,
                    Email = x.MaNguoiDungNavigation.Email,
                    SoDienThoai = x.MaNguoiDungNavigation.SoDienThoai,
                    x.SoTien,
                    x.NoiDung,
                    x.TrangThai,
                    x.MaGiaoDich,
                    x.NgayTao
                })
                .ToListAsync();

            return Json(new
            {
                success = true,
                data = list
            });
        }

        // DTO UPDATE
        public class UpdateRechargeStatusDto
        {
            public int MaNapTien { get; set; }
            public string TrangThai { get; set; } = "";
        }

        // POST: /Admin/RechargeManager/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateRechargeStatusDto model)
        {
            if (model.MaNapTien <= 0)
            {
                return Json(new { success = false, message = "Mã nạp tiền không hợp lệ." });
            }

            if (string.IsNullOrWhiteSpace(model.TrangThai) ||
                !AllowedStatuses.Contains(model.TrangThai))
            {
                return Json(new { success = false, message = "Trạng thái không hợp lệ." });
            }

            var record = await _ctx.LichSuNapTiens
                .FirstOrDefaultAsync(x => x.MaNapTien == model.MaNapTien);

            if (record == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bản ghi nạp tiền." });
            }

            var oldStatus = record.TrangThai ?? "";
            var newStatus = model.TrangThai;

            if (string.Equals(oldStatus, newStatus, System.StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = true, message = "Trạng thái không thay đổi." });
            }

            record.TrangThai = newStatus;
            await _ctx.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật trạng thái thành công." });
        }
    }
}

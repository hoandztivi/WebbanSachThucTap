using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectInternWebBanSach.Models;
using System.Security.Claims;
using ProjectInternWebBanSach.DTO;

namespace ProjectInternWebBanSach.Controllers
{
    [Authorize]
    public class RechargeController : Controller
    {
        private readonly QuanLyBanSachContext _context;

        public RechargeController(QuanLyBanSachContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Recharge()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ViewBag.UserId = userId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmRecharge([FromBody] ConfirmRechargeDto dto)
        {
            if (dto == null || dto.Amount < 50000 || string.IsNullOrWhiteSpace(dto.TransferCode))
            {
                Response.StatusCode = 400;
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                Response.StatusCode = 401;
                return Json(new { success = false, message = "Phiên đăng nhập không hợp lệ." });
            }

            var now = DateTime.Now;

            // Kiểm tra xem có yêu cầu gần đây không
            var last = _context.LichSuNapTiens
                .Where(x => x.MaNguoiDung == userId &&
                            (x.TrangThai == "Chờ thanh toán" || x.TrangThai == "Chờ duyệt"))
                .OrderByDescending(x => x.NgayTao)
                .FirstOrDefault();

            if (last != null)
            {
                // Nếu đã ở trạng thái Chờ duyệt -> bắt user chờ admin xử lý
                if (last.TrangThai == "Chờ duyệt")
                {
                    Response.StatusCode = 400;
                    return Json(new
                    {
                        success = false,
                        message = "Bạn đang có yêu cầu nạp tiền chờ duyệt. Vui lòng chờ admin xử lý hoặc liên hệ hỗ trợ."
                    });
                }

                // Nếu vẫn là "Chờ thanh toán" và chưa qua 10 phút
                if (last.TrangThai == "Chờ thanh toán" &&
                    last.NgayTao.HasValue &&
                    last.NgayTao.Value.AddMinutes(10) > now)
                {
                    var wait = last.NgayTao.Value.AddMinutes(10) - now;
                    var minutes = Math.Ceiling(wait.TotalMinutes);
                    Response.StatusCode = 400;
                    return Json(new
                    {
                        success = false,
                        message = $"Bạn vừa gửi yêu cầu nạp tiền. Vui lòng thử lại sau khoảng {minutes} phút."
                    });
                }
            }

            // Tạo bản ghi mới
            var lichSu = new LichSuNapTien
            {
                MaNguoiDung = userId,
                SoTien = dto.Amount,
                NoiDung = dto.TransferCode,
                TrangThai = "Chờ thanh toán",
                MaGiaoDich = dto.TransferCode,
                NgayTao = now
            };

            _context.LichSuNapTiens.Add(lichSu);
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                message = "Đã xác nhận thanh toán. Vui lòng chờ admin kiểm tra và duyệt."
            });
        }
        //hủy nạp
        [HttpPost]
        public IActionResult Cancel(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var item = _context.LichSuNapTiens
                .FirstOrDefault(x => x.MaNapTien == id && x.MaNguoiDung == userId);

            if (item == null)
                return NotFound();

            if (item.TrangThai == "Hoàn thành")
                return BadRequest("Không thể hủy giao dịch đã hoàn thành.");

            _context.LichSuNapTiens.Remove(item);
            _context.SaveChanges();

            return Json(new { success = true, message = "Đã hủy giao dịch." });
        }

    }
}

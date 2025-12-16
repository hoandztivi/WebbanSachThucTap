using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectInternWebBanSach.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

namespace ProjectInternWebBanSach.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserManagementController : Controller
    {
        private readonly QuanLyBanSachContext _ctx;
        private readonly IWebHostEnvironment _env;

        public UserManagementController(QuanLyBanSachContext ctx, IWebHostEnvironment env)
        {
            _ctx = ctx;
            _env = env;
        }

        // ================== HÀM HASH MẬT KHẨU SHA256 ==================
        private static string HashPasswordSha256(string password)
        {
            if (string.IsNullOrEmpty(password)) return "";

            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hashBytes = sha.ComputeHash(bytes);

            // chuyển sang chuỗi hex: abcd1234...
            return BitConverter.ToString(hashBytes)
                .Replace("-", "")
                .ToLowerInvariant();
        }

        // ================== VIEW: Quản lý vai trò =====================
        [HttpGet]
        public IActionResult Role()
        {
            return View();
        }

        // ================== Lấy danh sách vai trò ====================
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _ctx.VaiTros
                .OrderBy(r => r.MaVaiTro)
                .Select(r => new
                {
                    r.MaVaiTro,
                    r.TenVaiTro,
                    r.MoTa
                })
                .ToListAsync();

            return Json(new
            {
                success = true,
                data = roles
            });
        }

        // DTO thêm vai trò
        public class RoleCreateDto
        {
            public string TenVaiTro { get; set; } = "";
            public string? MoTa { get; set; }
        }

        // ================== Thêm vai trò =============================
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] RoleCreateDto model)
        {
            if (string.IsNullOrWhiteSpace(model.TenVaiTro))
            {
                return Json(new
                {
                    success = false,
                    message = "Tên vai trò không được để trống."
                });
            }

            var ten = model.TenVaiTro.Trim();

            // Kiểm tra trùng tên
            var existed = await _ctx.VaiTros
                .AnyAsync(v => v.TenVaiTro == ten);

            if (existed)
            {
                return Json(new
                {
                    success = false,
                    message = "Tên vai trò đã tồn tại."
                });
            }

            var role = new VaiTro
            {
                TenVaiTro = ten,
                MoTa = model.MoTa?.Trim()
            };

            _ctx.VaiTros.Add(role);
            await _ctx.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Thêm vai trò thành công.",
                data = new
                {
                    role.MaVaiTro,
                    role.TenVaiTro,
                    role.MoTa
                }
            });
        }

        // ================== Xoá vai trò ==============================
        [HttpPost]
        public async Task<IActionResult> DeleteRole([FromBody] int id)
        {
            var role = await _ctx.VaiTros.FindAsync(id);
            if (role == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Không tìm thấy vai trò cần xoá."
                });
            }

            _ctx.VaiTros.Remove(role);

            try
            {
                await _ctx.SaveChangesAsync();
                return Json(new
                {
                    success = true,
                    message = "Xoá vai trò thành công."
                });
            }
            catch (DbUpdateException)
            {
                return Json(new
                {
                    success = false,
                    message = "Không thể xoá vai trò vì đang có dữ liệu liên quan."
                });
            }
        }

        // ================== VIEW: Danh sách user =====================
        [HttpGet]
        public async Task<IActionResult> ListofUsers()
        {
            var roles = await _ctx.VaiTros
                .OrderBy(r => r.TenVaiTro)
                .ToListAsync();

            ViewBag.Roles = roles;
            return View();
        }

        // ================== Lấy danh sách người dùng =================
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _ctx.NguoiDungs
                .Include(u => u.MaVaiTroNavigation)
                .OrderByDescending(u => u.NgayTao)
                .Select(u => new
                {
                    u.MaNguoiDung,
                    u.HoTen,
                    u.Email,
                    u.SoDienThoai,
                    u.DiaChi,
                    u.AnhDaiDien,
                    u.SoDu,
                    u.TrangThai,
                    u.NgayTao,
                    VaiTro = u.MaVaiTroNavigation != null
                        ? u.MaVaiTroNavigation.TenVaiTro
                        : null
                })
                .ToListAsync();

            return Json(new { success = true, data = users });
        }

        // ================== Upload avatar ============================
        [HttpPost]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "File không hợp lệ." });

            var folder = Path.Combine(_env.WebRootPath, "img", "anhdaidien");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var path = Path.Combine(folder, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Json(new
            {
                success = true,
                fileName = fileName,
                fileUrl = "/img/anhdaidien/" + fileName
            });
        }

        // ================== DTO thêm user ============================
        public class CreateUserDto
        {
            public string? HoTen { get; set; }
            public string? Email { get; set; }
            public string? MatKhau { get; set; }
            public string? SoDienThoai { get; set; }
            public string? DiaChi { get; set; }
            public decimal? SoDu { get; set; }
            public int? MaVaiTro { get; set; }
            public string? AnhDaiDien { get; set; }
        }

        // ================== Thêm user ================================
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Email))
                return Json(new { success = false, message = "Email không được để trống." });

            if (string.IsNullOrWhiteSpace(model.MatKhau))
                return Json(new { success = false, message = "Mật khẩu không được để trống." });

            var email = model.Email.Trim();

            // CHECK TRÙNG EMAIL
            var existed = await _ctx.NguoiDungs
                .AnyAsync(u => u.Email == email);

            if (existed)
            {
                return Json(new
                {
                    success = false,
                    message = "Email này đã được sử dụng. Vui lòng chọn email khác."
                });
            }

            // Băm mật khẩu trước khi lưu
            var hashedPassword = HashPasswordSha256(model.MatKhau);

            var user = new NguoiDung
            {
                HoTen = model.HoTen,
                Email = email,
                MatKhau = hashedPassword,
                SoDienThoai = model.SoDienThoai,
                DiaChi = model.DiaChi,
                SoDu = model.SoDu ?? 0,
                MaVaiTro = model.MaVaiTro,
                AnhDaiDien = model.AnhDaiDien,
                NgayTao = DateTime.Now,
                TrangThai = true
            };

            _ctx.NguoiDungs.Add(user);
            await _ctx.SaveChangesAsync();

            return Json(new { success = true, message = "Thêm người dùng thành công." });
        }

        // ================== Xóa user ================================
        [HttpPost]
        public async Task<IActionResult> DeleteUser([FromBody] int id)
        {
            var user = await _ctx.NguoiDungs.FindAsync(id);
            if (user == null)
                return Json(new { success = false, message = "Không tìm thấy người dùng." });

            // xóa avatar nếu có
            if (!string.IsNullOrEmpty(user.AnhDaiDien))
            {
                var fullPath = Path.Combine(
                    _env.WebRootPath,
                    user.AnhDaiDien.TrimStart('/').Replace("/", "\\")
                );

                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }

            _ctx.NguoiDungs.Remove(user);
            await _ctx.SaveChangesAsync();

            return Json(new { success = true, message = "Xoá người dùng thành công." });
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using ProjectInternWebBanSach.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ProjectInternWebBanSach.DTO;
using Microsoft.AspNetCore.Authorization;

namespace ProjectInternWebBanSach.Controllers
{
    public class AccountController : Controller
    {
        private readonly QuanLyBanSachContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public AccountController(QuanLyBanSachContext context, IWebHostEnvironment env, IConfiguration config)
        {
            _context = context;
            _env = env;
            _config = config;
        }

        // --------------------- LOGIN ---------------------
        [HttpGet]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public IActionResult Login()
        {
            // Chỉ nhớ email để prefill (KHÔNG nhớ mật khẩu)
            ViewBag.SavedEmail = Request.Cookies["UserEmail"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public IActionResult Login(
            [FromForm(Name = "Email")] string email,
            [FromForm(Name = "MatKhau")] string matkhau,
            bool RememberMe)
        {
            // Xoá các cookie cũ (nếu có)
            Response.Cookies.Delete("UserEmail");
            Response.Cookies.Delete("UserPassword");

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(matkhau))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            var emailNorm = email.Trim().ToLowerInvariant();
            string hashed = GetSHA256(matkhau);

            var user = _context.NguoiDungs
                .FirstOrDefault(u => u.Email == emailNorm && u.MatKhau == hashed && u.TrangThai == true);

            if (user == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không chính xác!";
                return View();
            }

            // ---------------------- Lưu session----------------------
            HttpContext.Session.SetInt32("MaNguoiDung", user.MaNguoiDung);
            HttpContext.Session.SetString("HoTen", user.HoTen ?? "");
            HttpContext.Session.SetString("Email", user.Email ?? "");
            HttpContext.Session.SetString("VaiTro", user.MaVaiTroNavigation?.TenVaiTro ?? "user");
            HttpContext.Session.SetString("AnhDaiDien", user.AnhDaiDien ?? "default.png");
            HttpContext.Session.SetString("SoDu", user.SoDu?.ToString() ?? "0");

            HttpContext.Session.SetString("SoDienThoai", user.SoDienThoai ?? "");
            HttpContext.Session.SetString("NgayTao", (user.NgayTao ?? DateTime.Now).ToString("dd/MM/yyyy HH:mm"));

            // ---------- Sinh JWT Token & LƯU VÀO COOKIE HTTPONLY ----------
            string token = GenerateJwtToken(user);

            var tokenCookie = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(7),
                HttpOnly = true,
                Secure = true,                 // yêu cầu HTTPS
                SameSite = SameSiteMode.Lax,
                Path = "/"
            };
            Response.Cookies.Append("AccessToken", token, tokenCookie);

            // ---------- Nếu RememberMe thì chỉ lưu email không lưu password----------
            if (RememberMe)
            {
                var emailCookie = new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddDays(7),
                    HttpOnly = true,
                    Secure = true, //nếu domain là http thì để false
                    SameSite = SameSiteMode.Lax,
                    Path = "/"
                };
                Response.Cookies.Append("UserEmail", emailNorm, emailCookie);
            }

            return RedirectToAction("Index", "Home");
        }

        // --------------------- LOGOUT ---------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("UserEmail");
            Response.Cookies.Delete("AccessToken"); // xoá JWT
            return RedirectToAction("Index", "Home");
        }
        // --------------------- REGISTER ---------------------
        [HttpGet]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public IActionResult Register(NguoiDung model, IFormFile? AnhDaiDien)
        {
            if (string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.MatKhau) ||
                string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View(model);
            }

            if (model.MatKhau != model.ConfirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View(model);
            }

            var emailNorm = model.Email.Trim().ToLowerInvariant();
            var existing = _context.NguoiDungs.FirstOrDefault(x => x.Email == emailNorm);
            if (existing != null)
            {
                ViewBag.Error = "Email đã được đăng ký.";
                return View(model);
            }

            // ---------- Lưu ảnh đại diện ----------
            string fileName = "default.png";
            if (AnhDaiDien != null && AnhDaiDien.Length > 0)
            {
                string uploadFolder = Path.Combine(_env.WebRootPath, "img/anhdaidien");
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                fileName = Guid.NewGuid().ToString() + Path.GetExtension(AnhDaiDien.FileName);
                string filePath = Path.Combine(uploadFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                AnhDaiDien.CopyTo(stream);
            }

            // ---------- Mã hóa mật khẩu ----------
            var newUser = new NguoiDung
            {
                HoTen = model.HoTen,
                Email = emailNorm,
                MatKhau = GetSHA256(model.MatKhau),
                AnhDaiDien = fileName,
                SoDienThoai = model.SoDienThoai,
                DiaChi = model.DiaChi,
                NgayTao = DateTime.Now,
                TrangThai = true,
                MaVaiTro = 2, // user
                SoDu = 0
            };

            _context.NguoiDungs.Add(newUser);
            _context.SaveChanges();

            ViewBag.Success = "Đăng ký thành công! Mời bạn đăng nhập.";
            return RedirectToAction("Login");
        }

        // --------------------- HÀM MÃ HÓA SHA256 ---------------------
        private string GetSHA256(string input)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLower();
        }

        // --------------------- HÀM SINH JWT TOKEN ---------------------
        private string GenerateJwtToken(NguoiDung user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.MaNguoiDung.ToString()),
                new Claim(ClaimTypes.Name, user.HoTen ?? user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.MaVaiTroNavigation?.TenVaiTro ?? "user"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //-----------------------HỒ SƠ----------------------
        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult Profile()
        {
            if (Request.Cookies.TryGetValue("AccessToken", out var token))
                ViewBag.AccessToken = token;// đưa token ra ViewBag
            else
                ViewBag.AccessToken = null;

            return View();
        }
        //---------------CHANEGE PASSWORD----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult ChangePassword([FromForm] ChangePasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword) ||
                string.IsNullOrWhiteSpace(dto.NewPassword) ||
                string.IsNullOrWhiteSpace(dto.ConfirmPassword))
                return BadRequest("Vui lòng nhập đầy đủ thông tin.");

            if (dto.NewPassword.Length < 6)
                return BadRequest("Mật khẩu mới phải có ít nhất 6 ký tự.");

            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest("Mật khẩu mới và xác nhận không khớp.");

            var userId = HttpContext.Session.GetInt32("MaNguoiDung");
            if (userId == null) return Unauthorized("Phiên đăng nhập đã hết. Vui lòng đăng nhập lại.");

            var user = _context.NguoiDungs.FirstOrDefault(x => x.MaNguoiDung == userId && x.TrangThai == true);
            if (user == null) return BadRequest("Không tìm thấy người dùng.");

            if (!string.Equals(user.MatKhau, GetSHA256(dto.CurrentPassword), StringComparison.OrdinalIgnoreCase))
                return BadRequest("Mật khẩu hiện tại không chính xác.");

            if (string.Equals(user.MatKhau, GetSHA256(dto.NewPassword), StringComparison.OrdinalIgnoreCase))
                return BadRequest("Mật khẩu mới không được trùng mật khẩu hiện tại.");

            user.MatKhau = GetSHA256(dto.NewPassword);
            _context.SaveChanges();

            // Đăng xuất
            HttpContext.Session.Clear();
            Response.Cookies.Delete("AccessToken");

            return Ok("Đổi mật khẩu thành công. Vui lòng đăng nhập lại.");
        }
    }
}

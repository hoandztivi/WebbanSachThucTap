using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectInternWebBanSach.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProjectInternWebBanSach.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AuthController : Controller
    {
        private readonly QuanLyBanSachContext _context;
        private readonly IConfiguration _config;

        public AuthController(QuanLyBanSachContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // =========================================================
        // LOGIN (GET)
        // =========================================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            ViewBag.SavedEmail = Request.Cookies["AdminEmail"];
            return View();
        }

        // =========================================================
        // LOGIN (POST)
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public IActionResult Login(
            [FromForm] string Email,
            [FromForm] string Password,
            bool RememberMe)
        {
            // Xoá cookie cũ
            Response.Cookies.Delete("AdminEmail");
            Response.Cookies.Delete("AccessToken");
            Response.Cookies.Delete("RefreshToken");

            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            // Validate input
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                if (isAjax)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Vui lòng nhập đầy đủ email và mật khẩu."
                    });
                }

                ViewBag.Error = "Vui lòng nhập đầy đủ email và mật khẩu.";
                ViewBag.SavedEmail = Email;
                return View();
            }

            var emailNorm = Email.Trim().ToLowerInvariant();
            var hashed = GetSHA256(Password);

            // Chỉ cho phép vai trò Admin
            var user = _context.NguoiDungs
                .Include(u => u.MaVaiTroNavigation)
                .FirstOrDefault(u =>
                    u.Email == emailNorm &&
                    u.MatKhau == hashed &&
                    u.TrangThai == true &&
                    u.MaVaiTro == 1);

            if (user == null)
            {
                if (isAjax)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Tài khoản không tồn tại, mật khẩu sai hoặc không có quyền quản trị."
                    });
                }

                ViewBag.Error = "Tài khoản không tồn tại, mật khẩu sai hoặc không có quyền quản trị.";
                ViewBag.SavedEmail = Email;
                return View();
            }

            // ---------- Sinh Access Token ----------
            string accessToken = GenerateJwtToken(user);

            // ---------- Sinh Refresh Token ----------
            var refreshToken = GenerateRefreshToken(user.MaNguoiDung);
            _context.RefreshTokens.Add(refreshToken);

            // ---------- Ghi lịch sử đăng nhập ----------
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();

            var history = new LichSuDangNhap
            {
                MaNguoiDung = user.MaNguoiDung,
                ThietBi = "Admin Panel",
                TrinhDuyet = userAgent,
                DiaChiIp = ip,
                Token = refreshToken.GiaTriToken,
                NgayDangNhap = DateTime.Now,
                TrangThai = "Đăng nhập admin thành công"
            };

            _context.LichSuDangNhaps.Add(history);
            _context.SaveChanges();

            // ---------- Lưu AccessToken vào cookie ----------
            double expireMinutes = Convert.ToDouble(_config["Jwt:ExpireMinutes"]);
            var accessCookie = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
                HttpOnly = true,
                // dev HTTP thì tạm cho false, production mới true
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            };
            Response.Cookies.Append("AccessToken", accessToken, accessCookie);

            // ---------- Lưu RefreshToken vào cookie ----------
            var refreshCookie = new CookieOptions
            {
                Expires = refreshToken.ThoiHan,
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            };
            Response.Cookies.Append("RefreshToken", refreshToken.GiaTriToken, refreshCookie);

            // ---------- Ghi nhớ email admin ----------
            if (RememberMe)
            {
                var emailCookie = new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddDays(7),
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Path = "/"
                };
                Response.Cookies.Append("AdminEmail", emailNorm, emailCookie);
            }

            var redirectUrl = Url.Action("Index", "Dashboard", new { area = "Admin" });

            if (isAjax)
            {
                return Json(new
                {
                    success = true,
                    message = "Đăng nhập quản trị thành công!",
                    redirectUrl = redirectUrl
                });
            }

            return Redirect(redirectUrl!);
        }

        // =========================================================
        // LOGOUT ADMIN
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public IActionResult Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
            {
                var activeTokens = _context.RefreshTokens
                    .Where(t => t.MaNguoiDung == userId &&
                                t.ThuHoiLuc == null &&
                                t.ThoiHan > DateTime.UtcNow)
                    .ToList();

                foreach (var t in activeTokens)
                {
                    t.ThuHoiLuc = DateTime.UtcNow;
                    t.ThuHoiTuIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                }

                _context.SaveChanges();
            }

            Response.Cookies.Delete("AdminEmail");
            Response.Cookies.Delete("AccessToken");
            Response.Cookies.Delete("RefreshToken");

            return RedirectToAction("Login", "Auth", new { area = "Admin" });
        }

        // =========================================================
        // HÀM PHỤ
        // =========================================================
        private string GetSHA256(string input)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLower();
        }

        private string GenerateJwtToken(NguoiDung user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.MaNguoiDung.ToString()),
                    new Claim(ClaimTypes.Name, user.HoTen ?? user.Email!),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim(ClaimTypes.Role, user.MaVaiTroNavigation?.TenVaiTro ?? "user"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("Avatar", user.AnhDaiDien ?? "default.png"),
                    new Claim("SoDu", (user.SoDu ?? 0).ToString()),
                    new Claim(ClaimTypes.MobilePhone, user.SoDienThoai ?? ""),
                    new Claim(ClaimTypes.StreetAddress, user.DiaChi ?? "")
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

        private RefreshToken GenerateRefreshToken(int maNguoiDung)
        {
            var randomBytes = new byte[64];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            string token = Convert.ToBase64String(randomBytes);

            return new RefreshToken
            {
                MaNguoiDung = maNguoiDung,
                GiaTriToken = token,
                NgayTao = DateTime.UtcNow,
                ThoiHan = DateTime.UtcNow.AddDays(
                    Convert.ToDouble(_config["Jwt:RefreshTokenDays"])
                ),
                TaoTuIp = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProjectInternWebBanSach.DTO;
using ProjectInternWebBanSach.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Net;

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

        //DTO for Cloudflare Turnstile response
        public class TurnstileVerifyResponse
        {
            public bool success { get; set; }
            public string? challenge_ts { get; set; }
            public string? hostname { get; set; }
        }

        // =========================================================
        // LOGIN
        // =========================================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            ViewBag.SavedEmail = Request.Cookies["UserEmail"];
            ViewBag.TurnstileSiteKey = _config["CloudflareTurnstile:SiteKey"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [FromForm(Name = "Email")] string email,
            [FromForm(Name = "MatKhau")] string matkhau,
            bool RememberMe,
            [FromForm(Name = "cf-turnstile-response")] string turnstileResponse)
            {
            //VERIFY CLOUDFLARE TURNSTILE =====
            if (string.IsNullOrWhiteSpace(turnstileResponse))
            {
                return Json(new
                {
                    success = false,
                    message = "Vui lòng xác minh bảo mật (Turnstile)."
                });
            }

            var secretKey = _config["CloudflareTurnstile:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                // Phòng trường hợp cấu hình thiếu
                return Json(new
                {
                    success = false,
                    message = "Thiếu cấu hình Cloudflare Turnstile."
                });
            }

            using (var http = new HttpClient())
            {
                var form = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["secret"] = secretKey,
                    ["response"] = turnstileResponse,
                    ["remoteip"] = HttpContext.Connection.RemoteIpAddress?.ToString() ?? ""
                });

                var resp = await http.PostAsync("https://challenges.cloudflare.com/turnstile/v0/siteverify", form);
                if (!resp.IsSuccessStatusCode)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không kết nối được máy chủ xác minh bảo mật."
                    });
                }

                var verifyResult = await resp.Content.ReadFromJsonAsync<TurnstileVerifyResponse>();
                if (verifyResult == null || !verifyResult.success)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Xác minh bảo mật thất bại. Vui lòng thử lại."
                    });
                }
            }
            // Xoá cookie cũ
            Response.Cookies.Delete("UserEmail");
            Response.Cookies.Delete("UserPassword");
            Response.Cookies.Delete("AccessToken");
            Response.Cookies.Delete("RefreshToken");

            // Validate input
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(matkhau))
            {
                return Json(new
                {
                    success = false,
                    message = "Vui lòng nhập đầy đủ thông tin."
                });
            }

            var emailNorm = email.Trim().ToLowerInvariant();
            string hashed = GetSHA256(matkhau);

            // Tìm user
            var user = _context.NguoiDungs
                .FirstOrDefault(u => u.Email == emailNorm && u.MatKhau == hashed && u.TrangThai == true);

            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Email hoặc mật khẩu không chính xác!"
                });
            }

            // ---------- Sinh Access Token ----------
            string accessToken = GenerateJwtToken(user);

            // ---------- Sinh Refresh Token ----------
            var refreshToken = GenerateRefreshToken(user.MaNguoiDung);
            _context.RefreshTokens.Add(refreshToken);

            // ---------- Lấy IP & User-Agent để ghi lịch sử ----------
            var ip = HttpContext.Connection.RemoteIpAddress;
            string? ipAddress = ip?.ToString();

            var userAgent = Request.Headers["User-Agent"].ToString();

            // ---------- Ghi lịch sử đăng nhập ----------
            var history = new LichSuDangNhap
            {
                MaNguoiDung = user.MaNguoiDung,
                ThietBi = "Website",
                TrinhDuyet = userAgent,
                DiaChiIp = ipAddress,
                Token = refreshToken.GiaTriToken,
                NgayDangNhap = DateTime.Now,
                TrangThai = "Đăng nhập thành công"
            };

            _context.LichSuDangNhaps.Add(history);

            // Lưu RefreshToken + Lịch sử
            _context.SaveChanges();

            // ---------- Lưu AccessToken vào cookie ----------
            double expireMinutes = Convert.ToDouble(_config["Jwt:ExpireMinutes"]);
            var accessCookie = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
                HttpOnly = true,
                Secure = true,                 // nếu dev HTTP thì tạm sửa false
                SameSite = SameSiteMode.Lax,
                Path = "/"
            };
            Response.Cookies.Append("AccessToken", accessToken, accessCookie);

            // ---------- Lưu RefreshToken vào cookie ----------
            var refreshCookie = new CookieOptions
            {
                Expires = refreshToken.ThoiHan,
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            };
            Response.Cookies.Append("RefreshToken", refreshToken.GiaTriToken, refreshCookie);

            // ---------- Nếu RememberMe thì nhớ email ----------
            if (RememberMe)
            {
                var emailCookie = new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddDays(7),
                    HttpOnly = true,
                    Secure = true,   // nếu chỉ chạy http thì cho false
                    SameSite = SameSiteMode.Lax,
                    Path = "/"
                };
                Response.Cookies.Append("UserEmail", emailNorm, emailCookie);
            }

            return Json(new
            {
                success = true,
                message = "Đăng nhập thành công!",
                redirectUrl = Url.Action("Index", "Home")
            });
        }

        // =========================================================
        // LOGOUT
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Thu hồi tất cả refresh token còn hiệu lực của user hiện tại
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
            {
                var activeTokens = _context.RefreshTokens
                    .Where(t =>
                        t.MaNguoiDung == userId &&
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

            Response.Cookies.Delete("UserEmail");
            Response.Cookies.Delete("AccessToken");
            Response.Cookies.Delete("RefreshToken");

            return RedirectToAction("Index", "Home");
        }

        // =========================================================
        // REGISTER
        // =========================================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public IActionResult Register(NguoiDung model, IFormFile? AnhDaiDien)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(model.HoTen) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.MatKhau) ||
                string.IsNullOrWhiteSpace(model.ConfirmPassword) ||
                string.IsNullOrWhiteSpace(model.SoDienThoai))
            {
                return Json(new
                {
                    success = false,
                    message = "Vui lòng nhập đầy đủ các trường bắt buộc."
                });
            }

            if (model.MatKhau.Length < 6)
            {
                return Json(new
                {
                    success = false,
                    message = "Mật khẩu phải có ít nhất 6 ký tự."
                });
            }

            if (model.MatKhau != model.ConfirmPassword)
            {
                return Json(new
                {
                    success = false,
                    message = "Mật khẩu xác nhận không khớp."
                });
            }

            var emailNorm = model.Email.Trim().ToLowerInvariant();
            var existing = _context.NguoiDungs.FirstOrDefault(x => x.Email == emailNorm);
            if (existing != null)
            {
                return Json(new
                {
                    success = false,
                    message = "Email đã được đăng ký."
                });
            }

            // Lưu ảnh đại diện
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

            // Tạo user mới
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
                MaVaiTro = 2,
                SoDu = 0
            };

            _context.NguoiDungs.Add(newUser);
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                message = "Đăng ký thành công! Mời bạn đăng nhập.",
                redirectUrl = Url.Action("Login", "Account")
            });
        }

        // =========================================================
        // PROFILE
        // =========================================================
        [Authorize]
        public IActionResult Profile()
        {
            if (Request.Cookies.TryGetValue("AccessToken", out var token))
                ViewBag.AccessToken = token;
            else
                ViewBag.AccessToken = "***";

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            NguoiDung? user = null;

            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
            {
                user = _context.NguoiDungs
                    .FirstOrDefault(x => x.MaNguoiDung == userId && x.TrangThai == true);
            }

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.FullName = user.HoTen ?? user.Email;
            ViewBag.Email = user.Email;
            ViewBag.SoDu = user.SoDu ?? 0;
            ViewBag.Phone = user.SoDienThoai ?? "Chưa có số điện thoại";
            ViewBag.NgayTao = (user.NgayTao ?? DateTime.Now).ToString("dd/MM/yyyy HH:mm");
            ViewBag.Avatar = user.AnhDaiDien ?? "default.png";

            return View();
        }

        // =========================================================
        // CHANGE PASSWORD
        // =========================================================
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

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Phiên đăng nhập đã hết hoặc token không hợp lệ. Vui lòng đăng nhập lại.");

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized("Token không hợp lệ.");

            var user = _context.NguoiDungs.FirstOrDefault(x => x.MaNguoiDung == userId && x.TrangThai == true);
            if (user == null) return BadRequest("Không tìm thấy người dùng.");

            if (!string.Equals(user.MatKhau, GetSHA256(dto.CurrentPassword), StringComparison.OrdinalIgnoreCase))
                return BadRequest("Mật khẩu hiện tại không chính xác.");

            if (string.Equals(user.MatKhau, GetSHA256(dto.NewPassword), StringComparison.OrdinalIgnoreCase))
                return BadRequest("Mật khẩu mới không được trùng mật khẩu hiện tại.");

            user.MatKhau = GetSHA256(dto.NewPassword);
            _context.SaveChanges();

            // Thu hồi toàn bộ refresh token còn hiệu lực
            var tokens = _context.RefreshTokens
                .Where(t =>
                    t.MaNguoiDung == userId &&
                    t.ThuHoiLuc == null &&
                    t.ThoiHan > DateTime.UtcNow)
                .ToList();

            foreach (var t in tokens)
            {
                t.ThuHoiLuc = DateTime.UtcNow;
                t.ThuHoiTuIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            }

            _context.SaveChanges();

            // Xoá cookie
            Response.Cookies.Delete("AccessToken");
            Response.Cookies.Delete("RefreshToken");

            return Ok("Đổi mật khẩu thành công. Vui lòng đăng nhập lại.");
        }

        // =========================================================
        // REFRESH TOKEN
        // =========================================================
        [HttpPost]
        [AllowAnonymous]
        public IActionResult RefreshToken()
        {
            if (!Request.Cookies.TryGetValue("RefreshToken", out var refreshTokenValue))
                return Unauthorized("Không có refresh token.");

            var refreshToken = _context.RefreshTokens
                .FirstOrDefault(x => x.GiaTriToken == refreshTokenValue);

            if (refreshToken == null ||
                refreshToken.ThuHoiLuc != null ||
                refreshToken.ThoiHan <= DateTime.UtcNow)
            {
                return Unauthorized("Refresh token không hợp lệ hoặc đã hết hạn.");
            }

            var user = _context.NguoiDungs
                .FirstOrDefault(x => x.MaNguoiDung == refreshToken.MaNguoiDung && x.TrangThai == true);

            if (user == null)
                return Unauthorized("Không tìm thấy người dùng.");

            // Không rotate refresh token
            // Chỉ tạo access token mới
            var newAccessToken = GenerateJwtToken(user);

            double expireMinutes = Convert.ToDouble(_config["Jwt:ExpireMinutes"]);

            Response.Cookies.Append("AccessToken", newAccessToken, new CookieOptions
            {
                Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

            return Json(new
            {
                success = true,
                message = "Đã làm mới AccessToken."
            });
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
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.MaNguoiDung.ToString()),
                    new Claim(ClaimTypes.Name, user.HoTen ?? user.Email),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.MaVaiTroNavigation?.TenVaiTro ?? "user"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("Avatar", user.AnhDaiDien ?? "default.png"),
                    new Claim("SoDu", (user.SoDu ?? 0).ToString()),
                    new Claim(ClaimTypes.MobilePhone, user.SoDienThoai ?? ""),
                    new Claim(ClaimTypes.StreetAddress, user.DiaChi ?? ""),
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

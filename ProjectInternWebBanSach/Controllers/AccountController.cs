using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using ProjectInternWebBanSach.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
        public IActionResult Login()
        {
            string savedEmail = Request.Cookies["UserEmail"];
            string savedPassword = Request.Cookies["UserPassword"];

            ViewBag.SavedEmail = savedEmail;
            ViewBag.SavedPassword = savedPassword;

            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string matkhau, bool RememberMe)
        {
            Response.Cookies.Delete("UserEmail");
            Response.Cookies.Delete("UserPassword");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(matkhau))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            string hashed = GetSHA256(matkhau);

            var user = _context.NguoiDungs
                .FirstOrDefault(u => u.Email == email && u.MatKhau == hashed && u.TrangThai == true);

            if (user == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không chính xác!";
                return View();
            }

            // ---------- Lưu session ----------
            HttpContext.Session.SetInt32("MaNguoiDung", user.MaNguoiDung);
            HttpContext.Session.SetString("HoTen", user.HoTen ?? "");
            HttpContext.Session.SetString("Email", user.Email ?? "");
            HttpContext.Session.SetString("VaiTro", user.MaVaiTroNavigation?.TenVaiTro ?? "user");
            HttpContext.Session.SetString("AnhDaiDien", user.AnhDaiDien ?? "default.png");
            HttpContext.Session.SetString("SoDu", user.SoDu?.ToString("N0") ?? "0");

            // ---------- Sinh JWT Token ----------
            string token = GenerateJwtToken(user);

            // ---------- Nếu RememberMe thì lưu cookie ----------
            if (RememberMe)
            {
                CookieOptions option = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(7),
                    HttpOnly = true
                };
                Response.Cookies.Append("UserEmail", email, option);
                Response.Cookies.Append("UserPassword", matkhau, option);
            }
                return RedirectToAction("Index", "Home");
            //Cho API hoặc test Postman: trả token ra JSON
            /* return Json(new
             {
                 success = true,
                 message = "Đăng nhập thành công!",
                 token = token
             }); */
        }

        // --------------------- LOGOUT ---------------------
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("UserEmail");
            Response.Cookies.Delete("UserPassword");
            return RedirectToAction("Index", "Home");
        }

        // --------------------- REGISTER ---------------------
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(NguoiDung model, IFormFile? AnhDaiDien)
        {
            if (string.IsNullOrEmpty(model.Email) ||
                string.IsNullOrEmpty(model.MatKhau) ||
                string.IsNullOrEmpty(model.ConfirmPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View(model);
            }

            if (model.MatKhau != model.ConfirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View(model);
            }

            var existing = _context.NguoiDungs.FirstOrDefault(x => x.Email == model.Email);
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

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    AnhDaiDien.CopyTo(stream);
                }
            }

            // ---------- Mã hóa mật khẩu ----------
            var newUser = new NguoiDung
            {
                HoTen = model.HoTen,
                Email = model.Email,
                MatKhau = GetSHA256(model.MatKhau),
                AnhDaiDien = fileName,
                SoDienThoai = model.SoDienThoai,
                DiaChi = model.DiaChi,
                NgayTao = DateTime.Now,
                TrangThai = true,
                MaVaiTro = 2,// auto là user
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
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToHexString(hash).ToLower();
            }
        }

        // --------------------- HÀM SINH JWT TOKEN ---------------------
        private string GenerateJwtToken(NguoiDung user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim("UserId", user.MaNguoiDung.ToString()),
                new Claim("Role", user.MaVaiTroNavigation?.TenVaiTro ?? "user"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        //-----------------------HỒ SƠ----------------------
        public IActionResult Profile()
        {
            return View();
        }
    }
}

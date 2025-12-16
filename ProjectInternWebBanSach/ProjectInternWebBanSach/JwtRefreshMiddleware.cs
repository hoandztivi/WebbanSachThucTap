using Microsoft.IdentityModel.Tokens;
using ProjectInternWebBanSach.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProjectInternWebBanSach
{
    public class JwtRefreshMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtRefreshMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            QuanLyBanSachContext db,
            IConfiguration config)
        {
            // Nếu đã authenticated rồi thì thôi
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                await _next(context);
                return;
            }

            // Bỏ qua một số path, không cần refresh
            var path = context.Request.Path;
            if (path.StartsWithSegments("/Account", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWithSegments("/css", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWithSegments("/js", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWithSegments("/images", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // Không có refresh token -> bỏ qua
            if (!context.Request.Cookies.TryGetValue("RefreshToken", out var refreshTokenValue) ||
                string.IsNullOrEmpty(refreshTokenValue))
            {
                await _next(context);
                return;
            }

            // Tìm refresh token trong DB
            var refreshToken = db.RefreshTokens
                .FirstOrDefault(x => x.GiaTriToken == refreshTokenValue);

            if (refreshToken == null ||
                refreshToken.ThuHoiLuc != null ||
                refreshToken.ThoiHan <= DateTime.UtcNow)
            {
                await _next(context);
                return;
            }

            // Lấy user tương ứng
            var user = db.NguoiDungs
                .FirstOrDefault(x => x.MaNguoiDung == refreshToken.MaNguoiDung && x.TrangThai == true);

            if (user == null)
            {
                await _next(context);
                return;
            }

            // ----------------- Rotate refresh token -----------------
            var newRefreshToken = GenerateRefreshToken(user.MaNguoiDung, config, context);

            refreshToken.ThuHoiLuc = DateTime.UtcNow;
            refreshToken.ThuHoiTuIp = context.Connection.RemoteIpAddress?.ToString();
            refreshToken.ThayTheBangToken = newRefreshToken.GiaTriToken;

            db.RefreshTokens.Add(newRefreshToken);

            // ----------------- Tạo AccessToken mới -----------------
            var newAccessToken = GenerateJwtToken(user, config);

            await db.SaveChangesAsync();

            // Ghi lại cookie AccessToken
            double expireMinutes = Convert.ToDouble(config["Jwt:ExpireMinutes"]);
            var accessCookie = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
                HttpOnly = true,
                Secure = true,        // nếu chạy http thì có thể tạm false
                SameSite = SameSiteMode.Lax,
                Path = "/"
            };
            context.Response.Cookies.Append("AccessToken", newAccessToken, accessCookie);

            // Ghi lại cookie RefreshToken
            var refreshCookie = new CookieOptions
            {
                Expires = newRefreshToken.ThoiHan,
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            };
            context.Response.Cookies.Append("RefreshToken", newRefreshToken.GiaTriToken, refreshCookie);

            // Set HttpContext.User để request hiện tại dùng luôn
            var principal = BuildPrincipalFromJwt(newAccessToken, config);
            context.User = principal;

            // Tiếp tục pipeline (Authorize sẽ check context.User đã có claim)
            await _next(context);
        }

        // ================== HÀM PHỤ ==================

        private static string GenerateJwtToken(NguoiDung user, IConfiguration config)
        {
            var jwtSettings = config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.MaNguoiDung.ToString()),
            new Claim(ClaimTypes.Name, user.HoTen ?? user.Email!),
            new Claim(ClaimTypes.Email, user.Email !),
            new Claim(ClaimTypes.Role, user.MaVaiTroNavigation?.TenVaiTro ?? "user"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("Avatar", user.AnhDaiDien ?? "default.png"),
            new Claim("SoDu", (user.SoDu ?? 0).ToString())
        };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(jwtSettings["ExpireMinutes"])
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static RefreshToken GenerateRefreshToken(
            int maNguoiDung,
            IConfiguration config,
            HttpContext context)
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
                    Convert.ToDouble(config["Jwt:RefreshTokenDays"])
                ),
                TaoTuIp = context.Connection.RemoteIpAddress?.ToString()
            };
        }

        private static ClaimsPrincipal BuildPrincipalFromJwt(string jwt, IConfiguration config)
        {
            var jwtSettings = config.GetSection("Jwt");
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                // Ở đây không cần check lifetime vì token vừa tự tay mình tạo
                ValidateLifetime = false
            };

            var principal = tokenHandler.ValidateToken(jwt, parameters, out _);
            return principal;
        }
    }
}

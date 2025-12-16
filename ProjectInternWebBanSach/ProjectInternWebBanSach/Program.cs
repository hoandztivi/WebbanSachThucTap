using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectInternWebBanSach.Models;
using System.Text;
using ProjectInternWebBanSach;

var builder = WebApplication.CreateBuilder(args);

// ---------------------- Add services ----------------------
builder.Services.AddControllersWithViews();

// EF Core
builder.Services.AddDbContext<QuanLyBanSachContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session nếu cần thiết- đag dùng jwt
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ---------------------- JWT Config ----------------------
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // dev môi trường
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };

    //Lấy JWT từ cookie AccessToken
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            if (ctx.Request.Cookies.TryGetValue("AccessToken", out var token))
            {
                ctx.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

// ---------------------- Authorization ----------------------
builder.Services.AddAuthorization();

var app = builder.Build();

// ---------------------- Middleware ----------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session- đag dùng jwt ko dùng tới
app.UseSession();

//Bật Authentication + Authorization
app.UseAuthentication();

//Nếu chưa auth thì thử refresh bằng RefreshToken

app.UseMiddleware<JwtRefreshMiddleware>();

app.UseAuthorization();

// Map API controller (để dùng JWT API)
app.MapControllers();

//Map MVC router cho Area Admin
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Auth}/{action=Login}/{id?}");
// Map MVC routes mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);
app.Run();

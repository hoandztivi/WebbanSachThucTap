using Microsoft.EntityFrameworkCore;
using ProjectInternWebBanSach.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------------------- Add services to the container ----------------------
builder.Services.AddControllersWithViews();

// Thêm DbContext (Entity Framework)
builder.Services.AddDbContext<QuanLyBanSachContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Thêm Session để lưu đăng nhập
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // hết hạn sau 30 phút
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ---------------------- Configure the HTTP request pipeline ----------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

//Cho phép load CSS/JS từ wwwroot
app.UseStaticFiles();

app.UseRouting();

// Bật session (phải trước Authorization)
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

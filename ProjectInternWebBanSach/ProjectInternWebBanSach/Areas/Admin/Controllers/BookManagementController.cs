using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectInternWebBanSach.Models;

namespace ProjectInternWebBanSach.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BookManagementController : Controller
    {
        private readonly QuanLyBanSachContext _ctx;
        private readonly IWebHostEnvironment _env;

        public BookManagementController(QuanLyBanSachContext ctx, IWebHostEnvironment env)
        {
            _ctx = ctx;
            _env = env;
        }

        // ======================= THỂ LOẠI =======================

        [HttpGet]
        public IActionResult CategoryBooks()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _ctx.TheLoaiSaches
                .OrderBy(t => t.TenTheLoai)
                .Select(t => new
                {
                    t.MaTheLoai,
                    t.TenTheLoai,
                    t.MoTa,
                    t.MaTheLoaiCha
                })
                .ToListAsync();

            return Json(new
            {
                success = true,
                data = categories
            });
        }

        public class CategoryCreateDto
        {
            public string TenTheLoai { get; set; } = "";
            public string? MoTa { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDto model)
        {
            if (string.IsNullOrWhiteSpace(model.TenTheLoai))
            {
                return Json(new
                {
                    success = false,
                    message = "Tên thể loại không được để trống."
                });
            }

            var entity = new TheLoaiSach
            {
                TenTheLoai = model.TenTheLoai.Trim(),
                MoTa = model.MoTa?.Trim(),
                MaTheLoaiCha = null
            };

            _ctx.TheLoaiSaches.Add(entity);
            await _ctx.SaveChangesAsync();

            entity.MaTheLoaiCha = entity.MaTheLoai;
            await _ctx.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Thêm thể loại sách thành công.",
                data = new
                {
                    entity.MaTheLoai,
                    entity.TenTheLoai,
                    entity.MoTa,
                    entity.MaTheLoaiCha
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory([FromBody] int id)
        {
            var category = await _ctx.TheLoaiSaches.FindAsync(id);
            if (category == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Không tìm thấy thể loại cần xoá."
                });
            }

            _ctx.TheLoaiSaches.Remove(category);
            await _ctx.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Xoá thể loại sách thành công."
            });
        }

        // ======================= SÁCH – VIEW =====================

        [HttpGet]
        public async Task<IActionResult> ListofBooks()
        {
            var categories = await _ctx.TheLoaiSaches
                .OrderBy(t => t.TenTheLoai)
                .ToListAsync();

            ViewBag.Categories = categories;
            return View();
        }

        // ======================= SÁCH – LẤY DANH SÁCH =====

        [HttpGet]
        public async Task<IActionResult> GetBooks()
        {
            var books = await _ctx.Saches
                .Include(s => s.MaTheLoaiNavigation)
                .OrderByDescending(s => s.NgayTao)
                .Select(s => new
                {
                    s.MaSach,
                    s.TieuDe,
                    s.TacGia,
                    s.NhaXuatBan,
                    s.Gia,
                    s.GiamGia,
                    s.SoLuong,
                    s.HinhAnh,
                    s.MoTa,
                    s.NgayTao,
                    TenTheLoai = s.MaTheLoaiNavigation != null
                        ? s.MaTheLoaiNavigation.TenTheLoai
                        : null
                })
                .ToListAsync();

            return Json(new
            {
                success = true,
                data = books
            });
        }

        // ======================= SÁCH – THÊM MỚI=========

        public class CreateBookDto
        {
            public string? TieuDe { get; set; }
            public string? TacGia { get; set; }
            public string? NhaXuatBan { get; set; }
            public decimal Gia { get; set; }
            public decimal? GiamGia { get; set; }
            public int SoLuong { get; set; }
            public string? HinhAnh { get; set; }
            public string? MoTa { get; set; }
            public int MaTheLoai { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] CreateBookDto model)
        {
            if (string.IsNullOrWhiteSpace(model.TieuDe))
            {
                return Json(new { success = false, message = "Tiêu đề sách không được để trống." });
            }

            if (model.Gia <= 0)
            {
                return Json(new { success = false, message = "Giá sách phải lớn hơn 0." });
            }

            if (model.SoLuong < 0)
            {
                return Json(new { success = false, message = "Số lượng không hợp lệ." });
            }

            var category = await _ctx.TheLoaiSaches.FindAsync(model.MaTheLoai);
            if (category == null)
            {
                return Json(new { success = false, message = "Thể loại không tồn tại." });
            }

            var book = new Sach
            {
                TieuDe = model.TieuDe.Trim(),
                TacGia = model.TacGia?.Trim(),
                NhaXuatBan = model.NhaXuatBan?.Trim(),
                Gia = model.Gia,
                GiamGia = model.GiamGia ?? 0,
                SoLuong = model.SoLuong,
                HinhAnh = string.IsNullOrWhiteSpace(model.HinhAnh) ? null : model.HinhAnh.Trim(),
                MoTa = string.IsNullOrWhiteSpace(model.MoTa) ? null : model.MoTa.Trim(),
                MaTheLoai = model.MaTheLoai,
                NgayTao = DateTime.Now
            };

            _ctx.Saches.Add(book);
            await _ctx.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Thêm sách mới thành công.",
                data = new
                {
                    book.MaSach,
                    book.TieuDe,
                    book.TacGia,
                    book.NhaXuatBan,
                    book.Gia,
                    book.GiamGia,
                    book.SoLuong,
                    book.HinhAnh,
                    book.MoTa,
                    book.NgayTao,
                    TenTheLoai = category.TenTheLoai
                }
            });
        }

        // ======================= SÁCH – XOÁ==============

        [HttpPost]
        public async Task<IActionResult> DeleteBook([FromBody] int id)
        {
            var book = await _ctx.Saches.FindAsync(id);
            if (book == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Không tìm thấy sách cần xoá."
                });
            }

            // giữ lại path ảnh để xóa
            var imagePath = book.HinhAnh;

            _ctx.Saches.Remove(book);

            try
            {
                await _ctx.SaveChangesAsync();

                // ===== XÓA FILE ẢNH TRONG wwwroot/img/sach (NẾU CÓ) =====
                if (!string.IsNullOrWhiteSpace(imagePath))
                {
                    var relative = imagePath.TrimStart('/');
                    var fullPath = Path.Combine(_env.WebRootPath,
                                                relative.Replace("/", Path.DirectorySeparatorChar.ToString()));

                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }

                return Json(new
                {
                    success = true,
                    message = "Xoá sách thành công."
                });
            }
            catch (DbUpdateException)
            {
                return Json(new
                {
                    success = false,
                    message = "Không thể xoá sách vì đang có dữ liệu liên quan."
                });
            }
        }


        // ======================= UPLOAD ẢNH SÁCH =================

        [HttpPost]
        public async Task<IActionResult> UploadBookImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Không có file ảnh được chọn."
                });
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "img", "sach");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Đường dẫn lưu trong DB / hiển thị
            var urlPath = $"/img/sach/{fileName}";

            return Json(new
            {
                success = true,
                message = "Tải ảnh lên thành công.",
                path = urlPath
            });
        }
    }
}

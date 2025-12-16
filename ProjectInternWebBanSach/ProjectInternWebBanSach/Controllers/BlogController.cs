using Microsoft.AspNetCore.Mvc;
using ProjectInternWebBanSach.Models;
using System.Linq;

namespace ProjectInternWebBanSach.Controllers
{
    public class BlogController : Controller
    {
        private readonly QuanLyBanSachContext _ctx;

        public BlogController(QuanLyBanSachContext ctx)
        {
            _ctx = ctx;
        }

        // Trang danh sách bài viết
        public IActionResult BaiViet()
        {
            var posts = _ctx.BaiViets
                .OrderByDescending(x => x.NgayTao)
                .ToList();

            return View(posts);
        }

        // Chi tiết bài viết
        public IActionResult ChiTiet(int id)
        {
            var post = _ctx.BaiViets.Find(id);

            if (post == null) return NotFound();

            return View(post);
        }
    }
}

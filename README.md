I. Giới thiệu tổng quan
Tên đề tài: Thiết kế và xây dựng website thương mại điện tử bán sách trên nền tảng ASP.NET MVC
Mục tiêu:
Xây dựng hệ thống web giúp khách hàng mua sách trực tuyến dễ dàng và giúp quản trị viên quản lý toàn bộ hoạt động (sách, đơn hàng, doanh thu, người dùng) hiệu quả.
II. Chức năng hệ thống

1. Chức năng người dùng (Khách hàng)
   STT Chức năng Mô tả
   1 Đăng ký / Đăng nhập / Đăng xuất Người dùng tạo tài khoản, đăng nhập bằng email & mật khẩu, đăng xuất khi cần.
   2 Lưu cookie đăng nhập (timeout 15 phút) Tự động đăng xuất nếu người dùng không hoạt động trong 15 phút.
   3 Lưu thiết bị đăng nhập (Device Tracking) Hệ thống ghi lại thông tin thiết bị, trình duyệt, IP. Có thể đăng xuất thiết bị cũ.
   4 Quên mật khẩu (OTP) Gửi mã OTP qua email để đặt lại mật khẩu.
   5 Xem danh mục và chi tiết sách Xem danh mục theo thể loại, xem chi tiết thông tin sách.
   6 Tìm kiếm và lọc sách Tìm theo tên, tác giả, thể loại, giá tiền.
   7 Giỏ hàng Thêm sách vào giỏ, sửa số lượng, xóa sách khỏi giỏ.
   8 Thanh toán Nhập địa chỉ giao hàng, chọn phương thức thanh toán, áp dụng mã giảm giá.
   9 Nạp tiền vào tài khoản (API VietQR) Nạp tiền trực tiếp vào tài khoản thông qua mã QR động của VietQR.
   10 Sách yêu thích Lưu sách yêu thích để xem lại sau.
   11 Đánh giá & bình luận sách Gửi đánh giá (1–5 sao), bình luận, trả lời bình luận khác.
   12 Lịch sử mua hàng Xem các đơn hàng đã đặt, trạng thái giao hàng, tổng tiền.
   13 Nạp tiền và quản lý số dư Hiển thị số dư tài khoản (SoDu), lịch sử nạp tiền.
   14 Xem tin tức / bài viết Đọc các bài viết giới thiệu, blog, thông tin khuyến mãi.

2. Chức năng quản trị (Admin)
   STT Chức năng Mô tả
   1 Đăng nhập quản trị viên Quản lý website với quyền truy cập riêng.
   2 Quản lý người dùng Thêm, sửa, khóa, xóa tài khoản.
   3 Quản lý danh mục thể loại Thêm, sửa, xóa thể loại sách (cha – con).
   4 Quản lý sách CRUD đầy đủ: thêm mới, cập nhật, xóa, tìm kiếm.
   5 Quản lý đơn hàng Xác nhận, hủy, cập nhật trạng thái đơn hàng.
   6 Quản lý mã giảm giá Tạo và áp dụng mã giảm giá cho người dùng.
   7 Quản lý bài viết / banner Cập nhật bài viết, banner quảng cáo, khuyến mãi.
   8 Quản lý đánh giá / bình luận Kiểm duyệt, xóa bình luận không hợp lệ.
   9 Thống kê & báo cáo Xem doanh thu, số lượng đơn hàng, top sách bán chạy.
   10 Cấu hình website Chỉnh thông tin liên hệ, logo, hotline, mạng xã hội.

3. Chức năng Nhân Viên (Staff)
   STT Chức năng Mô tả
   1 Duyệt đơn hàng Kiểm duyệt đơn hàng khi người dùng đặt đơn

III. Các trang giao diện chính
Trang Mô tả
Trang chủ (Home) Banner, danh mục sách nổi bật, tin tức mới.
Trang danh mục sách Liệt kê sách theo thể loại, có lọc và phân trang.
Trang chi tiết sách Thông tin sách, hình ảnh, đánh giá, gợi ý sách liên quan.
Trang giỏ hàng Danh sách sách đã chọn, cập nhật số lượng, tổng tiền.
Trang thanh toán Thông tin người mua, địa chỉ giao hàng, mã giảm giá.
Trang nạp tiền VietQR Sinh mã QR để nạp tiền, xem lịch sử giao dịch.
Trang đánh giá & bình luận Hiển thị bình luận, cho phép phản hồi.
Trang bài viết / blog Hiển thị tin tức, giới thiệu sách.
Trang cấu hình website (Admin) Quản trị banner, bài viết, thông tin liên hệ.
Trang quản trị & thống kê Dashboard hiển thị doanh thu, biểu đồ doanh số, sách bán chạy.

IV. Thuật toán và xử lý kỹ thuật
Tình huống Thuật toán / Xử lý
Đăng nhập người dùng So khớp email & mật khẩu (mã hóa SHA256), tạo cookie đăng nhập, timeout sau 15 phút.
JWT Token (xác thực nâng cao) Khi mở rộng API, mỗi người dùng nhận 1 JWT token có userId, exp, signature .
Ghi nhớ thiết bị đăng nhập Sinh mã Token (GUID), lưu vào bảng LichSuDangNhap và cookie DeviceToken.
Nạp tiền VietQR Gọi API https://api.vietqr.io/v2/generate → tạo mã QR động → xác minh thanh toán → cập nhật SoDu.
Tìm kiếm sách Tìm theo tên hoặc mô tả (LINQ .Contains()
Phân trang LINQ .Skip((page-1)_pageSize).Take(pageSize)
Tính tổng tiền giỏ hàng Duyệt danh sách: Sum(SoLuong _ DonGia)
Thống kê doanh thu SQL SELECT SUM(TongTien), MONTH(NgayDat) FROM DonHang GROUP BY MONTH(NgayDat)
Đánh giá sách Trung bình cộng điểm đánh giá trong DanhGiaSach.
Sinh mã giảm giá Guid.NewGuid().ToString().Substring(0,8).ToUpper()
Bảo mật session Cookie timeout 15 phút, SlidingExpiration=true.
Lưu file ảnh Kiểm tra định dạng (.jpg, .png), lưu vào thư mục /Uploads/Books/.
Dữ liệu xử lý : Database first Scaffold-DbContext "Data Source=Hoandeptrai;Initial Catalog=QuanLyBanSach;Integrated Security=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models

V. Công nghệ và công cụ sử dụng
Hạng mục Công nghệ / Công cụ
Ngôn ngữ lập trình C#
Framework ASP.NET MVC 5, NET 9.0
Cơ sở dữ liệu Microsoft SQL Server 2019
ORM / Data Layer Entity framework
Front-end HTML5, CSS3, TailwindCSS, JavaScript,
Thư viện hỗ trợ TailwindCSS , Entity
API tích hợp VietQR API (nạp tiền), Gmail SMTP (OTP reset password)
Bảo mật Forms Authentication, JWT Token, Cookie Timeout, Hash SHA256
IDE phát triển Visual Studio 2022, SQL Server Management Studio
Máy chủ triển khai IIS (Internet Information Services)
Hệ điều hành Windows 10 / Server 2019
Công cụ kiểm thử Postman, Selenium, Unit Test của Visual Studio

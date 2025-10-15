using System;
using System.Collections.Generic;

namespace ProjectInternWebBanSach.Models;

public partial class NguoiDung
{
    public int MaNguoiDung { get; set; }

    public string? HoTen { get; set; }

    public string? Email { get; set; }

    public string? MatKhau { get; set; }

    public string? AnhDaiDien { get; set; }

    public string? SoDienThoai { get; set; }

    public string? DiaChi { get; set; }

    public int? MaVaiTro { get; set; }

    public DateTime? NgayTao { get; set; }

    public bool? TrangThai { get; set; }

    public decimal? SoDu { get; set; }

    public virtual ICollection<DanhGiaSach> DanhGiaSaches { get; set; } = new List<DanhGiaSach>();

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();

    public virtual ICollection<LichSuDangNhap> LichSuDangNhaps { get; set; } = new List<LichSuDangNhap>();

    public virtual ICollection<LichSuNapTien> LichSuNapTiens { get; set; } = new List<LichSuNapTien>();

    public virtual VaiTro? MaVaiTroNavigation { get; set; }

    public virtual ICollection<SachYeuThich> SachYeuThiches { get; set; } = new List<SachYeuThich>();

    public virtual ICollection<TraLoiDanhGium> TraLoiDanhGia { get; set; } = new List<TraLoiDanhGium>();



    // lưu coookie
    public bool RememberMe { get; set; }
}

using System;
using System.Collections.Generic;

namespace ProjectInternWebBanSach.Models;

public partial class Sach
{
    public int MaSach { get; set; }

    public string? TieuDe { get; set; }

    public string? TacGia { get; set; }

    public string? NhaXuatBan { get; set; }

    public decimal? Gia { get; set; }

    public decimal? GiamGia { get; set; }

    public int? SoLuong { get; set; }

    public string? HinhAnh { get; set; }

    public string? MoTa { get; set; }

    public int? MaTheLoai { get; set; }

    public DateTime? NgayTao { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();

    public virtual ICollection<DanhGiaSach> DanhGiaSaches { get; set; } = new List<DanhGiaSach>();

    public virtual TheLoaiSach? MaTheLoaiNavigation { get; set; }

    public virtual ICollection<SachYeuThich> SachYeuThiches { get; set; } = new List<SachYeuThich>();
}

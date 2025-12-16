using System;
using System.Collections.Generic;

namespace ProjectInternWebBanSach.Models;

public partial class DonHang
{
    public int MaDonHang { get; set; }

    public int? MaNguoiDung { get; set; }

    public DateTime? NgayDat { get; set; }

    public decimal? TongTien { get; set; }

    public string? DiaChiGiao { get; set; }

    public string? TrangThai { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual NguoiDung? MaNguoiDungNavigation { get; set; }
}

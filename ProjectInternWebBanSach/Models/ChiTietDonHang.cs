using System;
using System.Collections.Generic;

namespace ProjectInternWebBanSach.Models;

public partial class ChiTietDonHang
{
    public int MaChiTiet { get; set; }

    public int? MaDonHang { get; set; }

    public int? MaSach { get; set; }

    public int? SoLuong { get; set; }

    public decimal? DonGia { get; set; }

    public virtual DonHang? MaDonHangNavigation { get; set; }

    public virtual Sach? MaSachNavigation { get; set; }
}

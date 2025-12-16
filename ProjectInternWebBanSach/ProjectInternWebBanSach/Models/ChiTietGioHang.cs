using System;
using System.Collections.Generic;

namespace ProjectInternWebBanSach.Models;

public partial class ChiTietGioHang
{
    public int MaChiTiet { get; set; }

    public int? MaGioHang { get; set; }

    public int? MaSach { get; set; }

    public int? SoLuong { get; set; }

    public decimal? DonGia { get; set; }

    public virtual GioHang? MaGioHangNavigation { get; set; }

    public virtual Sach? MaSachNavigation { get; set; }
}

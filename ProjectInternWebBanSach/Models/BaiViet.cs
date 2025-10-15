using System;
using System.Collections.Generic;

namespace ProjectInternWebBanSach.Models;

public partial class BaiViet
{
    public int MaBaiViet { get; set; }

    public string? TieuDe { get; set; }

    public string? NoiDung { get; set; }

    public string? HinhAnh { get; set; }

    public DateTime? NgayTao { get; set; }

    public string? TacGia { get; set; }

    public bool? NoiBat { get; set; }
}

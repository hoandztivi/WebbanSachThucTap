using System;
using System.Collections.Generic;

namespace ProjectInternWebBanSach.Models;

public partial class MaGiamGium
{
    public int MaGiamGia { get; set; }

    public string? MaCode { get; set; }

    public string? MoTa { get; set; }

    public decimal? GiaTriGiam { get; set; }

    public DateTime? NgayBatDau { get; set; }

    public DateTime? NgayKetThuc { get; set; }

    public bool? DangHoatDong { get; set; }
}

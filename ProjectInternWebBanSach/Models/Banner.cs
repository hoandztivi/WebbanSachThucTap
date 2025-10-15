using System;
using System.Collections.Generic;

namespace ProjectInternWebBanSach.Models;

public partial class Banner
{
    public int MaBanner { get; set; }

    public string? TieuDe { get; set; }

    public string? HinhAnh { get; set; }

    public string? MoTa { get; set; }

    public string? LienKet { get; set; }

    public DateTime? NgayTao { get; set; }

    public bool? TrangThai { get; set; }
}

using System;
using System.Collections.Generic;

namespace ProjectInternWebBanSach.Models;

public partial class LichSuNapTien
{
    public int MaNapTien { get; set; }

    public int MaNguoiDung { get; set; }

    public decimal SoTien { get; set; }

    public string? NoiDung { get; set; }

    public string? TrangThai { get; set; }

    public string? MaGiaoDich { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;
}

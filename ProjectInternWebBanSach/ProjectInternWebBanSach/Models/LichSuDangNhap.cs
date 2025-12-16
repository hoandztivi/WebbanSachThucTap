using System;
using System.Collections.Generic;

namespace ProjectInternWebBanSach.Models;

public partial class LichSuDangNhap
{
    public int MaDangNhap { get; set; }

    public int MaNguoiDung { get; set; }

    public string? ThietBi { get; set; }

    public string? TrinhDuyet { get; set; }

    public string? DiaChiIp { get; set; }

    public string? Token { get; set; }

    public DateTime? NgayDangNhap { get; set; }

    public string? TrangThai { get; set; }

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;
}

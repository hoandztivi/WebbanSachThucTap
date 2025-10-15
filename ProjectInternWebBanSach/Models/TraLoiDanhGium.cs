using System;
using System.Collections.Generic;

namespace ProjectInternWebBanSach.Models;

public partial class TraLoiDanhGium
{
    public int MaTraLoi { get; set; }

    public int? MaDanhGia { get; set; }

    public int? MaNguoiDung { get; set; }

    public string? NoiDung { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual DanhGiaSach? MaDanhGiaNavigation { get; set; }

    public virtual NguoiDung? MaNguoiDungNavigation { get; set; }
}

using System;
using System.Collections.Generic;

namespace ProjectInternWebBanSach.Models;

public partial class DanhGiaSach
{
    public int MaDanhGia { get; set; }

    public int? MaSach { get; set; }

    public int? MaNguoiDung { get; set; }

    public string? NoiDung { get; set; }

    public int? Diem { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual NguoiDung? MaNguoiDungNavigation { get; set; }

    public virtual Sach? MaSachNavigation { get; set; }

    public virtual ICollection<TraLoiDanhGium> TraLoiDanhGia { get; set; } = new List<TraLoiDanhGium>();
}

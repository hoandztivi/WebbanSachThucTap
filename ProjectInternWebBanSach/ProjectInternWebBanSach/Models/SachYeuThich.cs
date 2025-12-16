using System;
using System.Collections.Generic;

namespace ProjectInternWebBanSach.Models;

public partial class SachYeuThich
{
    public int MaYeuThich { get; set; }

    public int? MaNguoiDung { get; set; }

    public int? MaSach { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual NguoiDung? MaNguoiDungNavigation { get; set; }

    public virtual Sach? MaSachNavigation { get; set; }
}

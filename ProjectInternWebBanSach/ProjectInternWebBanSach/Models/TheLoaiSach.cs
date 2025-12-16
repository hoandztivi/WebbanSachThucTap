using System;
using System.Collections.Generic;

namespace ProjectInternWebBanSach.Models;

public partial class TheLoaiSach
{
    public int MaTheLoai { get; set; }

    public string? TenTheLoai { get; set; }

    public string? MoTa { get; set; }

    public int? MaTheLoaiCha { get; set; }

    public virtual ICollection<TheLoaiSach> InverseMaTheLoaiChaNavigation { get; set; } = new List<TheLoaiSach>();

    public virtual TheLoaiSach? MaTheLoaiChaNavigation { get; set; }

    public virtual ICollection<Sach> Saches { get; set; } = new List<Sach>();
}

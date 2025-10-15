using System;
using System.Collections.Generic;

namespace ProjectInternWebBanSach.Models;

public partial class QuenMatKhau
{
    public string? Email { get; set; }

    public string? MaOtp { get; set; }

    public DateTime? NgayTao { get; set; }
}

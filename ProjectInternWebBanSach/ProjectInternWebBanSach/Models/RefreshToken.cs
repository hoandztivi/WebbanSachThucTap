namespace ProjectInternWebBanSach.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public int MaNguoiDung { get; set; }
        public string GiaTriToken { get; set; } = null!;

        public DateTime ThoiHan { get; set; }
        public DateTime NgayTao { get; set; }
        public string? TaoTuIp { get; set; }

        public DateTime? ThuHoiLuc { get; set; }
        public string? ThuHoiTuIp { get; set; }
        public string? ThayTheBangToken { get; set; }

        public bool DaHetHan => DateTime.UtcNow >= ThoiHan;
        public bool ConHieuLuc => ThuHoiLuc == null && !DaHetHan;

        public virtual NguoiDung NguoiDung { get; set; } = null!;
    }
}

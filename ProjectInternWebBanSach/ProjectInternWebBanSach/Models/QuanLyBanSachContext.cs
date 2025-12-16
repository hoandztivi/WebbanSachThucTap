using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProjectInternWebBanSach.Models
{
    public partial class QuanLyBanSachContext : DbContext
    {
        public QuanLyBanSachContext()
        {
        }

        public QuanLyBanSachContext(DbContextOptions<QuanLyBanSachContext> options)
            : base(options)
        {
        }

        public virtual DbSet<BaiViet> BaiViets { get; set; }

        public virtual DbSet<Banner> Banners { get; set; }

        public virtual DbSet<CauHinhWebsite> CauHinhWebsites { get; set; }

        public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

        public virtual DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }

        public virtual DbSet<DanhGiaSach> DanhGiaSaches { get; set; }

        public virtual DbSet<DonHang> DonHangs { get; set; }

        public virtual DbSet<GioHang> GioHangs { get; set; }

        public virtual DbSet<LichSuDangNhap> LichSuDangNhaps { get; set; }

        public virtual DbSet<LichSuNapTien> LichSuNapTiens { get; set; }

        public virtual DbSet<MaGiamGium> MaGiamGia { get; set; }

        public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

        public virtual DbSet<QuenMatKhau> QuenMatKhaus { get; set; }

        public virtual DbSet<Sach> Saches { get; set; }

        public virtual DbSet<SachYeuThich> SachYeuThiches { get; set; }

        public virtual DbSet<TheLoaiSach> TheLoaiSaches { get; set; }

        public virtual DbSet<TraLoiDanhGium> TraLoiDanhGia { get; set; }

        public virtual DbSet<VaiTro> VaiTros { get; set; }

        public virtual DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
            => optionsBuilder.UseSqlServer("Data Source=Hoandeptrai;Initial Catalog=QuanLyBanSach;Integrated Security=True;");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BaiViet>(entity =>
            {
                entity.HasKey(e => e.MaBaiViet).HasName("PK__BaiViet__AEDD5647A7D837D1");

                entity.ToTable("BaiViet");

                entity.Property(e => e.HinhAnh).HasMaxLength(255);
                entity.Property(e => e.NgayTao)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.NoiBat).HasDefaultValue(false);
                entity.Property(e => e.TacGia).HasMaxLength(100);
                entity.Property(e => e.TieuDe).HasMaxLength(200);
            });

            modelBuilder.Entity<Banner>(entity =>
            {
                entity.HasKey(e => e.MaBanner).HasName("PK__Banner__508B4A49D325DEBA");

                entity.ToTable("Banner");

                entity.Property(e => e.HinhAnh).HasMaxLength(255);
                entity.Property(e => e.LienKet).HasMaxLength(255);
                entity.Property(e => e.MoTa).HasMaxLength(255);
                entity.Property(e => e.NgayTao)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.TieuDe).HasMaxLength(100);
                entity.Property(e => e.TrangThai).HasDefaultValue(true);
            });

            modelBuilder.Entity<CauHinhWebsite>(entity =>
            {
                entity.HasKey(e => e.MaCauHinh).HasName("PK__CauHinhW__F0685B7D365ACE13");

                entity.ToTable("CauHinhWebsite");

                entity.Property(e => e.DiaChi).HasMaxLength(255);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Facebook).HasMaxLength(255);
                entity.Property(e => e.Instagram).HasMaxLength(255);
                entity.Property(e => e.Logo).HasMaxLength(255);
                entity.Property(e => e.SoDienThoai).HasMaxLength(20);
                entity.Property(e => e.TenWebsite).HasMaxLength(100);
                entity.Property(e => e.Zalo).HasMaxLength(255);
            });

            // ========= CHI TIẾT ĐƠN HÀNG =========
            modelBuilder.Entity<ChiTietDonHang>(entity =>
            {
                entity.HasKey(e => e.MaChiTiet).HasName("PK__ChiTietD__CDF0A11437F41DFD");

                entity.ToTable("ChiTietDonHang");

                entity.Property(e => e.DonGia).HasColumnType("decimal(10, 2)");

                // MỚI THÊM
                entity.Property(e => e.GhiChu)
                      .HasMaxLength(255);

                entity.Property(e => e.ThanhToan)
                      .HasMaxLength(50)
                      .HasDefaultValue("Thanh toán khi nhận hàng");

                entity.HasOne(d => d.MaDonHangNavigation).WithMany(p => p.ChiTietDonHangs)
                    .HasForeignKey(d => d.MaDonHang)
                    .HasConstraintName("FK_ChiTietDonHang_DonHang");

                entity.HasOne(d => d.MaSachNavigation).WithMany(p => p.ChiTietDonHangs)
                    .HasForeignKey(d => d.MaSach)
                    .HasConstraintName("FK_ChiTietDonHang_Sach");
            });

            modelBuilder.Entity<ChiTietGioHang>(entity =>
            {
                entity.HasKey(e => e.MaChiTiet).HasName("PK__ChiTietG__CDF0A114747187A8");

                entity.ToTable("ChiTietGioHang");

                entity.Property(e => e.DonGia).HasColumnType("decimal(10, 2)");

                entity.HasOne(d => d.MaGioHangNavigation).WithMany(p => p.ChiTietGioHangs)
                    .HasForeignKey(d => d.MaGioHang)
                    .HasConstraintName("FK__ChiTietGi__MaGio__5812160E");

                entity.HasOne(d => d.MaSachNavigation).WithMany(p => p.ChiTietGioHangs)
                    .HasForeignKey(d => d.MaSach)
                    .HasConstraintName("FK__ChiTietGi__MaSac__59063A47");
            });

            modelBuilder.Entity<DanhGiaSach>(entity =>
            {
                entity.HasKey(e => e.MaDanhGia).HasName("PK__DanhGiaS__AA9515BF5034E594");

                entity.ToTable("DanhGiaSach");

                entity.Property(e => e.NgayTao)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.DanhGiaSaches)
                    .HasForeignKey(d => d.MaNguoiDung)
                    .HasConstraintName("FK__DanhGiaSa__MaNgu__4AB81AF0");

                entity.HasOne(d => d.MaSachNavigation).WithMany(p => p.DanhGiaSaches)
                    .HasForeignKey(d => d.MaSach)
                    .HasConstraintName("FK__DanhGiaSa__MaSac__49C3F6B7");
            });

            modelBuilder.Entity<DonHang>(entity =>
            {
                entity.HasKey(e => e.MaDonHang).HasName("PK__DonHang__129584ADFF92D690");

                entity.ToTable("DonHang");

                entity.Property(e => e.DiaChiGiao).HasMaxLength(255);
                entity.Property(e => e.NgayDat)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.TongTien).HasColumnType("decimal(12, 2)");
                entity.Property(e => e.TrangThai)
                    .HasMaxLength(50)
                    .HasDefaultValue("Chờ xác nhận");

                entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.DonHangs)
                    .HasForeignKey(d => d.MaNguoiDung)
                    .HasConstraintName("FK__DonHang__MaNguoi__5BE2A6F2");
            });

            modelBuilder.Entity<GioHang>(entity =>
            {
                entity.HasKey(e => e.MaGioHang).HasName("PK__GioHang__F5001DA3262C518E");

                entity.ToTable("GioHang");

                entity.Property(e => e.NgayTao)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.GioHangs)
                    .HasForeignKey(d => d.MaNguoiDung)
                    .HasConstraintName("FK__GioHang__MaNguoi__5441852A");
            });

            modelBuilder.Entity<LichSuDangNhap>(entity =>
            {
                entity.HasKey(e => e.MaDangNhap).HasName("PK__LichSuDa__C869B8C0B3BF7040");

                entity.ToTable("LichSuDangNhap");

                entity.Property(e => e.DiaChiIp)
                    .HasMaxLength(50)
                    .HasColumnName("DiaChiIP");
                entity.Property(e => e.NgayDangNhap)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.ThietBi).HasMaxLength(100);
                entity.Property(e => e.Token).HasMaxLength(200);
                entity.Property(e => e.TrangThai)
                    .HasMaxLength(50)
                    .HasDefaultValue("Hoạt động");
                entity.Property(e => e.TrinhDuyet).HasMaxLength(100);

                entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.LichSuDangNhaps)
                    .HasForeignKey(d => d.MaNguoiDung)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LichSuDangNhap_NguoiDung");
            });

            modelBuilder.Entity<LichSuNapTien>(entity =>
            {
                entity.HasKey(e => e.MaNapTien).HasName("PK__LichSuNa__86747C765C8A2476");

                entity.ToTable("LichSuNapTien");

                entity.Property(e => e.MaGiaoDich).HasMaxLength(100);
                entity.Property(e => e.NgayTao)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.NoiDung).HasMaxLength(255);
                entity.Property(e => e.SoTien).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.TrangThai)
                    .HasMaxLength(50)
                    .HasDefaultValue("Chờ xác nhận");

                entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.LichSuNapTiens)
                    .HasForeignKey(d => d.MaNguoiDung)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LichSuNapTien_NguoiDung");
            });

            modelBuilder.Entity<MaGiamGium>(entity =>
            {
                entity.HasKey(e => e.MaGiamGia).HasName("PK__MaGiamGi__EF9458E472837EA0");

                entity.Property(e => e.DangHoatDong).HasDefaultValue(true);
                entity.Property(e => e.GiaTriGiam).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.MaCode).HasMaxLength(50);
                entity.Property(e => e.MoTa).HasMaxLength(255);
                entity.Property(e => e.NgayBatDau).HasColumnType("datetime");
                entity.Property(e => e.NgayKetThuc).HasColumnType("datetime");
            });

            modelBuilder.Entity<NguoiDung>(entity =>
            {
                entity.HasKey(e => e.MaNguoiDung).HasName("PK__NguoiDun__C539D762A5F7BEB2");

                entity.ToTable("NguoiDung");

                entity.HasIndex(e => e.Email, "UQ__NguoiDun__A9D10534A0C540B8").IsUnique();

                entity.Property(e => e.AnhDaiDien).HasMaxLength(255);
                entity.Property(e => e.DiaChi).HasMaxLength(255);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.HoTen).HasMaxLength(100);
                entity.Property(e => e.MatKhau).HasMaxLength(255);
                entity.Property(e => e.NgayTao)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.SoDienThoai).HasMaxLength(15);
                entity.Property(e => e.SoDu)
                    .HasDefaultValue(0m)
                    .HasColumnType("decimal(18, 2)");
                entity.Property(e => e.TrangThai).HasDefaultValue(true);

                entity.HasOne(d => d.MaVaiTroNavigation).WithMany(p => p.NguoiDungs)
                    .HasForeignKey(d => d.MaVaiTro)
                    .HasConstraintName("FK__NguoiDung__MaVai__3C69FB99");
            });

            modelBuilder.Entity<QuenMatKhau>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToTable("QuenMatKhau");

                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.MaOtp)
                    .HasMaxLength(10)
                    .HasColumnName("MaOTP");
                entity.Property(e => e.NgayTao)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<Sach>(entity =>
            {
                entity.HasKey(e => e.MaSach).HasName("PK__Sach__B235742D751F0620");

                entity.ToTable("Sach");

                entity.Property(e => e.Gia).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.GiamGia).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.HinhAnh).HasMaxLength(255);
                entity.Property(e => e.NgayCapNhat).HasColumnType("datetime");
                entity.Property(e => e.NgayTao)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.NhaXuatBan).HasMaxLength(100);
                entity.Property(e => e.TacGia).HasMaxLength(100);
                entity.Property(e => e.TieuDe).HasMaxLength(200);

                entity.HasOne(d => d.MaTheLoaiNavigation).WithMany(p => p.Saches)
                    .HasForeignKey(d => d.MaTheLoai)
                    .HasConstraintName("FK__Sach__MaTheLoai__45F365D3");
            });

            modelBuilder.Entity<SachYeuThich>(entity =>
            {
                entity.HasKey(e => e.MaYeuThich).HasName("PK__SachYeuT__B9007E4C4822790D");

                entity.ToTable("SachYeuThich");

                entity.Property(e => e.NgayTao)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.SachYeuThiches)
                    .HasForeignKey(d => d.MaNguoiDung)
                    .HasConstraintName("FK__SachYeuTh__MaNgu__6EF57B66");

                entity.HasOne(d => d.MaSachNavigation).WithMany(p => p.SachYeuThiches)
                    .HasForeignKey(d => d.MaSach)
                    .HasConstraintName("FK__SachYeuTh__MaSac__6FE99F9F");
            });

            modelBuilder.Entity<TheLoaiSach>(entity =>
            {
                entity.HasKey(e => e.MaTheLoai).HasName("PK__TheLoaiS__D73FF34A2EE257C2");

                entity.ToTable("TheLoaiSach");

                entity.Property(e => e.MoTa).HasMaxLength(255);
                entity.Property(e => e.TenTheLoai).HasMaxLength(100);

                entity.HasOne(d => d.MaTheLoaiChaNavigation).WithMany(p => p.InverseMaTheLoaiChaNavigation)
                    .HasForeignKey(d => d.MaTheLoaiCha)
                    .HasConstraintName("FK__TheLoaiSa__MaThe__4316F928");
            });

            modelBuilder.Entity<TraLoiDanhGium>(entity =>
            {
                entity.HasKey(e => e.MaTraLoi).HasName("PK__TraLoiDa__33F7A78D4B7709D4");

                entity.Property(e => e.NgayTao)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.MaDanhGiaNavigation).WithMany(p => p.TraLoiDanhGia)
                    .HasForeignKey(d => d.MaDanhGia)
                    .HasConstraintName("FK__TraLoiDan__MaDan__4F7CD00D");

                entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.TraLoiDanhGia)
                    .HasForeignKey(d => d.MaNguoiDung)
                    .HasConstraintName("FK__TraLoiDan__MaNgu__5070F446");
            });

            modelBuilder.Entity<VaiTro>(entity =>
            {
                entity.HasKey(e => e.MaVaiTro).HasName("PK__VaiTro__C24C41CFBE4497F6");

                entity.ToTable("VaiTro");

                entity.Property(e => e.MoTa).HasMaxLength(255);
                entity.Property(e => e.TenVaiTro).HasMaxLength(50);
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshToken");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.GiaTriToken)
                      .IsRequired()
                      .HasMaxLength(500);

                entity.HasOne(e => e.NguoiDung)
                      .WithMany(u => u.DanhSachTokenLamMoi)
                      .HasForeignKey(e => e.MaNguoiDung);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
namespace ProjectInternWebBanSach.DTO
{
    public class CheckoutInput
    {
        // Thông tin liên hệ
        public string? HoTen { get; set; }
        public string? SoDienThoai { get; set; }

        public string? Province { get; set; }          // nếu có select Tỉnh/TP
        public string? DiaChiDayDu { get; set; }
        public string? GhiChu { get; set; }            // ghi chú CHUNG đơn hàng

        // Thanh toán / vận chuyển / mã giảm giá
        public string? ShippingMethod { get; set; }    // "standard" | "express"
        public string? PaymentMethod { get; set; }     // "wallet" | "cod"
        public string? CouponCode { get; set; }

        //Ghi chú riêng cho từng dòng giỏ hàng
        public Dictionary<int, string>? GhiChuChiTiet { get; set; }

        // Trạng thái sửa hay mua
        public bool IsEdit { get; set; }
        public int? OrderId { get; set; }
    }
}

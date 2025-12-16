using System;

namespace ProjectInternWebBanSach.DTO
{
    public class BalanceChangeDto
    {
        public DateTime Time { get; set; }          // Thời gian giao dịch
        public string Type { get; set; } = "";      // "Nạp tiền" / "Thanh toán đơn hàng"
        public decimal Amount { get; set; }         // Số tiền
        public string Note { get; set; } = "";      // Ghi chú
    }
}

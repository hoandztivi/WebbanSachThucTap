// ================== TAB NAVIGATION ==================
document.addEventListener('DOMContentLoaded', () => {
    const tabButtons = document.querySelectorAll('.tab-btn');
    const tabContents = document.querySelectorAll('.tab-content');

    tabButtons.forEach(button => {
        button.addEventListener('click', () => {
            const targetTab = button.dataset.tab;

            tabButtons.forEach(btn => btn.classList.remove('active'));
            tabContents.forEach(content => content.classList.remove('active'));

            button.classList.add('active');
            document.getElementById(targetTab)?.classList.add('active');

            localStorage.setItem('manage_activeTab', targetTab);
        });
    });

    // Restore active tab
    const savedTab = localStorage.getItem('manage_activeTab');
    if (savedTab) {
        const btn = document.querySelector(`.tab-btn[data-tab="${savedTab}"]`);
        if (btn) btn.click();
    }

    // ========== LỌC ĐƠN HÀNG ==========
    const orderFilter = document.getElementById("orderFilter");
    if (orderFilter) {
        orderFilter.addEventListener("change", () => {
            const value = orderFilter.value;
            document.querySelectorAll(".order-row").forEach(row => {
                const status = row.dataset.status;
                if (value === "all" || value === status) {
                    row.style.display = "";
                } else {
                    row.style.display = "none";
                }
            });
        });
    }
});

// ================== HÀM TIỆN ÍCH ==================
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('vi-VN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    }).format(date);
}

function copyTransactionCode(code) {
    navigator.clipboard.writeText(code).then(() => {
        if (typeof showToast === 'function') {
            showToast('Đã sao chép mã giao dịch', 'success');
        } else {
            alert('Đã sao chép mã giao dịch');
        }
    }).catch(err => console.error('Failed to copy:', err));
}

function getStatusBadge(status) {
    const statusMap = {
        'pending': { class: 'status-pending', text: 'Chờ xác nhận' },
        'processing': { class: 'status-processing', text: 'Đang xử lý' },
        'shipping': { class: 'status-shipping', text: 'Đang giao' },
        'completed': { class: 'status-completed', text: 'Hoàn thành' },
        'cancelled': { class: 'status-cancelled', text: 'Đã hủy' }
    };

    const info = statusMap[status] || statusMap['pending'];
    return `<span class="status-badge ${info.class}">${info.text}</span>`;
}

function scrollToSection(sectionId) {
    const element = document.getElementById(sectionId);
    if (element) element.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

// Cho chỗ khác gọi nếu cần
window.manageUtils = {
    formatCurrency,
    formatDate,
    copyTransactionCode,
    getStatusBadge,
    scrollToSection
};

// ================== HỦY NẠP TIỀN ==================
function cancelRecharge(id) {
    if (!confirm("Bạn chắc chắn muốn hủy giao dịch này?")) return;

    fetch("/Recharge/Cancel?id=" + id, {
        method: "POST"
    })
        .then(res => res.json())
        .then(data => {
            if (typeof showToast === 'function') {
                showToast(data.message || "Không thể hủy giao dịch.", data.success ? "success" : "error");
            }
            if (data.success) {
                setTimeout(() => location.reload(), 800);
            }
        })
        .catch(() => {
            if (typeof showToast === 'function') {
                showToast("Lỗi kết nối đến máy chủ.", "error");
            }
        });
}
window.cancelRecharge = cancelRecharge;

// ================== HỦY ĐƠN HÀNG ==================
function cancelOrder(id) {
    if (!confirm("Bạn chắc chắn muốn hủy đơn hàng này?")) return;

    fetch("/Manage/CancelOrder?id=" + id, {
        method: "POST"
    })
        .then(res => res.json())
        .then(data => {
            if (typeof showToast === 'function') {
                showToast(data.message || "Có lỗi xảy ra.", data.success ? "success" : "error");
            }
            if (data.success) {
                setTimeout(() => location.reload(), 800);
            }
        })
        .catch(() => {
            if (typeof showToast === 'function') {
                showToast("Lỗi kết nối đến máy chủ.", "error");
            }
        });
}
window.cancelOrder = cancelOrder;

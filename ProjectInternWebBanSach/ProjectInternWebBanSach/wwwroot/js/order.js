const orderApiBase = '/Admin/OrderManagement';

const toast = (msg, type) => {
    if (typeof window.showToast === "function") {
        window.showToast(msg, type);
    } else {
        alert(msg);
    }
};

let allOrders = [];
let currentFilter = 'all';

document.addEventListener('DOMContentLoaded', () => {
    const filterSelect = document.getElementById('orderFilter');
    if (filterSelect) {
        filterSelect.addEventListener('change', () => {
            currentFilter = filterSelect.value;
            loadOrders();
        });
    }

    const tblBody = document.getElementById('tblOrdersBody');
    if (tblBody) {
        tblBody.addEventListener('click', (e) => {
            const btnSave = e.target.closest('.btn-save-status');
            if (btnSave) {
                const id = btnSave.dataset.id;
                handleUpdateStatus(id);
                return;
            }

            const btnDelete = e.target.closest('.btn-delete-order');
            if (btnDelete) {
                const id = btnDelete.dataset.id;
                handleDeleteOrder(id);
                return;
            }
        });
    }

    loadOrders();
});

/* ===== LOAD ORDERS ===== */

async function loadOrders() {
    try {
        const url = `${orderApiBase}/GetOrders?status=${encodeURIComponent(currentFilter)}`;
        const res = await fetch(url);
        const json = await res.json();

        if (!json.success) {
            toast("Không tải được danh sách đơn hàng.", "error");
            return;
        }

        allOrders = json.data || [];
        renderOrders(allOrders);
    } catch (err) {
        console.error(err);
        toast("Có lỗi khi tải danh sách đơn hàng.", "error");
    }
}

function formatDate(dateStr) {
    if (!dateStr) return '';
    const d = new Date(dateStr);
    if (isNaN(d.getTime())) return '';
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    return `${day}/${month}/${year}`;
}

function formatMoney(v) {
    if (v == null) return '0₫';
    return v.toLocaleString('vi-VN') + '₫';
}

/* ===== RENDER TABLE ===== */

function renderOrders(list) {
    const tbody = document.getElementById('tblOrdersBody');
    const empty = document.getElementById('orderEmpty');
    const totalOrdersSpan = document.getElementById('totalOrders');

    if (!tbody) return;

    if (!Array.isArray(list) || list.length === 0) {
        tbody.innerHTML = '';
        if (empty) empty.style.display = 'block';
        if (totalOrdersSpan) totalOrdersSpan.textContent = '0';
        return;
    }

    if (empty) empty.style.display = 'none';
    if (totalOrdersSpan) totalOrdersSpan.textContent = list.length.toString();

    const statusOptions = [
        "Chờ xác nhận",
        "Đang xử lý",
        "Đang giao",
        "Hoàn thành",
        "Đã hủy"
    ];

    const rowsHtml = list.map(o => {
        const chiTietList = o.chiTiet || o.ChiTiet || [];
        const chiTietHtml = chiTietList.map(ct => {
            const tenSach = ct.tenSach || ct.TenSach || '';
            const soLuong = ct.soLuong || ct.SoLuong || 0;
            const donGia = ct.donGia || ct.DonGia || 0;
            const thanhToan = (ct.thanhToan || ct.ThanhToan || '').toString().toLowerCase();

            let payLabel = '';
            if (thanhToan === 'wallet') payLabel = 'Ví';
            else if (thanhToan === 'momo') payLabel = 'Momo';
            else if (thanhToan === 'vnpay') payLabel = 'VNPay';

            return `
                <div class="order-item-line">
                    <div class="order-item-title">${tenSach}</div>
                    <div class="order-item-meta">
                        SL: ${soLuong} · ${formatMoney(donGia)} ${payLabel ? '· ' + payLabel : ''}
                    </div>
                </div>
            `;
        }).join('');

        const currentStatus = o.trangThai || o.TrangThai || '';
        const orderId = o.maDonHang || o.MaDonHang;

        const statusSelectHtml = `
            <select class="order-status-select" data-id="${orderId}">
                ${statusOptions.map(st => `
                    <option value="${st}" ${st === currentStatus ? 'selected' : ''}>${st}</option>
                `).join('')}
            </select>
        `;

        return `
            <tr>
                <td>#${orderId}</td>
                <td>${o.hoTen || o.HoTen || ''}</td>
                <td>${o.soDienThoai || o.SoDienThoai || ''}</td>
                <td>${o.email || o.Email || ''}</td>
                <td>${o.diaChiGiao || o.DiaChiGiao || ''}</td>
                <td>${formatDate(o.ngayDat || o.NgayDat)}</td>
                <td>${formatMoney(o.tongTien || o.TongTien)}</td>
                <td>
                    <div class="order-items-cell">
                        ${chiTietHtml || '<span class="order-item-empty">Không có chi tiết</span>'}
                    </div>
                </td>
                <td>
                    ${statusSelectHtml}
                </td>
                <td>
                    <button class="btn-save-status" data-id="${orderId}">
                        Lưu
                    </button>
                </td>
                <td>
                    <button class="btn-delete-order" data-id="${orderId}">
                        Xóa
                    </button>
                </td>
            </tr>
        `;
    }).join('');

    tbody.innerHTML = rowsHtml;
}

/* ===== UPDATE STATUS ===== */

async function handleUpdateStatus(orderId) {
    if (!orderId) return;

    const select = document.querySelector(`select.order-status-select[data-id="${orderId}"]`);
    if (!select) {
        toast("Không tìm thấy trạng thái cho đơn này.", "error");
        return;
    }

    const newStatus = select.value;

    try {
        const res = await fetch(`${orderApiBase}/UpdateStatus`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                maDonHang: parseInt(orderId),
                trangThai: newStatus
            })
        });

        const json = await res.json();
        if (!json.success) {
            toast(json.message || "Cập nhật trạng thái thất bại.", "error");
            return;
        }

        toast(json.message || "Cập nhật trạng thái thành công.", "success");
        await loadOrders();
    } catch (err) {
        console.error(err);
        toast("Có lỗi khi cập nhật trạng thái.", "error");
    }
}

/* ===== DELETE ORDER ===== */

async function handleDeleteOrder(orderId) {
    if (!orderId) return;

    const ok = confirm(`Bạn chắc chắn muốn xóa đơn hàng #${orderId}? Hành động này không thể hoàn tác.`);
    if (!ok) return;

    try {
        const res = await fetch(`${orderApiBase}/Delete`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                maDonHang: parseInt(orderId)
            })
        });

        const json = await res.json();
        if (!json.success) {
            toast(json.message || "Xóa đơn hàng thất bại.", "error");
            return;
        }

        toast(json.message || "Xóa đơn hàng thành công.", "success");
        await loadOrders();
    } catch (err) {
        console.error(err);
        toast("Có lỗi khi xóa đơn hàng.", "error");
    }
}

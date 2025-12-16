const rechargeApiBase = '/Admin/RechargeManager';

const toast = (msg, type) => {
    if (typeof window.showToast === "function") {
        window.showToast(msg, type);
    } else {
        alert(msg);
    }
};

let allRecharges = [];
let currentFilter = 'all';

document.addEventListener('DOMContentLoaded', () => {
    const filterSelect = document.getElementById('statusFilter');
    if (filterSelect) {
        filterSelect.addEventListener('change', () => {
            currentFilter = filterSelect.value;
            loadRecharges();
        });
    }

    const tbody = document.getElementById('tblRechargeBody');
    if (tbody) {
        tbody.addEventListener('click', (e) => {
            const btnPending = e.target.closest('.btn-set-pending');
            if (btnPending) {
                const id = btnPending.dataset.id;
                updateStatus(id, 'Chờ thanh toán');
                return;
            }

            const btnSuccess = e.target.closest('.btn-set-success');
            if (btnSuccess) {
                const id = btnSuccess.dataset.id;
                updateStatus(id, 'Hoàn thành');
                return;
            }
        });
    }

    loadRecharges();
});

async function loadRecharges() {
    try {
        const url = `${rechargeApiBase}/GetRecharges?status=${encodeURIComponent(currentFilter)}`;
        const res = await fetch(url);
        const json = await res.json();

        if (!json.success) {
            toast("Không tải được lịch sử nạp tiền.", "error");
            return;
        }

        allRecharges = json.data || [];
        renderRecharges(allRecharges);
    } catch (err) {
        console.error(err);
        toast("Có lỗi khi tải lịch sử nạp tiền.", "error");
    }
}

function formatDate(dateStr) {
    if (!dateStr) return '';
    const d = new Date(dateStr);
    if (isNaN(d.getTime())) return '';
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    const hh = String(d.getHours()).padStart(2, '0');
    const mm = String(d.getMinutes()).padStart(2, '0');
    return `${day}/${month}/${year} ${hh}:${mm}`;
}

function formatMoney(v) {
    if (v == null) return '0₫';
    return v.toLocaleString('vi-VN') + '₫';
}

function getStatusBadgeHtml(status) {
    const st = (status || '').toLowerCase();

    if (st === 'chờ thanh toán') {
        return `<span class="status-badge status-pending">Chờ thanh toán</span>`;
    }
    if (st === 'hoàn thành') {
        return `<span class="status-badge status-success">Hoàn thành</span>`;
    }
    if (st === 'hoàn tiền hủy đơn') {
        return `<span class="status-badge status-refund">Hoàn tiền hủy đơn</span>`;
    }
    return `<span class="status-badge">${status || ''}</span>`;
}

/* ===== RENDER TABLE ===== */

function renderRecharges(list) {
    const tbody = document.getElementById('tblRechargeBody');
    const empty = document.getElementById('rechargeEmpty');
    const totalSpan = document.getElementById('totalRecharges');

    if (!tbody) return;

    if (!Array.isArray(list) || list.length === 0) {
        tbody.innerHTML = '';
        if (empty) empty.style.display = 'block';
        if (totalSpan) totalSpan.textContent = '0';
        return;
    }

    if (empty) empty.style.display = 'none';
    if (totalSpan) totalSpan.textContent = list.length.toString();

    const rowsHtml = list.map(r => {
        const id = r.maNapTien || r.MaNapTien;
        const soTien = r.soTien || r.SoTien;
        const status = r.trangThai || r.TrangThai;
        const hoTen = r.hoTen || r.HoTen || '';
        const email = r.email || r.Email || '';
        const soDienThoai = r.soDienThoai || r.SoDienThoai || '';
        const noiDung = r.noiDung || r.NoiDung || '';
        const maGiaoDich = r.maGiaoDich || r.MaGiaoDich || '';
        const ngayTao = r.ngayTao || r.NgayTao;

        return `
            <tr>
                <td>#${id}</td>
                <td>
                    <div>${hoTen}</div>
                    <div style="font-size:0.75rem; color:#64748b;">ID: ${r.maNguoiDung || r.MaNguoiDung}</div>
                </td>
                <td>${soDienThoai}</td>
                <td>${email}</td>
                <td>${formatMoney(soTien)}</td>
                <td>${noiDung || '-'}</td>
                <td>${maGiaoDich || '-'}</td>
                <td>${formatDate(ngayTao)}</td>
                <td>${getStatusBadgeHtml(status)}</td>
                <td>
                    <div class="btn-status-row">
                        <button class="btn-set-pending" data-id="${id}">
                            Chờ thanh toán
                        </button>
                        <button class="btn-set-success" data-id="${id}">
                            Hoàn thành
                        </button>
                    </div>
                </td>
            </tr>
        `;
    }).join('');

    tbody.innerHTML = rowsHtml;
}

/* ===== UPDATE STATUS ===== */

async function updateStatus(maNapTien, newStatus) {
    if (!maNapTien) return;

    try {
        const res = await fetch(`${rechargeApiBase}/UpdateStatus`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                maNapTien: parseInt(maNapTien),
                trangThai: newStatus
            })
        });

        const json = await res.json();
        if (!json.success) {
            toast(json.message || "Cập nhật trạng thái thất bại.", "error");
            return;
        }

        toast(json.message || "Cập nhật trạng thái thành công.", "success");
        await loadRecharges();
    } catch (err) {
        console.error(err);
        toast("Có lỗi khi cập nhật trạng thái.", "error");
    }
}

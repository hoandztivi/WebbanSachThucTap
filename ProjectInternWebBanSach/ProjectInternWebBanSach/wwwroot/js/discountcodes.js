const dcApiBase = '/Admin/DiscountcodeManager';

const toast = (msg, type) => {
    if (typeof window.showToast === "function") {
        window.showToast(msg, type);
    } else {
        alert(msg);
    }
};

let dcList = [];

document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('btnSave')?.addEventListener('click', saveCode);
    document.getElementById('btnClear')?.addEventListener('click', clearForm);

    const tbody = document.getElementById('dcTableBody');
    if (tbody) {
        tbody.addEventListener('click', (e) => {
            const editBtn = e.target.closest('.btn-edit');
            const delBtn = e.target.closest('.btn-delete');

            if (editBtn) {
                const id = parseInt(editBtn.dataset.id);
                const code = dcList.find(x => x.maGiamGia === id || x.MaGiamGia === id);
                if (code) fillForm(code);
            }

            if (delBtn) {
                const id = parseInt(delBtn.dataset.id);
                handleDelete(id);
            }
        });
    }

    loadCodes();
});

async function loadCodes() {
    try {
        const res = await fetch(`${dcApiBase}/GetCodes`);
        const json = await res.json();
        if (!json.success) {
            toast("Không tải được danh sách mã giảm.", "error");
            return;
        }
        dcList = json.data || [];
        renderTable(dcList);
    } catch (err) {
        console.error(err);
        toast("Lỗi khi tải danh sách mã giảm.", "error");
    }
}

function formatDate(d) {
    if (!d) return '';
    const dt = new Date(d);
    if (isNaN(dt.getTime())) return '';
    const day = String(dt.getDate()).padStart(2, '0');
    const month = String(dt.getMonth() + 1).padStart(2, '0');
    const year = dt.getFullYear();
    return `${day}/${month}/${year}`;
}

function renderTable(list) {
    const tbody = document.getElementById('dcTableBody');
    const empty = document.getElementById('dcEmpty');
    const total = document.getElementById('dcTotal');
    if (!tbody) return;

    if (!Array.isArray(list) || list.length === 0) {
        tbody.innerHTML = '';
        if (empty) empty.style.display = 'block';
        if (total) total.textContent = '0';
        return;
    }

    if (empty) empty.style.display = 'none';
    if (total) total.textContent = list.length.toString();

    const rows = list.map(x => {
        const id = x.maGiamGia || x.MaGiamGia;
        const code = x.maCode || x.MaCode || '';
        const desc = x.moTa || x.MoTa || '';
        const value = x.giaTriGiam || x.GiaTriGiam || 0;
        const start = x.ngayBatDau || x.NgayBatDau;
        const end = x.ngayKetThuc || x.NgayKetThuc;
        const active = x.dangHoatDong ?? x.DangHoatDong;

        const now = new Date();
        const startDt = new Date(start);
        const endDt = new Date(end);
        const isExpired = end && endDt < now;
        const statusLabel = !active
            ? '<span class="dc-status dc-status-off">Tắt</span>'
            : (isExpired
                ? '<span class="dc-status dc-status-expired">Hết hạn</span>'
                : '<span class="dc-status dc-status-on">Đang chạy</span>');

        const valueLabel = value < 100
            ? `${value}%`
            : `${value.toLocaleString('vi-VN')}đ`;

        return `
            <tr>
                <td>${code}</td>
                <td>${desc || '-'}</td>
                <td>${valueLabel}</td>
                <td>${formatDate(start)}</td>
                <td>${formatDate(end)}</td>
                <td>${statusLabel}</td>
                <td class="dc-actions">
                    <button class="btn-edit" data-id="${id}">Sửa</button>
                    <button class="btn-delete" data-id="${id}">Xóa</button>
                </td>
            </tr>
        `;
    }).join('');

    tbody.innerHTML = rows;
}

function clearForm() {
    document.getElementById('dcId').value = '0';
    document.getElementById('dcCode').value = '';
    document.getElementById('dcDesc').value = '';
    document.getElementById('dcValue').value = '';
    document.getElementById('dcStart').value = '';
    document.getElementById('dcEnd').value = '';
    document.getElementById('dcActive').value = 'true';
    const msg = document.getElementById('dcMessage');
    if (msg) msg.style.display = 'none';
}

function fillForm(code) {
    document.getElementById('dcId').value = code.maGiamGia || code.MaGiamGia;
    document.getElementById('dcCode').value = (code.maCode || code.MaCode || '').trim();
    document.getElementById('dcDesc').value = code.moTa || code.MoTa || '';
    document.getElementById('dcValue').value = code.giaTriGiam || code.GiaTriGiam || 0;

    const start = code.ngayBatDau || code.NgayBatDau;
    const end = code.ngayKetThuc || code.NgayKetThuc;

    document.getElementById('dcStart').value = start ? start.toString().substring(0, 10) : '';
    document.getElementById('dcEnd').value = end ? end.toString().substring(0, 10) : '';
    document.getElementById('dcActive').value = (code.dangHoatDong ?? code.DangHoatDong) ? 'true' : 'false';
}

async function saveCode() {
    const id = parseInt(document.getElementById('dcId').value || '0');
    const code = document.getElementById('dcCode').value.trim();
    const desc = document.getElementById('dcDesc').value.trim();
    const value = parseFloat(document.getElementById('dcValue').value || '0');
    const start = document.getElementById('dcStart').value;
    const end = document.getElementById('dcEnd').value;
    const active = document.getElementById('dcActive').value === 'true';

    const msg = document.getElementById('dcMessage');

    if (!code) {
        showMsg("Mã code không được để trống.", "error");
        return;
    }
    if (value <= 0) {
        showMsg("Giá trị giảm phải lớn hơn 0.", "error");
        return;
    }
    if (start && end && new Date(end) < new Date(start)) {
        showMsg("Ngày kết thúc phải sau ngày bắt đầu.", "error");
        return;
    }

    const payload = {
        maGiamGia: id,
        maCode: code,
        moTa: desc,
        giaTriGiam: value,
        ngayBatDau: start ? new Date(start).toISOString() : new Date().toISOString(),
        ngayKetThuc: end ? new Date(end).toISOString() : new Date().toISOString(),
        dangHoatDong: active
    };

    try {
        const res = await fetch(`${dcApiBase}/Save`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        const json = await res.json();
        if (!json.success) {
            showMsg(json.message || "Lưu mã giảm giá thất bại.", "error");
            return;
        }

        showMsg(json.message || "Lưu mã giảm giá thành công.", "success");
        await loadCodes();
        document.getElementById('dcId').value = '0';
    } catch (err) {
        console.error(err);
        showMsg("Có lỗi khi lưu mã giảm giá.", "error");
    }
}

function showMsg(text, type) {
    const msg = document.getElementById('dcMessage');
    if (!msg) return;
    msg.textContent = text;
    msg.className = 'dc-message ' + (type === 'success' ? 'success' : 'error');
    msg.style.display = 'block';
}

async function handleDelete(id) {
    if (!confirm("Bạn chắc chắn muốn xóa mã giảm giá này?")) return;

    try {
        const res = await fetch(`${dcApiBase}/Delete`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(id)
        });

        const json = await res.json();
        if (!json.success) {
            toast(json.message || "Xóa mã giảm giá thất bại.", "error");
            return;
        }

        toast(json.message || "Xóa mã giảm giá thành công.", "success");
        await loadCodes();
    } catch (err) {
        console.error(err);
        toast("Có lỗi khi xóa mã giảm giá.", "error");
    }
}

const apiRoleBase = '/Admin/UserManagement';

const toast = (msg, type) => {
    if (typeof window.showToast === "function") {
        window.showToast(msg, type);
    } else {
        alert(msg);
    }
};

document.addEventListener('DOMContentLoaded', () => {
    loadRoles();

    const btnCreate = document.getElementById('btnRoleCreate');
    const btnReset = document.getElementById('btnRoleReset');

    if (btnCreate) btnCreate.addEventListener('click', createRole);
    if (btnReset) btnReset.addEventListener('click', resetRoleForm);
});

/* =========== LOAD LIST =========== */

async function loadRoles() {
    try {
        const res = await fetch(`${apiRoleBase}/GetRoles`);
        const json = await res.json();

        if (!json.success) {
            toast("Không tải được danh sách vai trò.", "error");
            return;
        }

        const data = json.data || [];
        renderRoles(data);
    } catch (err) {
        console.error(err);
        toast("Có lỗi khi tải danh sách vai trò.", "error");
    }
}

function renderRoles(roles) {
    const tbody = document.getElementById('tblRolesBody');
    const empty = document.getElementById('roleEmpty');
    const totalEl = document.getElementById('totalRoles');

    tbody.innerHTML = '';

    if (!roles.length) {
        empty.style.display = 'block';
        totalEl.textContent = '0';
        return;
    }

    empty.style.display = 'none';
    totalEl.textContent = roles.length.toString();

    roles.forEach(r => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${r.maVaiTro}</td>
            <td>${r.tenVaiTro || ''}</td>
            <td>${r.moTa || ''}</td>
            <td class="cell-actions">
                <button type="button"
                        class="link-danger"
                        onclick="deleteRole(${r.maVaiTro})">
                    Xoá
                </button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

/* =========== CREATE =========== */

async function createRole() {
    const name = document.getElementById('txtTenVaiTro').value.trim();
    const desc = document.getElementById('txtMoTa').value.trim();
    const msgBox = document.getElementById('roleMessage');

    if (!name) {
        showRoleMessage("Vui lòng nhập tên vai trò.", false);
        toast("Tên vai trò không được để trống.", "error");
        return;
    }

    try {
        const res = await fetch(`${apiRoleBase}/CreateRole`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                tenVaiTro: name,
                moTa: desc || null
            })
        });

        const json = await res.json();
        showRoleMessage(json.message || "", json.success);
        toast(json.message || (json.success ? "Thêm vai trò thành công." : "Thêm vai trò thất bại."), json.success ? "success" : "error");

        if (json.success) {
            resetRoleForm();
            loadRoles();
        }
    } catch (err) {
        console.error(err);
        showRoleMessage("Có lỗi xảy ra. Vui lòng thử lại.", false);
        toast("Có lỗi xảy ra. Vui lòng thử lại.", "error");
    }
}

/* =========== DELETE =========== */

async function deleteRole(id) {
    if (!confirm('Bạn chắc chắn muốn xoá vai trò này?')) return;

    try {
        const res = await fetch(`${apiRoleBase}/DeleteRole`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(id)
        });

        const json = await res.json();
        toast(json.message || (json.success ? "Đã xoá vai trò." : "Xoá vai trò thất bại."), json.success ? "success" : "error");

        if (json.success) {
            loadRoles();
        }
    } catch (err) {
        console.error(err);
        toast("Có lỗi xảy ra khi xoá vai trò.", "error");
    }
}

/* =========== UI HELPERS =========== */

function resetRoleForm() {
    document.getElementById('txtTenVaiTro').value = '';
    document.getElementById('txtMoTa').value = '';

    const msgBox = document.getElementById('roleMessage');
    msgBox.style.display = 'none';
    msgBox.textContent = '';
    msgBox.classList.remove('success', 'error');
}

function showRoleMessage(msg, isSuccess) {
    const msgBox = document.getElementById('roleMessage');
    msgBox.textContent = msg;
    msgBox.style.display = msg ? 'block' : 'none';
    msgBox.classList.toggle('success', !!isSuccess);
    msgBox.classList.toggle('error', !isSuccess);
}

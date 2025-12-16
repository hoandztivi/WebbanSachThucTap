const userApiBase = '/Admin/UserManagement';

const toast = (msg, type) => {
    if (typeof window.showToast === "function") {
        window.showToast(msg, type);
    } else {
        alert(msg);
    }
};

let allUsers = [];

document.addEventListener('DOMContentLoaded', () => {
    loadUsers();

    const searchInput = document.getElementById('txtSearchUser');
    if (searchInput) {
        searchInput.addEventListener('input', () => filterUsers(searchInput.value));
    }

    const btnCreate = document.getElementById('btnUserCreate');
    const btnReset = document.getElementById('btnUserReset');
    const fileAvatar = document.getElementById('fileAvatar');

    if (btnCreate) btnCreate.addEventListener('click', createUser);
    if (btnReset) btnReset.addEventListener('click', resetUserForm);
    if (fileAvatar) fileAvatar.addEventListener('change', uploadAvatar);
});

/* ============ LOAD & FILTER ============ */

async function loadUsers() {
    try {
        const res = await fetch(`${userApiBase}/GetUsers`);
        const json = await res.json();

        if (!json.success) {
            toast("Không tải được danh sách người dùng.", "error");
            return;
        }

        allUsers = json.data || [];
        renderUsers(allUsers);
    } catch (err) {
        console.error(err);
        toast("Có lỗi khi tải danh sách người dùng.", "error");
    }
}

function filterUsers(keyword) {
    keyword = (keyword || '').toLowerCase();

    if (!keyword) {
        renderUsers(allUsers);
        return;
    }

    const filtered = allUsers.filter(u => {
        const name = (u.hoTen || '').toLowerCase();
        const email = (u.email || '').toLowerCase();
        const phone = (u.soDienThoai || '').toLowerCase();
        const role = (u.vaiTro || '').toLowerCase();
        return name.includes(keyword)
            || email.includes(keyword)
            || phone.includes(keyword)
            || role.includes(keyword);
    });

    renderUsers(filtered);
}

function renderUsers(users) {
    const tbody = document.getElementById('tblUsersBody');
    const empty = document.getElementById('userEmpty');
    const totalEl = document.getElementById('totalUsers');

    tbody.innerHTML = '';

    if (!users.length) {
        empty.style.display = 'block';
        totalEl.textContent = '0';
        return;
    }

    empty.style.display = 'none';
    totalEl.textContent = users.length.toString();

    users.forEach(u => {
        const tr = document.createElement('tr');

        const avatarPath = u.anhDaiDien && u.anhDaiDien.trim()
            ? "/img/anhdaidien/" + u.anhDaiDien
            : "/img/anhdaidien/default.png";

        const balance = formatCurrency(u.soDu ?? 0);
        const dateStr = u.ngayTao
            ? new Date(u.ngayTao).toLocaleDateString('vi-VN')
            : '';

        const statusHtml = u.trangThai === false
            ? `<span class="tag-status tag-status-off">Khoá</span>`
            : `<span class="tag-status tag-status-on">Hoạt động</span>`;

        tr.innerHTML = `
            <td>
                <img src="${avatarPath}" alt="${u.hoTen || ''}" class="user-avatar">
            </td>
            <td>${u.maNguoiDung}</td>
            <td>${u.hoTen || ''}</td>
            <td>${u.email || ''}</td>
            <td>${u.vaiTro || ''}</td>
            <td><span class="user-balance">${balance}</span></td>
            <td>${statusHtml}</td>
            <td>${dateStr}</td>
            <td class="cell-actions">
                <button type="button"
                        class="link-danger"
                        onclick="deleteUser(${u.maNguoiDung})">
                    Xoá
                </button>
            </td>
        `;

        tbody.appendChild(tr);
    });
}

function formatCurrency(value) {
    try {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND',
            maximumFractionDigits: 0
        }).format(value || 0);
    } catch {
        return value;
    }
}

/* ============ UPLOAD AVATAR ============ */

async function uploadAvatar(e) {
    const file = e.target.files[0];
    if (!file) return;

    const formData = new FormData();
    formData.append("file", file);

    try {
        const res = await fetch(`${userApiBase}/UploadAvatar`, {
            method: "POST",
            body: formData
        });

        const json = await res.json();
        if (!json.success) {
            toast(json.message || "Tải ảnh thất bại.", "error");
            return;
        }

        // Chỉ lưu fileName vào DB
        document.getElementById("txtAvatarPath").value = json.fileName;

        // Hiển thị preview từ fileUrl
        document.getElementById("imgAvatarPreview").src = json.fileUrl;
        document.getElementById("imgAvatarPreview").style.display = "block";

        toast("Tải ảnh đại diện thành công!", "success");

    } catch (err) {
        console.error(err);
        toast("Có lỗi khi tải ảnh đại diện.", "error");
    }
}

/* ============ CREATE USER ============ */

async function createUser() {
    const hoTen = document.getElementById('txtHoTen').value.trim();
    const email = document.getElementById('txtEmail').value.trim();
    const password = document.getElementById('txtPassword').value.trim();
    const phone = document.getElementById('txtPhone').value.trim();
    const address = document.getElementById('txtAddress').value.trim();
    const roleId = document.getElementById('ddlRole').value;
    const balanceStr = document.getElementById('txtBalance').value;
    const avatarPath = document.getElementById('txtAvatarPath').value.trim();
    const msgBox = document.getElementById('userMessage');

    const balance = balanceStr ? Number(balanceStr) : 0;

    if (!email) {
        showUserMessage("Vui lòng nhập email.", false);
        toast("Email không được để trống.", "error");
        return;
    }
    if (!password) {
        showUserMessage("Vui lòng nhập mật khẩu.", false);
        toast("Mật khẩu không được để trống.", "error");
        return;
    }

    try {
        const res = await fetch(`${userApiBase}/CreateUser`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                hoTen: hoTen || null,
                email: email,
                matKhau: password,
                soDienThoai: phone || null,
                diaChi: address || null,
                soDu: balance,
                maVaiTro: roleId ? Number(roleId) : null,
                anhDaiDien: avatarPath || null
            })
        });

        const json = await res.json();
        showUserMessage(json.message || "", json.success);
        toast(json.message || (json.success ? "Thêm người dùng thành công." : "Thêm người dùng thất bại."), json.success ? "success" : "error");

        if (json.success) {
            resetUserForm();
            loadUsers();
        }
    } catch (err) {
        console.error(err);
        showUserMessage("Có lỗi xảy ra. Vui lòng thử lại.", false);
        toast("Có lỗi xảy ra. Vui lòng thử lại.", "error");
    }
}

/* ============ DELETE USER ============ */

async function deleteUser(id) {
    if (!confirm('Bạn chắc chắn muốn xoá người dùng này?')) return;

    try {
        const res = await fetch(`${userApiBase}/DeleteUser`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(id)
        });

        const json = await res.json();
        toast(json.message || (json.success ? "Đã xoá người dùng." : "Xoá người dùng thất bại."), json.success ? "success" : "error");

        if (json.success) {
            allUsers = allUsers.filter(u => u.maNguoiDung !== id);
            renderUsers(allUsers);
        }
    } catch (err) {
        console.error(err);
        toast("Có lỗi xảy ra khi xoá người dùng.", "error");
    }
}

/* ============ UI HELPERS ============ */

function resetUserForm() {
    document.getElementById('txtHoTen').value = '';
    document.getElementById('txtEmail').value = '';
    document.getElementById('txtPassword').value = '';
    document.getElementById('txtPhone').value = '';
    document.getElementById('txtAddress').value = '';
    document.getElementById('ddlRole').value = '';
    document.getElementById('txtBalance').value = '';
    document.getElementById('txtAvatarPath').value = '';

    const preview = document.getElementById('imgAvatarPreview');
    if (preview) {
        preview.src = '';
        preview.style.display = 'none';
    }

    const msgBox = document.getElementById('userMessage');
    msgBox.style.display = 'none';
    msgBox.textContent = '';
    msgBox.classList.remove('success', 'error');

    const fileAvatar = document.getElementById('fileAvatar');
    if (fileAvatar) {
        fileAvatar.value = '';
    }
}

function showUserMessage(msg, isSuccess) {
    const msgBox = document.getElementById('userMessage');
    msgBox.textContent = msg;
    msgBox.style.display = msg ? 'block' : 'none';
    msgBox.classList.toggle('success', !!isSuccess);
    msgBox.classList.toggle('error', !isSuccess);
}

const apiBase = '/Admin/BookManagement';

const toast = (msg, type) => {
    if (typeof window.showToast === "function") {
        window.showToast(msg, type);
    } else {
        alert(msg);
    }
};

document.addEventListener('DOMContentLoaded', () => {
    loadCategories();

    const btnCreate = document.getElementById('btnCreate');
    const btnReset = document.getElementById('btnReset');

    if (btnCreate) {
        btnCreate.addEventListener('click', createCategory);
    }

    if (btnReset) {
        btnReset.addEventListener('click', resetForm);
    }
});

async function loadCategories() {
    try {
        const res = await fetch(`${apiBase}/GetCategories`);
        const json = await res.json();

        if (!json.success) {
            toast("Không tải được dữ liệu thể loại.", "error");
            return;
        }

        const data = json.data || [];
        const tbody = document.getElementById('tblBody');
        tbody.innerHTML = '';

        data.forEach(c => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${c.maTheLoai}</td>
                <td>
                    <span class="category-tag">${c.tenTheLoai}</span>
                </td>
                <td>${c.moTa || ""}</td>
                <td class="cell-actions">
                    <button type="button"
                            class="link-danger"
                            onclick="deleteCategory(${c.maTheLoai})">
                        Xoá
                    </button>
                </td>
            `;
            tbody.appendChild(tr);
        });

        document.getElementById('totalCount').textContent = data.length;
        document.getElementById('emptyState').style.display =
            data.length === 0 ? 'block' : 'none';

    } catch (err) {
        console.error(err);
        toast("Có lỗi khi tải dữ liệu.", "error");
    }
}

async function createCategory() {
    const ten = document.getElementById('txtTenTheLoai').value.trim();
    const moTa = document.getElementById('txtMoTa').value.trim();

    if (!ten) {
        showInlineMessage("Vui lòng nhập tên thể loại.", false);
        toast("Tên thể loại không được để trống.", "error");
        return;
    }

    try {
        const res = await fetch(`${apiBase}/CreateCategory`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                tenTheLoai: ten,
                moTa: moTa || null
            })
        });

        const json = await res.json();
        showInlineMessage(json.message || "", json.success);
        toast(
            json.message || (json.success ? "Thêm thành công." : "Thêm thất bại."),
            json.success ? "success" : "error"
        );

        if (json.success) {
            resetForm();
            loadCategories();
        }
    } catch (err) {
        console.error(err);
        showInlineMessage("Có lỗi xảy ra. Vui lòng thử lại.", false);
        toast("Có lỗi xảy ra. Vui lòng thử lại.", "error");
    }
}

async function deleteCategory(id) {
    if (!confirm('Bạn chắc chắn muốn xoá thể loại này?')) return;

    try {
        const res = await fetch(`${apiBase}/DeleteCategory`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(id)
        });

        const json = await res.json();
        toast(
            json.message || (json.success ? "Đã xoá." : "Xoá thất bại."),
            json.success ? "success" : "error"
        );

        if (json.success) {
            loadCategories();
        }
    } catch (err) {
        console.error(err);
        toast("Có lỗi xảy ra khi xoá.", "error");
    }
}

function resetForm() {
    document.getElementById('txtTenTheLoai').value = '';
    document.getElementById('txtMoTa').value = '';

    const msgBox = document.getElementById('messageBox');
    msgBox.style.display = 'none';
    msgBox.textContent = '';
    msgBox.classList.remove('success', 'error');
}

function showInlineMessage(msg, isSuccess) {
    const msgBox = document.getElementById('messageBox');
    msgBox.textContent = msg;
    msgBox.style.display = msg ? 'block' : 'none';
    msgBox.classList.toggle('success', !!isSuccess);
    msgBox.classList.toggle('error', !isSuccess);
}

const apiBase = '/Admin/BookManagement';

const toast = (msg, type) => {
    if (typeof window.showToast === "function") {
        window.showToast(msg, type);
    } else {
        alert(msg);
    }
};

let allBooks = [];

document.addEventListener('DOMContentLoaded', () => {
    loadBooks();

    const searchInput = document.getElementById('txtSearch');
    if (searchInput) {
        searchInput.addEventListener('input', () => filterBooks(searchInput.value));
    }

    const btnCreate = document.getElementById('btnCreateBook');
    const btnReset = document.getElementById('btnResetBook');
    const fileImage = document.getElementById('fileImage');

    if (btnCreate) btnCreate.addEventListener('click', createBook);
    if (btnReset) btnReset.addEventListener('click', resetBookForm);
    if (fileImage) fileImage.addEventListener('change', uploadImage);
});

/* =========== LOAD & FILTER =========== */

async function loadBooks() {
    try {
        const res = await fetch(`${apiBase}/GetBooks`);
        const json = await res.json();

        if (!json.success) {
            toast("Không tải được danh sách sách.", "error");
            return;
        }

        allBooks = json.data || [];
        renderBooks(allBooks);
    } catch (err) {
        console.error(err);
        toast("Có lỗi khi tải danh sách sách.", "error");
    }
}

function filterBooks(keyword) {
    keyword = (keyword || '').toLowerCase();

    if (!keyword) {
        renderBooks(allBooks);
        return;
    }

    const filtered = allBooks.filter(b => {
        const title = (b.tieuDe || '').toLowerCase();
        const author = (b.tacGia || '').toLowerCase();
        const cat = (b.tenTheLoai || '').toLowerCase();
        return title.includes(keyword)
            || author.includes(keyword)
            || cat.includes(keyword);
    });

    renderBooks(filtered);
}

function renderBooks(books) {
    const tbody = document.getElementById('tblBooksBody');
    const empty = document.getElementById('booksEmpty');
    const totalEl = document.getElementById('totalBooks');

    tbody.innerHTML = '';

    if (!books.length) {
        empty.style.display = 'block';
        totalEl.textContent = '0';
        return;
    }

    empty.style.display = 'none';
    totalEl.textContent = books.length.toString();

    books.forEach(b => {
        const tr = document.createElement('tr');

        const gia = formatCurrency(b.gia);
        const giam = b.giamGia ? formatCurrency(b.giamGia) : '';
        const dateStr = b.ngayTao
            ? new Date(b.ngayTao).toLocaleDateString('vi-VN')
            : '';

        const imgHtml = b.hinhAnh
            ? `<img src="${b.hinhAnh}" alt="${b.tieuDe || ''}" class="book-thumb" />`
            : '';

        tr.innerHTML = `
            <td>${imgHtml}</td>
            <td>${b.maSach}</td>
            <td>${b.tieuDe || ''}</td>
            <td>
                ${b.tenTheLoai
                ? `<span class="tag-category">${b.tenTheLoai}</span>`
                : ''}
            </td>
            <td>${b.tacGia || ''}</td>
            <td>${b.nhaXuatBan || ''}</td>
            <td>
                <span class="price">${gia}</span>
                ${giam
                ? `<span class="price-discount">-${giam}</span>`
                : ''}
            </td>
            <td>${b.soLuong ?? 0}</td>
            <td>${dateStr}</td>
            <td class="cell-actions">
                <button type="button"
                        class="link-danger"
                        onclick="deleteBook(${b.maSach})">
                    Xoá
                </button>
            </td>
        `;

        tbody.appendChild(tr);
    });
}

function formatCurrency(value) {
    if (value == null) return '';
    try {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND',
            maximumFractionDigits: 0
        }).format(value);
    } catch {
        return value;
    }
}

/* =========== XOÁ =========== */

async function deleteBook(id) {
    if (!confirm('Bạn chắc chắn muốn xoá sách này?')) return;

    try {
        const res = await fetch(`${apiBase}/DeleteBook`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(id)
        });

        const json = await res.json();
        toast(json.message || (json.success ? "Đã xoá sách." : "Xoá sách thất bại."), json.success ? "success" : "error");

        if (json.success) {
            allBooks = allBooks.filter(b => b.maSach !== id);
            renderBooks(allBooks);
        }
    } catch (err) {
        console.error(err);
        toast("Có lỗi xảy ra khi xoá sách.", "error");
    }
}

/* =========== UPLOAD ẢNH =========== */

async function uploadImage(e) {
    const file = e.target.files[0];
    if (!file) return;

    const formData = new FormData();
    formData.append("file", file);

    try {
        const res = await fetch(`${apiBase}/UploadBookImage`, {
            method: "POST",
            body: formData
        });

        const json = await res.json();
        if (!json.success) {
            toast(json.message || "Tải ảnh thất bại.", "error");
            return;
        }

        // Lưu path vào input readonly để gửi lên khi tạo sách
        const txtImage = document.getElementById('txtImage');
        if (txtImage) txtImage.value = json.path;

        const preview = document.getElementById('imgPreview');
        if (preview) {
            preview.src = json.path;
            preview.style.display = 'block';
        }

        toast("Tải ảnh lên thành công.", "success");
    } catch (err) {
        console.error(err);
        toast("Có lỗi khi tải ảnh lên.", "error");
    }
}

/* =========== THÊM SÁCH =========== */

async function createBook() {
    const title = document.getElementById('txtTitle').value.trim();
    const author = document.getElementById('txtAuthor').value.trim();
    const publisher = document.getElementById('txtPublisher').value.trim();
    const catId = document.getElementById('ddlCategory').value;
    const priceStr = document.getElementById('txtPrice').value;
    const discountStr = document.getElementById('txtDiscount').value;
    const quantityStr = document.getElementById('txtQuantity').value;
    const imagePath = document.getElementById('txtImage').value.trim();
    const description = document.getElementById('txtDescription').value.trim();
    const msgBox = document.getElementById('bookMessage');

    const price = Number(priceStr || 0);
    const discount = discountStr ? Number(discountStr) : 0;
    const quantity = quantityStr ? parseInt(quantityStr, 10) : 0;

    if (!title) {
        showBookMessage("Vui lòng nhập tiêu đề sách.", false);
        toast("Tiêu đề sách không được để trống.", "error");
        return;
    }
    if (!catId) {
        showBookMessage("Vui lòng chọn thể loại sách.", false);
        toast("Bạn chưa chọn thể loại.", "error");
        return;
    }
    if (!price || price <= 0) {
        showBookMessage("Giá sách phải lớn hơn 0.", false);
        toast("Giá sách không hợp lệ.", "error");
        return;
    }
    if (quantity < 0) {
        showBookMessage("Số lượng không hợp lệ.", false);
        toast("Số lượng không hợp lệ.", "error");
        return;
    }

    try {
        const res = await fetch(`${apiBase}/CreateBook`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                tieuDe: title,
                tacGia: author || null,
                nhaXuatBan: publisher || null,
                gia: price,
                giamGia: discount || 0,
                soLuong: quantity,
                hinhAnh: imagePath || null,
                moTa: description || null,
                maTheLoai: Number(catId)
            })
        });

        const json = await res.json();
        showBookMessage(json.message || "", json.success);
        toast(json.message || (json.success ? "Thêm sách thành công." : "Thêm sách thất bại."), json.success ? "success" : "error");

        if (json.success) {
            resetBookForm();
            loadBooks();
        }
    } catch (err) {
        console.error(err);
        showBookMessage("Có lỗi xảy ra. Vui lòng thử lại.", false);
        toast("Có lỗi xảy ra. Vui lòng thử lại.", "error");
    }
}

function resetBookForm() {
    document.getElementById('txtTitle').value = '';
    document.getElementById('txtAuthor').value = '';
    document.getElementById('txtPublisher').value = '';
    document.getElementById('ddlCategory').value = '';
    document.getElementById('txtPrice').value = '';
    document.getElementById('txtDiscount').value = '';
    document.getElementById('txtQuantity').value = '0';
    document.getElementById('txtImage').value = '';
    document.getElementById('txtDescription').value = '';

    const preview = document.getElementById('imgPreview');
    if (preview) {
        preview.src = '';
        preview.style.display = 'none';
    }

    const msgBox = document.getElementById('bookMessage');
    msgBox.style.display = 'none';
    msgBox.textContent = '';
    msgBox.classList.remove('success', 'error');
}

function showBookMessage(msg, isSuccess) {
    const msgBox = document.getElementById('bookMessage');
    msgBox.textContent = msg;
    msgBox.style.display = msg ? 'block' : 'none';
    msgBox.classList.toggle('success', !!isSuccess);
    msgBox.classList.toggle('error', !isSuccess);
}

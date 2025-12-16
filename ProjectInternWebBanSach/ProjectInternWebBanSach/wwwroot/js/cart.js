// ====== UPDATE BADGE ======
function updateCartBadge(count) {
    const d = document.getElementById("cartCountDesktop");
    const m = document.getElementById("cartCountMobile");
    if (d) d.textContent = count;
    if (m) m.textContent = count;
}

// ====== REMOVE ONE ITEM ======
document.addEventListener("click", async e => {
    const btn = e.target.closest(".remove-item");
    if (!btn) return;

    const id = btn.dataset.id;

    const res = await fetch(`/Cart/Remove?id=${id}`, { method: "POST" });

    const data = await res.json();

    if (!data.success) return;

    updateCartBadge(data.count);

    // Xóa element item khỏi HTML
    btn.closest(".cart-item").remove();

    // Nếu sạch hết → hiện giỏ hàng trống
    if (data.count === 0) {
        document.getElementById("cartContent").classList.add("hidden");
        document.getElementById("emptyCart").classList.remove("hidden");
    }

    if (typeof showToast === "function") {
        showToast("Đã xóa sản phẩm", "success");
    }
    location.reload();
});

// ====== Xoá tất cả ======
document.getElementById("clearCartBtn")?.addEventListener("click", async () => {
    if (!confirm("Bạn có chắc muốn xóa toàn bộ sản phẩm trong giỏ hàng?")) return;

    const res = await fetch("/Cart/Clear", { method: "POST" });
    const data = await res.json();

    if (!data.success) return;

    updateCartBadge(0);

    document.getElementById("cartItems").innerHTML = "";
    document.getElementById("cartContent").classList.add("hidden");
    document.getElementById("emptyCart").classList.remove("hidden");

    if (typeof showToast === "function") {
        showToast("Đã xóa tất cả sản phẩm", "success");
    }
});
// ====== UPDATE QUANTITY======
document.addEventListener("submit", async e => {
    const form = e.target.closest("form");
    if (!form) return;

    // Form cập nhật số lượng có input name='soLuong'
    if (!form.querySelector("input[name='soLuong']")) return;

    e.preventDefault();

    const maChiTiet = form.querySelector("input[name='maChiTiet']").value;
    const soLuong = form.querySelector("input[name='soLuong']").value;

    const formData = new FormData();
    formData.append("maChiTiet", maChiTiet);
    formData.append("soLuong", soLuong);

    const res = await fetch("/Cart/UpdateQuantity", {
        method: "POST",
        body: formData
    });

    const data = await res.json();

    if (!data.success) return;

    // Cập nhật badge
    updateCartBadge(data.count);

    // Cập nhật line total
    const lineSpan = form.parentElement
        .querySelector("span.font-bold");
    if (lineSpan) {
        lineSpan.textContent = data.lineTotal.toLocaleString("vi-VN") + "₫";
    }

    // Cập nhật subtotal
    const subtotalElem = document.querySelector("#subtotalValue");
    if (subtotalElem) {
        subtotalElem.textContent = data.subtotal.toLocaleString("vi-VN") + "₫";
    }

    // Cập nhật total
    const totalElem = document.querySelector("#totalValue");
    if (totalElem) {
        totalElem.textContent = data.total.toLocaleString("vi-VN") + "₫";
    }

    if (typeof showToast === "function") {
        showToast(data.msg, "success");
    }
    setTimeout(() => {
        window.location.reload();
    }, 400);
});

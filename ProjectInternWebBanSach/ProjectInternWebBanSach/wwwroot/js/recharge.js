let qrCheckInterval = null;

document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("rechargeForm");
    const resetBtn = document.getElementById("btnReset");
    const amountInput = document.getElementById("amountInput");
    const confirmBtn = document.getElementById("btnConfirm");

    if (form) form.addEventListener("submit", onSubmit);
    if (resetBtn) resetBtn.addEventListener("click", resetForm);
    if (confirmBtn) confirmBtn.addEventListener("click", confirmPayment);

    if (amountInput) {
        amountInput.addEventListener("input", () => {
            const v = parseInt(amountInput.value || "0");
            amountInput.classList.toggle("border-red-500", v < 50000);
        });
    }

    // Khôi phục state khi F5
    restoreState();
});

// Lấy userId từ data của Razor (JWT)
function getUserId() {
    const root = document.getElementById("rechargeRoot");
    return root?.dataset.userId || "0";
}

function getStorageKey() {
    return "recharge_state_" + getUserId();
}

// Lưu trạng thái hiện tại vào localStorage
function saveState() {
    const amount = document.getElementById("amountInput")?.value || "";
    const transferCode = document.getElementById("transferCode")?.textContent || "—";
    const qrImg = document.getElementById("qrImg");
    const expiredAt = document.getElementById("expiredAt")?.textContent || "—";
    const statusText = document.getElementById("statusText")?.textContent || "—";

    const state = {
        amount,
        transferCode,
        qrUrl: qrImg?.src || "",
        expiredAt,
        statusText
    };

    localStorage.setItem(getStorageKey(), JSON.stringify(state));
}

// Khôi phục trạng thái từ localStorage
function restoreState() {
    const raw = localStorage.getItem(getStorageKey());
    if (!raw) return;

    try {
        const state = JSON.parse(raw);
        const amountInput = document.getElementById("amountInput");
        const transferCodeEl = document.getElementById("transferCode");
        const qrImg = document.getElementById("qrImg");
        const qrPlaceholder = document.getElementById("qrPlaceholder");

        if (amountInput && state.amount) amountInput.value = state.amount;
        if (transferCodeEl && state.transferCode) transferCodeEl.textContent = state.transferCode;

        if (qrImg && state.qrUrl) {
            qrImg.src = state.qrUrl;
            qrImg.classList.remove("hidden");
        }

        if (qrPlaceholder && state.qrUrl) {
            qrPlaceholder.classList.add("hidden");
        }

        if (state.expiredAt) {
            document.getElementById("expiredAt").textContent = state.expiredAt;
        }
        if (state.statusText) {
            document.getElementById("statusText").textContent = state.statusText;
        }

        setupCopyButton();
    } catch {
        // lỗi parse thì xóa luôn
        localStorage.removeItem(getStorageKey());
    }
}

async function onSubmit(e) {
    e.preventDefault();
    generateQR();
}

// Tạo mã QR VietQR
function generateQR() {
    const amountInput = document.getElementById("amountInput");
    const amount = parseInt(amountInput.value || "0");

    if (!amount || amount < 50000) {
        showToast("Số tiền tối thiểu là 50.000 VND", "error");
        amountInput.focus();
        return;
    }

    const userId = getUserId();
    const transferCode = `NAP${userId}${Date.now().toString().slice(-6)}`;

    const transferCodeEl = document.getElementById("transferCode");
    transferCodeEl.textContent = transferCode;

    const qrImg = document.getElementById("qrImg");
    const qrPlaceholder = document.getElementById("qrPlaceholder");

    qrPlaceholder.classList.remove("hidden");
    qrPlaceholder.innerHTML = `
        <div class="text-center">
            <svg class="w-12 h-12 text-orange-500 mx-auto mb-3 animate-spin" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                      d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
            <p class="text-gray-500 text-sm">Đang tạo mã QR...</p>
        </div>
    `;

    const url =
        `https://img.vietqr.io/image/970415-0325899425-compact2.png` +
        `?amount=${amount}` +
        `&addInfo=${encodeURIComponent(transferCode)}` +
        `&accountName=${encodeURIComponent("LE QUI HOAN")}`;

    qrImg.onload = () => {
        qrImg.classList.remove("hidden");
        qrPlaceholder.classList.add("hidden");

        const expireTime = new Date(Date.now() + 15 * 60 * 1000);
        const expireText = expireTime.toLocaleTimeString("vi-VN");
        document.getElementById("expiredAt").textContent = expireText;

        document.getElementById("statusText").textContent = "Chờ thanh toán";

        setupCopyButton();
        saveState();
        showToast("Mã QR đã được tạo thành công!", "success");
    };

    qrImg.onerror = () => {
        qrPlaceholder.innerHTML = `
            <div class="text-center">
                <svg class="w-12 h-12 text-red-500 mx-auto mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                          d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <p class="text-red-500 text-sm">Không thể tạo mã QR</p>
                <p class="text-gray-400 text-xs mt-1">Vui lòng thử lại</p>
            </div>
        `;
        showToast("Lỗi khi tạo mã QR. Vui lòng thử lại!", "error");
    };

    qrImg.src = url;
}

function setupCopyButton() {
    const btn = document.getElementById("btnCopyCode");
    if (!btn) return;
    btn.onclick = () => {
        const content = document.getElementById("transferCode").textContent;
        copyText(content, "Đã copy nội dung chuyển khoản");
    };
}

// Copy text
function copyText(text, message = "Đã copy") {
    if (!text || text === "—") {
        showToast("Chưa có nội dung để copy", "error");
        return;
    }

    navigator.clipboard
        .writeText(text)
        .then(() => showToast(message, "success"))
        .catch(() => {
            const ta = document.createElement("textarea");
            ta.value = text;
            ta.style.position = "fixed";
            ta.style.opacity = "0";
            document.body.appendChild(ta);
            ta.select();
            try {
                document.execCommand("copy");
                showToast(message, "success");
            } catch {
                showToast("Không thể copy, vui lòng copy thủ công!", "error");
            }
            document.body.removeChild(ta);
        });
}

// Set số tiền nhanh
function setAmount(amount) {
    const input = document.getElementById("amountInput");
    if (!input) return;
    input.value = amount;
    input.focus();
    input.classList.add("ring-4", "ring-orange-100");
    setTimeout(() => input.classList.remove("ring-4", "ring-orange-100"), 250);
}

// Reset form
function resetForm() {
    const amountInput = document.getElementById("amountInput");
    const qrImg = document.getElementById("qrImg");
    const qrPlaceholder = document.getElementById("qrPlaceholder");

    if (amountInput) amountInput.value = "50000";
    if (qrImg) {
        qrImg.classList.add("hidden");
        qrImg.src = "";
    }
    if (qrPlaceholder) {
        qrPlaceholder.classList.remove("hidden");
        qrPlaceholder.innerHTML = `
            <svg class="w-16 h-16 text-gray-300 mx-auto mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                      d="M12 4v1m6 11h2m-6 0h-2v4m0-11v3m0 0h.01M12 12h4.01M16 20h4M4 12h4m12 0h.01M5 8h2a1 1 0 001-1V5a1 1 0 00-1-1H5a1 1 0 00-1 1v2a1 1 0 001 1zM5 20h2a1 1 0 001-1v-2a1 1 0 00-1-1H5a1 1 0 00-1 1v2a1 1 0 001 1z" />
            </svg>
            <p class="text-gray-400 text-sm font-medium">Chưa tạo mã QR</p>
            <p class="text-gray-400 text-xs mt-1">Vui lòng nhập số tiền bên phải</p>
        `;
    }

    document.getElementById("transferCode").textContent = "—";
    document.getElementById("expiredAt").textContent = "—";
    document.getElementById("statusText").textContent = "—";

    if (qrCheckInterval) {
        clearInterval(qrCheckInterval);
        qrCheckInterval = null;
    }

    localStorage.removeItem(getStorageKey());

    showToast("Đã reset form", "info");
}

// Xác nhận thanh toán -> gọi API tạo LichSuNapTien
async function confirmPayment() {
    const amount = parseInt(document.getElementById("amountInput")?.value || "0");
    const transferCode = document.getElementById("transferCode")?.textContent?.trim();

    if (!transferCode || transferCode === "—") {
        showToast("Bạn cần tạo mã QR trước khi xác nhận thanh toán.", "error");
        return;
    }

    if (!amount || amount < 50000) {
        showToast("Số tiền không hợp lệ.", "error");
        return;
    }

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (!token) {
        showToast("Thiếu mã xác thực. Vui lòng tải lại trang.", "error");
        return;
    }

    const btn = document.getElementById("btnConfirm");
    btn.disabled = true;

    try {
        const res = await fetch("/Recharge/ConfirmRecharge", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": token
            },
            body: JSON.stringify({
                amount,
                transferCode
            })
        });

        const data = await res.json();

        if (data.success) {
            document.getElementById("statusText").textContent = "Chờ duyệt";
            saveState();
            showToast(data.message || "Đã xác nhận thanh toán.", "success");
        } else {
            showToast(data.message || "Không thể xác nhận thanh toán.", "error");
        }
    } catch (err) {
        console.error(err);
        showToast("Lỗi kết nối máy chủ. Vui lòng thử lại.", "error");
    } finally {
        btn.disabled = false;
    }
}

// Sử dụng toast global
function showToast(message, type = "success") {
    if (typeof window.showToast === "function") {
        window.showToast(message, type);
    } else {
        alert(message);
    }
}
// === Cache DOM ===
const modal = document.getElementById("modal");
const openBtn = document.getElementById("openModal");
const closeBtn = document.getElementById("closeModal");
const pwdForm = document.getElementById("pwdForm");

// === Modal Open/Close ===
openBtn?.addEventListener("click", () => modal?.classList.add("show"));
closeBtn?.addEventListener("click", () => modal?.classList.remove("show"));
window.addEventListener("click", e => { if (e.target === modal) modal?.classList.remove("show"); });
document.addEventListener("keydown", e => { if (e.key === "Escape") modal?.classList.remove("show"); });

// === Copy Token ===
document.querySelector(".btn-copy-token")?.addEventListener("click", async (e) => {
    const ta = document.getElementById("accessToken");
    if (!ta) return;

    try { await navigator.clipboard.writeText(ta.value) }
    catch {
        ta.select();
        ta.setSelectionRange(0, 99999);
        document.execCommand("copy");
    }

    const btn = e.currentTarget;
    const old = btn.textContent;
    btn.textContent = "Đã sao chép!";
    btn.style.background = "#16a34a";
    setTimeout(() => { btn.textContent = old || "Sao chép"; btn.style.background = "#f59e0b"; }, 1200);
});

// === Submit Change Password (AJAX) ===
pwdForm?.addEventListener("submit", async (e) => {
    e.preventDefault();

    const cur = document.getElementById("CurrentPassword")?.value.trim();
    const np = document.getElementById("NewPassword")?.value.trim();
    const cp = document.getElementById("ConfirmPassword")?.value.trim();
    const anti = pwdForm.querySelector('input[name="__RequestVerificationToken"]')?.value;

    // Client Validation
    if (!cur || !np || !cp) return showToast("Vui lòng nhập đầy đủ thông tin.", "error");
    if (np.length < 6) return showToast("Mật khẩu mới phải có ít nhất 6 ký tự.", "error");
    if (np !== cp) return showToast("Mật khẩu mới và xác nhận không khớp!", "error");

    // Button Loading
    const btn = pwdForm.querySelector('button[type="submit"]');
    const oldBtn = btn.textContent;
    btn.disabled = true;
    btn.textContent = "Đang xử lý...";

    try {
        const body = new URLSearchParams(new FormData(pwdForm));

        const res = await fetch(pwdForm.action, {
            method: "POST",
            headers: anti ? { "RequestVerificationToken": anti } : {},
            body,
            credentials: "same-origin"
        });

        const text = await res.text();

        if (!res.ok) return showToast(text || "Có lỗi xảy ra", "error");

        // Success
        showToast(text || "Đổi mật khẩu thành công", "success");
        setTimeout(() => window.location.href = "/Account/Login", 1200);

    } catch {
        showToast("Không thể kết nối máy chủ!", "error");
    } finally {
        btn.disabled = false;
        btn.textContent = oldBtn;
    }
});

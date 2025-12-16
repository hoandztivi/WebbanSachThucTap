/****************************************
 * ========== CONSTANTS (SVG Icons) ==========
 ****************************************/
const EYE_ON_PATHS = `
<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 
9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"></path>
`;

const EYE_OFF_PATHS = `
<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943
-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 
4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 
7.532l3.29 3.29M3 3l3.59 3.59"></path>
`;

/****************************************
 * ========== DOM UTILITIES ==========
 ****************************************/
const $ = (sel, root = document) => root.querySelector(sel);
const $$ = (sel, root = document) => [...root.querySelectorAll(sel)];

/****************************************
 * ========== ERROR HANDLING ==========
 ****************************************/
const showError = (inputEl, message) => {
    if (!inputEl) return;

    inputEl.classList.add("input-validation-error");

    let errorSpan = inputEl.parentElement?.nextElementSibling;
    if (!errorSpan || !errorSpan.classList.contains("validation-error")) {
        errorSpan = document.createElement("span");
        errorSpan.className = "validation-error";
        inputEl.parentElement.after(errorSpan);
    }

    errorSpan.textContent = message;
};

const clearError = (inputEl) => {
    if (!inputEl) return;

    inputEl.classList.remove("input-validation-error");

    const errorSpan = inputEl.parentElement?.nextElementSibling;
    if (errorSpan && errorSpan.classList.contains("validation-error")) {
        errorSpan.textContent = "";
    }
};

/****************************************
 * ========== VN PHONE NORMALIZER ==========
 ****************************************/
const normalizeVNPhone = (raw) => {
    let digits = (raw || "").replace(/\D/g, "");

    if (digits.startsWith("84")) digits = "0" + digits.slice(2);
    if (digits.length > 10) digits = digits.slice(0, 10);

    return digits;
};

/****************************************
 * ========== 1) PASSWORD TOGGLE ==========
 ****************************************/
const initPasswordToggles = () => {
    const buttons = $$(".toggle-password");
    if (!buttons.length) return;

    buttons.forEach((btn) => {
        const input = $("#" + btn.dataset.target);
        const icon = btn.querySelector(".eye-icon");

        if (!input || !icon) return;

        btn.addEventListener("click", () => {
            const toText = input.type === "password";
            input.type = toText ? "text" : "password";
            icon.innerHTML = toText ? EYE_OFF_PATHS : EYE_ON_PATHS;
        });
    });
};

/****************************************
 * ========== 2) PASSWORD MATCH ==========
 ****************************************/
const initPasswordMatchValidation = () => {
    const form = $(".register-form");
    const pw = $("#Password");
    const confirm = $("#ConfirmPassword");

    if (!form || !pw || !confirm) return;

    const validate = () => {
        if (!confirm.value) return true;
        if (pw.value === confirm.value) {
            clearError(confirm);
            return true;
        }
        showError(confirm, "Mật khẩu xác nhận không khớp");
        return false;
    };

    pw.addEventListener("input", validate);
    confirm.addEventListener("input", validate);

    form.addEventListener("submit", (e) => {
        if (!validate()) e.preventDefault();
    });
};

/****************************************
 * ========== 3) VIETNAM PHONE FORMAT ==========
 ****************************************/
const initPhoneFormatting = () => {
    const phone = $("#Phone");
    if (!phone) return;

    phone.addEventListener("input", (e) => {
        const normalized = normalizeVNPhone(e.target.value);
        e.target.value = normalized;

        if (normalized.length === 10 && !/^(03|05|07|08|09)\d{8}$/.test(normalized)) {
            showError(phone, "Số điện thoại có thể không hợp lệ");
        } else {
            clearError(phone);
        }
    });
};

/****************************************
 * ========== 4) AJAX REGISTER FORM ==========
 ****************************************/
const initAjaxRegister = () => {
    const form = $(".register-form");
    if (!form) return;

    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        const formData = new FormData(form);

        try {
            const res = await fetch(form.action, {
                method: "POST",
                body: formData,
                headers: { "X-Requested-With": "XMLHttpRequest" }
            });

            const data = await res.json();

            if (!data.success) {
                if (typeof showToast === "function") {
                    showToast(data.message || "Đăng ký thất bại", "error");
                }
                return;
            }

            if (typeof showToast === "function") {
                showToast("Đăng ký thành công!", "success");
            }

            if (data.redirectUrl) {
                setTimeout(() => location.href = data.redirectUrl, 1200);
            }

        } catch (err) {
            console.error(err);
            showToast?.("Có lỗi xảy ra, vui lòng thử lại.", "error");
        }
    });
};

/****************************************
 * ========== INIT ================
 ****************************************/
document.addEventListener("DOMContentLoaded", () => {
    initPasswordToggles();
    initPasswordMatchValidation();
    initPhoneFormatting();
    initAjaxRegister();
});

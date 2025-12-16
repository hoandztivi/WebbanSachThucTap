/* ============================
   CONSTANTS
============================ */
const LS_EMAIL_KEY = 'rememberedEmail';
const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

/* ============================
   DOM HELPERS
============================ */
const $ = (sel, root = document) => root.querySelector(sel);
const $$ = (sel, root = document) => [...root.querySelectorAll(sel)];

const setValidationMessage = (inputEl, message = '') => {
    const wrapper = inputEl?.closest(".form-group");
    if (!wrapper) return;
    const msgEl = wrapper.querySelector(".validation-message");
    if (msgEl) msgEl.textContent = message;
};

const setInputErrorStyle = (inputEl, isError) => {
    if (!inputEl) return;
    inputEl.style.borderColor = isError ? '#dc2626' : '';
};

/* ============================
   SHOW / HIDE PASSWORD
============================ */
const initPasswordToggle = () => {
    const toggleBtn = $('#togglePassword');
    const input = $('#password');
    const eye = $('#eyeIcon');
    const eyeSlash = $('#eyeSlashIcon');

    if (!toggleBtn || !input) return;

    toggleBtn.addEventListener('click', () => {
        const isHidden = input.type === 'password';
        input.type = isHidden ? 'text' : 'password';

        eye?.classList.toggle('hidden', !isHidden);
        eyeSlash?.classList.toggle('hidden', isHidden);
    });
};

/* ============================
   REMEMBER EMAIL (LocalStorage)
============================ */
const loadRememberedEmail = () => {
    const emailInput = $('#email');
    const chk = $('#rememberMe');
    const saved = localStorage.getItem(LS_EMAIL_KEY);

    if (saved && emailInput) {
        emailInput.value = saved;
        if (chk) chk.checked = true;
    }
};

const bindRememberEmail = () => {
    const form = $('#loginForm');
    const email = $('#email');
    const chk = $('#rememberMe');

    if (!form || !email) return;

    form.addEventListener('submit', () => {
        if (chk?.checked) {
            localStorage.setItem(LS_EMAIL_KEY, email.value.trim());
        } else {
            localStorage.removeItem(LS_EMAIL_KEY);
        }
    });
};

/* ============================
   INPUT UX VALIDATION
============================ */
const initValidationUX = () => {
    const inputs = $$('.form-input');
    inputs.forEach(input => {

        // Khi nhập → bỏ lỗi
        input.addEventListener('input', () => {
            setValidationMessage(input, '');
            setInputErrorStyle(input, false);
        });

        // Khi blur → check required
        input.addEventListener('blur', () => {
            const emptyRequired = input.hasAttribute('required') && !input.value.trim();
            if (emptyRequired) {
                setInputErrorStyle(input, true);
                setValidationMessage(input, "Không được để trống");
            }
        });

        input.addEventListener('focus', () => setInputErrorStyle(input, false));
    });
};

// Email sai format → báo lỗi
const initEmailValidation = () => {
    const emailInput = $('#email');
    if (!emailInput) return;

    emailInput.addEventListener('blur', () => {
        const val = emailInput.value.trim();
        if (val && !EMAIL_REGEX.test(val)) {
            setInputErrorStyle(emailInput, true);
            setValidationMessage(emailInput, "Email không hợp lệ");
        }
    });
};

/* ============================
   AJAX LOGIN
============================ */
const initAjaxLogin = () => {
    const form = $('#loginForm');
    const passwordInput = $('#password');

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
                // Sai mật khẩu / email
                showToast?.(data.message || "Đăng nhập thất bại", "error");
                if (passwordInput) passwordInput.value = "";
                return;
            }

            // Thành công
            showToast?.(data.message || "Đăng nhập thành công!", "success");

            if (data.redirectUrl) {
                setTimeout(() => location.href = data.redirectUrl, 600);
            }

        } catch (err) {
            console.error(err);
            showToast?.("Có lỗi xảy ra. Vui lòng thử lại!", "error");
        }
    });
};

/* ============================
   INIT
============================ */
document.addEventListener("DOMContentLoaded", () => {
    initPasswordToggle();
    loadRememberedEmail();
    bindRememberEmail();
    initValidationUX();
    initEmailValidation();
    initAjaxLogin();
});

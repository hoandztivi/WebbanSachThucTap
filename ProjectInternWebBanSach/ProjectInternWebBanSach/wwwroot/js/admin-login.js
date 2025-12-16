// Nhớ email admin trong localStorage
const LS_ADMIN_EMAIL_KEY = "adminRememberEmail";

const $ = (sel, root = document) => root.querySelector(sel);
const $$ = (sel, root = document) => [...root.querySelectorAll(sel)];

const setValidationMessage = (inputEl, message = "") => {
    const wrapper = inputEl?.closest(".form-group");
    if (!wrapper) return;
    let msgEl = wrapper.querySelector(".validation-message");
    if (!msgEl) {
        msgEl = document.createElement("span");
        msgEl.className = "validation-message";
        wrapper.appendChild(msgEl);
    }
    msgEl.textContent = message;
};

const setInputErrorStyle = (inputEl, isError) => {
    if (!inputEl) return;
    inputEl.style.borderColor = isError ? "#dc2626" : "";
};

/* Toggle password */
const initPasswordToggle = () => {
    const toggleBtn = $("#togglePassword");
    const input = $("#password");
    const eye = $("#eyeIcon");
    const eyeSlash = $("#eyeSlashIcon");

    if (!toggleBtn || !input) return;

    toggleBtn.addEventListener("click", () => {
        const isHidden = input.type === "password";
        input.type = isHidden ? "text" : "password";

        eye?.classList.toggle("hidden", !isHidden);
        eyeSlash?.classList.toggle("hidden", isHidden);
    });
};

/* Nhớ email bằng localStorage */
const loadRememberedEmail = () => {
    const emailInput = document.querySelector('input[name="Email"]');
    const chk = document.querySelector('input[name="RememberMe"]');
    const saved = localStorage.getItem(LS_ADMIN_EMAIL_KEY);

    if (saved && emailInput) {
        emailInput.value = saved;
        if (chk) chk.checked = true;
    }
};

const bindRememberEmail = () => {
    const form = $("#adminLoginForm");
    const email = document.querySelector('input[name="Email"]');
    const chk = document.querySelector('input[name="RememberMe"]');

    if (!form || !email) return;

    form.addEventListener("submit", () => {
        if (chk?.checked) {
            localStorage.setItem(LS_ADMIN_EMAIL_KEY, email.value.trim());
        } else {
            localStorage.removeItem(LS_ADMIN_EMAIL_KEY);
        }
    });
};

/* UX validation đơn giản */
const initValidationUX = () => {
    const inputs = $$(".form-input");
    inputs.forEach(input => {
        input.addEventListener("input", () => {
            setValidationMessage(input, "");
            setInputErrorStyle(input, false);
        });

        input.addEventListener("blur", () => {
            const emptyRequired =
                input.hasAttribute("required") && !input.value.trim();
            if (emptyRequired) {
                setInputErrorStyle(input, true);
                setValidationMessage(input, "Không được để trống");
            }
        });

        input.addEventListener("focus", () => setInputErrorStyle(input, false));
    });
};

/* AJAX LOGIN ADMIN */
const initAjaxLogin = () => {
    const form = $("#adminLoginForm");
    const passwordInput = $("#password");

    if (!form) return;

    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        const formData = new FormData(form);

        try {
            const res = await fetch(form.action, {
                method: "POST",
                body: formData,
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            const data = await res.json();

            const toast = (msg, type) => {
                if (typeof window.showToast === "function") {
                    window.showToast(msg, type);
                } else {
                    alert(msg);
                }
            };

            if (!data.success) {
                toast(data.message || "Đăng nhập thất bại", "error");
                if (passwordInput) passwordInput.value = "";
                return;
            }

            // Thành công → redirect Dashboard
            toast(data.message || "Đăng nhập quản trị thành công!", "success");

            const redirectUrl =
                data.redirectUrl || "/Admin/Dashboard/Index";

            setTimeout(() => {
                window.location.href = redirectUrl;
            }, 600);

        } catch (err) {
            console.error(err);
            const toast = (msg, type) => {
                if (typeof window.showToast === "function") {
                    window.showToast(msg, type);
                } else {
                    alert(msg);
                }
            };
            toast("Có lỗi xảy ra. Vui lòng thử lại!", "error");
        }
    });
};

/* INIT */
document.addEventListener("DOMContentLoaded", () => {
    console.log("admin-login.js loaded");
    initPasswordToggle();
    loadRememberedEmail();
    bindRememberEmail();
    initValidationUX();
    initAjaxLogin();
});

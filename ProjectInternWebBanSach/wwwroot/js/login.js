/* ============================
    CẤU HÌNH & HẰNG SỐ DÙNG CHUNG
    ============================ */
    const LS_EMAIL_KEY = 'rememberedEmail';
    const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

/* ============================
   TIỆN ÍCH DOM
   ============================ */
// Lấy phần tử an toàn (trả về null nếu không có)
const $ = (sel, root = document) => root.querySelector(sel);
const $$ = (sel, root = document) => [...root.querySelectorAll(sel)];

// Tạo/ghi nội dung thông báo lỗi trong vùng chứa .validation-message
const setValidationMessage = (inputEl, message = '') => {
  const wrap = inputEl?.parentElement;
    if (!wrap) return;
    const msgEl = $('.validation-message', wrap);
    if (msgEl) msgEl.textContent = message;
};

// Đổi viền input theo trạng thái lỗi (có thể thay bằng thêm class 'is-error')
const setInputErrorStyle = (inputEl, isError) => {
  if (!inputEl) return;
    inputEl.style.borderColor = isError ? '#dc2626' : '';
};

/* ============================
   CHỨC NĂNG: HIỆN/ẨN MẬT KHẨU
   ============================ */
const initPasswordToggle = () => {
  const toggleBtn = $('#togglePassword');
    const passwordInput = $('#password');
    const eye = $('#eyeIcon');
    const eyeSlash = $('#eyeSlashIcon');

    if (!toggleBtn || !passwordInput) return;

  toggleBtn.addEventListener('click', () => {
    const isPassword = passwordInput.type === 'password';
    passwordInput.type = isPassword ? 'text' : 'password';
    // Ẩn/hiện icon nếu tồn tại
    eye?.classList.toggle('hidden', !isPassword);       // khi chuyển sang text -> hiện eyeSlash, ẩn eye
    eyeSlash?.classList.toggle('hidden', isPassword);
  });
};

/* ======================================
   CHỨC NĂNG: NHỚ EMAIL (LOCAL STORAGE)
   ====================================== */
const loadRememberedEmail = () => {
  const emailInput = $('#email');
    const rememberChk = $('#rememberMe');
    if (!emailInput || !rememberChk) return;

    const saved = localStorage.getItem(LS_EMAIL_KEY);
    if (saved) {
        emailInput.value = saved;
    rememberChk.checked = true;
  }
};

const bindRememberOnSubmit = () => {
  const form = $('#loginForm');
    const emailInput = $('#email');
    const rememberChk = $('#rememberMe');
    if (!form || !emailInput || !rememberChk) return;

  form.addEventListener('submit', () => {
    if (rememberChk.checked) {
        localStorage.setItem(LS_EMAIL_KEY, emailInput.value.trim());
    } else {
        localStorage.removeItem(LS_EMAIL_KEY);
    }
  });
};

/* ======================================
   CHỨC NĂNG: VALIDATION TRẢI NGHIỆM NGƯỜI DÙNG
   ====================================== */
const attachInputValidationUX = () => {
  const inputs = $$('.form-input');
    if (!inputs.length) return;

  inputs.forEach((input) => {
        // Khi nhập: xóa thông báo và bỏ trạng thái lỗi
        input.addEventListener('input', () => {
            setValidationMessage(input, '');
            setInputErrorStyle(input, false);
        });

    // Khi rời khỏi ô: nếu required + rỗng => báo lỗi
    input.addEventListener('blur', () => {
      const requiredButEmpty = input.hasAttribute('required') && !input.value.trim();
    setInputErrorStyle(input, requiredButEmpty);
    if (requiredButEmpty) setValidationMessage(input, 'Không được để trống');
    });

    // Khi focus: bỏ viền đỏ để người dùng nhập lại
    input.addEventListener('focus', () => {
        setInputErrorStyle(input, false);
    });
  });
};

const attachEmailValidation = () => {
  const emailInput = $('#email');
    if (!emailInput) return;

  emailInput.addEventListener('blur', () => {
    const val = emailInput.value.trim();
    if (val && !EMAIL_REGEX.test(val)) {
        setInputErrorStyle(emailInput, true);
    setValidationMessage(emailInput, 'Email không hợp lệ');
    }
  });
};

/* ============================
   KHỞI TẠO KHI DOM SẴN SÀNG
   ============================ */
document.addEventListener('DOMContentLoaded', () => {
        initPasswordToggle();
    loadRememberedEmail();
    bindRememberOnSubmit();
    attachInputValidationUX();
    attachEmailValidation();
});

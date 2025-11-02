// Template icon SVG (chỉ thay <path> bên trong .eye-icon)
        const EYE_ON_PATHS = `
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"></path>
        `;
        const EYE_OFF_PATHS = `
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21"></path>
        `;

// Lấy 1 phần tử / nhiều phần tử
const $  = (sel, root = document) => root.querySelector(sel);
const $$ = (sel, root = document) => [...root.querySelectorAll(sel)];

// Tạo/đặt thông điệp lỗi ngay sau vùng chứa input (sau parentElement)
const showError = (inputEl, message) => {
  if (!inputEl) return;
        inputEl.classList.add('input-validation-error');

        // chèn span .validation-error ngay sau parent nếu chưa có
        let errorSpan = inputEl.parentElement?.nextElementSibling;
        if (!errorSpan || !errorSpan.classList.contains('validation-error')) {
            errorSpan = document.createElement('span');
        errorSpan.className = 'validation-error';
        inputEl.parentElement?.after(errorSpan);
  }
        errorSpan.textContent = message || '';
};

const clearError = (inputEl) => {
  if (!inputEl) return;
        inputEl.classList.remove('input-validation-error');

        const errorSpan = inputEl.parentElement?.nextElementSibling;
        if (errorSpan && errorSpan.classList.contains('validation-error')) {
            errorSpan.textContent = '';
  }
};

// Chuẩn hoá sđt Việt Nam: chỉ giữ số, xử lý +84 → 0, giới hạn 10 chữ số
const normalizeVNPhone = (raw) => {
            let digits = (raw || '').replace(/\D/g, '');

        // Nếu bắt đầu bằng '84' và kế tiếp là '0' thì lược bớt trùng, còn nếu chỉ '84' thì thay bằng '0'
        if (digits.startsWith('84')) {
            digits = '0' + digits.slice(2);
  }
  // Giới hạn 10 số (phổ biến cho di động VN)
  if (digits.length > 10) digits = digits.slice(0, 10);
        return digits;
};

/* ==============================
   1) HIỆN/ẨN MẬT KHẨU (NHIỀU Ô)
   ============================== */
const initPasswordToggles = () => {
  const buttons = $$('.toggle-password');
        if (!buttons.length) return;

  buttons.forEach((btn) => {
    // Đảm bảo nút không submit form ngoài ý muốn
    if (!btn.hasAttribute('type')) btn.setAttribute('type', 'button');

    btn.addEventListener('click', () => {
      const targetId = btn.dataset.target;          // lấy từ data-target
        const input = document.getElementById(targetId);
        const icon  = btn.querySelector('.eye-icon'); // phần <svg> chứa <path>

            if (!input) return;

            const toText = input.type === 'password';
            input.type = toText ? 'text' : 'password';

            // Cập nhật icon (nếu tồn tại .eye-icon)
            if (icon) {
                icon.innerHTML = toText ? EYE_OFF_PATHS : EYE_ON_PATHS;
            icon.setAttribute('aria-label', toText ? 'Ẩn mật khẩu' : 'Hiện mật khẩu');
      }
    });
  });
};

/* ==============================================
   2) KIỂM TRA KHỚP MẬT KHẨU (REAL-TIME + SUBMIT)
   ============================================== */
const initPasswordMatchValidation = () => {
  const form            = $('.register-form');
            const password        = $('#Password');
            const confirmPassword = $('#ConfirmPassword');
            if (!form || !password || !confirmPassword) return;

  const validateMatch = () => {
    // Không báo lỗi khi confirm trống (đỡ gây khó chịu); chỉ báo khi đã có nhập
    if (!confirmPassword.value) {clearError(confirmPassword); return true; }
            const ok = password.value === confirmPassword.value;
            ok ? clearError(confirmPassword) : showError(confirmPassword, 'Mật khẩu xác nhận không khớp');
            return ok;
  };

            // Kiểm tra theo thời gian thực khi người dùng gõ
            confirmPassword.addEventListener('input', validateMatch);
  password.addEventListener('input', () => {
    // Khi đổi mật khẩu chính, kiểm lại confirm (nếu đã nhập)
    if (confirmPassword.value) validateMatch();
  });

  // Chặn submit nếu không khớp
  form.addEventListener('submit', (e) => {
    if (!validateMatch()) e.preventDefault();
  });
};

/* ====================================
   3) ĐỊNH DẠNG SỐ ĐIỆN THOẠI VIỆT NAM
   ==================================== */
const initPhoneFormatting = () => {
  const phoneInput = $('#Phone');
            if (!phoneInput) return;

  phoneInput.addEventListener('input', (e) => {
    const normalized = normalizeVNPhone(e.target.value);
            e.target.value = normalized;
            // Gợi ý báo lỗi đơn giản khi đủ 10 số mà không hợp lệ đầu số (optional)
            // Ví dụ: di động VN thường bắt đầu 03/05/07/08/09
            if (normalized.length === 10 && !/^(03|05|07|08|09)\d{8}$/.test(normalized)) {
                showError(phoneInput, 'Số điện thoại có thể không hợp lệ');
    } else {
                clearError(phoneInput);
    }
  });
};

/* ==============================
   KHỞI TẠO SAU KHI DOM SẴN SÀNG
   ============================== */
document.addEventListener('DOMContentLoaded', () => {
                initPasswordToggles();
            initPasswordMatchValidation();
            initPhoneFormatting();
});
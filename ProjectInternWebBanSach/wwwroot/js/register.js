// Toggle password visibility
document.addEventListener('DOMContentLoaded', function () {
    const toggleButtons = document.querySelectorAll('.toggle-password');

    toggleButtons.forEach(button => {
        button.addEventListener('click', function () {
            const targetId = this.getAttribute('data-target');
            const input = document.getElementById(targetId);
            const icon = this.querySelector('.eye-icon');

            if (input.type === 'password') {
                input.type = 'text';
                icon.innerHTML = `
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21"></path>
                `;
            } else {
                input.type = 'password';
                icon.innerHTML = `
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"></path>
                `;
            }
        });
    });

    // Client-side validation for password match
    const form = document.querySelector('.register-form');
    const password = document.getElementById('Password');
    const confirmPassword = document.getElementById('ConfirmPassword');

    if (form && password && confirmPassword) {
        form.addEventListener('submit', function (e) {
            if (password.value !== confirmPassword.value) {
                e.preventDefault();

                // Add error styling
                confirmPassword.classList.add('input-validation-error');

                // Show error message
                let errorSpan = confirmPassword.parentElement.nextElementSibling;
                if (!errorSpan || !errorSpan.classList.contains('validation-error')) {
                    errorSpan = document.createElement('span');
                    errorSpan.className = 'validation-error';
                    confirmPassword.parentElement.after(errorSpan);
                }
                errorSpan.textContent = 'Mật khẩu xác nhận không khớp';

                confirmPassword.focus();
            }
        });

        // Remove error on input
        confirmPassword.addEventListener('input', function () {
            this.classList.remove('input-validation-error');
            const errorSpan = this.parentElement.nextElementSibling;
            if (errorSpan && errorSpan.classList.contains('validation-error')) {
                errorSpan.textContent = '';
            }
        });
    }

    // Phone number formatting (Vietnamese format)
    const phoneInput = document.getElementById('Phone');
    if (phoneInput) {
        phoneInput.addEventListener('input', function (e) {
            let value = e.target.value.replace(/\D/g, '');
            if (value.length > 11) {
                value = value.slice(0, 11);
            }
            e.target.value = value;
        });
    }
});
// Login Page JavaScript

(function () {
    'use strict';

    // Toggle Password Visibility
    const togglePassword = document.getElementById('togglePassword');
    const passwordInput = document.getElementById('password');
    const eyeIcon = document.getElementById('eyeIcon');
    const eyeSlashIcon = document.getElementById('eyeSlashIcon');

    if (togglePassword && passwordInput) {
        togglePassword.addEventListener('click', function () {
            const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
            passwordInput.setAttribute('type', type);

            // Toggle icons
            eyeIcon.classList.toggle('hidden');
            eyeSlashIcon.classList.toggle('hidden');
        });
    }

    // Remember Me - Save/Load Email
    const loginForm = document.getElementById('loginForm');
    const emailInput = document.getElementById('email');
    const rememberMeCheckbox = document.getElementById('rememberMe');

    // Load saved email on page load
    if (emailInput && rememberMeCheckbox) {
        const savedEmail = localStorage.getItem('rememberedEmail');
        if (savedEmail) {
            emailInput.value = savedEmail;
            rememberMeCheckbox.checked = true;
        }
    }

    // Save email on form submit
    if (loginForm && emailInput && rememberMeCheckbox) {
        loginForm.addEventListener('submit', function () {
            if (rememberMeCheckbox.checked) {
                localStorage.setItem('rememberedEmail', emailInput.value);
            } else {
                localStorage.removeItem('rememberedEmail');
            }
        });
    }

    // Form validation enhancement
    const formInputs = document.querySelectorAll('.form-input');
    formInputs.forEach(function (input) {
        // Remove validation message on input
        input.addEventListener('input', function () {
            const validationMsg = this.parentElement.querySelector('.validation-message');
            if (validationMsg) {
                validationMsg.textContent = '';
            }
        });

        // Add error styling on blur if empty and required
        input.addEventListener('blur', function () {
            if (this.hasAttribute('required') && !this.value.trim()) {
                this.style.borderColor = '#dc2626';
            }
        });

        // Remove error styling on focus
        input.addEventListener('focus', function () {
            this.style.borderColor = '';
        });
    });

    // Email validation
    if (emailInput) {
        emailInput.addEventListener('blur', function () {
            const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (this.value && !emailPattern.test(this.value)) {
                this.style.borderColor = '#dc2626';
                let validationMsg = this.parentElement.querySelector('.validation-message');
                if (validationMsg) {
                    validationMsg.textContent = 'Email không hợp lệ';
                }
            }
        });
    }

})();
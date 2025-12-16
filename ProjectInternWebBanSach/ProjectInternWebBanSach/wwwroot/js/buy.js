document.addEventListener('DOMContentLoaded', () => {
    const subtotalInput = document.getElementById('subtotalAmount');
    const shippingRadios = document.querySelectorAll('.shipping-radio');
    const shippingFeeEl = document.getElementById('shippingFee');
    const totalAmountEl = document.getElementById('totalAmount');

    const couponCodeInput = document.getElementById('couponCodeInput');
    const applyCouponBtn = document.getElementById('applyCouponBtn');
    const couponDiscountRow = document.getElementById('couponDiscountRow');
    const couponDiscountEl = document.getElementById('couponDiscount');
    const couponCodeHidden = document.getElementById('appliedCouponCode');

    const form = document.getElementById('checkoutForm');

    const subtotal = subtotalInput ? parseFloat(subtotalInput.value) || 0 : 0;
    let currentShippingFee = 30000;
    let currentDiscount = 0;

    // ========== FORMAT TIỀN ==========
    function formatCurrency(n) {
        return (n || 0).toLocaleString('vi-VN') + '₫';
    }

    // ========== CẬP NHẬT TỔNG ==========
    function updateTotal() {
        let total = subtotal + currentShippingFee - currentDiscount;
        if (total < 0) total = 0;

        if (shippingFeeEl) shippingFeeEl.textContent = formatCurrency(currentShippingFee);
        if (totalAmountEl) totalAmountEl.textContent = formatCurrency(total);
    }

    // ========== INIT SHIPPING ==========
    const checkedRadio = document.querySelector('.shipping-radio:checked');
    if (checkedRadio && checkedRadio.dataset.fee) {
        currentShippingFee = parseFloat(checkedRadio.dataset.fee) || 0;
    }
    updateTotal();

    // Khi đổi phương thức giao hàng (tiêu chuẩn / nhanh)
    shippingRadios.forEach(r => {
        r.addEventListener('change', () => {
            if (r.checked) {
                currentShippingFee = parseFloat(r.dataset.fee) || 0;
                updateTotal();
            }
        });
    });

    // ========== ÁP DỤNG MÃ GIẢM GIÁ ==========
    if (applyCouponBtn) {
        applyCouponBtn.addEventListener('click', () => {
            const code = couponCodeInput.value.trim();

            if (!code) {
                if (typeof showToast === 'function') showToast('Vui lòng nhập mã giảm giá.', 'error');
                return;
            }

            fetch('/Checkout/ApplyCoupon', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8'
                },
                body: 'code=' + encodeURIComponent(code)
            })
                .then(res => res.json())
                .then(data => {
                    if (!data) return;

                    if (data.success) {
                        currentDiscount = data.discount || 0;

                        if (couponDiscountRow) {
                            couponDiscountRow.classList.remove('hidden');
                        }
                        if (couponDiscountEl) {
                            couponDiscountEl.textContent = '-' + formatCurrency(currentDiscount);
                        }
                        if (couponCodeHidden) {
                            couponCodeHidden.value = data.code || code;
                        }

                        updateTotal();
                        if (typeof showToast === 'function') {
                            showToast(data.message || 'Áp dụng mã giảm giá thành công.', 'success');
                        }
                    } else {
                        currentDiscount = 0;

                        if (couponDiscountRow) {
                            couponDiscountRow.classList.add('hidden');
                        }
                        if (couponDiscountEl) {
                            couponDiscountEl.textContent = '-0₫';
                        }
                        if (couponCodeHidden) {
                            couponCodeHidden.value = '';
                        }

                        updateTotal();
                        if (typeof showToast === 'function') {
                            showToast(data.message || 'Mã giảm giá không hợp lệ.', 'error');
                        }
                    }
                })
                .catch(() => {
                    if (typeof showToast === 'function') {
                        showToast('Có lỗi khi áp dụng mã giảm giá. Vui lòng thử lại.', 'error');
                    }
                });
        });
    }

    // ========== SUBMIT ĐẶT HÀNG (AJAX) ==========
    if (form) {
        form.addEventListener('submit', (e) => {
            e.preventDefault();

            const formData = new FormData(form);

            fetch(form.action, {
                method: 'POST',
                body: formData // có luôn __RequestVerificationToken
            })
                .then(res => res.json())
                .then(data => {
                    if (!data) return;

                    if (data.success) {
                        // Lưu message để show ở trang Manage nếu cần
                        try {
                            localStorage.setItem('orderSuccessMessage', data.message || 'Đặt hàng thành công!');
                        } catch { }

                        if (typeof showToast === 'function') {
                            showToast(data.message || 'Đặt hàng thành công! Đang chuyển hướng...', 'success');
                        }

                        const url = data.redirectUrl || '/Manage/Manage?tab=shipping';
                        setTimeout(() => {
                            window.location.href = url;
                        }, 1500);
                    } else {
                        if (typeof showToast === 'function') {
                            showToast(data.message || 'Có lỗi xảy ra, vui lòng thử lại.', 'error');
                        }
                    }
                })
                .catch(() => {
                    if (typeof showToast === 'function') {
                        showToast('Không thể gửi yêu cầu. Vui lòng kiểm tra kết nối.', 'error');
                    }
                });
        });
    }
});

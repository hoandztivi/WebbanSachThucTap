// ===== Helpers =====
const $ = (sel, root = document) => root.querySelector(sel);

// ===== Init =====
document.addEventListener('DOMContentLoaded', () => {
    initQuantity();
});

// ----- Quantity + sync with BuyNow & AddToCart -----
function initQuantity() {
    const qtyInput = $('#quantity');
    const buyNowHidden = $('#buyNowQtyHidden');
    const addToCartBtn = document.querySelector('.add-to-cart-btn');
    if (!qtyInput) return;

    function syncQty() {
        let v = parseInt(qtyInput.value || '1', 10);
        if (isNaN(v) || v < 1) v = 1;
        qtyInput.value = v;
        if (buyNowHidden) buyNowHidden.value = v;
        if (addToCartBtn) addToCartBtn.dataset.qty = v;
    }

    window.incrementQuantity = () => {
        qtyInput.value = (parseInt(qtyInput.value || 1, 10) + 1);
        syncQty();
    };
    window.decrementQuantity = () => {
        qtyInput.value = Math.max(1, parseInt(qtyInput.value || 1, 10) - 1);
        syncQty();
    };

    qtyInput.addEventListener('change', syncQty);
    qtyInput.addEventListener('blur', syncQty);
    syncQty();
}

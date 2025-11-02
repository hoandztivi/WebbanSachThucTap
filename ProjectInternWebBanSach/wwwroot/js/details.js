// Wait for DOM to be ready
document.addEventListener('DOMContentLoaded', function() {
    initializeReviewSection();
    initializeTabs();
    initializeRatingInput();
});

// Initialize Review Section
function initializeReviewSection() {
    const btnWriteReview = document.getElementById('btnWriteReview');
    const btnAskQuestion = document.getElementById('btnAskQuestion');
    const reviewForm = document.getElementById('reviewForm');
    const questionForm = document.getElementById('questionForm');

    if (btnWriteReview && reviewForm) {
        btnWriteReview.addEventListener('click', function() {
            // Hide question form
            if (questionForm) {
                questionForm.classList.add('hidden');
            }
            
            // Toggle review form
            if (reviewForm.classList.contains('hidden')) {
                reviewForm.classList.remove('hidden');
                reviewForm.classList.add('animate-fadeIn');
                
                // Scroll to form
                reviewForm.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
            } else {
                reviewForm.classList.add('hidden');
            }
        });
    }

    if (btnAskQuestion && questionForm) {
        btnAskQuestion.addEventListener('click', function() {
            // Hide review form
            if (reviewForm) {
                reviewForm.classList.add('hidden');
            }
            
            // Toggle question form
            if (questionForm.classList.contains('hidden')) {
                questionForm.classList.remove('hidden');
                questionForm.classList.add('animate-fadeIn');
                
                // Scroll to form
                questionForm.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
            } else {
                questionForm.classList.add('hidden');
            }
        });
    }
}

// Initialize Tabs
function initializeTabs() {
    const tabItems = document.querySelectorAll('.tab-item');
    
    tabItems.forEach(tab => {
        tab.addEventListener('click', function() {
            const tabName = this.getAttribute('data-tab');
            
            // Remove active class from all tabs
            tabItems.forEach(t => t.classList.remove('active'));
            
            // Add active class to clicked tab
            this.classList.add('active');
            
            // Here you can add logic to show/hide different content based on tab
            // For now, just switching the active state
            console.log('Switched to tab:', tabName);
        });
    });
}

// Initialize Rating Input (Star Rating)
function initializeRatingInput() {
    const ratingInputs = document.querySelectorAll('.rating-input input[type="radio"]');
    const starLabels = document.querySelectorAll('.star-label');
    
    // Handle star hover effect
    starLabels.forEach((label, index) => {
        label.addEventListener('mouseenter', function() {
            const stars = Array.from(starLabels);
            const currentIndex = stars.indexOf(this);
            
            // Highlight stars up to hovered star
            stars.forEach((star, i) => {
                if (i >= currentIndex) {
                    star.style.color = '#fbbf24';
                } else {
                    star.style.color = '#d1d5db';
                }
            });
        });
    });
    
    // Reset on mouse leave if no selection
    const ratingContainer = document.querySelector('.rating-input');
    if (ratingContainer) {
        ratingContainer.addEventListener('mouseleave', function() {
            const checkedInput = this.querySelector('input[type="radio"]:checked');
            if (!checkedInput) {
                starLabels.forEach(star => {
                    star.style.color = '';
                });
            }
        });
    }
    
    // Handle rating selection
    ratingInputs.forEach(input => {
        input.addEventListener('change', function() {
            const value = parseInt(this.value);
            const stars = Array.from(starLabels).reverse();
            
            stars.forEach((star, index) => {
                if (index < value) {
                    star.style.color = '#fbbf24';
                } else {
                    star.style.color = '#d1d5db';
                }
            });
        });
    });
}

// Quantity Controls (if not already in Details.cshtml)
function incrementQuantity() {
    const qtyInput = document.getElementById('quantity');
    if (qtyInput) {
        const currentValue = parseInt(qtyInput.value) || 1;
        qtyInput.value = currentValue + 1;
    }
}

function decrementQuantity() {
    const qtyInput = document.getElementById('quantity');
    if (qtyInput) {
        const currentValue = parseInt(qtyInput.value) || 1;
        qtyInput.value = Math.max(1, currentValue - 1);
    }
}

// Submit Review Function
function submitReview() {
    const name = document.getElementById('reviewName')?.value.trim();
    const email = document.getElementById('reviewEmail')?.value.trim();
    const phone = document.getElementById('reviewPhone')?.value.trim();
    const rating = document.querySelector('input[name="rating"]:checked')?.value;
    const title = document.getElementById('reviewTitle')?.value.trim();
    const content = document.getElementById('reviewContent')?.value.trim();
    const video = document.getElementById('reviewVideo')?.value.trim();
    const images = document.getElementById('reviewImages')?.files;

    // Basic validation
    if (!name || !email || !content) {
        alert('Vui lòng điền đầy đủ thông tin bắt buộc (Tên, Email, Nội dung)');
        return;
    }

    if (!rating) {
        alert('Vui lòng chọn số sao đánh giá');
        return;
    }

    // Email validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
        alert('Email không hợp lệ');
        return;
    }

    // Here you would normally send the data to the server
    console.log('Submitting review:', {
        name,
        email,
        phone,
        rating,
        title,
        content,
        video,
        images: images?.length || 0
    });

    // TODO: Make AJAX call to submit review
    alert('Đánh giá của bạn đã được gửi thành công!');
    
    // Reset form
    document.getElementById('reviewForm')?.querySelector('form')?.reset();
    document.getElementById('reviewForm')?.classList.add('hidden');
}

// Submit Question Function
function submitQuestion() {
    const name = document.getElementById('questionName')?.value.trim();
    const email = document.getElementById('questionEmail')?.value.trim();
    const phone = document.getElementById('questionPhone')?.value.trim();
    const content = document.getElementById('questionContent')?.value.trim();

    // Basic validation
    if (!name || !email || !content) {
        alert('Vui lòng điền đầy đủ thông tin bắt buộc (Tên, Email, Nội dung)');
        return;
    }

    // Email validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
        alert('Email không hợp lệ');
        return;
    }

    // Here you would normally send the data to the server
    console.log('Submitting question:', {
        name,
        email,
        phone,
        content
    });

    // TODO: Make AJAX call to submit question
    alert('Câu hỏi của bạn đã được gửi thành công!');
    
    // Reset form
    document.getElementById('questionForm')?.querySelector('form')?.reset();
    document.getElementById('questionForm')?.classList.add('hidden');
}

// Image Preview (optional enhancement)
function handleImagePreview() {
    const imageInput = document.getElementById('reviewImages');
    if (imageInput) {
        imageInput.addEventListener('change', function(e) {
            const files = e.target.files;
            if (files && files.length > 0) {
                console.log(`${files.length} hình ảnh đã được chọn`);
                // You can add image preview functionality here
            }
        });
    }
}

// Initialize image preview on load
document.addEventListener('DOMContentLoaded', handleImagePreview);

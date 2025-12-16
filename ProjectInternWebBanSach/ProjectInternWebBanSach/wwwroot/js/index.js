document.addEventListener("DOMContentLoaded", function () {
    const container = document.querySelector(".carousel-container");
    const slides = document.querySelectorAll(".carousel-slide");
    const totalSlides = slides.length;
    if (totalSlides <= 1) return; // có trên 1 banner mới chạy

    let index = 0;
    let startX = 0;
    let currentTranslate = 0;
    let prevTranslate = 0;
    let isDragging = false;
    let movedDistance = 0;
    let autoSlide = setInterval(nextSlide, 3000);

    function nextSlide() {
        index = (index + 1) % totalSlides;
        updateCarousel(true);
    }

    function updateCarousel(animate = true) {
        container.style.transition = animate ? "transform 0.4s ease-in-out" : "none";
        container.style.transform = `translateX(-${index * 100}%)`;
    }

    function getPositionX(e) {
        return e.type.includes("mouse") ? e.pageX : e.touches[0].clientX;
    }

    slides.forEach((slide) => {
        const link = slide.querySelector("a");
        if (link) link.addEventListener("click", handleLinkClick);

        slide.addEventListener("dragstart", (e) => e.preventDefault());
        slide.addEventListener("mousedown", touchStart);
        slide.addEventListener("touchstart", touchStart);
        slide.addEventListener("mousemove", touchMove);
        slide.addEventListener("touchmove", touchMove);
        slide.addEventListener("mouseup", touchEnd);
        slide.addEventListener("mouseleave", touchEnd);
        slide.addEventListener("touchend", touchEnd);
    });

    function touchStart(e) {
        clearInterval(autoSlide);
        isDragging = true;
        startX = getPositionX(e);
        movedDistance = 0;
        prevTranslate = -index * container.clientWidth;
        container.style.transition = "none";
    }

    function touchMove(e) {
        if (!isDragging) return;
        const currentX = getPositionX(e);
        const movedX = currentX - startX;
        movedDistance = Math.abs(movedX);
        currentTranslate = prevTranslate + movedX;
        container.style.transform = `translateX(${currentTranslate}px)`;
    }

    function touchEnd() {
        if (!isDragging) return;
        isDragging = false;
        const movedBy = currentTranslate - prevTranslate;
        const threshold = container.clientWidth * 0.25;

        if (movedBy < -threshold && index < totalSlides - 1) index++;
        if (movedBy > threshold && index > 0) index--;

        updateCarousel(true);
        autoSlide = setInterval(nextSlide, 5000);
    }

    //Ngăn không cho click khi vừa kéo
    function handleLinkClick(e) {
        if (movedDistance > 10) {
            e.preventDefault(); // ngăn mở link nếu có kéo
        }
    }
});

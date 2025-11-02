class MeteorEffect {
    constructor() {
        this.canvas = document.getElementById('starsCanvas');
        if (!this.canvas) return;

        this.ctx = this.canvas.getContext('2d');
        this.meteors = [];
        this.isActive = true;
        this.animationId = null;

        this.init();
        this.bindEvents();
    }

    init() {
        this.resizeCanvas();
        this.createMeteors();
        this.animate();
    }

    resizeCanvas() {
        this.canvas.width = window.innerWidth;
        this.canvas.height = window.innerHeight;
    }

    createMeteors() {
        this.meteors = [];
        const count = Math.floor((this.canvas.width * this.canvas.height) / 60000);
        for (let i = 0; i < count; i++) this.meteors.push(this.createMeteor());
    }

    createMeteor() {
        return {
            x: Math.random() * this.canvas.width + 100,
            y: -50,
            length: Math.random() * 80 + 20,
            speed: Math.random() * 10 + 4,
            angle: Math.random() * Math.PI / 6 + Math.PI / 4,
            opacity: Math.random() * 0.8 + 0.2,
            color: this.getRandomColor()
        };
    }

    getRandomColor() {
        const colors = ['#00d4ff', '#ff6b9d', '#00ff88', '#ffaa00', '#ff4757', '#ffffff'];
        return colors[Math.floor(Math.random() * colors.length)];
    }

    updateMeteor(m) {
        m.x -= Math.cos(m.angle) * m.speed;
        m.y += Math.sin(m.angle) * m.speed;
        if (m.x < -m.length || m.y > this.canvas.height + 50) {
            m.x = Math.random() * this.canvas.width + 100;
            m.y = -50;
        }
    }

    drawMeteor(m) {
        const ctx = this.ctx;
        ctx.save();
        const g = ctx.createLinearGradient(m.x, m.y, m.x + Math.cos(m.angle) * m.length, m.y - Math.sin(m.angle) * m.length);
        g.addColorStop(0, m.color);
        g.addColorStop(1, m.color + '00');
        ctx.strokeStyle = g;
        ctx.lineWidth = 2;
        ctx.lineCap = 'round';
        ctx.shadowColor = m.color;
        ctx.shadowBlur = 10;
        ctx.beginPath();
        ctx.moveTo(m.x, m.y);
        ctx.lineTo(m.x + Math.cos(m.angle) * m.length, m.y - Math.sin(m.angle) * m.length);
        ctx.stroke();
        ctx.restore();
    }

    animate() {
        this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
        this.meteors.forEach(m => { this.updateMeteor(m); this.drawMeteor(m); });
        this.animationId = requestAnimationFrame(() => this.animate());
    }

    bindEvents() {
        window.addEventListener('resize', () => { this.resizeCanvas(); this.createMeteors(); });
    }
}

document.addEventListener("DOMContentLoaded", function () {
    // ==== HIỆU ỨNG SAO BĂNG ====
    new MeteorEffect();

    // ==== ĐỔI LOGO TỰ ĐỘNG ====
    const logo = document.getElementById("logoImage");
    if (logo) {
        const logos = ["/images/logo1.png", "/images/logo2.png"];
        let index = 0;
        setInterval(() => {
            logo.style.opacity = 0;
            setTimeout(() => {
                index = (index + 1) % logos.length;
                logo.src = logos[index];
                logo.style.opacity = 1;
            }, 500);
        }, 3000);
    }

    // ==== MENU PC ====
    const categoryMenu = document.getElementById("categoryMenu");
    const categoryToggle = document.getElementById("categoryToggle");
    const categoryDropdown = document.querySelector("#categoryMenu > ul, #categoryMenu > div, #categoryMenu > *:not(button)");
    const arrowIcon = document.getElementById("arrowIcon");

    if (categoryToggle && categoryDropdown && categoryMenu) {
        categoryDropdown.classList.add("hidden");

        categoryToggle.addEventListener("click", (e) => {
            e.stopPropagation();
            const isHidden = categoryDropdown.classList.contains("hidden");
            categoryDropdown.classList.toggle("hidden", !isHidden);
            categoryDropdown.classList.toggle("block", isHidden);
            arrowIcon.classList.toggle("rotate-180");
        });

    }

    // ==== MENU MOBILE ====
    const mobileToggle = document.getElementById("mobileCategoryToggle");
    const mobileDropdown = document.getElementById("mobileCategoryDropdown");
    const mobileArrow = document.getElementById("mobileArrowIcon");

    if (mobileToggle && mobileDropdown) {
        mobileToggle.addEventListener("click", (e) => {
            e.stopPropagation();
            const isHidden = mobileDropdown.classList.contains("hidden");
            mobileDropdown.classList.toggle("hidden", !isHidden);
            mobileDropdown.classList.toggle("block", isHidden);
            mobileArrow.classList.toggle("rotate-180");
        });
    }

    // ==== MENU MOBILE CHÍNH (3 GẠCH) ====
    const mobileMenuToggle = document.getElementById("mobileMenuToggle");
    const mobileNav = document.getElementById("mobileNav");

    if (mobileMenuToggle && mobileNav) {
        mobileMenuToggle.addEventListener("click", () => {
            mobileNav.classList.toggle("hidden");
        });
    }

    // ==== HÀM CLICK NGOÀI====
    document.addEventListener("click", (e) => {
        // Logic click ngoài cho PC
        if (categoryToggle && categoryDropdown && categoryMenu) {
            if (categoryDropdown.classList.contains("block") && !categoryMenu.contains(e.target)) {
                categoryDropdown.classList.add("hidden");
                categoryDropdown.classList.remove("block");
                if (arrowIcon) arrowIcon.classList.remove("rotate-180");
            }
        }

        // Logic click ngoài cho Mobile Category Dropdown
        if (mobileToggle && mobileDropdown) {
            const mobileParentLi = mobileToggle.parentElement; // Lấy thẻ <li> cha
            if (mobileDropdown.classList.contains("block") && !mobileParentLi.contains(e.target)) {
                mobileDropdown.classList.add("hidden");
                mobileDropdown.classList.remove("block");
                if (mobileArrow) mobileArrow.classList.remove("rotate-180");
            }
        }
    });

    // ==== NÚT BACK TO TOP ====
    const backToTopButton = document.getElementById("backToTop");
    if (backToTopButton) {
        window.addEventListener("scroll", () => {
            if (window.scrollY > 300) {
                backToTopButton.classList.add("show");
            } else {
                backToTopButton.classList.remove("show");
            }
        });

        backToTopButton.addEventListener("click", () => {
            window.scrollTo({
                top: 0,
                behavior: "smooth"
            });
        });
    }
    // ==== MOBILE AVATAR MENU ====
    const avatarBtn = document.getElementById("mobileAvatarBtn");
    const avatarMenu = document.getElementById("mobileAvatarMenu");

    if (avatarBtn && avatarMenu) {
        avatarBtn.addEventListener("click", (e) => {
            e.stopPropagation();
            avatarMenu.classList.toggle("hidden");
        });

        // Bấm ra ngoài thì ẩn menu
        document.addEventListener("click", (e) => {
            if (!avatarMenu.classList.contains("hidden") && !avatarBtn.contains(e.target)) {
                avatarMenu.classList.add("hidden");
            }
        });
    }
});
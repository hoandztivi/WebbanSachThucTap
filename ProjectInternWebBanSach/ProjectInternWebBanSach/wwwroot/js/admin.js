document.addEventListener('DOMContentLoaded', function () {
    const menuToggle = document.getElementById('menuToggle');
    const closeSidebar = document.getElementById('closeSidebar');
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('overlay');
    const menuToggles = document.querySelectorAll('.menu-toggle');

    // Toggle sidebar on mobile
    if (menuToggle) {
        menuToggle.addEventListener('click', function () {
            sidebar.classList.add('show');
            overlay.classList.add('show');
        });
    }

    // Close sidebar
    if (closeSidebar) {
        closeSidebar.addEventListener('click', function () {
            sidebar.classList.remove('show');
            overlay.classList.remove('show');
        });
    }

    // Close sidebar when clicking overlay
    if (overlay) {
        overlay.addEventListener('click', function () {
            sidebar.classList.remove('show');
            overlay.classList.remove('show');
        });
    }

    // Submenu toggle
    menuToggles.forEach(function (toggle) {
        toggle.addEventListener('click', function (e) {
            e.preventDefault();

            const submenu = this.nextElementSibling;
            const arrow = this.querySelector('.menu-arrow');

            this.classList.toggle('active');

            if (submenu && submenu.classList.contains('submenu')) {
                submenu.classList.toggle('show');
                submenu.classList.toggle('hidden');
            }

            if (arrow) {
                arrow.style.transform = this.classList.contains('active')
                    ? 'rotate(180deg)'
                    : 'rotate(0deg)';
            }
        });
    });

    // Active menu item
    const currentPath = window.location.pathname;
    const menuLinks = document.querySelectorAll('aside a');

    menuLinks.forEach(function (link) {
        if (link.getAttribute('href') === currentPath) {
            link.classList.add('active');

            const parentSubmenu = link.closest('.submenu');
            if (parentSubmenu) {
                parentSubmenu.classList.add('show');
                parentSubmenu.classList.remove('hidden');

                const parentToggle = parentSubmenu.previousElementSibling;
                if (parentToggle) {
                    parentToggle.classList.add('active');
                    const arrow = parentToggle.querySelector('.menu-arrow');
                    if (arrow) {
                        arrow.style.transform = 'rotate(180deg)';
                    }
                }
            }
        }
    });

    // Close mobile menu when clicking a link
    menuLinks.forEach(function (link) {
        link.addEventListener('click', function () {
            if (window.innerWidth < 1024) {
                sidebar.classList.remove('show');
                overlay.classList.remove('show');
            }
        });
    });

    // Handle window resize
    let resizeTimer;
    window.addEventListener('resize', function () {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(function () {
            if (window.innerWidth >= 1024) {
                sidebar.classList.remove('show');
                overlay.classList.remove('show');
            }
        }, 250);
    });
});
// ========== TOAST ==========
// Dùng chung toàn admin: showToast("Nội dung", "success" | "error" | "info")
function showToast(message, type = "success") {
    const toast = document.getElementById("toast");
    if (!toast) return;

    toast.className = `toast toast--${type} show`;
    toast.innerHTML = `
        ${type === "success"
            ? '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/></svg>'
            : type === "error"
                ? '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>'
                : '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M12 2a10 10 0 110 20 10 10 0 010-20z"/></svg>'
        }
        <span>${message}</span>
    `;

    setTimeout(() => {
        toast.classList.remove("show");
    }, 3000);
}

// ========== HIỆU ỨNG SAO BĂNG ==========

class MeteorEffect {
    constructor() {
        this.canvas = document.getElementById('starsCanvas');
        if (!this.canvas) return;

        this.ctx = this.canvas.getContext('2d');
        this.meteors = [];
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
        const g = ctx.createLinearGradient(
            m.x,
            m.y,
            m.x + Math.cos(m.angle) * m.length,
            m.y - Math.sin(m.angle) * m.length
        );
        g.addColorStop(0, m.color);
        g.addColorStop(1, m.color + '00');
        ctx.strokeStyle = g;
        ctx.lineWidth = 2;
        ctx.lineCap = 'round';
        ctx.shadowColor = m.color;
        ctx.shadowBlur = 10;
        ctx.beginPath();
        ctx.moveTo(m.x, m.y);
        ctx.lineTo(
            m.x + Math.cos(m.angle) * m.length,
            m.y - Math.sin(m.angle) * m.length
        );
        ctx.stroke();
        ctx.restore();
    }

    animate() {
        this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
        this.meteors.forEach(m => {
            this.updateMeteor(m);
            this.drawMeteor(m);
        });
        this.animationId = requestAnimationFrame(() => this.animate());
    }

    bindEvents() {
        window.addEventListener('resize', () => {
            this.resizeCanvas();
            this.createMeteors();
        });
    }
}

document.addEventListener("DOMContentLoaded", function () {
    new MeteorEffect();
});
document.addEventListener("DOMContentLoaded", () => {
    const btn = document.getElementById("adminAvatarBtn");
    const menu = document.getElementById("adminDropdown");

    if (btn && menu) {
        btn.addEventListener("click", (e) => {
            e.stopPropagation();
            menu.classList.toggle("hidden");
        });

        // Click ra ngoài sẽ đóng menu
        document.addEventListener("click", () => {
            menu.classList.add("hidden");
        });
    }
});

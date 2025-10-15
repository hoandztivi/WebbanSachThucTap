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

document.addEventListener('DOMContentLoaded', () => new MeteorEffect());

// Tự động đổi logo mỗi 3 giây
document.addEventListener("DOMContentLoaded", () => {
    const logo = document.getElementById("logoImage");
    if (!logo) return;

    const logos = [
        "/images/logo1.png",
        "/images/logo2.png"
    ];
    let index = 0;

    setInterval(() => {
        logo.style.opacity = 0; // hiệu ứng mờ dần

        setTimeout(() => {
            index = (index + 1) % logos.length;
            logo.src = logos[index];
            logo.style.opacity = 1; // hiện dần lại
        }, 500); // delay nhỏ cho hiệu ứng
    }, 3000); // đổi sau mỗi 3 giây
});

document.addEventListener("DOMContentLoaded", function () {
    // ===== DANH SÁCH BÀI NHẠC (đổi tên file cho đúng) =====
    const songs = [
        "/music/minhanhnoinayxloitaianh.mp3",
        //"/music/mp3/song2.mp3",
        //"/music/mp3/song3.mp3"
    ];

    if (!songs.length) return;

    // Chọn random 1 bài
    const randomSong = songs[Math.floor(Math.random() * songs.length)];

    // Lấy element
    const audio = document.getElementById("bgMusic");
    const btnToggle = document.getElementById("musicToggleBtn");
    const btnVolUp = document.getElementById("volUpBtn");
    const btnVolDown = document.getElementById("volDownBtn");
    const volLabel = document.getElementById("volLabel");

    if (!audio) return;

    // ===== THIẾT LẬP BAN ĐẦU =====
    let currentVolume = 0.4;
    audio.src = randomSong;
    audio.volume = currentVolume;
    audio.loop = true; // muốn lặp lại

    updateVolumeLabel();
    updateToggleButton(); // set text theo trạng thái hiện tại

    // ===== HÀM CẬP NHẬT UI =====
    function updateVolumeLabel() {
        if (volLabel) {
            volLabel.textContent = Math.round(audio.volume * 100) + "%";
        }
    }

    function updateToggleButton() {
        if (!btnToggle) return;
        if (audio.paused) {
            btnToggle.textContent = "🔈 Bật nhạc";
        } else {
            btnToggle.textContent = "🔊 Tắt nhạc";
        }
    }

    // ===== THỬ PHÁT KHI VÀO TRANG =====
    function tryPlayOnLoad() {
        audio.play()
            .then(() => {
                console.log("Phát nhạc ngay khi vào trang:", randomSong);
                updateToggleButton();
            })
            .catch((err) => {
                console.log("Autoplay bị chặn, chờ user tương tác...", err);
                attachUnlockEvents();
                updateToggleButton(); // lúc này đang paused
            });
    }

    // ===== UNLOCK AUTOPLAY BẰNG USER TƯƠNG TÁC =====
    function unlockByUser() {
        audio.play()
            .then(() => {
                console.log("Phát nhạc sau khi user tương tác");
                updateToggleButton();
                removeUnlockEvents();
            })
            .catch((err) => {
                console.log("Vẫn bị chặn:", err);
            });
    }

    function attachUnlockEvents() {
        document.addEventListener("click", unlockByUser, { once: true });
        document.addEventListener("keydown", unlockByUser, { once: true });
        document.addEventListener("touchstart", unlockByUser, { once: true });
    }

    function removeUnlockEvents() {
        document.removeEventListener("click", unlockByUser);
        document.removeEventListener("keydown", unlockByUser);
        document.removeEventListener("touchstart", unlockByUser);
    }

    // Gọi luôn khi vào trang
    tryPlayOnLoad();

    // ===== SỰ KIỆN NÚT BẬT / TẮT =====
    if (btnToggle) {
        btnToggle.addEventListener("click", function () {
            if (audio.paused) {
                audio.play()
                    .then(() => {
                        updateToggleButton();
                    })
                    .catch((err) => {
                        console.log("Không phát được:", err);
                    });
            } else {
                audio.pause();
                updateToggleButton();
            }
        });
    }

    // ===== NÚT TĂNG GIẢM ÂM LƯỢNG =====
    if (btnVolUp) {
        btnVolUp.addEventListener("click", function () {
            let v = audio.volume + 0.1;
            if (v > 1) v = 1;
            audio.volume = v;
            updateVolumeLabel();
        });
    }

    if (btnVolDown) {
        btnVolDown.addEventListener("click", function () {
            let v = audio.volume - 0.1;
            if (v < 0) v = 0;
            audio.volume = v;
            updateVolumeLabel();
        });
    }
});

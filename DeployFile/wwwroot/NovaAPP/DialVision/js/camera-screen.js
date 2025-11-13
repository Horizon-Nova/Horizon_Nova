// CameraScreen 專用 JavaScript
let __cameraStream = null;

function stopCamera() {
	if (__cameraStream) {
		__cameraStream.getTracks().forEach(t => t.stop?.());
		__cameraStream = null;
	}
}

function capturePhoto() {
	const video = document.getElementById('cameraVideo');
	const canvas = document.getElementById('cameraCanvas');
	if (!video || !canvas) return;
	const w = video.videoWidth || video.clientWidth;
	const h = video.videoHeight || video.clientHeight;
	if (!w || !h) return;
	canvas.width = w;
	canvas.height = h;
	const ctx = canvas.getContext('2d');
	ctx.drawImage(video, 0, 0, w, h);
	canvas.style.display = 'block';
	// 存為 DataURL
	const dataUrl = canvas.toDataURL('image/jpeg', 0.9);
	window.capturedMeterPhoto = dataUrl;
}

function retakePhoto() {
	const canvas = document.getElementById('cameraCanvas');
	if (canvas) canvas.style.display = 'none';
}

// 嘗試自動啟動（若瀏覽器阻擋，顯示提示並提供手動按鈕）
document.addEventListener('DOMContentLoaded', () => {
	(async () => {
		try {
			const constraints = { video: { facingMode: { ideal: 'environment' } }, audio: false };
			const stream = await navigator.mediaDevices.getUserMedia(constraints);
			__cameraStream = stream;
			const video = document.getElementById('cameraVideo');
			video.srcObject = stream;
			await video.play();
		} catch (err) {
			// 靜默失敗，使用者可返回或重整授權
		}
	})();
	if (window.lucide?.createIcons) {
		window.lucide.createIcons();
	}
});

// 防止資源洩漏：當離開頁面時停止攝影機
window.addEventListener('beforeunload', stopCamera);



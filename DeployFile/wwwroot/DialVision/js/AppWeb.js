document.addEventListener('DOMContentLoaded', () => {
    // 禁止圖片右鍵、長按選單
    document.querySelectorAll('img').forEach(img => {
        img.setAttribute('draggable', 'false');
        img.addEventListener('contextmenu', e => e.preventDefault());
        img.addEventListener('touchstart', e => {
            if (e.touches.length > 1) e.preventDefault();
        });
    });

    // 禁止全頁右鍵
    document.addEventListener('contextmenu', e => e.preventDefault());

    // 禁止長按選取（Android/Safari）
    document.body.addEventListener('touchstart', e => {
        if (e.touches.length > 1) e.preventDefault();
    }, { passive: false });

    document.querySelectorAll('button').forEach(btn => {
        btn.setAttribute('draggable', 'false');
        btn.addEventListener('dragstart', e => e.preventDefault());
    });

    document.querySelectorAll('.function-card').forEach(el => {
        el.addEventListener('click', function () {
            const func = this.getAttribute('data-function');
            if (window.FlutterBridge?.postMessage) {
                window.FlutterBridge.postMessage(func);
            } else {
                console.warn("FlutterBridge not available.");
            }
        });
    });

});

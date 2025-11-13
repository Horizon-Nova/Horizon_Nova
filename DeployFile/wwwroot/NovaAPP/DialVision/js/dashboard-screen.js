// DashboardScreen 專用 JavaScript
$(function () {
    // 關閉按鈕旋轉效果
    $(document).on('click', '#btnClose', function() {
        const $btn = $(this);
        
        // 防止動畫進行中重複點擊
        if ($btn.hasClass('rotating')) {
            return;
        }
        
        $btn.addClass('rotating');
        $btn.toggleClass('rotated');
        
        // 等待動畫完成後移除鎖定（0.6s = 600ms）
        setTimeout(function() {
            $btn.removeClass('rotating');
        }, 600);
    });
});

// 清單項目點擊處理函數
function handleMeterClick(element) {
    const $item = $(element);
    const meterId = $item.data('meter-id');
    const title = $item.find('.dashboard-list-title').text().trim();
    const meta = $item.find('.dashboard-list-meta').text().trim();
    
    // 解析數據（格式：每月 | 2025-11-13 | 1天(30h)）
    const metaParts = meta.split('|').map(s => s.trim());
    const cycle = metaParts[0] || '';
    const nextCheck = metaParts[1] || '';
    
    // 解析標題（格式：地點-場域-錶盤類型）
    const titleParts = title.split('-');
    const location = titleParts[0] || '';
    const area = titleParts[1] || '';
    const type = titleParts[2] || '';
    
    // 存儲到全局變數，供 register-screen.js 使用
    window.currentEditMeter = {
        id: meterId,
        location: location,
        area: area,
        type: type,
        cycle: cycle,
        nextCheck: nextCheck
    };
    
    // 切換到修改畫面
    if (typeof switchScreen === 'function') {
        switchScreen('register');
    }
}


/**
 * 通用模態框管理系統 v2.0
 * 提供統一的模態框開啟/關閉功能
 * 修復版本：解決 Modal 只能開啟一次、點擊無效等問題
 */

// 顯示模態框
function showModal(modalId) {
    const modal = document.getElementById(modalId);
    if (!modal) {
        console.warn(`[Modal] 找不到 Modal: ${modalId}`);
        return;
    }
    
    modal.classList.remove('hidden');
    // 清除內聯 display，交由類名控制（例如 .flex）
    modal.style.display = '';
    
    document.body.style.overflow = 'hidden';
    
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
}

// 關閉模態框
function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (!modal) {
        console.warn(`[Modal] 找不到 Modal: ${modalId}`);
        return;
    }
    
    modal.classList.add('hidden');
    // 清除內聯 display，避免影響下次僅移除 hidden 即可顯示
    modal.style.display = '';
    const openModals = document.querySelectorAll(
        '.fixed:not(.hidden)[id$="-modal"], .fixed:not(.hidden)[id$="Modal"], .fixed:not(.hidden)[id$="-help"], .fixed:not(.hidden)[id$="help"]'
    );
    if (openModals.length === 0) {
        document.body.style.overflow = 'auto';
    }
}

// 初始化模態框事件監聽
document.addEventListener('DOMContentLoaded', function() {
    // ESC 鍵關閉最上層的 Modal
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            const visibleModals = document.querySelectorAll(
                '.fixed:not(.hidden)[id$="-modal"], .fixed:not(.hidden)[id$="Modal"], .fixed:not(.hidden)[id$="-help"], .fixed:not(.hidden)[id$="help"]'
            );

            if (visibleModals.length > 0) {
                const topModal = visibleModals[visibleModals.length - 1];
                if (topModal && topModal.id) {
                    closeModal(topModal.id);
                }
            }
        }
    });
    
});


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
    
    // 移除 hidden 類別
    modal.classList.remove('hidden');
    
    // 設定顯示方式（使用 flex 讓內容置中）
    modal.style.display = 'flex';
    
    // 禁止背景滾動
    document.body.style.overflow = 'hidden';
    
    // 重新初始化 lucide icons
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
    
    // 加上 hidden 類別
    modal.classList.add('hidden');
    
    // 隱藏 Modal
    modal.style.display = 'none';
    
    // 檢查是否還有其他 Modal 開啟，如果沒有才恢復滾動
    const openModals = document.querySelectorAll('.fixed:not(.hidden)[id$="-modal"]');
    if (openModals.length === 0) {
        document.body.style.overflow = 'auto';
    }
    
    console.log(`[Modal] 已關閉: ${modalId}`);
}

// 初始化模態框事件監聽
document.addEventListener('DOMContentLoaded', function() {
    console.log('[Modal] 初始化 Modal 系統...');
    
    // ESC 鍵關閉最上層的 Modal
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            // 找到所有可見的 Modal（id 結尾是 -modal 的）
            const visibleModals = document.querySelectorAll('.fixed:not(.hidden)[id$="-modal"]');
            
            // 關閉最上層的 Modal（最後一個）
            if (visibleModals.length > 0) {
                const topModal = visibleModals[visibleModals.length - 1];
                if (topModal.id) {
                    closeModal(topModal.id);
                    console.log('[Modal] ESC 關閉 Modal:', topModal.id);
                }
            }
        }
    });
    
    console.log('[Modal] Modal 系統初始化完成');
    console.log('[Modal] 關閉方式：X 按鈕或 ESC 鍵');
});


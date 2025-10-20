/**
 * Modal.js v4.0 - 純粹的 Modal 顯示/隱藏管理
 * 
 * 設計理念：
 * - Modal 內容由伺服器端 PartialView 渲染
 * - JavaScript 只負責顯示/隱藏 Modal
 * - 不需要任何 AJAX 載入或 DOM 操作
 * 
 * 用法：
 * - showModal('modalId') - 顯示 Modal
 * - closeModal('modalId') - 關閉 Modal
 */

// 全域變數
let openModals = new Set();

/**
 * 顯示 Modal
 * @param {string} modalId - Modal 的 ID
 */
function showModal(modalId) {
    const modalElement = document.getElementById(modalId);
    
    if (!modalElement) {
        console.warn(`[Modal] 找不到模態框: ${modalId}`);
        return;
    }
    
    // 使用 Bootstrap Modal API
    const modal = new bootstrap.Modal(modalElement, {
        backdrop: 'static',
        keyboard: true
    });
    
    modal.show();
    
    // 記錄開啟的 Modal
    openModals.add(modalId);
    
    // 防止 body 滾動
    document.body.style.overflow = 'hidden';
    
    // 監聽關閉事件
    modalElement.addEventListener('hidden.bs.modal', () => {
        openModals.delete(modalId);
        if (openModals.size === 0) {
            document.body.style.overflow = '';
        }
    });
    
    // 重新初始化 Lucide 圖標
    if (window.lucide && lucide.createIcons) {
        lucide.createIcons();
    }
}

/**
 * 關閉 Modal
 * @param {string} modalId - Modal 的 ID
 */
function closeModal(modalId) {
    const modalElement = document.getElementById(modalId);
    
    if (!modalElement) {
        console.warn(`[Modal] 找不到模態框: ${modalId}`);
        return;
    }
    
    // 使用 Bootstrap Modal API 關閉
    const modal = bootstrap.Modal.getInstance(modalElement);
    if (modal) {
        modal.hide();
    } else {
        // 如果沒有實例，直接隱藏
        modalElement.classList.remove('show');
        modalElement.style.display = 'none';
        document.body.classList.remove('modal-open');
        
        // 移除 backdrop
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) {
            backdrop.remove();
        }
    }
    
    // 從開啟列表移除
    openModals.delete(modalId);
    
    // 恢復 body 滾動
    if (openModals.size === 0) {
        document.body.style.overflow = '';
    }
}

/**
 * 初始化所有 Modal
 * 在頁面載入時自動執行
 */
function initModals() {
    // 監聽所有 Modal 的關閉事件
    document.querySelectorAll('.modal').forEach(modal => {
        modal.addEventListener('hidden.bs.modal', () => {
            const modalId = modal.id;
            openModals.delete(modalId);
            if (openModals.size === 0) {
                document.body.style.overflow = '';
            }
        });
    });
    
    // 監聽 ESC 鍵關閉最上層 Modal
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && openModals.size > 0) {
            const topModal = Array.from(openModals).pop();
            closeModal(topModal);
        }
    });
}

// 頁面載入時初始化
document.addEventListener('DOMContentLoaded', initModals);

// 全域暴露函數
window.showModal = showModal;
window.closeModal = closeModal;
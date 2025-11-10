/**
 * Modal.js v5.1 - 統一的 Modal 顯示/隱藏與 AJAX 載入管理
 * 
 * 設計理念：
 * - 支援靜態 Modal 直接顯示
 * - 支援 AJAX 載入 PartialView 後顯示
 * - 統一使用 jQuery 語法（符合架構規範）
 * - 使用 Bootstrap Modal API
 * - 自動在 Modal 完全顯示後初始化 Lucide 圖標
 * 
 * 用法：
 * - showModal('modalId') - 直接顯示 Modal
 * - showModal('modalId', { url, method, data, container }) - AJAX 載入後顯示
 * - closeModal('modalId') - 關閉 Modal
 * 
 * 更新日誌：
 * v5.1 - 修正 Lucide 圖標初始化時機，改在 'shown.bs.modal' 事件後執行
 */

// 全域變數
let openModals = new Set();

/**
 * 顯示 Modal
 * @param {string} modalId - Modal 的 ID
 * @param {object} options - 可選配置
 * @param {string} options.url - AJAX 請求 URL
 * @param {string} options.method - HTTP 方法（預設 GET）
 * @param {object} options.data - 請求資料
 * @param {string} options.container - 要替換內容的容器 ID（預設與 modalId 相同）
 */
const showModal = (modalId, options = null) => {
    // 場景 1：直接顯示靜態 Modal
    if (!options || !options.url) {
        displayModal(modalId);
        return;
    }
    
    // 場景 2：先顯示外殼，再 AJAX 置換內容（可驗證樣式/顯示問題）
    const { url, method = 'GET', data = {}, container = null } = options;

    // 先顯示 Modal 外殼（即使資料未到也先顯示，以排除「資料不存在」因素）
    displayModal(modalId);
    
    $.ajax({
        type: method,
        url: url,
        data: data,
        success: (html) => {
            // 替換容器內容
            const targetContainer = container || modalId;
            $(`#${targetContainer}`).html(html);
            // 此處不再重複呼叫 displayModal，避免閃爍與多次初始化
        },
        error: () => {
            // 保持已顯示的外殼，僅提示錯誤，方便驗證樣式
            alert('載入失敗，系統發生錯誤。');
        }
    });
};

/**
 * 實際顯示 Modal（內部函數）
 * @param {string} modalId - Modal 的 ID
 */
const displayModal = (modalId) => {
    const $modalElement = $(`#${modalId}`);
    
    if ($modalElement.length === 0) {
        console.warn(`[Modal] 找不到模態框: ${modalId}`);
        return;
    }
    
    // 使用 Bootstrap Modal API
    const modalElement = $modalElement[0];
    const modal = new bootstrap.Modal(modalElement, {
        backdrop: 'static',
        keyboard: true
    });
    
    // 監聽 Modal 完全顯示後的事件
    $modalElement.one('shown.bs.modal', () => {
        // 重新初始化 Lucide 圖標（Modal 完全顯示後）
        window.lucide?.createIcons?.();
    });
    
    modal.show();
    
    // 記錄開啟的 Modal
    openModals.add(modalId);
    
    // 防止 body 滾動
    $('body').css('overflow', 'hidden');
    
    // 監聽關閉事件
    $modalElement.on('hidden.bs.modal', () => {
        openModals.delete(modalId);
        if (openModals.size === 0) {
            $('body').css('overflow', '');
        }
    });
};

/**
 * 關閉 Modal
 * @param {string} modalId - Modal 的 ID
 */
const closeModal = (modalId) => {
    const $modalElement = $(`#${modalId}`);
    
    if ($modalElement.length === 0) {
        console.warn(`[Modal] 找不到模態框: ${modalId}`);
        return;
    }
    
    // 使用 Bootstrap Modal API 關閉
    const modalElement = $modalElement[0];
    const modal = bootstrap.Modal.getInstance(modalElement);
    
    if (modal) {
        modal.hide();
    } else {
        // 如果沒有實例，直接隱藏
        $modalElement.removeClass('show').css('display', 'none');
        $('body').removeClass('modal-open');
        
        // 移除 backdrop
        $('.modal-backdrop').remove();
    }
    
    // 從開啟列表移除
    openModals.delete(modalId);
    
    // 恢復 body 滾動
    if (openModals.size === 0) {
        $('body').css('overflow', '');
    }
};

/**
 * 初始化所有 Modal
 * 在頁面載入時自動執行
 */
const initModals = () => {
    // 監聽所有 Modal 的關閉事件
    $('.modal').each(function() {
        const $modal = $(this);
        $modal.on('hidden.bs.modal', () => {
            const modalId = $modal.attr('id');
            openModals.delete(modalId);
            if (openModals.size === 0) {
                $('body').css('overflow', '');
            }
        });
    });
    
    // 監聽 ESC 鍵關閉最上層 Modal
    $(document).on('keydown', (e) => {
        if (e.key === 'Escape' && openModals.size > 0) {
            const topModal = Array.from(openModals).pop();
            closeModal(topModal);
        }
    });
};

// 頁面載入時初始化
$(document).ready(initModals);

// 全域暴露函數
window.showModal = showModal;
window.closeModal = closeModal;
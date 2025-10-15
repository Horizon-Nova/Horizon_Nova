/**
 * =============================================================================
 * 通用模態框管理系統 v2.0
 * =============================================================================
 * 提供統一的模態框開啟/關閉功能
 * 修復版本：解決 Modal 只能開啟一次、點擊無效等問題
 * 
 * =============================================================================
 * 使用方式
 * =============================================================================
 * 
 * 1. 基本使用
 * ------------
 * // 開啟 Modal
 * showModal('modalId');
 * 
 * // 關閉 Modal
 * closeModal('modalId');
 * 
 * 
 * 2. HTML 結構要求
 * ----------------
 * Modal 容器必須包含以下屬性：
 * - id: Modal 的唯一識別碼
 * - class: 必須包含 'hidden' 和 'fixed'
 * 
 * 範例：
 * <div id="myModal" class="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center hidden">
 *     <div class="bg-white rounded-lg p-6">
 *         <!-- Modal 內容 -->
 *         <button onclick="closeModal('myModal')">關閉</button>
 *     </div>
 * </div>
 * 
 * 
 * 3. 按鈕綁定範例
 * ----------------
 * <!-- 開啟按鈕 -->
 * <button onclick="showModal('userFormModal')">新增用戶</button>
 * 
 * <!-- 關閉按鈕 -->
 * <button onclick="closeModal('userFormModal')">取消</button>
 * 
 * <!-- 切換 Modal (先關閉再開啟) -->
 * <button onclick="closeModal('detailModal'); showModal('editModal')">編輯</button>
 * 
 * 
 * 4. 命名規範建議
 * ----------------
 * 為了統一管理，建議使用以下命名規範：
 * 
 * - 表單類: {feature}FormModal      例如: userFormModal, roleFormModal
 * - 詳情類: {feature}DetailModal    例如: userDetailModal, roleDetailModal
 * - 說明類: {feature}-help          例如: aimodel-help, navigation-help
 * - 確認類: {action}-modal          例如: delete-modal, confirm-modal
 * - 預覽類: {feature}PreviewModal   例如: configPreviewModal
 * 
 * 
 * 5. 注意事項
 * ------------
 * DO (正確做法):
 * - 使用 showModal() 和 closeModal() 兩個標準函數
 * - Modal ID 必須唯一
 * - Modal 容器必須有 'hidden' class
 * - 按鈕使用 onclick="closeModal('id')" 或 onclick="showModal('id')"
 * 
 * DON'T (錯誤做法):
 * - 不要自定義 showXxxModal() 或 closeXxxModal() 等函數
 * - 不要使用 jQuery 的 .show() 或 .hide()
 * - 不要直接操作 display 樣式
 * - 不要忘記加 event.stopPropagation() 防止事件冒泡
 * 
 * 
 * 6. 進階用法
 * ------------
 * // 防止事件冒泡
 * <button onclick="event.stopPropagation(); showModal('helpModal')">
 *     說明
 * </button>
 * 
 * // 表單提交後關閉
 * function submitForm() {
 *     // 提交邏輯...
 *     closeModal('formModal');
 *     location.reload();
 * }
 * 
 * 
 * 7. 鍵盤支援
 * ------------
 * - ESC 鍵: 自動關閉最上層的 Modal
 * - 支援多層 Modal 管理
 * 
 * =============================================================================
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


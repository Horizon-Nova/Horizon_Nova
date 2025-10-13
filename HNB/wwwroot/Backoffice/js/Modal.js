/**
 * 通用模態框管理系統
 * 提供統一的模態框開啟/關閉功能
 */

// 顯示模態框
function showModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.remove('hidden');
        modal.style.display = 'flex';
        document.body.style.overflow = 'hidden';
        
        // 重新初始化 lucide icons
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }
    }
}

// 關閉模態框
function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.add('hidden');
        modal.style.display = 'none';
        document.body.style.overflow = 'auto';
    }
}

// 初始化模態框事件監聽
document.addEventListener('DOMContentLoaded', function() {
    // 1. 所有關閉/取消按鈕統一處理
    document.querySelectorAll('[id^="close"], [id*="cancel"], [id*="Cancel"]').forEach(btn => {
        btn.addEventListener('click', function() {
            const modal = this.closest('.fixed');
            if (modal && modal.id) {
                closeModal(modal.id);
            }
        });
    });

    // 2. 所有帶有 .modal-close 類的按鈕統一處理（"我知道了"按鈕）
    document.addEventListener('click', function(e) {
        if (e.target.classList.contains('modal-close') || e.target.closest('.modal-close')) {
            const button = e.target.classList.contains('modal-close') ? e.target : e.target.closest('.modal-close');
            const modal = button.closest('.fixed');
            if (modal && modal.id) {
                closeModal(modal.id);
            }
        }
    });

    // 3. 點擊背景關閉模態框
    document.addEventListener('click', function(e) {
        if (e.target.classList.contains('bg-black') && e.target.classList.contains('bg-opacity-50')) {
            const modal = e.target.closest('.fixed');
            if (modal && modal.id) {
                closeModal(modal.id);
            }
        }
    });

    // 4. ESC 鍵關閉模態框（只針對模態框，不包括側欄等其他 fixed 元素）
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            // 找到所有可見的模態框
            const visibleModals = document.querySelectorAll('.fixed:not(.hidden)');
            visibleModals.forEach(modal => {
                // 確認是模態框（有背景遮罩）
                if (modal.classList.contains('bg-black') && modal.classList.contains('bg-opacity-50') && modal.id) {
                    closeModal(modal.id);
                }
            });
        }
    });
});


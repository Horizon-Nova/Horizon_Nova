/**
 * 使用方式（所有參數皆為必要或可選）：
 *
 * showModal('modalId');
 *
 * showModal('modalId', {
 *   url: '/Backoffice/PermissionManagement/LoadUserDetail',
 *   method: 'GET',                 // 選填，預設 GET
 *   data: { id: 1 },               // 選填，會併入 AJAX 請求
 *   container: 'modalBodyId'       // 選填，不填則覆寫整個 modalId
 * });
 *
 * closeModal('modalId');
 *
 * 注意：
 * - modalId 必須對應已存在於 DOM 的 Modal 外層元素。
 * - container 應指向 Modal 內部可被覆寫的元素（例如 modal-content 或 modal-body）。
 * - 此模組不做額外防呆，呼叫端需確保資料與標記一致。
 */
let openModals = new Set();

const showModal = (modalId, options = null) => {
    if (!options || !options.url) {
        displayModal(modalId);
        return;
    }

    const { url, method = 'GET', data = {}, container = modalId } = options;
    const $container = $(`#${container}`);

    $.ajax({
        type: method,
        url,
        data,
        success: (html) => {
            const $html = $(html);
            const $fullModal = $html.filter(`#${modalId}`).add($html.find(`#${modalId}`));

            if ($fullModal.length > 0) {
                $(`#${modalId}`).replaceWith($fullModal.first());
                displayModal(modalId);
                return;
            }

            $container.html(html);
            displayModal(modalId);
        },
        error: () => {
            $container.html('<div class="alert alert-danger small mb-0">資料載入失敗，請稍後再試。</div>');
            displayModal(modalId);
        }
    });
};

const displayModal = (modalId) => {
    const $modalElement = $(`#${modalId}`);

    const modalElement = $modalElement[0];
    const modal = new bootstrap.Modal(modalElement, {
        backdrop: 'static',
        keyboard: true
    });
    
    $modalElement.one('shown.bs.modal', () => {
        window.lucide?.createIcons?.();
    });
    
    modal.show();
    
    openModals.add(modalId);
    
    $('body').css('overflow', 'hidden');
    
    $modalElement.on('hidden.bs.modal', () => {
        openModals.delete(modalId);
        if (openModals.size === 0) {
            $('body').css('overflow', '');
        }
    });
};

const closeModal = (modalId) => {
    const $modalElement = $(`#${modalId}`);

    const modalElement = $modalElement[0];
    const modal = bootstrap.Modal.getInstance(modalElement);
    
    if (modal) {
        modal.hide();
    } else {
        $modalElement.removeClass('show').css('display', 'none');
        $('body').removeClass('modal-open');
        
        $('.modal-backdrop').remove();
    }
    
    openModals.delete(modalId);
    
    if (openModals.size === 0) {
        $('body').css('overflow', '');
    }
};

const initModals = () => {
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
    
    $(document).on('keydown', (e) => {
        if (e.key === 'Escape' && openModals.size > 0) {
            const topModal = Array.from(openModals).pop();
            closeModal(topModal);
        }
    });
};

$(document).ready(initModals);

window.showModal = showModal;
window.closeModal = closeModal;
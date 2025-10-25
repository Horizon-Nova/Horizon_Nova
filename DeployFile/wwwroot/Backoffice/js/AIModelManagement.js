/**
 * AI 模型管理系統 JavaScript
 * AI Model Management JS
 */

// 刪除 AI 服務
function deleteAIService(id) {
    if (!confirm('確定要刪除此 AI 服務嗎？')) {
        return;
    }

    $.ajax({
        type: 'POST',
        url: '/Backoffice/AIModel/Delete',
        data: { id: id },
        success: (response) => {
            if (response && response.success) {
                alert('刪除成功');
                location.reload();
            } else {
                alert(response && response.message ? response.message : '刪除失敗');
            }
        },
        error: () => {
            alert('系統發生錯誤');
        }
    });
}

// 載入 AI 配置詳情 Modal
function loadAIConfigModal(id = null) {
    showModal('aiConfigModal', {
        url: '/Backoffice/AIModel/LoadDetail',
        method: 'GET',
        data: { id: id },
        container: 'aiModal'
    });
}

// 儲存 AI 配置
function saveAIConfig() {
    const $form = $('#aiConfigForm');
    if ($form.length === 0) return;

    const formElement = $form[0];
    if (!formElement.checkValidity()) {
        formElement.reportValidity();
        return;
    }

    $.ajax({
        type: 'POST',
        url: '/Backoffice/AIModel/SubmitConfig',
        data: $form.serialize(),
        success: (response) => {
            if (response && response.success) {
                alert('儲存成功');
                location.reload();
            } else {
                alert(response && response.message ? response.message : '儲存失敗');
            }
        },
        error: () => {
            alert('系統發生錯誤');
        }
    });
}

// 模型類型切換配置顯示
function toggleModelConfig() {
    const $modelType = $('#modelType');
    if ($modelType.length === 0) return;

    $modelType.on('change', function() {
        const type = $(this).val();
        const $textConfig = $('#textConfig');
        const $imageConfig = $('#imageConfig');

        if (type === 'text') {
            $textConfig.removeClass('d-none');
            $imageConfig.addClass('d-none');
        } else if (type === 'image') {
            $textConfig.addClass('d-none');
            $imageConfig.removeClass('d-none');
        } else {
            $textConfig.addClass('d-none');
            $imageConfig.addClass('d-none');
        }
    });
}

// 頁面載入完成後初始化
$(document).ready(function() {
    if (window.lucide) {
        window.lucide.createIcons();
    }

    // 初始化模型類型切換
    toggleModelConfig();

    // 測試所有連線按鈕
    $('#testAllConnectionsBtn').on('click', function() {
        alert('測試所有連線功能開發中...');
    });

    // 刷新統計按鈕
    $('#refreshStatsBtn').on('click', function() {
        location.reload();
    });

    // 匯出配置按鈕
    $('#exportConfigBtn').on('click', function() {
        alert('匯出配置功能開發中...');
    });

    // 搜尋功能
    $('#searchInput').on('input', function() {
        const searchTerm = $(this).val().toLowerCase();
        $('[data-service-id]').each(function() {
            const $card = $(this);
            const text = $card.text().toLowerCase();
            if (text.includes(searchTerm)) {
                $card.show();
            } else {
                $card.hide();
            }
        });
    });
});


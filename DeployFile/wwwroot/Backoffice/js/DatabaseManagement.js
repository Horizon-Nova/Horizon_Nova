/**
 * 資料庫管理系統 JavaScript - 基本功能版本
 * Database Management JS - Basic Functions Only
 */


// 顯示操作結果
function showResult(message, type = 'info') {
    const $resultDiv = $('#operationResult');
    if ($resultDiv.length === 0) return;

    let icon = '';
    let alertClass = '';

    switch (type) {
        case 'success':
            icon = 'check-circle';
            alertClass = 'alert-success';
            break;
        case 'error':
            icon = 'x-circle';
            alertClass = 'alert-danger';
            break;
        case 'warning':
            icon = 'alert-triangle';
            alertClass = 'alert-warning';
            break;
        case 'loading':
            icon = 'loader-2';
            alertClass = 'alert-info';
            break;
        default:
            icon = 'info';
            alertClass = 'alert-info';
            break;
    }

    const spinnerClass = type === 'loading' ? 'spinner-border spinner-border-sm' : '';
    const iconHtml = type === 'loading' 
        ? `<div class="${spinnerClass}" role="status"><span class="visually-hidden">載入中...</span></div>`
        : `<i data-lucide="${icon}" style="width: 1.25rem; height: 1.25rem;"></i>`;

    $resultDiv.html(`
        <div class="alert ${alertClass} d-flex align-items-center" role="alert">
            ${iconHtml}
            <span class="ms-2">${message}</span>
        </div>
    `);

    if (window.lucide && type !== 'loading') {
        window.lucide.createIcons();
    }
}

// 啟用備份按鈕
function enableGenerateButton() {
    const $generateBtn = $('#btnBackup');
    $generateBtn.prop('disabled', false);
    updateStepStatus(4, true);
}

// 禁用備份按鈕
function disableGenerateButton() {
    const $generateBtn = $('#btnBackup');
    $generateBtn.prop('disabled', true);
    updateStepStatus(4, false);
}

// 更新步驟狀態
function updateStepStatus(stepNumber, isActive) {
    const $stepElement = $(`.vstack .d-flex:nth-child(${stepNumber}) .badge`);
    if (isActive) {
        $stepElement.removeClass('bg-secondary').addClass('bg-primary');
    } else {
        $stepElement.removeClass('bg-primary').addClass('bg-secondary');
    }
}

// 測試連線 (使用 jQuery 序列化)
function testConnection(event) {
    event.preventDefault();

    const provider = $('#databaseType').val();
    const connectionString = $('#connectionString').val();

    if (!provider) {
        showResult('請選擇資料庫類型', 'error');
        disableGenerateButton();
        return;
    }

    if (!connectionString.trim()) {
        showResult('請填寫連線字串', 'error');
        disableGenerateButton();
        return;
    }

    showResult('測試連線中...', 'loading');
    disableGenerateButton();

    $.ajax({
        type: 'POST',
        url: '/Backoffice/Database/TestConnection',
        data: $('#FormData').serialize(),
        success: (response) => {
            if (response && response.success) {
                showResult('連線成功！正在獲取資料表...', 'success');
                enableGenerateButton();
                loadDatabaseTables();
            } else {
                const message = response && response.Message ? response.Message : '未知錯誤';
                showResult(`連線失敗: ${message}`, 'error');
                disableGenerateButton();
            }
        },
        error: () => {
            showResult('連線失敗，請稍後再試。', 'error');
            disableGenerateButton();
        }
    });
}

// 備份資料表
function backupTables(event) {
    event.preventDefault();

    const provider = $('#databaseType').val();
    const connectionString = $('#connectionString').val();
    const outputPath = $('#outputPath').val();

    if (!provider) {
        showResult('請選擇資料庫類型', 'error');
        return;
    }

    if (!connectionString.trim()) {
        showResult('請填寫連線字串', 'error');
        return;
    }

    if (!outputPath.trim()) {
        showResult('請填寫輸出位置', 'error');
        return;
    }
    let contextName = '';
    const databaseMatch = connectionString.match(/Database=([^;]+)/i);
    if (databaseMatch) {
        contextName = databaseMatch[1] + 'DbContext';
    }

    showResult('正在備份資料表...', 'loading');
    const $form = $('#FormData');
    $form.append(`<input type="hidden" name="ContextName" value="${contextName}">`);
    $form.append(`<input type="hidden" name="OutputDirectory" value="${outputPath}">`);

    $.ajax({
        type: 'POST',
        url: '/Backoffice/Database/SubmitBackup',
        data: $form.serialize(),
        success: (response) => {
            if (response && response.success) {
                showResult('資料表備份成功！', 'success');
            } else {
                const message = response && response.message ? response.message : '未知錯誤';
                showResult(`備份失敗: ${message}`, 'error');
            }
        },
        error: () => {
            showResult('備份失敗，請稍後再試。', 'error');
        },
        complete: () => {
            $form.find('input[name="ContextName"]').remove();
            $form.find('input[name="OutputDirectory"]').remove();
        }
    });
}

// 載入資料庫資料表
function loadDatabaseTables() {
    const provider = $('#databaseType').val();
    const connectionString = $('#connectionString').val();

    if (!provider || !connectionString.trim()) {
        showResult('請先填寫連線資訊', 'error');
        return;
    }

    showResult('正在獲取資料表...', 'loading');

    $.ajax({
        type: 'POST',
        url: '/Backoffice/Database/LoadDatabaseTables',
        data: $('#FormData').serialize(),
        success: (response) => {
            if (response && response.success) {
                displayDatabaseTables(response.tables);
                showTableManagement();
                showResult(`成功獲取 ${response.tables.length} 個資料表`, 'success');
            } else {
                const message = response && response.message ? response.message : '獲取資料表失敗';
                showResult(`獲取資料表失敗: ${message}`, 'error');
            }
        },
        error: () => {
            showResult('獲取資料表失敗，請稍後再試。', 'error');
        }
    });
}

// 顯示資料表列表
function displayDatabaseTables(tables) {
    const $container = $('#databaseTablesContainer');

    if (!tables || tables.length === 0) {
        $container.html(`
            <div class="text-center py-5">
                <div class="p-4 bg-light rounded-circle d-inline-flex align-items-center justify-content-center mb-3" style="width: 4rem; height: 4rem;">
                    <i data-lucide="database" class="text-muted" style="width: 2rem; height: 2rem;"></i>
                </div>
                <h4 class="h5 mb-2">沒有找到資料表</h4>
                <p class="text-muted">此資料庫中沒有可用的資料表</p>
            </div>
        `);
        if (window.lucide) {
            window.lucide.createIcons();
        }
        return;
    }

    let html = '<div class="row g-3">';

    tables.forEach(table => {
        html += `
            <div class="col-md-6 col-lg-4">
                <div class="card h-100">
                    <div class="card-body">
                        <div class="d-flex align-items-center justify-content-between mb-2">
                            <h5 class="card-title h6 mb-0">${table}</h5>
                            <button type="button" onclick="showTableDetails('${table}')" 
                                    class="btn btn-sm btn-outline-primary" 
                                    title="查看詳情">
                                <i data-lucide="eye" style="width: 1rem; height: 1rem;"></i>
                            </button>
                        </div>
                        <p class="card-text text-muted small mb-0">資料表</p>
                    </div>
                </div>
            </div>
        `;
    });

    html += '</div>';
    $container.html(html);

    if (window.lucide) {
        window.lucide.createIcons();
    }
}

// 顯示資料表管理區域
function showTableManagement() {
    $('#tableManagement').show();
}

// 隱藏資料表管理區域
function hideTableManagement() {
    $('#tableManagement').hide();
}

// 顯示資料表詳情
function showTableDetails(tableName) {
    const provider = $('#databaseType').val();
    const connectionString = $('#connectionString').val();

    if (!provider || !connectionString) {
        showResult('請先測試連線以獲取資料表詳情', 'warning');
        return;
    }

    showResult(`正在載入資料表 "${tableName}" 的詳情...`, 'info');
    
    showModal('tableDetailsModal', {
        url: '/Backoffice/Database/LoadDetail',
        method: 'GET',
        data: { 
            tableName: tableName,
            provider: provider,
            connectionString: connectionString
        },
        container: 'databaseModal'
    });
}


// 重新整理狀態
function refreshStatus() {
    location.reload();
}

// 頁面載入完成後初始化
$(document).ready(function () {
    if (window.lucide) {
        window.lucide.createIcons();
    }

    disableGenerateButton();

    $('#databaseType, #connectionString').on('change input', function () {
        disableGenerateButton();
    });
});
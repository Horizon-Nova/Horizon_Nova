/**
 * 資料庫管理系統 JavaScript - 基本功能版本
 * Database Management JS - Basic Functions Only
 */


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
function testConnection() {
    const provider = $('#databaseType').val();
    const connectionString = $('#connectionString').val();

    if (!provider) {
        showToast('[失敗] 請選擇資料庫類型', 'error');
        disableGenerateButton();
        return;
    }

    if (!connectionString.trim()) {
        showToast('[失敗] 請填寫連線字串', 'error');
        disableGenerateButton();
        return;
    }

    showToast('[資訊] 測試連線中...', 'info');
    disableGenerateButton();

    $.ajax({
        type: 'POST',
        url: '/Backoffice/Database/TestConnection',
        data: $('#FormData').serialize(),
        success: (response) => {
            if (response?.success) {
                showToast('[成功] 連線成功，正在載入資料表', 'success');
                enableGenerateButton();
                loadDatabaseTables();
            } else {
                const message = response?.message ?? response?.Message ?? '未知錯誤';
                showToast(`[失敗] 連線失敗：${message}`, 'error');
                disableGenerateButton();
            }
        },
        error: () => {
            showToast('[失敗] 連線失敗，請稍後再試', 'error');
            disableGenerateButton();
        }
    });
}

// 備份資料表
function backupTables() {
    const provider = $('#databaseType').val();
    const connectionString = $('#connectionString').val();
    const outputPath = $('#outputPath').val();

    if (!provider) {
        showToast('[失敗] 請選擇資料庫類型', 'error');
        return;
    }

    if (!connectionString.trim()) {
        showToast('[失敗] 請填寫連線字串', 'error');
        return;
    }

    if (!outputPath.trim()) {
        showToast('[失敗] 請填寫輸出位置', 'error');
        return;
    }
    let contextName = '';
    const databaseMatch = connectionString.match(/Database=([^;]+)/i);
    if (databaseMatch) {
        contextName = databaseMatch[1] + 'DbContext';
    }

    showToast('[資訊] 正在備份資料表...', 'info');
    const $form = $('#FormData');
    $form.append(`<input type="hidden" name="ContextName" value="${contextName}">`);
    $form.append(`<input type="hidden" name="OutputDirectory" value="${outputPath}">`);

    $.ajax({
        type: 'POST',
        url: '/Backoffice/Database/SubmitBackup',
        data: $form.serialize(),
        success: (response) => {
            if (response?.success) {
                showToast('[成功] 資料表備份完成', 'success');
            } else {
                const message = response?.message ?? '未知錯誤';
                showToast(`[失敗] 備份失敗：${message}`, 'error');
            }
        },
        error: () => {
            showToast('[失敗] 備份失敗，請稍後再試', 'error');
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
        showToast('[失敗] 請先填寫連線資訊', 'error');
        return;
    }

    showToast('[資訊] 正在載入資料表...', 'info');

    $.ajax({
        type: 'POST',
        url: '/Backoffice/Database/LoadDatabaseTables',
        data: $('#FormData').serialize(),
        success: (response) => {
            if (response?.success) {
                displayDatabaseTables(response.tables);
                showTableManagement();
                const tableCount = response.tables?.length ?? 0;
                showToast(`[成功] 成功載入 ${tableCount} 個資料表`, 'success');
            } else {
                const message = response?.message ?? '獲取資料表失敗';
                showToast(`[失敗] 獲取資料表失敗：${message}`, 'error');
            }
        },
        error: () => {
            showToast('[失敗] 獲取資料表失敗，請稍後再試', 'error');
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
                            <button type="button"
                                    class="btn btn-sm btn-outline-primary js-db-table-detail"
                                    data-table="${table}"
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
        showToast('[失敗] 請先測試連線以獲取資料表詳情', 'error');
        return;
    }

    showToast(`[資訊] 正在載入資料表 ${tableName} 的詳情`, 'info');
    
    showModal('tableDetailsModal', {
        url: '/Backoffice/Database/LoadDetail',
        method: 'GET',
        data: { 
            tableName: tableName,
            provider: provider,
            connectionString: connectionString
        }
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

    $(document)
        .off('click.db', '.js-db-open-help')
        .on('click.db', '.js-db-open-help', function () {
            showModal('helpModal');
        })
        .off('click.db', '.js-db-test-connection')
        .on('click.db', '.js-db-test-connection', function (e) {
            e.preventDefault();
            testConnection();
        })
        .off('click.db', '.js-db-backup')
        .on('click.db', '.js-db-backup', function (e) {
            e.preventDefault();
            backupTables();
        })
        .off('click.db', '.js-db-table-detail')
        .on('click.db', '.js-db-table-detail', function () {
            const table = $(this).data('table');
            showTableDetails(table);
        });
});
/**
 * 資料庫管理系統 JavaScript - 基本功能版本
 * Database Management JS - Basic Functions Only
 */

// 顯示說明視窗
function showHelpModal() {
    const modal = document.getElementById('helpModal');
    if (modal) {
        const modalEl = new bootstrap.Modal(modal);
        modalEl.show();
    }
}

// 通用的 Modal 顯示函數
function showGenericModal(title, url, data, saveCallback) {
    const modal = document.getElementById('edModal');
    const modalEl = new bootstrap.Modal(modal);
    
    document.getElementById('edTitle').textContent = title;
    
    const modalBody = modal.querySelector('.modal-body');
    modalBody.innerHTML = `
        <div class="text-center py-4">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">載入中...</span>
            </div>
            <p class="mt-2 text-muted">載入中...</p>
                </div>
    `;
    
    const saveBtn = document.getElementById('btnEdSave');
    if (saveCallback) {
        saveBtn.style.display = 'block';
        saveBtn.onclick = saveCallback;
            } else {
        saveBtn.style.display = 'none';
    }
    
    modalEl.show();
    
    if (url) {
        loadModalContent(url, data);
    }
}

// 載入 Modal 內容
function loadModalContent(url, data) {
    const modalBody = document.querySelector('#edModal .modal-body');
    
    $.ajax({
        url: url,
        type: data ? 'POST' : 'GET',
        contentType: data ? 'application/json' : undefined,
        data: data ? JSON.stringify(data) : undefined,
        success: (response) => {
            modalBody.innerHTML = response;
    if (window.lucide) {
        window.lucide.createIcons();
    }
        },
        error: (xhr, status, error) => {
            console.error('載入內容失敗:', error);
            modalBody.innerHTML = `
                <div class="alert alert-danger">
                    <i data-lucide="alert-circle" class="w-4 h-4 me-2"></i>
                    載入失敗: ${error}
                </div>
            `;
    if (window.lucide) {
        window.lucide.createIcons();
    }
        }
    });
}

// 顯示操作結果
function showResult(message, type = 'info') {
    const resultDiv = document.getElementById('operationResult');
    if (!resultDiv) return;
    
    let icon = '';
    let bgColor = '';
    let textColor = '';

    switch (type) {
        case 'success':
            icon = 'check-circle';
            bgColor = 'bg-green-100';
            textColor = 'text-green-800';
            break;
        case 'error':
            icon = 'x-circle';
            bgColor = 'bg-red-100';
            textColor = 'text-red-800';
            break;
        case 'warning':
            icon = 'alert-triangle';
            bgColor = 'bg-yellow-100';
            textColor = 'text-yellow-800';
            break;
        case 'loading':
            icon = 'loader-2';
            bgColor = 'bg-blue-100';
            textColor = 'text-blue-800';
            break;
        default:
            icon = 'info';
            bgColor = 'bg-blue-100';
            textColor = 'text-blue-800';
            break;
    }

    resultDiv.innerHTML = `
        <div class="flex items-center p-3 rounded-lg ${bgColor} ${textColor}">
            <i data-lucide="${icon}" class="w-5 h-5 mr-3 ${type === 'loading' ? 'animate-spin' : ''}"></i>
            <span>${message}</span>
        </div>
    `;
    
    if (window.lucide) {
        window.lucide.createIcons();
    }
}

// 啟用生成模型按鈕
function enableGenerateButton() {
    const generateBtn = $('button[onclick="generateModels(event)"]');
    generateBtn.prop('disabled', false);
    generateBtn.removeClass('opacity-50 cursor-not-allowed');
    generateBtn.addClass('hover:bg-blue-600');
    
    // 更新步驟4的狀態
    updateStepStatus(4, true);
}

// 禁用生成模型按鈕
function disableGenerateButton() {
    const generateBtn = $('button[onclick="generateModels(event)"]');
    generateBtn.prop('disabled', true);
    generateBtn.addClass('opacity-50 cursor-not-allowed');
    generateBtn.removeClass('hover:bg-blue-600');
    updateStepStatus(4, false);
}

// 更新步驟狀態
function updateStepStatus(stepNumber, isActive) {
    const stepElement = $(`.space-y-2 .flex:nth-child(${stepNumber}) span:first-child`);
    if (isActive) {
        stepElement.removeClass('bg-slate-100 text-slate-400').addClass('bg-blue-100 text-blue-600');
    } else {
        stepElement.removeClass('bg-blue-100 text-blue-600').addClass('bg-slate-100 text-slate-400');
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
        url: '/HnbBackoffice/Database/TestConnection',
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

// 生成程式碼模型
function generateModels(event) {
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
    
    showResult('正在生成程式碼模型...', 'loading');
    const form = $('#FormData');
    form.append(`<input type="hidden" name="ContextName" value="${contextName}">`);
    form.append(`<input type="hidden" name="OutputDirectory" value="${outputPath}">`);

    $.ajax({
        type: 'POST',
        url: '/HnbBackoffice/Database/GenerateModels',
        data: form.serialize(),
        success: (response) => {
            if (response && response.success) {
                showResult('程式碼模型生成成功！', 'success');
            } else {
                const message = response && response.Message ? response.Message : '未知錯誤';
                showResult(`生成失敗: ${message}`, 'error');
            }
        },
        error: () => {
            showResult('生成失敗，請稍後再試。', 'error');
        },
        complete: () => {
            form.find('input[name="ContextName"]').remove();
            form.find('input[name="OutputDirectory"]').remove();
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
        url: '/HnbBackoffice/Database/LoadDatabaseTables',
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
    const container = document.getElementById('databaseTablesContainer');
    
    if (!tables || tables.length === 0) {
        container.innerHTML = `
            <div class="text-center py-12">
                <div class="p-4 bg-slate-100 rounded-full w-16 h-16 mx-auto mb-4 flex items-center justify-center">
                    <i data-lucide="database" class="w-8 h-8 text-slate-400"></i>
                </div>
                <h4 class="text-lg font-medium text-slate-900 mb-2">沒有找到資料表</h4>
                <p class="text-slate-500">此資料庫中沒有可用的資料表</p>
            </div>
        `;
        return;
    }
    
    let html = '<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">';
    
    tables.forEach(table => {
        html += `
            <div class="bg-slate-50 rounded-lg p-4 border border-slate-200 hover:border-slate-300 transition-colors">
                <div class="flex items-center justify-between mb-2">
                    <h5 class="font-medium text-slate-900">${table}</h5>
                    <div class="flex space-x-1">
                        <button type="button" onclick="showTableDetails('${table}')" 
                                class="p-1 text-slate-400 hover:text-blue-600 transition-colors" 
                                title="查看詳情">
                            <i data-lucide="eye" class="w-4 h-4"></i>
                        </button>
                    </div>
                </div>
                <p class="text-sm text-slate-500">資料表</p>
            </div>
        `;
    });
    
    html += '</div>';
    container.innerHTML = html;
    
    // 重新初始化圖標
    if (window.lucide) {
        window.lucide.createIcons();
    }
}

// 顯示資料表管理區域
function showTableManagement() {
    const tableManagement = document.getElementById('tableManagement');
    if (tableManagement) {
        tableManagement.style.display = 'block';
    }
}

// 隱藏資料表管理區域
function hideTableManagement() {
    const tableManagement = document.getElementById('tableManagement');
    if (tableManagement) {
        tableManagement.style.display = 'none';
    }
}

// 顯示資料表詳情
function showTableDetails(tableName) {
    const provider = document.getElementById('databaseType').value;
    const connectionString = document.getElementById('connectionString').value;
    
    if (!provider || !connectionString) {
        showResult('請先測試連線以獲取資料表詳情', 'warning');
        return;
    }
    
    // 顯示載入狀態
    showResult(`正在載入資料表 "${tableName}" 的詳情...`, 'info');
    displayTableDetailsModal(tableName, null);
}

// 顯示資料表詳情彈出視窗
function displayTableDetailsModal(tableName, columns) {
    // 使用 AJAX 載入部分視圖
    $.ajax({
        url: '/HnbBackoffice/Database/LoadTableDetailsPartial',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            Provider: document.getElementById('databaseType').value,
            ConnectionString: document.getElementById('connectionString').value,
            TableName: tableName
        }),
        success: (response) => {
            if (response) {
                // 清空容器並載入新的內容
                $('#tableDetailsModalContainer').html(response);
                
                // 更新標題
                const titleElement = document.getElementById('tableDetailsModalTitle');
                if (titleElement) {
                    titleElement.textContent = `資料表詳情: ${tableName}`;
                }
            } else {
                showResult('載入資料表詳情失敗', 'error');
            }
        },
        error: () => {
            showResult('載入資料表詳情失敗，請稍後再試。', 'error');
        }
    });
}


// 重新整理狀態
function refreshStatus() {
    location.reload();
}

// 頁面載入完成後初始化
$(document).ready(function() {
    if (window.lucide) {
        window.lucide.createIcons();
    }
    
    // 初始化時禁用生成模型按鈕
    disableGenerateButton();
    
    // 監聽表單欄位變更
    $('#databaseType, #connectionString').on('change input', function() {
        // 當表單欄位變更時，禁用生成按鈕（需要重新測試連線）
        disableGenerateButton();
    });
});
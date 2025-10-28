// Designer 主要腳本
// 整合 GrapesJS、主題管理、工具列、面板調整等功能

// ============================================
// 全域變數
// ============================================
let editor = null;
let currentPageId = null;
let currentPageTitle = 'Untitled Page';
let openTabs = [];  // 記錄打開的分頁


// ============================================
// Toast 通知函數
// ============================================
function showError(message) {
    $('#errorToastMessage').text(message);
    const toast = new bootstrap.Toast($('#errorToast')[0]);
    toast.show();
}

function showSuccess(message) {
    $('#successToastMessage').text(message);
    const toast = new bootstrap.Toast($('#successToast')[0]);
    toast.show();
}

// ============================================
// 初始化
// ============================================
$(function() {
    initGrapesJS();
    initComponentBlocks();
    bridgeToCustomUI();
    initDarkMode();
    loadPagesList(); // 載入頁面列表
    
    // URL 參數檢查
    const urlParams = new URLSearchParams(window.location.search);
    const editId = urlParams.get('edit');
    if (editId) {
        // 載入頁面並加入到分頁列表
        setTimeout(() => {
            loadPage(editId);
            // 頁面載入成功後會在 loadPage 的 success 回調中加入分頁
        }, 500);
    }
});

// ============================================
// GrapesJS 初始化
// ============================================
function initGrapesJS() {
    editor = grapesjs.init({
        container: '#canvas',
        height: '100%',
        width: 'auto',
        
        // 隱藏 GrapesJS 預設 UI
        panels: { defaults: [] },
        
        canvas: {
            styles: [],
            scripts: []
        },
        
        storageManager: false,
        
        // 使用 translate 模式（流式布局），而非 absolute（自由定位）
        // translate: 元素按 HTML 順序排列，支援響應式
        // absolute: 元素可自由定位，但響應式會有問題
        
        // Block Manager
        blockManager: {
            appendTo: '#tab-components',
        },
        
        // Style Manager
        styleManager: {
            appendTo: '#grapesjs-traits-container',
            sectors: [
                {
                    name: '尺寸',
                    open: true,
                    buildProps: ['width', 'height', 'max-width', 'min-height', 'margin', 'padding']
                },
                {
                    name: '文字',
                    open: false,
                    buildProps: ['font-family', 'font-size', 'font-weight', 'letter-spacing', 'color', 'line-height', 'text-align']
                },
                {
                    name: '背景',
                    open: false,
                    buildProps: ['background-color', 'background-image', 'background-repeat', 'background-position', 'background-size']
                },
                {
                    name: '邊框',
                    open: false,
                    buildProps: ['border-radius', 'border', 'border-width', 'border-style', 'border-color']
                },
                {
                    name: '陰影與效果',
                    open: false,
                    buildProps: ['box-shadow', 'opacity', 'transition']
                },
                {
                    name: '布局',
                    open: false,
                    buildProps: ['display', 'position', 'top', 'right', 'bottom', 'left', 'flex-direction', 'justify-content', 'align-items']
                }
            ]
        },
        
        // Layer Manager
        layerManager: {
            appendTo: '#tab-layers',
        },
        
        // 裝置管理器（響應式）
        deviceManager: {
            devices: [
                { name: 'Desktop', width: '' },
                { name: 'Tablet', width: '768px' },
                { name: 'Mobile', width: '375px' }
            ]
        },
        
        plugins: ['gjs-blocks-basic'],
        
        // 初始內容
        components: `
            <div class="text-center text-muted py-5" style="display: flex; flex-direction: column; align-items: center; gap: 16px; margin-top: 100px;">
                <div style="font-size: 48px; opacity: 0.3;">
                    <i data-lucide="sparkles" style="width: 48px; height: 48px;"></i>
                </div>
                <p style="font-size: 18px; margin: 0;">從左側拖曳元素到這裡開始設計</p>
                <p style="font-size: 14px; margin: 0; opacity: 0.6;">選擇一個組件開始建立您的頁面</p>
            </div>
        `,
    });
    
    // 事件監聽
    editor.on('component:selected', updatePropertiesPanel);
    
    // 拖曳組件後移除提示文字
    editor.on('block:drag:stop', function(component) {
        const emptyMsg = editor.getWrapper().find('.text-center.text-muted');
        if (emptyMsg.length) {
            emptyMsg[0].remove();
        }
    });
}

// ============================================
// GrapesJS 組件定義
// ============================================
function initComponentBlocks() {
    const bm = editor.BlockManager;
    bm.getAll().reset();
    
    // 基礎組件
    bm.add('heading', {
        label: '<i data-lucide="heading" style="width:16px;height:16px;"></i> 標題',
        category: '基礎組件',
        content: '<h2 class="mb-3">標題文字</h2>'
    });
    
    bm.add('text', {
        label: '<i data-lucide="type" style="width:16px;height:16px;"></i> 文字',
        category: '基礎組件',
        content: '<p class="mb-0">這是一段文字</p>'
    });
    
    bm.add('button', {
        label: '<i data-lucide="square" style="width:16px;height:16px;"></i> 按鈕',
        category: '基礎組件',
        content: '<button class="btn btn-primary">按鈕文字</button>'
    });
    
    bm.add('image', {
        label: '<i data-lucide="image" style="width:16px;height:16px;"></i> 圖片',
        category: '基礎組件',
        content: '<img src="https://via.placeholder.com/300x200" class="img-fluid" alt="示例圖片">'
    });
    
    bm.add('icon', {
        label: '<i data-lucide="star" style="width:16px;height:16px;"></i> 圖標',
        category: '基礎組件',
        content: '<i data-lucide="star" style="width: 48px; height: 48px;"></i>'
    });
    
    bm.add('spacer', {
        label: '<i data-lucide="minus" style="width:16px;height:16px;"></i> 間距',
        category: '基礎組件',
        content: '<div class="spacer" style="height: 40px;"></div>'
    });
    
    // 布局組件
    bm.add('container', {
        label: '<i data-lucide="box" style="width:16px;height:16px;"></i> 容器',
        category: '布局組件',
        content: {
            type: 'container',
            droppable: true,
            classes: ['container-fluid', 'py-4'],
            components: '<p class="text-muted text-center mb-0">拖曳元素到這裡</p>'
        }
    });
    
    bm.add('row', {
        label: '<i data-lucide="columns" style="width:16px;height:16px;"></i> 行',
        category: '布局組件',
        content: {
            type: 'row',
            droppable: '.col, .col-md-6, .col-lg-4',
            classes: ['row', 'g-3'],
            components: [{
                type: 'column',
                classes: ['col-12'],
                components: '<p class="text-muted text-center mb-0">拖曳 Column 到這裡</p>'
            }]
        }
    });
    
    bm.add('column', {
        label: '<i data-lucide="sidebar" style="width:16px;height:16px;"></i> 列',
        category: '布局組件',
        content: {
            type: 'column',
            droppable: true,
            classes: ['col-md-6'],
            components: '<p class="text-muted text-center mb-0 p-3 border">Column (6/12)</p>'
        }
    });
    
    bm.add('section', {
        label: '<i data-lucide="layout" style="width:16px;height:16px;"></i> 區域',
        category: '布局組件',
        content: {
            type: 'section',
            droppable: true,
            classes: ['py-5'],
            components: {
                type: 'container',
                droppable: true,
                classes: ['container'],
                components: '<p class="text-muted text-center mb-0">Section - 拖曳元素到這裡</p>'
            }
        }
    });
    
    bm.add('card', {
        label: '<i data-lucide="credit-card" style="width:16px;height:16px;"></i> 卡片',
        category: '布局組件',
        content: `
            <div class="card" style="max-width: 18rem;">
                <div class="card-body">
                    <h5 class="card-title">卡片標題</h5>
                    <p class="card-text mb-3">這是卡片的內容文字</p>
                    <a href="#" class="btn btn-primary btn-sm">按鈕</a>
                </div>
            </div>
        `
    });
    
    // 導航組件
    bm.add('navbar', {
        label: '<i data-lucide="menu" style="width:16px;height:16px;"></i> 導航欄',
        category: '導航組件',
        content: `
            <nav class="navbar navbar-expand-lg navbar-light bg-light">
                <div class="container-fluid">
                    <a class="navbar-brand" href="#">Logo</a>
                    <button class="navbar-toggler" type="button">
                        <span class="navbar-toggler-icon"></span>
                    </button>
                    <div class="collapse navbar-collapse">
                        <ul class="navbar-nav ms-auto">
                            <li class="nav-item"><a class="nav-link active" href="#">首頁</a></li>
                            <li class="nav-item"><a class="nav-link" href="#">產品</a></li>
                            <li class="nav-item"><a class="nav-link" href="#">關於</a></li>
                        </ul>
                    </div>
                </div>
            </nav>
        `
    });
    
    // 表單組件
    bm.add('input', {
        label: '<i data-lucide="text-cursor-input" style="width:16px;height:16px;"></i> 輸入框',
        category: '表單組件',
        content: `
            <div class="mb-3">
                <label class="form-label">標籤</label>
                <input type="text" class="form-control" placeholder="請輸入...">
            </div>
        `
    });
    
    bm.add('textarea', {
        label: '<i data-lucide="align-left" style="width:16px;height:16px;"></i> 文本域',
        category: '表單組件',
        content: `
            <div class="mb-3">
                <label class="form-label">文本域</label>
                <textarea class="form-control" rows="3" placeholder="請輸入..."></textarea>
            </div>
        `
    });
    
    bm.add('select', {
        label: '<i data-lucide="list" style="width:16px;height:16px;"></i> 下拉選單',
        category: '表單組件',
        content: `
            <div class="mb-3">
                <label class="form-label">下拉選單</label>
                <select class="form-select">
                    <option selected>請選擇...</option>
                    <option value="1">選項 1</option>
                    <option value="2">選項 2</option>
                </select>
            </div>
        `
    });
}

// ============================================
// UI 橋接與屬性面板
// ============================================
function bridgeToCustomUI() {
    setTimeout(function() {
        // 替換 GrapesJS 的 gjs-one-bg 為 Bootstrap 的 bg-body
        $('.gjs-one-bg').removeClass('gjs-one-bg').addClass('bg-body');
        
        // 初始化 Lucide icons
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }
    }, 100);
    
    // 監聽 GrapesJS 渲染事件，持續替換背景
    editor.on('layer:render', function() {
        setTimeout(() => $('.gjs-one-bg').removeClass('gjs-one-bg').addClass('bg-body'), 10);
    });
    
    editor.on('styleManager:change', function() {
        setTimeout(() => $('.gjs-one-bg').removeClass('gjs-one-bg').addClass('bg-body'), 10);
    });
}

function updatePropertiesPanel(component) {
    if (!component) {
        $('#no-selection').removeClass('d-none');
        $('#element-properties').addClass('d-none');
        return;
    }
    
    $('#no-selection').addClass('d-none');
    $('#element-properties').removeClass('d-none');
    
    // 顯示元素類型
    const tagName = component.get('tagName');
    const classes = component.getClasses().join(' ');
    const type = getComponentTypeName(tagName, classes);
    $('#element-type').val(type);
}

function getComponentTypeName(tagName, classes) {
    const typeMap = {
        'H1': '標題 (H1)', 'H2': '標題 (H2)', 'H3': '標題 (H3)',
        'P': '文字', 'BUTTON': '按鈕', 'A': '連結',
        'IMG': '圖片', 'DIV': '區塊', 'SECTION': '區域',
        'NAV': '導航欄', 'INPUT': '輸入框', 'TEXTAREA': '文本域',
        'SELECT': '下拉選單'
    };
    
    // Bootstrap 組件識別
    if (classes.includes('container')) return '容器';
    if (classes.includes('row')) return '行';
    if (classes.includes('col')) return '列';
    if (classes.includes('card')) return '卡片';
    if (classes.includes('navbar')) return '導航欄';
    
    return typeMap[tagName.toUpperCase()] || tagName;
}

// ============================================
// 主題管理
// ============================================
function toggleDarkMode() {
    const $html = $('html');
    const currentTheme = $html.attr('data-bs-theme');
    const $icon = $('#btn-dark-mode i');

    if (currentTheme === 'dark') {
        $html.attr('data-bs-theme', 'light');
        $icon.attr('data-lucide', 'sun');
        localStorage.setItem('designer-theme', 'light');
    } else {
        $html.attr('data-bs-theme', 'dark');
        $icon.attr('data-lucide', 'moon');
        localStorage.setItem('designer-theme', 'dark');
    }

    lucide.createIcons();
}

function initDarkMode() {
    const savedTheme = localStorage.getItem('designer-theme');
    // 預設為暗色模式，除非用戶明確選擇淺色
    if (savedTheme === 'light') {
        $('html').attr('data-bs-theme', 'light');
        $('#btn-dark-mode i').attr('data-lucide', 'sun');
    } else {
        $('html').attr('data-bs-theme', 'dark');
        $('#btn-dark-mode i').attr('data-lucide', 'moon');
        // 如果沒有儲存過主題，預設儲存為暗色
        if (!savedTheme) {
            localStorage.setItem('designer-theme', 'dark');
        }
    }
    lucide.createIcons();
}

// ============================================
// 工具列功能
// ============================================
function toggleLeftPanel() {
    $('#left-panel').toggle();
}

function toggleRightPanel() {
    $('#properties-panel').toggle();
}

function createNewPage() {
    if (confirm('確定要創建新頁面嗎？未儲存的變更將會遺失。')) {
        editor.setComponents(`
            <div class="text-center text-muted py-5" style="display: flex; flex-direction: column; align-items: center; gap: 16px; margin-top: 100px;">
                <div style="font-size: 48px; opacity: 0.3;">
                    <i data-lucide="sparkles" style="width: 48px; height: 48px;"></i>
                </div>
                <p style="font-size: 18px; margin: 0;">從左側拖曳元素到這裡開始設計</p>
                <p style="font-size: 14px; margin: 0; opacity: 0.6;">選擇一個組件開始建立您的頁面</p>
            </div>
        `);
        currentPageId = null;
        currentPageTitle = 'Untitled Page';
        window.history.pushState({}, '', '/Backoffice/Designer/Designer');
        
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }
    }
}

function undo() {
    editor.UndoManager.undo();
}

function redo() {
    editor.UndoManager.redo();
}

function setViewport(type, btn) {
    $(btn).siblings().removeClass('active');
    $(btn).addClass('active');
    
    switch(type) {
        case 'mobile':
            editor.setDevice('Mobile');
            break;
        case 'tablet':
            editor.setDevice('Tablet');
            break;
        case 'desktop':
        default:
            editor.setDevice('Desktop');
            break;
    }
}

function exportHTML() {
    const html = editor.getHtml();
    const css = editor.getCss();
    
    const fullHTML = `<!DOCTYPE html>
<html lang="zh-TW">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>${currentPageTitle || '新頁面'}</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
    <style>${css}</style>
</head>
<body>
${html}
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>`;
    
    const modal = new bootstrap.Modal($('#htmlPreviewModal')[0]);
    $('#html-output').val(fullHTML);
    modal.show();
    
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
}

function copyHTMLToClipboard() {
    const htmlOutput = $('#html-output')[0];
    htmlOutput.select();
    document.execCommand('copy');
    
    const $copyBtn = $('#copy-html-btn');
    const originalText = $copyBtn.html();
    $copyBtn.html('<i data-lucide="check"></i> 已複製！');
    
    setTimeout(() => {
        $copyBtn.html(originalText);
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }
    }, 2000);
}

function downloadHTML() {
    const htmlContent = $('#html-output').val();
    const blob = new Blob([htmlContent], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    
    const a = document.createElement('a');
    a.href = url;
    a.download = 'GeneratedPage-' + Date.now() + '.html';
    document.body.appendChild(a);
    a.click();
    
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}

// ============================================
// 頁面列表管理
// ============================================
function loadPagesList() {
    $.ajax({
        url: '/Backoffice/Designer/ListPages',
        type: 'GET',
        dataType: 'json',
        success: function(data) {
            if (data && data.success) {
                renderPagesList(data.pages);      // 左側垂直清單（所有頁面）
                renderPageTabs();                 // 上方水平標籤（只顯示打開的分頁）
            } else {
                showError('無法載入頁面列表');
            }
        },
        error: function() {
            showError('載入頁面列表失敗');
        }
    });
}

// 渲染左側垂直頁面列表
function renderPagesList(pages) {
    const $container = $('.pages-list-container');
    $container.empty();
    
    if (!pages || pages.length === 0) {
        $container.html('<p class="text-muted small text-center py-3">尚無頁面</p>');
        return;
    }
    
    pages.forEach(page => {
        const isActive = currentPageId === page.id;
        const $item = $(`
            <div class="file-item p-2 rounded mb-2 d-flex align-items-center justify-content-between ${isActive ? 'active' : ''}" data-page-id="${page.id}">
                <div class="d-flex align-items-center flex-fill" style="min-width: 0;">
                    <i data-lucide="file-text" style="width: 16px; height: 16px; flex-shrink: 0;"></i>
                    <span class="small ms-2 text-truncate" style="flex: 1;">${page.title || 'Untitled'}</span>
                </div>
                <div class="btn-group btn-group-sm ms-2" style="flex-shrink: 0;">
                    <button class="btn btn-outline-primary btn-sm px-2 py-0" onclick="editPage('${page.id}')" title="編輯">
                        <i data-lucide="pencil" style="width: 14px; height: 14px;"></i>
                    </button>
                    <button class="btn btn-outline-info btn-sm px-2 py-0" onclick="viewPage('${page.id}')" title="查看">
                        <i data-lucide="eye" style="width: 14px; height: 14px;"></i>
                    </button>
                    <button class="btn btn-outline-danger btn-sm px-2 py-0" onclick="deletePage('${page.id}')" title="刪除">
                        <i data-lucide="trash-2" style="width: 14px; height: 14px;"></i>
                    </button>
                </div>
            </div>
        `);
        
        $container.append($item);
    });
    
    // 初始化 Lucide icons
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
}

// 渲染上方水平標籤導覽（只顯示打開的分頁）
function renderPageTabs() {
    const $container = $('#page-tabs-bar');
    $container.empty();
    
    if (openTabs.length === 0) {
        $container.html('<span class="text-muted small">尚無打開的分頁</span>');
        return;
    }
    
    openTabs.forEach(tab => {
        const isActive = currentPageId === tab.id;
        const $tab = $(`
            <div class="page-tab ${isActive ? 'active' : ''}" data-page-id="${tab.id}">
                <i data-lucide="file-text" style="width: 14px; height: 14px;"></i>
                <span class="page-tab-title">${tab.title || 'Untitled'}</span>
                <button class="page-tab-close" onclick="closePageTab('${tab.id}'); event.stopPropagation();" title="關閉分頁">
                    <i data-lucide="x" style="width: 14px; height: 14px;"></i>
                </button>
            </div>
        `);
        
        // 點擊標籤切換到該頁面
        $tab.on('click', function() {
            switchToTab(tab.id);
        });
        
        // 右鍵選單：查看
        $tab.on('contextmenu', function(e) {
            e.preventDefault();
            viewPage(tab.id);
        });
        
        $container.append($tab);
    });
    
    // 初始化 Lucide icons
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
}

// 關閉分頁（不刪除頁面）
function closePageTab(pageId) {
    // 從打開的分頁列表中移除
    openTabs = openTabs.filter(tab => tab.id !== pageId);
    
    // 如果關閉的是當前頁面
    if (currentPageId === pageId) {
        // 如果還有其他打開的分頁，切換到最後一個
        if (openTabs.length > 0) {
            switchToTab(openTabs[openTabs.length - 1].id);
        } else {
            // 否則回到新頁面狀態
            createNewPage();
        }
    } else {
        // 只是移除該標籤，重新渲染
        renderPageTabs();
    }
}

// 切換到指定分頁
function switchToTab(pageId) {
    if (currentPageId === pageId) return;
    
    if (currentPageId && confirm('確定要切換到其他頁面嗎？未儲存的變更將會遺失。')) {
        window.location.href = `/Backoffice/Designer/Designer?edit=${pageId}`;
    } else if (!currentPageId) {
        window.location.href = `/Backoffice/Designer/Designer?edit=${pageId}`;
    }
}

// 打開頁面進行編輯（從左側列表點擊）
function editPage(pageId) {
    // 檢查該頁面是否已經在分頁中
    const existingTab = openTabs.find(tab => tab.id === pageId);
    
    if (!existingTab) {
        // 如果未打開，加入到分頁列表（需要從列表中找到頁面資訊）
        // 這裡先暫時使用 pageId，實際標題會在載入後更新
        $.ajax({
            url: '/Backoffice/Designer/LoadPage',
            type: 'GET',
            data: { id: pageId },
            dataType: 'json',
            success: function(data) {
                if (data && data.success && data.page) {
                    openTabs.push({ id: pageId, title: data.page.title || 'Untitled' });
                    switchToTab(pageId);
                }
            }
        });
    } else {
        // 如果已打開，直接切換
        switchToTab(pageId);
    }
}

function viewPage(pageId) {
    window.open(`/Backoffice/Designer/View/${pageId}`, '_blank');
}

function deletePage(pageId) {
    if (!confirm('確定要刪除這個頁面嗎？此操作無法復原。')) {
        return;
    }
    
    $.ajax({
        url: '/Backoffice/Designer/DeletePage',
        type: 'DELETE',
        data: { id: pageId },
        dataType: 'json',
        success: function(data) {
            if (data && data.success) {
                showSuccess('頁面已刪除');
                loadPagesList(); // 重新載入列表
                
                // 如果刪除的是當前頁面，清空畫布
                if (currentPageId === pageId) {
                    createNewPage();
                }
            } else {
                showError('刪除失敗: ' + (data.error || '未知錯誤'));
            }
        },
        error: function() {
            showError('刪除失敗，請稍後再試');
        }
    });
}

// ============================================
// 儲存/載入功能
// ============================================
function savePage() {
    const html = editor.getHtml();
    const css = editor.getCss();
    const gjsData = JSON.stringify(editor.getProjectData());
    
    let title = currentPageTitle;
    if (!currentPageId) {
        title = prompt('請輸入頁面標題:', currentPageTitle) || currentPageTitle;
        currentPageTitle = title;
    }
    
    const pageData = {
        pageId: currentPageId,
        title: title,
        html: html,
        css: css,
        gjsData: gjsData
    };
    
    $.ajax({
        url: '/Backoffice/Designer/SavePage',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(pageData),
        dataType: 'json',
        success: function(data) {
            if (data.success) {
                currentPageId = data.pageId;
                showSuccess(`頁面已儲存！`);
                window.history.pushState({}, '', `/Backoffice/Designer/Designer?edit=${data.pageId}`);
                loadPagesList(); // 重新載入頁面列表
            } else {
                showError('儲存失敗: ' + data.error);
            }
        },
        error: function(xhr, status, error) {
            showError('儲存失敗，請稍後再試');
        }
    });
}

function loadPage(pageId) {
    $.ajax({
        url: '/Backoffice/Designer/LoadPage',
        type: 'GET',
        data: { id: pageId },
        dataType: 'json',
        success: function(data) {
            if (data && data.success && data.page) {
                currentPageId = data.page.pageId;
                currentPageTitle = data.page.title;
                
                // 加入到打開的分頁列表（如果尚未加入）
                const existingTab = openTabs.find(tab => tab.id === currentPageId);
                if (!existingTab) {
                    openTabs.push({ id: currentPageId, title: currentPageTitle });
                }
                
                if (data.page.gjsData) {
                    editor.loadProjectData(JSON.parse(data.page.gjsData));
                } else if (data.page.html) {
                    editor.setComponents(data.page.html);
                }
                
                // 重新渲染分頁列表
                renderPageTabs();
                loadPagesList();  // 也更新左側列表的 active 狀態
                
                showSuccess('頁面已載入: ' + currentPageTitle);
            } else {
                showError('載入失敗: ' + (data.error || '未知錯誤'));
            }
        },
        error: function(xhr, status, error) {
            showError('載入失敗，請稍後再試');
        }
    });
}

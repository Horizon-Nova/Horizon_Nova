// NavigationManagement.js - 目錄管理專用腳本

// 圖標選擇器
function openIconPicker(targetId) {
    loadIconsFromAPI(targetId);
    showModal('icon-picker-modal');
}

// 全域變數儲存圖標數據和目標輸入框
let allIconTags = {};
let currentIconTarget = null;

function loadIconsFromAPI(targetId) {
    currentIconTarget = targetId;
    
    if (Object.keys(allIconTags).length === 0) {
        $.ajax({
            type: 'GET',
            url: 'https://cdn.jsdelivr.net/npm/lucide-static@0.517.0/tags.json',
            dataType: 'json',
            success: (iconTags) => {
                allIconTags = iconTags;
                renderIcons(Object.keys(iconTags));
                $('#iconCount').text(Object.keys(iconTags).length);
            },
            error: () => showToast('載入圖標失敗，請檢查網路連線。', 'error')
        });
    } else {
        renderIcons(Object.keys(allIconTags));
    }
}

// 過濾圖標
function filterIcons() {
    const searchTerm = $('#iconSearchInput').val()?.toLowerCase() || '';
    const allIconNames = Object.keys(allIconTags);
    
    if (!searchTerm) {
        renderIcons(allIconNames);
        return;
    }
    
    const filteredIcons = allIconNames.filter(iconName => {
        const tags = allIconTags[iconName] || [];
        const tagString = tags.join(' ').toLowerCase();
        return iconName.toLowerCase().includes(searchTerm) || tagString.includes(searchTerm);
    });
    
    renderIcons(filteredIcons);
}

// 渲染圖標網格
function renderIcons(iconNames) {
    const $iconGrid = $('#iconGrid');
    const $noResults = $('#iconNoResults');
    
    $iconGrid.empty();
    
    if (iconNames.length === 0) {
        $iconGrid.hide();
        $noResults.show();
        window.lucide?.createIcons?.();
        return;
    }
    
    $iconGrid.show();
    $noResults.hide();
    
    const displayIcons = iconNames.slice(0, 200);
    
    displayIcons.forEach(iconName => {
        $iconGrid.append(`
            <div class="col-6 col-md-3 col-lg-2">
                <button type="button" class="btn btn-outline-secondary w-100 p-3 icon-option" data-icon="${iconName}" onclick="selectIconOption(this)">
                    <i data-lucide="${iconName}" class="mb-2" style="width: 1.5rem; height: 1.5rem;"></i>
                    <div class="small text-truncate">${iconName}</div>
                </button>
            </div>
        `);
    });
    
    window.lucide?.createIcons?.();
}

function selectIconOption(btn) {
    $('.icon-option').removeClass('active');
    $(btn).addClass('active');
}

function selectIcon() {
    const selectedIcon = $('.icon-option.active').data('icon');
    if (!selectedIcon) {
        showToast('請選擇一個圖標', 'warning');
        return;
    }
    
    $(currentIconTarget).val(selectedIcon);
    window.lucide?.createIcons?.();
    closeModal('icon-picker-modal');
}


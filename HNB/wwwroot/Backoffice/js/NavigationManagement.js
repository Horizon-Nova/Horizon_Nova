// NavigationManagement.js - 目錄管理專用腳本

$(document).ready(() => {
    window.lucide?.createIcons?.();
    $(document).on('shown.bs.modal', '#NavModal', () => {
        loadParentOptions();
    });
    initializeDragAndDrop();
});
function loadParentOptions() {
    $.ajax({
        type: 'GET',
        url: '/Backoffice/SidebarNavigation/LoadParentOptions',
        success: (html) => {
            const $select = $('#parent_code');
            const current = $select.data('current') || '';
            $select.html(html);
            if (current) $select.val(current);
        },
        error: () => console.warn('無法載入上層目錄選項')
    });
}

// 展開/收合子項目
function toggleChildren(btn) {
    const $btn = $(btn);
    const $item = $btn.closest('.nav-tree-item');
    const $children = $item.find('.nav-children').first();
    const $icon = $btn.find('.nav-chevron-tree');
    
    if ($children.length === 0) return;
    
    const isHidden = $children.hasClass('hidden');
    $children.toggleClass('hidden');
    $icon.toggleClass('rotated');
    
    if (isHidden) {
        window.lucide?.createIcons?.();
    }
}

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

// ===== 拖放排序功能 =====
let draggedElement = null;
let draggedId = null;
let draggedParentCode = null;

function initializeDragAndDrop() {
    const $treeContainer = $('#navigationTreeContent');
    if ($treeContainer.length === 0) return;
    
    $treeContainer.on('dragstart', '.nav-tree-item', handleDragStart);
    $treeContainer.on('dragend', '.nav-tree-item', handleDragEnd);
    $treeContainer.on('dragover', '.nav-tree-item', handleDragOver);
    $treeContainer.on('dragleave', '.nav-tree-item', handleDragLeave);
    $treeContainer.on('drop', '.nav-tree-item', handleDrop);
}

function handleDragStart(e) {
    draggedElement = this;
    draggedId = $(this).data('nav-id');
    draggedParentCode = $(this).data('parent-code') || '';
    
    $(this).addClass('dragging');
    e.originalEvent.dataTransfer.effectAllowed = 'move';
    e.originalEvent.dataTransfer.setData('text/html', this.innerHTML);
}

function handleDragEnd(e) {
    $(this).removeClass('dragging');
    $('.nav-tree-item').removeClass('drag-over');
    
    draggedElement = null;
    draggedId = null;
    draggedParentCode = null;
}

function handleDragOver(e) {
    if (e.preventDefault) {
        e.preventDefault();
    }
    
    if (this === draggedElement) {
        return false;
    }
    
    const targetParentCode = $(this).data('parent-code') || '';
    if (draggedParentCode !== targetParentCode) {
        return false;
    }
    
    $(this).addClass('drag-over');
    e.originalEvent.dataTransfer.dropEffect = 'move';
    
    return false;
}

function handleDragLeave(e) {
    $(this).removeClass('drag-over');
}

function handleDrop(e) {
    if (e.stopPropagation) {
        e.stopPropagation();
    }
    
    if (this === draggedElement) {
        return false;
    }
    
    const targetParentCode = $(this).data('parent-code') || '';
    if (draggedParentCode !== targetParentCode) {
        showToast('只能在同一層級內調整順序', 'warning');
        return false;
    }
    
    const targetId = $(this).data('nav-id');
    const parentCode = draggedParentCode;
    const $siblings = parentCode === ''
        ? $('.nav-tree-item[data-parent-code=""]')
        : $(`.nav-tree-item[data-parent-code="${parentCode}"]`);
    
    const ids = [];
    $siblings.each(function() {
        const id = $(this).data('nav-id');
        if (id !== draggedId) {
            ids.push(id);
        }
    });
    
    const targetIndex = ids.indexOf(targetId);
    const draggedIndex = $siblings.index(draggedElement);
    const dropIndex = $siblings.index(this);
    
    if (draggedIndex < dropIndex) {
        ids.splice(targetIndex + 1, 0, draggedId);
    } else {
        ids.splice(targetIndex, 0, draggedId);
    }
    
    const newOrder = ids.map((id, index) => ({
        id: id,
        sort_order: index
    }));
    
    updateNavigationOrder(newOrder);
    
    return false;
}

function updateNavigationOrder(orderList) {
    $.ajax({
        type: 'POST',
        url: '/Backoffice/SidebarNavigation/UpdateSortOrder',
        contentType: 'application/json',
        data: JSON.stringify(orderList),
        success: (response) => {
            if (response?.success) {
                location.reload();
            } else {
                showToast(response?.message || '更新排序失敗', 'error');
            }
        },
        error: () => showToast('更新排序失敗，系統發生錯誤。', 'error')
    });
}

// 篩選功能
function applyFilters() {
    const searchKeyword = $('#searchInput').val()?.toLowerCase() || '';
    const statusFilter = $('#statusFilter').val() || '';
    const levelFilter = $('#levelFilter').val() || '';
    const $treeContainer = $('#navigationTreeContent');
    
    if ($treeContainer.length === 0) return;
    
    const $allItems = $treeContainer.find('.nav-tree-item');
    let visibleCount = 0;
    
    $allItems.each(function() {
        const $item = $(this);
        const $card = $item.find('.nav-item-card');
        
        if ($card.length === 0) return;
        
        const title = $card.find('h6').text()?.toLowerCase() || '';
        const code = $card.find('[data-lucide="hash"]').parent().text()?.toLowerCase() || '';
        const $statusBadge = $card.find('.badge').filter(function() {
            const text = $(this).text()?.trim();
            return text === '啟用' || text === '停用';
        }).first();
        const isActive = $statusBadge.text()?.trim() === '啟用';
        
        const parentCode = $item.data('parent-code') || '';
        let level = 1;
        if (parentCode !== '') {
            level = 2;
            const $parent = $(`.nav-tree-item[data-nav-id]`).filter(function() {
                const $codeSpan = $(this).find('[data-lucide="hash"]').parent();
                return $codeSpan.text() === parentCode;
            });
            if ($parent.length > 0 && $parent.data('parent-code')) {
                level = 3;
            }
        }
        
        const matchesSearch = searchKeyword === '' || title.includes(searchKeyword) || code.includes(searchKeyword);
        const matchesStatus = statusFilter === '' || 
            (statusFilter === 'active' && isActive) || 
            (statusFilter === 'inactive' && !isActive);
        const matchesLevel = levelFilter === '' || level === parseInt(levelFilter);
        
        const isVisible = matchesSearch && matchesStatus && matchesLevel;
        $item.css('display', isVisible ? '' : 'none');
        if (isVisible) visibleCount++;
    });
    
    const $emptyState = $treeContainer.find('.text-center');
    $emptyState.css('display', visibleCount === 0 ? '' : 'none');
}

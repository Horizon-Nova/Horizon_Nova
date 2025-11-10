// NavigationManagement.js - 目錄管理專用腳本

$(document).ready(() => {
    window.lucide?.createIcons?.();
    $('#searchInput').on('input', applyFilters);
    $('#statusFilter').on('change', applyFilters);
    $('#levelFilter').on('change', applyFilters);

    // 委派：開啟說明/新增 Modal
    $(document)
        .off('click.nav', '.js-open-navigation-help')
        .on('click.nav', '.js-open-navigation-help', function(e){ e.preventDefault(); showModal('navigation-help'); })
        .off('click.nav', '.js-open-nav-add')
        .on('click.nav', '.js-open-nav-add', function(){ showModal('nav-add-modal'); })
        .off('click.nav', '.js-icon-picker-confirm')
        .on('click.nav', '.js-icon-picker-confirm', function(){ selectIcon(); })
        .off('click.nav', '.js-nav-toggle-children')
        .on('click.nav', '.js-nav-toggle-children', function(e){ e.preventDefault(); toggleChildren(this); })
        .off('click.nav', '.js-nav-show-detail')
        .on('click.nav', '.js-nav-show-detail', function(){ const id=$(this).data('id'); showDetailModal(id); })
        .off('click.nav', '.js-nav-show-edit')
        .on('click.nav', '.js-nav-show-edit', function(){ const id=$(this).data('id'); showEditModal(id); })
        .off('click.nav', '.js-nav-show-delete')
        .on('click.nav', '.js-nav-show-delete', function(){ const id=$(this).data('id'); showDeleteModal(id); })
        .off('click.nav', '.js-open-icon-picker')
        .on('click.nav', '.js-open-icon-picker', function(){ openIconPicker(); })
        .off('click.nav', '.js-save-navigation')
        .on('click.nav', '.js-save-navigation', function(){ const action=$(this).data('action'); saveNavigation(action); })
        .off('input.nav', '.js-delete-confirm-input')
        .on('input.nav', '.js-delete-confirm-input', validateDeleteInput)
        .off('click.nav', '.js-confirm-delete')
        .on('click.nav', '.js-confirm-delete', confirmDelete);
    
    // 初始化拖放功能
    initializeDragAndDrop();

    // 載入上層目錄選項（在 Modal 開啟時載入）
    $(document).on('shown.bs.modal', '#nav-add-modal, #nav-edit-modal', function () {
        loadParentOptions($(this).attr('id'));
    });
});
// 載入上層目錄選項
const loadParentOptions = (modalId) => {
    $.ajax({
        type: 'GET',
        url: '/Backoffice/SidebarNavigation/LoadParentOptions',
        success: (html) => {
            const $select = modalId === 'nav-add-modal' ? $('#navAddParent') : $('#navEditParent');
            const current = $select.data('current') || '';
            $select.html(html);
            if (current) $select.val(current);
        },
        error: () => console.warn('無法載入上層目錄選項')
    });
};

// 便捷函數：顯示編輯 Modal
const showEditModal = (id) => showModal('nav-edit-modal', {
    url: '/Backoffice/SidebarNavigation/LoadDetail',
    method: 'GET',
    data: { id: id }
});

// 便捷函數：顯示詳情 Modal
const showDetailModal = (id) => showModal('nav-detail-modal', {
    url: '/Backoffice/SidebarNavigation/LoadDetail',
    method: 'GET',
    data: { id: id }
});

// 便捷函數：顯示刪除確認 Modal
const showDeleteModal = (id) => showModal('nav-delete-modal', {
    url: '/Backoffice/SidebarNavigation/LoadDetail',
    method: 'GET',
    data: { id: id }
});

// 篩選功能
const applyFilters = () => {
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
        
        const $titleElement = $card.find('h6');
        const title = $titleElement.text()?.toLowerCase() || '';
        
        const $codeElement = $card.find('[data-lucide="hash"]').parent();
        const code = $codeElement.text()?.toLowerCase() || '';
        
        const $statusBadge = $card.find('.badge').filter(function() {
            const text = $(this).text()?.trim();
            return text === '啟用' || text === '停用';
        }).first();
        const isActive = $statusBadge.text()?.trim() === '啟用';
        
        // 計算層級：根據父層數量
        const parentCode = $item.data('parent-code') || '';
        let level = 1;
        if (parentCode !== '') {
            // 有 parent_code 表示至少是第二層
            level = 2;
            // 檢查父項目的父項目是否存在
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
};

// 展開/收合子項目
const toggleChildren = (btn) => {
    if (!btn) return;
    const $btn = $(btn);
    const $item = $btn.closest('.nav-tree-item');
    
    if ($item.length === 0) return;
    
    const $children = $item.find('.nav-children').first();
    const $icon = $btn.find('.nav-chevron-tree');
    
    if ($children.length === 0) return;
    
    const isHidden = $children.hasClass('hidden');
    $children.toggleClass('hidden');
    $icon.toggleClass('rotated');
    
    if (isHidden) {
        window.lucide?.createIcons?.();
    }
};

// 儲存導航（新增或編輯）
const saveNavigation = (type) => {
    const formId = type === 'add' ? '#navAddForm' : '#navEditForm';
    const checkboxId = type === 'add' ? '#navAddIsActive' : '#navEditIsActive';
    
    // 將表單序列化為物件
    const formData = $(formId).serializeArray();
    const dataObject = {};
    
    formData.forEach(field => {
        dataObject[field.name] = field.value;
    });
    
    // 手動處理 checkbox 的值（確保 is_active 欄位總是被提交）
    dataObject['is_active'] = $(checkboxId).is(':checked');
    
    $.ajax({
        type: 'POST',
        url: '/Backoffice/SidebarNavigation/SubmitNavigation',
        data: dataObject,
        success: (response) => {
            response?.success
                ? (alert('儲存成功'), closeModal(`nav-${type}-modal`), location.reload())
                : alert(response?.message || '儲存失敗');
        },
        error: () => alert('失敗，系統發生錯誤。')
    });
};

// 驗證刪除輸入
const validateDeleteInput = () => {
    const input = $('#deleteConfirmInput').val();
    const expectedCode = $('#deleteNavId').data('code');
    $('#btnConfirmDelete').prop('disabled', input !== expectedCode);
};

// 確認刪除
const confirmDelete = () => {
    const id = $('#deleteNavId').val();
    if (!id) {
        alert('無法取得目錄 ID');
        return;
    }
    
    $.ajax({
        type: 'POST',
        url: '/Backoffice/SidebarNavigation/Delete',
        data: { id: id },
        success: (response) => {
            response?.success
                ? (alert('刪除成功'), closeModal('nav-delete-modal'), location.reload())
                : alert(response?.message || '刪除失敗');
        },
        error: () => alert('失敗，系統發生錯誤。')
    });
};

// 圖標選擇器
const openIconPicker = () => {
    loadIconsFromAPI();
    showModal('icon-picker-modal');
};

// 全域變數儲存圖標數據
let allIconTags = {};

const loadIconsFromAPI = () => {
    $.ajax({
        type: 'GET',
        url: 'https://cdn.jsdelivr.net/npm/lucide-static@0.517.0/tags.json',
        dataType: 'json',
        success: (iconTags) => {
            allIconTags = iconTags;
            renderIcons(Object.keys(iconTags));
            $('#iconCount').text(Object.keys(iconTags).length);
            
            // 綁定搜尋事件
            $('#iconSearchInput').off('input').on('input', filterIcons);
        },
        error: () => alert('載入圖標失敗，請檢查網路連線。')
    });
};

// 過濾圖標
const filterIcons = () => {
    const searchTerm = $('#iconSearchInput').val()?.toLowerCase() || '';
    const allIconNames = Object.keys(allIconTags);
    
    if (!searchTerm) {
        renderIcons(allIconNames);
        return;
    }
    
    // 根據圖標名稱和標籤過濾
    const filteredIcons = allIconNames.filter(iconName => {
        const tags = allIconTags[iconName] || [];
        const tagString = tags.join(' ').toLowerCase();
        
        return iconName.toLowerCase().includes(searchTerm) || tagString.includes(searchTerm);
    });
    
    renderIcons(filteredIcons);
};

// 渲染圖標網格
const renderIcons = (iconNames) => {
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
    
    // 限制顯示數量以提升效能
    const displayIcons = iconNames.slice(0, 200);
    
    displayIcons.forEach(iconName => {
        $iconGrid.append(`
            <div class="col-6 col-md-3 col-lg-2">
                <button type="button" class="btn btn-outline-secondary w-100 p-3 icon-option" data-icon="${iconName}">
                    <i data-lucide="${iconName}" class="mb-2" style="width: 1.5rem; height: 1.5rem;"></i>
                    <div class="small text-truncate">${iconName}</div>
                </button>
            </div>
        `);
    });
    
    window.lucide?.createIcons?.();
    
    $('.icon-option').on('click', function() {
        $('.icon-option').removeClass('active');
        $(this).addClass('active');
    });
};

const selectIcon = () => {
    const selectedIcon = $('.icon-option.active').data('icon');
    if (!selectedIcon) {
        alert('請選擇一個圖標');
        return;
    }
    
    // 判斷目前是新增還是編輯 Modal
    const isAddModalOpen = $('#nav-add-modal').hasClass('show');
    const inputId = isAddModalOpen ? '#navAddIcon' : '#navEditIcon';
    const previewId = isAddModalOpen ? '#navAddIconPreview' : '#navEditIconPreview';
    
    $(inputId).val(selectedIcon);
    $(`${previewId} i`).attr('data-lucide', selectedIcon).removeClass('text-muted').addClass('text-primary');
    window.lucide?.createIcons?.();
    closeModal('icon-picker-modal');
};

// ===== 拖放排序功能 =====
let draggedElement = null;
let draggedId = null;
let draggedParentCode = null;

const initializeDragAndDrop = () => {
    const $treeContainer = $('#navigationTreeContent');
    if ($treeContainer.length === 0) return;
    
    // 使用事件委派處理動態元素
    $treeContainer.on('dragstart', '.nav-tree-item', handleDragStart);
    $treeContainer.on('dragend', '.nav-tree-item', handleDragEnd);
    $treeContainer.on('dragover', '.nav-tree-item', handleDragOver);
    $treeContainer.on('dragleave', '.nav-tree-item', handleDragLeave);
    $treeContainer.on('drop', '.nav-tree-item', handleDrop);
};

const handleDragStart = function(e) {
    draggedElement = this;
    draggedId = $(this).data('nav-id');
    draggedParentCode = $(this).data('parent-code') || '';
    
    $(this).addClass('dragging');
    e.originalEvent.dataTransfer.effectAllowed = 'move';
    e.originalEvent.dataTransfer.setData('text/html', this.innerHTML);
};

const handleDragEnd = function(e) {
    $(this).removeClass('dragging');
    $('.nav-tree-item').removeClass('drag-over');
    
    draggedElement = null;
    draggedId = null;
    draggedParentCode = null;
};

const handleDragOver = function(e) {
    if (e.preventDefault) {
        e.preventDefault();
    }
    
    // 不能拖到自己身上
    if (this === draggedElement) {
        return false;
    }
    
    // 只能在同一層級內拖放（相同的 parent_code）
    const targetParentCode = $(this).data('parent-code') || '';
    if (draggedParentCode !== targetParentCode) {
        return false;
    }
    
    $(this).addClass('drag-over');
    e.originalEvent.dataTransfer.dropEffect = 'move';
    
    return false;
};

const handleDragLeave = function(e) {
    $(this).removeClass('drag-over');
};

const handleDrop = function(e) {
    if (e.stopPropagation) {
        e.stopPropagation();
    }
    
    // 不能拖到自己身上
    if (this === draggedElement) {
        return false;
    }
    
    // 只能在同一層級內拖放
    const targetParentCode = $(this).data('parent-code') || '';
    if (draggedParentCode !== targetParentCode) {
        alert('只能在同一層級內調整順序');
        return false;
    }
    
    const targetId = $(this).data('nav-id');
    
    // 收集同層級所有項目
    const parentCode = draggedParentCode;
    const $siblings = parentCode === ''
        ? $('.nav-tree-item[data-parent-code=""]')
        : $(`.nav-tree-item[data-parent-code="${parentCode}"]`);
    
    // 建立 ID 陣列（排除被拖動的項目）
    const ids = [];
    $siblings.each(function() {
        const id = $(this).data('nav-id');
        if (id !== draggedId) {
            ids.push(id);
        }
    });
    
    // 找到目標位置並插入被拖動的項目
    const targetIndex = ids.indexOf(targetId);
    const draggedIndex = $siblings.index(draggedElement);
    const dropIndex = $siblings.index(this);
    
    if (draggedIndex < dropIndex) {
        // 向下拖動：插入到目標之後
        ids.splice(targetIndex + 1, 0, draggedId);
    } else {
        // 向上拖動：插入到目標之前
        ids.splice(targetIndex, 0, draggedId);
    }
    
    // 建立新的排序物件陣列（排序號使用連續整數 0, 1, 2, 3...）
    const newOrder = ids.map((id, index) => ({
        id: id,
        sort_order: index
    }));
    
    // 發送批量更新請求
    updateNavigationOrder(newOrder);
    
    return false;
};

const updateNavigationOrder = (orderList) => {
    // 構建完整的導航物件陣列（只更新 sort_order，其他欄位從 DOM 讀取）
    const updates = orderList.map(item => {
        const $item = $(`.nav-tree-item[data-nav-id="${item.id}"]`);
        return {
            id: item.id,
            sort_order: item.sort_order,
            // 其他欄位保持不變，前端不需要傳遞
        };
    });
    
    $.ajax({
        type: 'POST',
        url: '/Backoffice/SidebarNavigation/UpdateSortOrder',
        contentType: 'application/json',
        data: JSON.stringify(updates),
        success: (response) => {
            if (response?.success) {
                // 成功後重新載入頁面以顯示新順序
                location.reload();
            } else {
                alert(response?.message || '更新排序失敗');
            }
        },
        error: () => alert('更新排序失敗，系統發生錯誤。')
    });
};
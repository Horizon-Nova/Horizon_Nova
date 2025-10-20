// NavigationManagement.js - 目錄管理專用腳本

$(document).ready(() => {
    window.lucide?.createIcons?.();
    $('#searchInput').on('input', applyFilters);
    $('#statusFilter').on('change', applyFilters);
    $('#levelFilter').on('change', applyFilters);
});

// 便捷函數：顯示編輯 Modal
const showEditModal = (id) => showModal('nav-edit-modal', {
    url: '/Backoffice/SidebarNavigation/LoadDetail',
    method: 'GET',
    data: { id: id },
    container: 'navigationModals'
});

// 便捷函數：顯示詳情 Modal
const showDetailModal = (id) => showModal('nav-detail-modal', {
    url: '/Backoffice/SidebarNavigation/LoadDetail',
    method: 'GET',
    data: { id: id },
    container: 'navigationModals'
});

// 便捷函數：顯示刪除確認 Modal
const showDeleteModal = (id) => showModal('nav-delete-modal', {
    url: '/Backoffice/SidebarNavigation/LoadDetail',
    method: 'GET',
    data: { id: id },
    container: 'navigationModals'
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
        
        const $statusBadge = $card.find('.badge');
        const isActive = $statusBadge.text()?.trim() === '啟用';
        
        const level = $item.closest('.nav-children').length > 0
            ? ($item.closest('.nav-children').closest('.nav-children').length > 0 ? 3 : 2)
            : 1;
        
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
    $.ajax({
        type: 'POST',
        url: '/Backoffice/SidebarNavigation/SubmitNavigation',
        data: $(formId).serialize(),
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
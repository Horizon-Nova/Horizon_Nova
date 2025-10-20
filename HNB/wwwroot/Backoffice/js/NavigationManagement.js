// NavigationManagement.js - 目錄管理專用腳本

document.addEventListener('DOMContentLoaded', () => {
    window.lucide?.createIcons?.();
    $('#searchInput').on('input', applyFilters);
    $('#statusFilter').on('change', applyFilters);
    $('#levelFilter').on('change', applyFilters);
});

// 載入導航資料並顯示指定的 Modal
const loadAndShowModal = (id, modalId) => {
    $.ajax({
        type: 'GET',
        url: '/Backoffice/SidebarNavigation/LoadDetail',
        data: { id: id },
        success: (html) => {
            // 替換整個 Modal 容器
            $('#navigationModals').html(html);
            // 重新初始化 Lucide 圖標
            window.lucide?.createIcons?.();
            // 顯示指定的 Modal
            showModal(modalId);
        },
        error: () => alert('載入失敗，系統發生錯誤。')
    });
};

// 便捷函數
const showEditModal = (id) => loadAndShowModal(id, 'nav-edit-modal');
const showDetailModal = (id) => loadAndShowModal(id, 'nav-detail-modal');
const showDeleteModal = (id) => loadAndShowModal(id, 'nav-delete-modal');

// 篩選功能
const applyFilters = () => {
    const searchKeyword = $('#searchInput').val()?.toLowerCase() || '';
    const statusFilter = $('#statusFilter').val() || '';
    const levelFilter = $('#levelFilter').val() || '';
    const treeContainer = $('#navigationTreeContent')[0];
    if (!treeContainer) return;
    
    const allItems = treeContainer.querySelectorAll('.nav-tree-item');
    let visibleCount = 0;
    
    allItems.forEach(item => {
        if (!item) return;
        
        const card = item.querySelector('.nav-item-card');
        if (!card) return;
        
        const titleElement = card.querySelector('h6');
        const title = titleElement?.textContent?.toLowerCase() || '';
        
        const codeElement = card.querySelector('[data-lucide="hash"]')?.parentElement;
        const code = codeElement?.textContent?.toLowerCase() || '';
        
        const statusBadge = card.querySelector('.badge');
        const isActive = statusBadge?.textContent?.trim() === '啟用';
        
        const level = item.closest('.nav-children') 
            ? (item.closest('.nav-children').closest('.nav-children') ? 3 : 2) 
            : 1;
        
        const matchesSearch = searchKeyword === '' || title.includes(searchKeyword) || code.includes(searchKeyword);
        const matchesStatus = statusFilter === '' || 
            (statusFilter === '1' && isActive) || 
            (statusFilter === '0' && !isActive);
        const matchesLevel = levelFilter === '' || level === parseInt(levelFilter);
        
        const isVisible = matchesSearch && matchesStatus && matchesLevel;
        item.style.display = isVisible ? '' : 'none';
        if (isVisible) visibleCount++;
    });
    
    const emptyState = treeContainer.querySelector('.text-center');
    if (emptyState) {
        emptyState.style.display = visibleCount === 0 ? '' : 'none';
    }
};

// 展開/收合子項目
const toggleChildren = (btn) => {
    if (!btn) return;
    const item = btn.closest('.nav-tree-item');
    if (!item) return;
    const children = item.querySelector('.nav-children');
    const icon = btn.querySelector('.nav-chevron-tree');
    if (!children) return;
    const isHidden = children.classList.contains('hidden');
    children.classList.toggle('hidden');
    icon?.classList.toggle('rotated');
    isHidden && window.lucide?.createIcons?.();
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

const loadIconsFromAPI = () => {
    fetch('https://unpkg.com/lucide-static@latest/icons.json')
        .then(response => response.json())
        .then(icons => {
            const iconGrid = $('#iconGrid');
            iconGrid.empty();
            Object.keys(icons).slice(0, 100).forEach(iconName => {
                iconGrid.append(`
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
        })
        .catch(() => alert('載入圖標失敗'));
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
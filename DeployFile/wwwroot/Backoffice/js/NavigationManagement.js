// ==================== 全局變數 ====================
let allIcons = [];
let deleteTarget = { id: null, title: '' };

// ==================== 模態框管理 ====================

/**
 * 顯示導航模態框（業務邏輯封裝）
 * @param {string} modalType - 模態框類型: 'new', 'edit', 'detail', 'delete', 'icon-picker'
 * @param {number|null} navId - 導航ID（編輯和詳情時需要）
 */
function showNavModal(modalType, navId = null) {
    if (modalType === 'delete') {
        showDeleteModal(navId);
        return;
    }

    // 區分新增和編輯
    const isEdit = navId != null && navId > 0;
    const modalId = modalType === 'detail' ? 'nav-detail-modal' : 'nav-edit-modal';
    
    // 載入資料
    $.ajax({
        url: '/Backoffice/SidebarNavigation/LoadDetail',
        type: 'GET',
        data: { id: navId || 0 },
        success: function (response) {
            // 根據類型更新頁面
            if (modalType === 'new' || modalType === 'edit') {
                updateEditModal(response, isEdit);
            } else if (modalType === 'detail') {
                updateDetailModal(response);
            }
            
            // 顯示模態框 - 使用統一的 showModal 函數
            showModal(modalId);
            
            // 重新初始化 lucide icons
            if (window.lucide) window.lucide.createIcons();
        },
        error: function () {
            alert('載入資料失敗');
        }
    });
}

/**
 * 關閉導航模態框（帶表單清理）
 * 注意：此函數保留是為了在關閉時執行額外的清理邏輯
 * 內部使用統一的 closeModal 函數
 */
function closeNavModal(modalId) {
    if (modalId === 'nav-edit-modal') {
        resetEditForm();
    }
    closeModal(modalId);
}

/**
 * 更新編輯模態框內容
 */
function updateEditModal(data, isEdit) {
    // 更新標題
    const modalTitle = document.querySelector('#nav-edit-modal #modalTitle');
    const modalIcon = document.querySelector('#nav-edit-modal [data-lucide="folder-plus"]');
    
    if (modalTitle) {
        modalTitle.textContent = isEdit ? '編輯目錄' : '新增目錄';
    }
    if (modalIcon) {
        modalIcon.setAttribute('data-lucide', isEdit ? 'edit' : 'folder-plus');
    }
    
    // 填充表單資料
    if (isEdit && data) {
        document.getElementById('navId').value = data.id || '';
        document.getElementById('navTitle').value = data.title || '';
        document.getElementById('navCode').value = data.code || '';
        document.getElementById('navIcon').value = data.icon || '';
        document.getElementById('navParent').value = data.parent_code || '';
        document.getElementById('navSort').value = data.sort_order || 0;
        document.getElementById('navUrl').value = data.url || '';
        document.getElementById('navIsActive').checked = data.is_active !== false;
        
        // 更新圖標預覽
        updateIconPreview(data.icon);
    } else {
        resetEditForm();
    }
    
    // 載入上層目錄選項
    loadParentOptions(data?.parent_code);
}

/**
 * 更新詳情模態框內容
 */
function updateDetailModal(data) {
    // 這裡可以動態更新詳情內容
    // 目前使用伺服器端渲染，所以不需要額外處理
}

/**
 * 重置編輯表單
 */
function resetEditForm() {
    document.getElementById('navId').value = '';
    document.getElementById('navTitle').value = '';
    document.getElementById('navCode').value = '';
    document.getElementById('navIcon').value = '';
    document.getElementById('navParent').value = '';
    document.getElementById('navSort').value = '0';
    document.getElementById('navUrl').value = '';
    document.getElementById('navIsActive').checked = true;
    updateIconPreview('');
}

// ==================== 上層目錄管理 ====================

/**
 * 載入上層目錄選項
 */
function loadParentOptions(selectedCode = null) {
    $.ajax({
        url: '/Backoffice/SidebarNavigation/GetParentOptions',
        type: 'GET',
        success: function (data) {
            const select = document.getElementById('navParent');
            if (!select) return;
            
            // 清空現有選項
            select.innerHTML = '<option value="">無（第一層目錄）</option>';
            
            // 新增選項
            data.forEach(item => {
                const option = document.createElement('option');
                option.value = item.code;
                option.textContent = item.full_path || item.title;
                if (item.code === selectedCode) {
                    option.selected = true;
                }
                select.appendChild(option);
            });
        },
        error: function () {
            console.error('載入上層目錄失敗');
        }
    });
}

// ==================== 圖標選擇器 ====================

/**
 * 打開圖標選擇器
 */
function openIconPicker() {
    // 顯示 Modal
    showModal('icon-picker-modal');
    // 如果尚未載入圖標，則從 API 載入
    if (allIcons.length === 0) {
        loadIconsFromAPI();
    }
}

/**
 * 載入圖標（從 Lucide API）
 * 使用 lucide-static 的 tags.json API
 */
async function loadIconsFromAPI() {
    const loading = document.getElementById('iconLoading');
    const error = document.getElementById('iconError');
    const grid = document.getElementById('iconGrid');
    
    // 顯示載入中
    if (loading) loading.classList.remove('hidden');
    if (error) error.classList.add('hidden');
    if (grid) grid.classList.add('hidden');
    
    try {
        // 使用最後一個穩定版本的 API（0.517.0）
        // 註：0.518.0 的 tags.json 是空的，所以使用 0.517.0
        const response = await fetch('https://cdn.jsdelivr.net/npm/lucide-static@0.517.0/tags.json');
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        const tagsData = await response.json();
        
        // 轉換為陣列格式
        allIcons = Object.keys(tagsData).map(name => ({
            name: name,
            tags: tagsData[name] || []
        })).sort((a, b) => a.name.localeCompare(b.name));
        
        console.log(`✅ 成功載入 ${allIcons.length} 個 Lucide 圖標`);
        
        // 更新統計
        document.getElementById('totalIcons').textContent = allIcons.length;
        document.getElementById('filteredIcons').textContent = allIcons.length;
        
        // 渲染圖標
        renderIcons(allIcons);
        
        // 隱藏載入中，顯示網格
        if (loading) loading.classList.add('hidden');
        if (grid) grid.classList.remove('hidden');
        
    } catch (err) {
        console.error('❌ 載入圖標失敗:', err);
        if (loading) loading.classList.add('hidden');
        if (error) {
            error.classList.remove('hidden');
            const errorMsg = document.getElementById('iconErrorMessage');
            if (errorMsg) {
                errorMsg.textContent = `載入失敗: ${err.message}. 請確認網路連線或稍後再試。`;
            }
        }
    }
}

/**
 * 渲染圖標網格
 */
function renderIcons(icons) {
    const grid = document.getElementById('iconGrid');
    const noResults = document.getElementById('noResults');
    
    if (!grid) return;
    
    // 清空網格
    grid.innerHTML = '';
    
    if (icons.length === 0) {
        if (noResults) noResults.classList.remove('hidden');
        grid.classList.add('hidden');
        return;
    }
    
    if (noResults) noResults.classList.add('hidden');
    grid.classList.remove('hidden');
    
    // 渲染每個圖標
    icons.forEach(icon => {
        const button = document.createElement('button');
        button.type = 'button';
        button.className = 'p-3 border border-slate-200 rounded-lg hover:border-purple-500 hover:bg-purple-50 transition-all flex flex-col items-center gap-2 group';
        button.title = icon.name;
        button.onclick = () => selectIcon(icon.name);
        
        button.innerHTML = `
            <i data-lucide="${icon.name}" class="w-6 h-6 text-slate-600 group-hover:text-purple-600"></i>
            <span class="text-xs text-slate-500 group-hover:text-purple-600 truncate w-full text-center">${icon.name}</span>
        `;
        
        grid.appendChild(button);
    });
    
    // 重新初始化圖標
    if (window.lucide) window.lucide.createIcons();
}

/**
 * 搜尋圖標
 */
function searchIcons() {
    const searchInput = document.getElementById('iconSearch');
    if (!searchInput) return;
    
    const keyword = searchInput.value.toLowerCase().trim();
    
    if (!keyword) {
        renderIcons(allIcons);
        document.getElementById('filteredIcons').textContent = allIcons.length;
        return;
    }
    
    // 過濾圖標
    const filtered = allIcons.filter(icon => {
        return icon.name.toLowerCase().includes(keyword) ||
               icon.tags.some(tag => tag.toLowerCase().includes(keyword));
    });
    
    renderIcons(filtered);
    document.getElementById('filteredIcons').textContent = filtered.length;
}

/**
 * 選擇圖標
 */
function selectIcon(iconName) {
    // 更新輸入框
    const iconInput = document.getElementById('navIcon');
    if (iconInput) {
        iconInput.value = iconName;
    }
    
    // 更新預覽
    updateIconPreview(iconName);
    
    // 使用統一的 closeModal 函數
    closeModal('icon-picker-modal');
}

/**
 * 更新圖標預覽
 */
function updateIconPreview(iconName) {
    const preview = document.getElementById('selectedIconPreview');
    if (!preview) return;
    
    if (iconName) {
        preview.innerHTML = `<i data-lucide="${iconName}" class="w-5 h-5 text-slate-600"></i>`;
    } else {
        preview.innerHTML = '<i data-lucide="image" class="w-5 h-5 text-slate-400"></i>';
    }
    
    if (window.lucide) window.lucide.createIcons();
}

// ==================== 刪除功能 ====================

/**
 * 顯示刪除確認模態框
 */
function showDeleteModal(navId) {
    // 取得導航資訊
    $.ajax({
        url: '/Backoffice/SidebarNavigation/LoadDetail',
        type: 'GET',
        data: { id: navId },
        success: function (data) {
            deleteTarget = { id: data.id, title: data.title };
            
            // 更新刪除模態框內容
            document.getElementById('deleteNavTitle').textContent = data.title;
            document.getElementById('deleteNavCode').textContent = data.code;
            document.getElementById('deleteConfirmInput').value = '';
            document.getElementById('deleteConfirmBtn').disabled = true;
            
            // 使用統一的 showModal 函數
            showModal('nav-delete-modal');
        },
        error: function () {
            alert('載入資料失敗');
        }
    });
}

/**
 * 驗證刪除輸入
 */
function validateDeleteInput() {
    const input = document.getElementById('deleteConfirmInput');
    const btn = document.getElementById('deleteConfirmBtn');
    
    if (!input || !btn) return;
    
    // 檢查輸入是否與目標標題完全匹配
    const isMatch = input.value === deleteTarget.title;
    btn.disabled = !isMatch;
}

/**
 * 確認刪除
 */
function confirmDelete() {
    if (!deleteTarget.id) return;
    
    $.ajax({
        url: '/Backoffice/SidebarNavigation/Delete',
        type: 'POST',
        data: { id: deleteTarget.id },
        success: function (response) {
            if (response.success) {
                alert('刪除成功');
                closeNavModal('nav-delete-modal');
                location.reload();
            } else {
                alert('刪除失敗: ' + (response.message || '未知錯誤'));
            }
        },
        error: function () {
            alert('刪除失敗');
        }
    });
}

// ==================== 表單提交 ====================

/**
 * 儲存導航
 */
function saveNavigation() {
    // 取得表單資料
    const formData = {
        id: parseInt(document.getElementById('navId').value) || 0,
        title: document.getElementById('navTitle').value,
        code: document.getElementById('navCode').value,
        icon: document.getElementById('navIcon').value,
        parent_code: document.getElementById('navParent').value || null,
        sort_order: parseInt(document.getElementById('navSort').value) || 0,
        url: document.getElementById('navUrl').value,
        is_active: document.getElementById('navIsActive').checked
    };
    
    // 驗證必填欄位
    if (!formData.title || !formData.title.trim()) {
        alert('請填寫標題');
        document.getElementById('navTitle').focus();
        return;
    }
    
    if (!formData.code || !formData.code.trim()) {
        alert('請填寫代碼');
        document.getElementById('navCode').focus();
        return;
    }
    
    // 驗證代碼格式（只允許字母、數字、底線、連字符）
    if (!/^[a-zA-Z0-9_-]+$/.test(formData.code)) {
        alert('代碼只能包含英文字母、數字、底線(_)和連字符(-)');
        document.getElementById('navCode').focus();
        return;
    }
    
    // 確保 URL 欄位不是 null
    if (!formData.url) {
        formData.url = '';
    }
    
    // 提交資料 - 統一使用 SubmitNavigation
    $.ajax({
        url: '/Backoffice/SidebarNavigation/SubmitNavigation',
        type: 'POST',
        data: formData,
        success: function (response) {
            if (response.success) {
                alert(response.message || '儲存成功');
                closeNavModal('nav-edit-modal');
                location.reload();
            } else {
                alert('儲存失敗：' + (response.message || '未知錯誤'));
            }
        },
        error: function (xhr, status, error) {
            console.error('AJAX Error:', status, error);
            console.error('Response:', xhr.responseText);
            alert('儲存失敗：伺服器錯誤，請查看控制台獲取詳細資訊');
        }
    });
}

// ==================== 事件監聽 ====================

document.addEventListener('DOMContentLoaded', function () {
    // 圖標搜尋
    const iconSearch = document.getElementById('iconSearch');
    if (iconSearch) {
        iconSearch.addEventListener('input', searchIcons);
    }
    
    // 刪除確認輸入驗證
    const deleteInput = document.getElementById('deleteConfirmInput');
    if (deleteInput) {
        deleteInput.addEventListener('input', validateDeleteInput);
    }
    
    // ESC 鍵關閉模態框（由 Modal.js 統一處理，此處保留作為備用）
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            const modals = ['nav-edit-modal', 'nav-detail-modal', 'nav-delete-modal', 'icon-picker-modal'];
            modals.forEach(modalId => {
                const modal = document.getElementById(modalId);
                if (modal && !modal.classList.contains('hidden')) {
                    if (modalId === 'nav-edit-modal') {
                        resetEditForm();
                    }
                    closeModal(modalId);
                }
            });
        }
    });
});


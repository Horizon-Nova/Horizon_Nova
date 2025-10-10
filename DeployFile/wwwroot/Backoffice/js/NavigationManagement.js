// ==================== 全局变量 ====================
let allIcons = [];
let deleteTarget = { id: null, title: '' };

// ==================== 预定义图标列表 ====================
// 常用的 Lucide Icons（避免 CORS 问题）
const COMMON_ICONS = [
    // 基础图标
    { name: 'home', tags: ['house', 'dashboard'] },
    { name: 'user', tags: ['person', 'profile', 'account'] },
    { name: 'users', tags: ['people', 'group', 'team'] },
    { name: 'settings', tags: ['config', 'preferences'] },
    { name: 'search', tags: ['find', 'magnifier'] },
    { name: 'menu', tags: ['hamburger', 'navigation'] },
    { name: 'x', tags: ['close', 'cancel', 'exit'] },
    { name: 'check', tags: ['done', 'complete', 'success'] },
    { name: 'plus', tags: ['add', 'new', 'create'] },
    { name: 'minus', tags: ['remove', 'subtract'] },
    
    // 文件和文件夹
    { name: 'folder', tags: ['directory', 'files'] },
    { name: 'folder-open', tags: ['directory', 'files'] },
    { name: 'folder-plus', tags: ['new folder', 'create'] },
    { name: 'folder-tree', tags: ['hierarchy', 'structure'] },
    { name: 'file', tags: ['document'] },
    { name: 'file-text', tags: ['document', 'text'] },
    { name: 'files', tags: ['documents', 'multiple'] },
    
    // 导航和方向
    { name: 'arrow-left', tags: ['back', 'previous'] },
    { name: 'arrow-right', tags: ['next', 'forward'] },
    { name: 'arrow-up', tags: ['top'] },
    { name: 'arrow-down', tags: ['bottom'] },
    { name: 'chevron-left', tags: ['back'] },
    { name: 'chevron-right', tags: ['next'] },
    { name: 'chevron-down', tags: ['dropdown'] },
    { name: 'chevron-up', tags: ['collapse'] },
    
    // 编辑操作
    { name: 'edit', tags: ['pencil', 'modify', 'change'] },
    { name: 'edit-2', tags: ['pencil', 'modify'] },
    { name: 'edit-3', tags: ['pencil', 'modify'] },
    { name: 'trash', tags: ['delete', 'remove', 'bin'] },
    { name: 'trash-2', tags: ['delete', 'remove', 'bin'] },
    { name: 'save', tags: ['floppy', 'disk'] },
    { name: 'copy', tags: ['duplicate', 'clone'] },
    
    // 媒体和图像
    { name: 'image', tags: ['photo', 'picture'] },
    { name: 'video', tags: ['movie', 'film'] },
    { name: 'music', tags: ['audio', 'sound'] },
    { name: 'camera', tags: ['photo'] },
    
    // 通信
    { name: 'mail', tags: ['email', 'message'] },
    { name: 'message-square', tags: ['chat', 'comment'] },
    { name: 'message-circle', tags: ['chat', 'comment'] },
    { name: 'phone', tags: ['call', 'telephone'] },
    { name: 'bell', tags: ['notification', 'alarm'] },
    
    // 状态和指示
    { name: 'info', tags: ['information', 'help'] },
    { name: 'alert-circle', tags: ['warning', 'caution'] },
    { name: 'alert-triangle', tags: ['warning', 'danger'] },
    { name: 'help-circle', tags: ['question', 'support'] },
    { name: 'check-circle', tags: ['success', 'done'] },
    { name: 'x-circle', tags: ['error', 'failed'] },
    
    // 可见性
    { name: 'eye', tags: ['view', 'visible', 'show'] },
    { name: 'eye-off', tags: ['hidden', 'invisible', 'hide'] },
    { name: 'lock', tags: ['secure', 'private'] },
    { name: 'unlock', tags: ['open', 'public'] },
    { name: 'shield', tags: ['security', 'protection'] },
    
    // 工具和设置
    { name: 'tool', tags: ['wrench', 'repair'] },
    { name: 'cog', tags: ['settings', 'gear'] },
    { name: 'sliders', tags: ['controls', 'adjust'] },
    { name: 'filter', tags: ['funnel', 'sort'] },
    { name: 'download', tags: ['save', 'export'] },
    { name: 'upload', tags: ['import', 'load'] },
    
    // 商业和金融
    { name: 'shopping-cart', tags: ['cart', 'basket', 'buy'] },
    { name: 'credit-card', tags: ['payment', 'card'] },
    { name: 'dollar-sign', tags: ['money', 'currency'] },
    { name: 'trending-up', tags: ['growth', 'increase'] },
    { name: 'trending-down', tags: ['decrease', 'decline'] },
    { name: 'bar-chart', tags: ['graph', 'statistics'] },
    { name: 'pie-chart', tags: ['graph', 'statistics'] },
    
    // 时间和日历
    { name: 'calendar', tags: ['date', 'schedule'] },
    { name: 'clock', tags: ['time', 'watch'] },
    
    // 地图和位置
    { name: 'map', tags: ['location', 'navigation'] },
    { name: 'map-pin', tags: ['location', 'marker'] },
    { name: 'navigation', tags: ['compass', 'direction'] },
    { name: 'globe', tags: ['world', 'international'] },
    
    // 设备
    { name: 'smartphone', tags: ['mobile', 'phone'] },
    { name: 'tablet', tags: ['ipad', 'device'] },
    { name: 'laptop', tags: ['computer', 'pc'] },
    { name: 'monitor', tags: ['screen', 'display'] },
    { name: 'printer', tags: ['print'] },
    
    // 社交媒体
    { name: 'share', tags: ['forward', 'social'] },
    { name: 'share-2', tags: ['forward', 'social'] },
    { name: 'heart', tags: ['like', 'favorite', 'love'] },
    { name: 'star', tags: ['favorite', 'bookmark', 'rating'] },
    { name: 'bookmark', tags: ['save', 'mark'] },
    
    // 布局和显示
    { name: 'layout', tags: ['grid', 'arrangement'] },
    { name: 'grid', tags: ['tiles', 'layout'] },
    { name: 'list', tags: ['menu', 'items'] },
    { name: 'columns', tags: ['layout', 'split'] },
    { name: 'sidebar', tags: ['panel', 'navigation'] },
    { name: 'maximize', tags: ['fullscreen', 'expand'] },
    { name: 'minimize', tags: ['reduce', 'collapse'] },
    
    // 文本编辑
    { name: 'bold', tags: ['text', 'format'] },
    { name: 'italic', tags: ['text', 'format'] },
    { name: 'underline', tags: ['text', 'format'] },
    { name: 'align-left', tags: ['text', 'format'] },
    { name: 'align-center', tags: ['text', 'format'] },
    { name: 'align-right', tags: ['text', 'format'] },
    
    // 数据库和存储
    { name: 'database', tags: ['storage', 'data'] },
    { name: 'server', tags: ['hosting', 'cloud'] },
    { name: 'hard-drive', tags: ['storage', 'disk'] },
    { name: 'archive', tags: ['box', 'storage'] },
    
    // 网络和连接
    { name: 'wifi', tags: ['wireless', 'internet'] },
    { name: 'link', tags: ['chain', 'url', 'hyperlink'] },
    { name: 'external-link', tags: ['open', 'new window'] },
    { name: 'cloud', tags: ['storage', 'sync'] },
    
    // 开发和代码
    { name: 'code', tags: ['programming', 'develop'] },
    { name: 'terminal', tags: ['console', 'command'] },
    { name: 'git-branch', tags: ['version', 'control'] },
    { name: 'git-commit', tags: ['version', 'control'] },
    { name: 'package', tags: ['box', 'module'] },
    
    // 组织和管理
    { name: 'building', tags: ['office', 'organization', 'company'] },
    { name: 'briefcase', tags: ['work', 'business', 'job'] },
    { name: 'clipboard', tags: ['paste', 'copy'] },
    { name: 'tag', tags: ['label', 'category'] },
    { name: 'hash', tags: ['number', 'tag'] },
    
    // 特殊
    { name: 'zap', tags: ['lightning', 'fast', 'power'] },
    { name: 'award', tags: ['badge', 'achievement'] },
    { name: 'gift', tags: ['present', 'reward'] },
    { name: 'flag', tags: ['banner', 'marker'] },
    { name: 'target', tags: ['goal', 'aim'] },
    { name: 'activity', tags: ['pulse', 'monitor'] },
    { name: 'airplay', tags: ['cast', 'stream'] },
    { name: 'anchor', tags: ['link', 'fixed'] },
    { name: 'aperture', tags: ['camera', 'focus'] },
    { name: 'at-sign', tags: ['email', 'mention'] },
    
    // 天气
    { name: 'sun', tags: ['weather', 'day'] },
    { name: 'moon', tags: ['weather', 'night'] },
    { name: 'cloud', tags: ['weather'] },
    { name: 'umbrella', tags: ['rain', 'weather'] },
    
    // AI 和技术
    { name: 'cpu', tags: ['processor', 'chip'] },
    { name: 'brain', tags: ['ai', 'intelligence'] },
    { name: 'radio', tags: ['signal', 'broadcast'] },
    { name: 'rss', tags: ['feed', 'news'] },
    
    // 衣服和时尚
    { name: 'shirt', tags: ['clothing', 'apparel'] }
];

// ==================== 模态框管理 ====================

/**
 * 显示模态框
 * @param {string} modalType - 模态框类型: 'new', 'edit', 'detail', 'delete', 'icon-picker'
 * @param {number|null} navId - 导航ID（编辑和详情时需要）
 */
function showNavModal(modalType, navId = null) {
    if (modalType === 'icon-picker') {
        document.getElementById('icon-picker-modal').classList.remove('hidden');
        document.body.style.overflow = 'hidden';
        if (allIcons.length === 0) {
            loadIconsFromAPI();
        }
        return;
    }

    if (modalType === 'delete') {
        showDeleteModal(navId);
        return;
    }

    // 区分新增和编辑
    const isEdit = navId != null && navId > 0;
    const modalId = modalType === 'detail' ? 'nav-detail-modal' : 'nav-edit-modal';
    
    // 加载数据
    $.ajax({
        url: '/Backoffice/SidebarNavigation/LoadDetail',
        type: 'GET',
        data: { id: navId || 0 },
        success: function (response) {
            // 根据类型更新页面
            if (modalType === 'new' || modalType === 'edit') {
                updateEditModal(response, isEdit);
            } else if (modalType === 'detail') {
                updateDetailModal(response);
            }
            
            // 显示模态框
            document.getElementById(modalId).classList.remove('hidden');
            document.body.style.overflow = 'hidden';
            
            // 重新初始化 lucide icons
            if (window.lucide) window.lucide.createIcons();
        },
        error: function () {
            alert('载入资料失败');
        }
    });
}

/**
 * 关闭模态框
 */
function closeNavModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.add('hidden');
        document.body.style.overflow = 'auto';
        
        // 如果是编辑框，清空表单
        if (modalId === 'nav-edit-modal') {
            resetEditForm();
        }
    }
}

/**
 * 更新编辑模态框内容
 */
function updateEditModal(data, isEdit) {
    // 更新标题
    const modalTitle = document.querySelector('#nav-edit-modal #modalTitle');
    const modalIcon = document.querySelector('#nav-edit-modal [data-lucide="folder-plus"]');
    
    if (modalTitle) {
        modalTitle.textContent = isEdit ? '編輯目錄' : '新增目錄';
    }
    if (modalIcon) {
        modalIcon.setAttribute('data-lucide', isEdit ? 'edit' : 'folder-plus');
    }
    
    // 填充表单数据
    if (isEdit && data) {
        document.getElementById('navId').value = data.id || '';
        document.getElementById('navTitle').value = data.title || '';
        document.getElementById('navCode').value = data.code || '';
        document.getElementById('navIcon').value = data.icon || '';
        document.getElementById('navParent').value = data.parent_code || '';
        document.getElementById('navSort').value = data.sort_order || 0;
        document.getElementById('navUrl').value = data.url || '';
        document.getElementById('navIsActive').checked = data.is_active !== false;
        
        // 更新图标预览
        updateIconPreview(data.icon);
    } else {
        resetEditForm();
    }
    
    // 加载上层目录选项
    loadParentOptions(data?.parent_code);
}

/**
 * 更新详情模态框内容
 */
function updateDetailModal(data) {
    // 这里可以动态更新详情内容
    // 目前使用服务器端渲染，所以不需要额外处理
}

/**
 * 重置编辑表单
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

// ==================== 上层目录管理 ====================

/**
 * 加载上层目录选项
 */
function loadParentOptions(selectedCode = null) {
    $.ajax({
        url: '/Backoffice/SidebarNavigation/GetParentOptions',
        type: 'GET',
        success: function (data) {
            const select = document.getElementById('navParent');
            if (!select) return;
            
            // 清空现有选项
            select.innerHTML = '<option value="">無（第一層目錄）</option>';
            
            // 添加选项
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
            console.error('加载上层目录失败');
        }
    });
}

// ==================== 图标选择器 ====================

/**
 * 加载图标（使用预定义列表）
 */
function loadIconsFromAPI() {
    const loading = document.getElementById('iconLoading');
    const error = document.getElementById('iconError');
    const grid = document.getElementById('iconGrid');
    
    // 显示加载中
    if (loading) loading.classList.remove('hidden');
    if (error) error.classList.add('hidden');
    if (grid) grid.classList.add('hidden');
    
    try {
        // 使用预定义的图标列表（避免 CORS 问题）
        allIcons = [...COMMON_ICONS];
        
        // 更新统计
        document.getElementById('totalIcons').textContent = allIcons.length;
        document.getElementById('filteredIcons').textContent = allIcons.length;
        
        // 渲染图标
        renderIcons(allIcons);
        
        // 隐藏加载中，显示网格
        if (loading) loading.classList.add('hidden');
        if (grid) grid.classList.remove('hidden');
        
    } catch (err) {
        console.error('加载图标失败:', err);
        if (loading) loading.classList.add('hidden');
        if (error) {
            error.classList.remove('hidden');
            const errorMsg = document.getElementById('iconErrorMessage');
            if (errorMsg) {
                errorMsg.textContent = `錯誤: ${err.message}`;
            }
        }
    }
}

/**
 * 渲染图标网格
 */
function renderIcons(icons) {
    const grid = document.getElementById('iconGrid');
    const noResults = document.getElementById('noResults');
    
    if (!grid) return;
    
    // 清空网格
    grid.innerHTML = '';
    
    if (icons.length === 0) {
        if (noResults) noResults.classList.remove('hidden');
        grid.classList.add('hidden');
        return;
    }
    
    if (noResults) noResults.classList.add('hidden');
    grid.classList.remove('hidden');
    
    // 渲染每个图标
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
    
    // 重新初始化图标
    if (window.lucide) window.lucide.createIcons();
}

/**
 * 搜索图标
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
    
    // 过滤图标
    const filtered = allIcons.filter(icon => {
        return icon.name.toLowerCase().includes(keyword) ||
               icon.tags.some(tag => tag.toLowerCase().includes(keyword));
    });
    
    renderIcons(filtered);
    document.getElementById('filteredIcons').textContent = filtered.length;
}

/**
 * 选择图标
 */
function selectIcon(iconName) {
    // 更新输入框
    const iconInput = document.getElementById('navIcon');
    if (iconInput) {
        iconInput.value = iconName;
    }
    
    // 更新预览
    updateIconPreview(iconName);
    
    // 关闭图标选择器
    closeNavModal('icon-picker-modal');
}

/**
 * 更新图标预览
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

// ==================== 删除功能 ====================

/**
 * 显示删除确认模态框
 */
function showDeleteModal(navId) {
    // 获取导航信息
    $.ajax({
        url: '/Backoffice/SidebarNavigation/LoadDetail',
        type: 'GET',
        data: { id: navId },
        success: function (data) {
            deleteTarget = { id: data.id, title: data.title };
            
            // 更新删除模态框内容
            document.getElementById('deleteNavTitle').textContent = data.title;
            document.getElementById('deleteNavCode').textContent = data.code;
            document.getElementById('deleteConfirmInput').value = '';
            document.getElementById('deleteConfirmBtn').disabled = true;
            
            // 显示模态框
            document.getElementById('nav-delete-modal').classList.remove('hidden');
            document.body.style.overflow = 'hidden';
        },
        error: function () {
            alert('載入資料失敗');
        }
    });
}

/**
 * 验证删除输入
 */
function validateDeleteInput() {
    const input = document.getElementById('deleteConfirmInput');
    const btn = document.getElementById('deleteConfirmBtn');
    
    if (!input || !btn) return;
    
    // 检查输入是否与目标标题完全匹配
    const isMatch = input.value === deleteTarget.title;
    btn.disabled = !isMatch;
}

/**
 * 确认删除
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

// ==================== 表单提交 ====================

/**
 * 保存导航
 */
function saveNavigation() {
    // 获取表单数据
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
    
    // 验证必填字段
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
    
    // 验证代码格式（只允许字母、数字、底线、连字符）
    if (!/^[a-zA-Z0-9_-]+$/.test(formData.code)) {
        alert('代碼只能包含英文字母、數字、底線(_)和連字符(-)');
        document.getElementById('navCode').focus();
        return;
    }
    
    // 确保 URL 字段不是 null
    if (!formData.url) {
        formData.url = '';
    }
    
    // 提交数据 - 统一使用 SubmitNavigation
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
            alert('儲存失敗：伺服器錯誤，請查看控制台獲取詳細信息');
        }
    });
}

// ==================== 事件监听 ====================

document.addEventListener('DOMContentLoaded', function () {
    // 图标搜索
    const iconSearch = document.getElementById('iconSearch');
    if (iconSearch) {
        iconSearch.addEventListener('input', searchIcons);
    }
    
    // 删除确认输入验证
    const deleteInput = document.getElementById('deleteConfirmInput');
    if (deleteInput) {
        deleteInput.addEventListener('input', validateDeleteInput);
    }
    
    // ESC 键关闭模态框
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            const modals = ['nav-edit-modal', 'nav-detail-modal', 'nav-delete-modal', 'icon-picker-modal'];
            modals.forEach(modalId => {
                const modal = document.getElementById(modalId);
                if (modal && !modal.classList.contains('hidden')) {
                    closeNavModal(modalId);
                }
            });
        }
    });
});


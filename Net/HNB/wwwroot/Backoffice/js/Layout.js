// Layout.js - Bootstrap 專用腳本

// 側欄狀態儲存 Key
const SIDEBAR_COLLAPSED_KEY = 'sidebar_collapsed_state_bootstrap';
const NAVIGATION_STATE_KEY = 'sidebar_navigation_state_bootstrap';

// 載入側欄狀態（使用 sessionStorage）
function loadSidebarState() {
    try {
        const saved = sessionStorage.getItem(SIDEBAR_COLLAPSED_KEY);
        return saved === 'true';
    } catch {
        return false;
    }
}

// 儲存側欄狀態（使用 sessionStorage）
function saveSidebarState(collapsed) {
    try {
        sessionStorage.setItem(SIDEBAR_COLLAPSED_KEY, collapsed ? 'true' : 'false');
    } catch {
        // 忽略錯誤
    }
}

// 處理導航項目的顯示/隱藏（根據側欄狀態）
function updateNavigationVisibility(isCollapsed) {
    const navigationState = loadNavigationState();
    
    document.querySelectorAll('.nav-children').forEach(children => {
        if (isCollapsed) {
            // 側欄收合時，隱藏所有子選單
            children.classList.add('hidden');
        } else {
            // 側欄展開時，恢復之前的狀態
            const itemId = children.id.replace('nav-children-', '');
            const isExpanded = navigationState[itemId] === true;
            
            if (isExpanded) {
                children.classList.remove('hidden');
                const toggle = document.querySelector(`[data-target="${children.id}"]`);
                if (toggle) {
                    const chevron = toggle.querySelector('.nav-chevron-toggle');
                    if (chevron) {
                        chevron.style.transform = 'rotate(90deg)';
                    }
                }
            } else {
                children.classList.add('hidden');
            }
        }
    });
    
    // 更新所有箭頭狀態
    document.querySelectorAll('.nav-chevron-toggle').forEach(chevron => {
        if (isCollapsed) {
            chevron.style.transform = 'rotate(0deg)';
        }
    });
}

// 啟用側欄動畫（只在用戶操作後啟用）
function enableSidebarAnimation() {
    const sidebar = document.getElementById('sidebar');
    const appMain = document.querySelector('.app-main');
    
    if (sidebar && !sidebar.classList.contains('sidebar-animated')) {
        sidebar.classList.add('sidebar-animated');
    }
    if (appMain && !appMain.classList.contains('sidebar-animated')) {
        appMain.classList.add('sidebar-animated');
    }
}

// 側欄切換 Function（可在 HTML 中使用 onclick 呼叫）
function ToggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    const iconChevron = document.getElementById('iconChevron');
    
    if (!sidebar) return;
    
    // 第一次操作時啟用動畫
    enableSidebarAnimation();
    
    const isCollapsed = sidebar.getAttribute('data-collapsed') === 'true';
    const newCollapsed = !isCollapsed;
    
    // 設定側欄狀態
    sidebar.setAttribute('data-collapsed', newCollapsed ? 'true' : 'false');
    
    // 更新圖示
    if (iconChevron) {
        if (newCollapsed) {
            iconChevron.style.transform = 'rotate(180deg)';
        } else {
            iconChevron.style.transform = 'rotate(0deg)';
        }
    }
    
    // 更新導航項目顯示狀態
    updateNavigationVisibility(newCollapsed);
    
    // 儲存狀態
    saveSidebarState(newCollapsed);
}

// 載入導航項目狀態（使用 sessionStorage）
function loadNavigationState() {
    try {
        const saved = sessionStorage.getItem(NAVIGATION_STATE_KEY);
        return saved ? JSON.parse(saved) : {};
    } catch {
        return {};
    }
}

// 儲存導航項目狀態（使用 sessionStorage）
function saveNavigationState(state) {
    try {
        sessionStorage.setItem(NAVIGATION_STATE_KEY, JSON.stringify(state));
    } catch {
        // 忽略錯誤
    }
}

// 導航項目切換 Function（可在 HTML 中使用 onclick 呼叫）
function ToggleNavigationItem(itemId, targetId) {
    const children = document.getElementById(targetId);
    if (!children) return;
    
    const navigationState = loadNavigationState();
    const isExpanded = !children.classList.contains('hidden');
    const toggle = document.querySelector(`[data-target="${targetId}"]`);
    const chevron = toggle ? toggle.querySelector('.nav-chevron-toggle') : null;
    
    if (isExpanded) {
        children.classList.add('hidden');
        if (chevron) {
            chevron.style.transform = 'rotate(0deg)';
        }
        navigationState[itemId] = false;
    } else {
        children.classList.remove('hidden');
        if (chevron) {
            chevron.style.transform = 'rotate(90deg)';
        }
        navigationState[itemId] = true;
    }
    
    saveNavigationState(navigationState);
    
    // 重新初始化 Lucide icons
    if (typeof lucide !== 'undefined' && typeof lucide.createIcons === 'function') {
        lucide.createIcons();
    }
}

// 手機版側欄開啟 Function
function OpenMobileSidebar() {
    const sidebar = document.getElementById('sidebar');
    const sidebarBackdrop = document.getElementById('sidebarBackdrop');
    
    if (sidebar) {
        sidebar.classList.add('show');
    }
    if (sidebarBackdrop) {
        sidebarBackdrop.classList.add('show');
    }
    document.body.style.overflow = 'hidden';
}

// 手機版側欄關閉 Function
function CloseMobileSidebar() {
    const sidebar = document.getElementById('sidebar');
    const sidebarBackdrop = document.getElementById('sidebarBackdrop');
    
    if (sidebar) {
        sidebar.classList.remove('show');
    }
    if (sidebarBackdrop) {
        sidebarBackdrop.classList.remove('show');
    }
    document.body.style.overflow = '';
}

// 使用者選單切換 Function
function ToggleUserMenu(forceState) {
    const userMenu = document.getElementById('userMenu');
    const btnUserMenu = document.getElementById('btnUserMenu');
    
    if (!userMenu || !btnUserMenu) return;
    
    const shouldOpen = typeof forceState === 'boolean' ? forceState : !userMenu.classList.contains('show');
    userMenu.classList.toggle('show', shouldOpen);
    btnUserMenu.setAttribute('aria-expanded', shouldOpen ? 'true' : 'false');
}

// 自動設定活躍導航項目
function setActiveNavigation() {
    const currentPath = window.location.pathname.toLowerCase();
    
    document.querySelectorAll('.nav-item').forEach(item => {
        item.classList.remove('active');
    });
    
    let activeItem = null;
    let bestMatchLength = 0;
    
    document.querySelectorAll('.nav-item').forEach(item => {
        const href = item.getAttribute('href')?.toLowerCase() || '';
        
        if (!href) return;
        
        if (href === currentPath) {
            if (href.length > bestMatchLength) {
                activeItem = item;
                bestMatchLength = href.length;
            }
        }
        else if (currentPath.startsWith(href) && href.length > 1) {
            if (href.length > bestMatchLength) {
                activeItem = item;
                bestMatchLength = href.length;
            }
        }
    });
    
    if (activeItem) {
        activeItem.classList.add('active');
        
        // 如果活躍項目在子選單內，自動展開父選單
        let parent = activeItem.closest('.nav-children');
        if (parent) {
            parent.classList.remove('hidden');
            
            const parentId = parent.id;
            const toggle = document.querySelector(`[data-target="${parentId}"]`);
            if (toggle) {
                const chevron = toggle.querySelector('.nav-chevron-toggle');
                if (chevron) {
                    chevron.style.transform = 'rotate(90deg)';
                }
                
                const itemId = toggle.getAttribute('data-item-id');
                if (itemId) {
                    try {
                        const navigationState = loadNavigationState();
                        navigationState[itemId] = true;
                        saveNavigationState(navigationState);
                    } catch {
                    }
                }
            }
        }
    }
}

// 初始化
document.addEventListener('DOMContentLoaded', function () {
    const sidebar = document.getElementById('sidebar');
    
    if (sidebar) {
        const savedState = sessionStorage.getItem(SIDEBAR_COLLAPSED_KEY);
        let isCollapsed = true; // 預設為收起
        
        const currentState = sidebar.getAttribute('data-collapsed');
        if (currentState !== null && savedState !== null && currentState === savedState) {
            isCollapsed = currentState === 'true';
        } else {
            if (savedState !== null) {
                isCollapsed = savedState === 'true';
            } else {
                if (currentState !== null) {
                    isCollapsed = currentState === 'true';
                }
            }
            
            // 只有在狀態不同時才設定，避免觸發動畫
            const shouldBeCollapsed = isCollapsed ? 'true' : 'false';
            if (currentState !== shouldBeCollapsed) {
                sidebar.setAttribute('data-collapsed', shouldBeCollapsed);
            }
        }
        
        // 更新圖示
        const iconChevron = document.getElementById('iconChevron');
        if (iconChevron) {
            if (isCollapsed) {
                iconChevron.style.transform = 'rotate(180deg)';
            } else {
                iconChevron.style.transform = 'rotate(0deg)';
            }
        }
        
        // 更新導航項目顯示狀態
        updateNavigationVisibility(isCollapsed);
        
        // 注意：不在初始化時啟用動畫，只有用戶操作時才啟用
    }
    
    // 初始化已展開的導航項目（只有在側欄展開時才恢復）
    const isSidebarCollapsed = sidebar && sidebar.getAttribute('data-collapsed') === 'true';
    if (!isSidebarCollapsed) {
        const navigationState = loadNavigationState();
        document.querySelectorAll('.nav-children').forEach(children => {
            const itemId = children.id.replace('nav-children-', '');
            const isExpanded = navigationState[itemId] === true;
            
            if (isExpanded) {
                children.classList.remove('hidden');
                const toggle = document.querySelector(`[data-target="${children.id}"]`);
                if (toggle) {
                    const chevron = toggle.querySelector('.nav-chevron-toggle');
                    if (chevron) {
                        chevron.style.transform = 'rotate(90deg)';
                    }
                }
            }
        });
    }
    
    // 點擊外部關閉使用者選單
    document.addEventListener('click', function (e) {
        const userMenu = document.getElementById('userMenu');
        const btnUserMenu = document.getElementById('btnUserMenu');
        
        if (!userMenu || !btnUserMenu) return;
        if (!userMenu.classList.contains('show')) return;
        if (!userMenu.contains(e.target) && !btnUserMenu.contains(e.target)) {
            ToggleUserMenu(false);
        }
    });
    
    // ESC 關閉選單和側欄
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            CloseMobileSidebar();
            ToggleUserMenu(false);
        }
    });
    
    // 自動設定活躍導航項目
    setActiveNavigation();
});

// Layout.js - Bootstrap 專用腳本

document.addEventListener('DOMContentLoaded', function () {
    // Initialize Lucide icons
    try { 
        if (window.lucide && lucide.createIcons) { 
            lucide.createIcons(); 
        } 
    } catch (e) {
        console.warn('Lucide icons initialization failed:', e);
    }

    const sidebar = document.getElementById('sidebar');
    const btnToggleSidebar = document.getElementById('btnToggleSidebar');
    const iconChevron = document.getElementById('iconChevron');
    const btnMobileMenu = document.getElementById('btnMobileMenu');
    const sidebarBackdrop = document.getElementById('sidebarBackdrop');
    const btnUserMenu = document.getElementById('btnUserMenu');
    const userMenu = document.getElementById('userMenu');

    // 側欄展開/收合
    function setCollapsed(collapsed) {
        if (!sidebar) return;
        sidebar.setAttribute('data-collapsed', collapsed ? 'true' : 'false');
        if (iconChevron) {
            if (collapsed) {
                iconChevron.style.transform = 'rotate(180deg)';
            } else {
                iconChevron.style.transform = 'rotate(0deg)';
            }
        }
    }

    if (btnToggleSidebar) {
        btnToggleSidebar.addEventListener('click', function () {
            const isCollapsed = sidebar && sidebar.getAttribute('data-collapsed') === 'true';
            setCollapsed(!isCollapsed);
        });
    }

    // 初始化側欄狀態
    if (sidebar && !sidebar.hasAttribute('data-collapsed')) {
        sidebar.setAttribute('data-collapsed', 'false');
    }

    // 手機版側欄開啟
    function openMobileSidebar() {
        if (!sidebar) return;
        sidebar.classList.add('show');
        if (sidebarBackdrop) sidebarBackdrop.classList.add('show');
        document.body.style.overflow = 'hidden';
    }

    // 手機版側欄關閉
    function closeMobileSidebar() {
        if (!sidebar) return;
        sidebar.classList.remove('show');
        if (sidebarBackdrop) sidebarBackdrop.classList.remove('show');
        document.body.style.overflow = '';
    }

    if (btnMobileMenu) {
        btnMobileMenu.addEventListener('click', openMobileSidebar);
    }

    if (sidebarBackdrop) {
        sidebarBackdrop.addEventListener('click', closeMobileSidebar);
    }

    // 使用者選單切換
    function toggleUserMenu(forceState) {
        if (!userMenu || !btnUserMenu) return;
        const shouldOpen = typeof forceState === 'boolean' ? forceState : !userMenu.classList.contains('show');
        userMenu.classList.toggle('show', shouldOpen);
        btnUserMenu.setAttribute('aria-expanded', shouldOpen ? 'true' : 'false');
    }

    if (btnUserMenu) {
        btnUserMenu.addEventListener('click', function (e) {
            e.stopPropagation();
            toggleUserMenu();
        });
    }

    // 點擊外部關閉選單
    document.addEventListener('click', function (e) {
        if (!userMenu || !btnUserMenu) return;
        if (!userMenu.classList.contains('show')) return;
        if (!userMenu.contains(e.target) && !btnUserMenu.contains(e.target)) {
            toggleUserMenu(false);
        }
    });

    // ESC 關閉選單和側欄
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeMobileSidebar();
            toggleUserMenu(false);
        }
    });

    // 初始化子選單功能
    initNavigationToggle();

    // 自動設定活躍導航項目
    setActiveNavigation();
});

// 子選單展開/收合功能
function initNavigationToggle() {
    const STORAGE_KEY = 'sidebar_navigation_state_bootstrap';
    
    // 載入已儲存的狀態
    const loadNavigationState = () => {
        try {
            const saved = localStorage.getItem(STORAGE_KEY);
            return saved ? JSON.parse(saved) : {};
        } catch {
            return {};
        }
    };
    
    // 儲存狀態
    const saveNavigationState = (state) => {
        try {
            localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
        } catch {
            // 忽略錯誤
        }
    };
    
    const navigationState = loadNavigationState();
    
    // 初始化已展開的項目
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
    
    // 綁定點擊事件
    document.querySelectorAll('.nav-toggle').forEach(toggle => {
        toggle.addEventListener('click', (e) => {
            e.preventDefault();
            
            const targetId = toggle.getAttribute('data-target');
            const itemId = toggle.getAttribute('data-item-id');
            const children = document.getElementById(targetId);
            const chevron = toggle.querySelector('.nav-chevron-toggle');
            
            if (!children) return;
            
            const isExpanded = !children.classList.contains('hidden');
            
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
            if (window.lucide && lucide.createIcons) {
                lucide.createIcons();
            }
        });
    });
    
    // 監聽側欄收合狀態
    const sidebar = document.getElementById('sidebar');
    if (sidebar) {
        const observer = new MutationObserver((mutations) => {
            mutations.forEach((mutation) => {
                if (mutation.type === 'attributes' && mutation.attributeName === 'data-collapsed') {
                    const isCollapsed = sidebar.getAttribute('data-collapsed') === 'true';
                    if (isCollapsed) {
                        // 側欄收合時，隱藏所有子選單
                        document.querySelectorAll('.nav-children').forEach(children => {
                            children.classList.add('hidden');
                        });
                        document.querySelectorAll('.nav-chevron-toggle').forEach(chevron => {
                            chevron.style.transform = 'rotate(0deg)';
                        });
                    } else {
                        // 側欄展開時，恢復之前的狀態
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
                }
            });
        });
        
        observer.observe(sidebar, { attributes: true });
    }
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
                    const STORAGE_KEY = 'sidebar_navigation_state_bootstrap';
                    try {
                        const saved = localStorage.getItem(STORAGE_KEY);
                        const state = saved ? JSON.parse(saved) : {};
                        state[itemId] = true;
                        localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
                    } catch {
                        // 忽略錯誤
                    }
                }
            }
        }
    }
}

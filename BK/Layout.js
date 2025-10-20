document.addEventListener('DOMContentLoaded', () => {
    initTheme();
    if (window.lucide) window.lucide.createIcons();

    wireSidebar();
    wireMobileSidebar();
    wireUserMenu();
    wireNavigationToggle();
    setActiveNavigation();
});

function initTheme() {
    const ls = localStorage.getItem('theme');
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    const useDark = (ls === 'dark') || (ls === null && prefersDark);
    document.documentElement.classList.toggle('dark', useDark);
}

/* 側欄摺疊 */
function wireSidebar() {
    const sidebar = document.getElementById('sidebar');
    const btnToggle = document.getElementById('btnToggleSidebar');
    const iconChevron = document.getElementById('iconChevron');
    const collapsedFlag = 'data-collapsed';
    if (!sidebar) return;

    const applyCollapsedUI = () => {
        const isCollapsed = sidebar.getAttribute(collapsedFlag) === 'true';
        document.querySelectorAll('[data-collapsed-target]').forEach(el => {
            if (isCollapsed) el.classList.add('hidden'); else el.classList.remove('hidden');
        });
        if (window.lucide && iconChevron) {
            iconChevron.setAttribute('data-lucide', isCollapsed ? 'chevron-right' : 'chevron-left');
            window.lucide.createIcons();
        }
    };

    btnToggle?.addEventListener('click', () => {
        const isCollapsed = sidebar.getAttribute(collapsedFlag) === 'true';
        sidebar.setAttribute(collapsedFlag, String(!isCollapsed));
        applyCollapsedUI();
    });

    if (!sidebar.hasAttribute(collapsedFlag)) sidebar.setAttribute(collapsedFlag, 'false');
    applyCollapsedUI();
}

/* 手機版側欄 */
function wireMobileSidebar() {
    const sidebar = document.getElementById('sidebar');
    const btnMobile = document.getElementById('btnMobileMenu');
    const backdrop = document.getElementById('sidebarBackdrop');
    if (!sidebar || !btnMobile || !backdrop) return;

    const openSidebar = () => {
        sidebar.classList.remove('-translate-x-full');
        backdrop.classList.remove('hidden');
        document.body.style.overflow = 'hidden';
    };

    const closeSidebar = () => {
        sidebar.classList.add('-translate-x-full');
        backdrop.classList.add('hidden');
        document.body.style.overflow = '';
    };

    btnMobile.addEventListener('click', () => {
        if (sidebar.classList.contains('-translate-x-full')) {
            openSidebar();
        } else {
            closeSidebar();
        }
    });

    backdrop.addEventListener('click', closeSidebar);

    // 點擊側邊欄連結時自動關閉（手機版）
    sidebar.querySelectorAll('a').forEach(link => {
        link.addEventListener('click', () => {
            if (window.innerWidth < 1024) {
                closeSidebar();
            }
        });
    });
}

/* 使用者選單 Dropdown */
function wireUserMenu() {
    const btn = document.getElementById('btnUserMenu');
    const menu = document.getElementById('userMenu');
    if (!btn || !menu) return;

    btn.addEventListener('click', (e) => {
        e.stopPropagation();
        menu.classList.toggle('hidden');
        btn.setAttribute('aria-expanded', menu.classList.contains('hidden') ? 'false' : 'true');
    });

    document.addEventListener('click', () => {
        if (!menu.classList.contains('hidden')) {
            menu.classList.add('hidden');
            btn.setAttribute('aria-expanded', 'false');
        }
    });

    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && !menu.classList.contains('hidden')) {
            menu.classList.add('hidden');
            btn.setAttribute('aria-expanded', 'false');
        }
    });
}

/* 導航項目摺疊功能 */
function wireNavigationToggle() {
    const STORAGE_KEY = 'sidebar_navigation_state';
    
    const loadNavigationState = () => {
        try {
            const saved = localStorage.getItem(STORAGE_KEY);
            return saved ? JSON.parse(saved) : {};
        } catch {
            return {};
        }
    };
    
    const saveNavigationState = (state) => {
        try {
            localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
        } catch {
        }
    };
    
    const navigationState = loadNavigationState();
    
    document.querySelectorAll('.nav-children').forEach(children => {
        const itemId = children.id.replace('nav-children-', '');
        const isExpanded = navigationState[itemId] === true;
        
        if (isExpanded) {
            children.classList.remove('hidden');
            const toggle = document.querySelector(`[data-target="${children.id}"]`);
            if (toggle) {
                const chevron = toggle.querySelector('.nav-chevron');
                if (chevron) {
                    chevron.style.transform = 'rotate(90deg)';
                }
            }
        }
    });
    
    document.querySelectorAll('.nav-toggle').forEach(toggle => {
        toggle.addEventListener('click', (e) => {
            e.preventDefault();
            
            const targetId = toggle.getAttribute('data-target');
            const itemId = toggle.getAttribute('data-item-id');
            const children = document.getElementById(targetId);
            const chevron = toggle.querySelector('.nav-chevron');
            
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
        });
    });
    
    const sidebar = document.getElementById('sidebar');
    if (sidebar) {
        const observer = new MutationObserver((mutations) => {
            mutations.forEach((mutation) => {
                if (mutation.type === 'attributes' && mutation.attributeName === 'data-collapsed') {
                    const isCollapsed = sidebar.getAttribute('data-collapsed') === 'true';
                    if (isCollapsed) {
                        document.querySelectorAll('.nav-children').forEach(children => {
                            children.classList.add('hidden');
                        });
                        document.querySelectorAll('.nav-chevron').forEach(chevron => {
                            chevron.style.transform = 'rotate(0deg)';
                        });
                    } else {
                        document.querySelectorAll('.nav-children').forEach(children => {
                            const itemId = children.id.replace('nav-children-', '');
                            const isExpanded = navigationState[itemId] === true;
                            
                            if (isExpanded) {
                                children.classList.remove('hidden');
                                const toggle = document.querySelector(`[data-target="${children.id}"]`);
                                if (toggle) {
                                    const chevron = toggle.querySelector('.nav-chevron');
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

/* 自動設定活躍導航項目 */
function setActiveNavigation() {
    const currentPath = window.location.pathname.toLowerCase();
    
    document.querySelectorAll('.nav-item').forEach(item => {
        item.classList.remove('active', 'bg-blue-50', 'text-blue-600');
    });
    
    let activeItem = null;
    let bestMatchLength = 0;
    
    document.querySelectorAll('.nav-item').forEach(item => {
        const href = item.getAttribute('href')?.toLowerCase() || '';
        const dataUrl = item.getAttribute('data-nav-url')?.toLowerCase() || '';
        const itemUrl = href || dataUrl;
        
        if (!itemUrl) return;
        
        if (itemUrl === currentPath) {
            if (itemUrl.length > bestMatchLength) {
                activeItem = item;
                bestMatchLength = itemUrl.length;
            }
        }
        else if (currentPath.startsWith(itemUrl) && itemUrl.length > 1) {
            if (itemUrl.length > bestMatchLength) {
                activeItem = item;
                bestMatchLength = itemUrl.length;
            }
        }
    });
    
    if (activeItem) {
        activeItem.classList.add('active', 'bg-blue-50', 'text-blue-600');
        
        let parent = activeItem.closest('.nav-children');
        while (parent) {
            parent.classList.remove('hidden');
            
            const parentId = parent.id;
            const toggle = document.querySelector(`[data-target="${parentId}"]`);
            if (toggle) {
                const chevron = toggle.querySelector('.nav-chevron');
                if (chevron) {
                    chevron.style.transform = 'rotate(90deg)';
                }
                
                const itemId = toggle.getAttribute('data-item-id');
                if (itemId) {
                    const STORAGE_KEY = 'sidebar_navigation_state';
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
            
            // 繼續向上查找父級
            parent = parent.parentElement?.closest('.nav-children');
        }
    }
}

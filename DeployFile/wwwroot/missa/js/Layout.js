document.addEventListener('DOMContentLoaded', () => {
    initTheme();
    if (window.lucide) window.lucide.createIcons();

    wireSidebar();
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

/* 導航項目摺疊功能 */
function wireNavigationToggle() {
    // 儲存展開狀態的 key
    const STORAGE_KEY = 'sidebar_navigation_state';

    // 從 localStorage 載入狀態
    const loadNavigationState = () => {
        try {
            const saved = localStorage.getItem(STORAGE_KEY);
            return saved ? JSON.parse(saved) : {};
        } catch {
            return {};
        }
    };

    // 儲存狀態到 localStorage
    const saveNavigationState = (state) => {
        try {
            localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
        } catch {
            // 忽略儲存錯誤
        }
    };

    // 初始化導航狀態
    const navigationState = loadNavigationState();

    // 設置初始狀態
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

    // 綁定點擊事件
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
                // 摺疊
                children.classList.add('hidden');
                if (chevron) {
                    chevron.style.transform = 'rotate(0deg)';
                }
                navigationState[itemId] = false;
            } else {
                // 展開
                children.classList.remove('hidden');
                if (chevron) {
                    chevron.style.transform = 'rotate(90deg)';
                }
                navigationState[itemId] = true;
            }

            // 儲存狀態
            saveNavigationState(navigationState);
        });
    });

    // 當側欄摺疊時，隱藏所有展開的子選單
    const sidebar = document.getElementById('sidebar');
    if (sidebar) {
        const observer = new MutationObserver((mutations) => {
            mutations.forEach((mutation) => {
                if (mutation.type === 'attributes' && mutation.attributeName === 'data-collapsed') {
                    const isCollapsed = sidebar.getAttribute('data-collapsed') === 'true';
                    if (isCollapsed) {
                        // 側欄摺疊時隱藏所有子選單
                        document.querySelectorAll('.nav-children').forEach(children => {
                            children.classList.add('hidden');
                        });
                        // 重置所有箭頭
                        document.querySelectorAll('.nav-chevron').forEach(chevron => {
                            chevron.style.transform = 'rotate(0deg)';
                        });
                    } else {
                        // 側欄展開時恢復之前的狀態
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
    
    // 移除所有現有的 active 類別
    document.querySelectorAll('.nav-item').forEach(item => {
        item.classList.remove('active');
    });
    
    // 找到匹配的導航項目並添加 active 類別
    document.querySelectorAll('.nav-item').forEach(item => {
        const href = item.getAttribute('href')?.toLowerCase() || '';
        
        // 精確匹配或路徑包含匹配
        if (href === currentPath || (href !== '' && currentPath.startsWith(href))) {
            item.classList.add('active');
        }
    });
}

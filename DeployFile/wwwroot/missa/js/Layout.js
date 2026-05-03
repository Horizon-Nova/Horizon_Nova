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
        item.classList.remove('active');
    });
    
    document.querySelectorAll('.nav-item').forEach(item => {
        const href = item.getAttribute('href')?.toLowerCase() || '';
        
        if (href === currentPath || (href !== '' && currentPath.startsWith(href))) {
            item.classList.add('active');
        }
    });
}

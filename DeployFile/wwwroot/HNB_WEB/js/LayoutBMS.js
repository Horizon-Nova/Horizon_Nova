document.addEventListener('DOMContentLoaded', () => {
    initSidebarToggle();
    lucide.createIcons();
});

function initSidebarToggle(sidebarId = 'sidebar', btnId = 'toggleBtn') {
    const sidebar = document.getElementById(sidebarId);
    const btn = document.getElementById(btnId);
    if (!sidebar || !btn) return;

    const LS = 'bmsSidebarCollapsed';

    const headerBar = sidebar.querySelector('.border-bottom.d-flex');
    let headerTitle = headerBar?.querySelector('.label');
    const headerTitleText = (headerTitle?.textContent || '').trim() || '管理系統';

    const items = Array.from(sidebar.querySelectorAll('.nav-link')).map(link => {
        const label = link.querySelector('.label');
        const text = (label?.textContent || '').trim();
        return { link, text };
    });

    function getIconAnchor(link) {
        return (
            link.querySelector('.icon') ||
            link.querySelector('svg.lucide') ||
            link.querySelector('[data-lucide]')
        );
    }

    // Tooltip 控制
    let tooltips = [];
    function enableTooltips() {
        items.forEach(({ link, text }) => { if (text) link.setAttribute('title', text); });
        tooltips = items.map(({ link }) => new bootstrap.Tooltip(link, { placement: 'right', trigger: 'hover' }));
    }
    function disableTooltips() {
        tooltips.forEach(t => t.dispose()); tooltips = [];
        items.forEach(({ link }) => link.removeAttribute('title'));
    }

    function applyCollapsed(collapsed) {
        sidebar.classList.toggle('collapsed', collapsed);

        if (collapsed) {
            if (headerTitle) { headerTitle.remove(); headerTitle = null; }
            headerBar?.classList.remove('justify-content-between');
            headerBar?.classList.add('justify-content-center');
        } else {
            if (!headerBar?.querySelector('.label') && headerTitleText) {
                const titleEl = document.createElement('div');
                titleEl.className = 'fw-semibold fs-5 label';
                titleEl.textContent = headerTitleText;
                headerBar.insertBefore(titleEl, btn);
                headerTitle = titleEl;
            }
            headerBar?.classList.remove('justify-content-center');
            headerBar?.classList.add('justify-content-between');
        }

        // Menu 項目
        items.forEach(({ link, text }) => {
            link.classList.toggle('justify-center', collapsed);
            link.classList.toggle('justify-start', !collapsed);

            const anchor = getIconAnchor(link);

            if (anchor) {
                anchor.classList.toggle('me-2', !collapsed);
            }

            const hasLabel = !!link.querySelector('.label');

            if (collapsed) {
                if (hasLabel) link.querySelector('.label').remove();
            } else {
                if (!hasLabel && text) {
                    const span = document.createElement('span');
                    span.className = 'label';
                    span.textContent = text;
                    if (anchor?.after) anchor.after(span);
                    else link.appendChild(span);
                }
            }
        });

        // Tooltip 只在收合時啟用
        if (collapsed) enableTooltips(); else disableTooltips();

        btn.innerHTML = collapsed
            ? '<i data-lucide="chevron-right"></i>'
            : '<i data-lucide="chevron-left"></i>';
        lucide.createIcons();

    }

    // 初始化：如果你有用 Lucide，建議確保 createIcons() 已跑完再 init
    // 但本函式已做「即時取錨點」，就算之後被替換也能正常運作
    const initCollapsed = localStorage.getItem(LS) === '1';
    applyCollapsed(initCollapsed);

    btn.addEventListener('click', () => {
        const next = !sidebar.classList.contains('collapsed');
        applyCollapsed(next);
        localStorage.setItem(LS, next ? '1' : '0');
    });
}


function bmsLogout() {
    console.log('logout clicked');
}

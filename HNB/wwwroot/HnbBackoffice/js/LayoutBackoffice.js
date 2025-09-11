document.addEventListener('DOMContentLoaded', () => {
    initTheme();
    if (window.lucide) window.lucide.createIcons();
    wireSidebar();
    wireUserMenu();
    initUserTable();
    bindTabButtons();
});

function initTheme() {
    const ls = localStorage.getItem('theme');
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    const useDark = (ls === 'dark') || (ls === null && prefersDark);
    document.documentElement.classList.toggle('dark', useDark);
}

// 側欄摺疊
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

// 使用者選單 Dropdown 
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

function bindTabButtons(defaultTabIndex = 0) {
    document.addEventListener('click', function (e) {
        if (e.target.classList.contains('hn-tab')) {
            activateTabButton(e.target);
        }
    });

    const tabs = document.querySelectorAll('.hn-tab');
    if (tabs.length > 0 && tabs[defaultTabIndex]) {
        activateTabButton(tabs[defaultTabIndex]);
    }
}

// 專門處理樣式切換
function activateTabButton(btn) {
    document.querySelectorAll('.hn-tab').forEach(b => {
        b.classList.remove('bg-slate-900', 'text-white');
        b.classList.add('hover:bg-slate-100');
    });
    btn.classList.remove('hover:bg-slate-100');
    btn.classList.add('bg-slate-900', 'text-white');
}

// DataTables 初始化
function initUserTable() {
    const $info = document.querySelector('.md\\:col-span-9 header .text-sm.text-slate-500:last-child');

    const table = $('#tablediv').DataTable({
        pageLength: 10,
        lengthMenu: [10, 25, 50, 100],
        order: [],
        columnDefs: [
            { targets: -1, orderable: false } // 最後一欄「操作」不排序
        ],
        language: {
            url: 'https://cdn.datatables.net/plug-ins/2.1.8/i18n/zh-HANT.json'
        },
        drawCallback: function () {
            const api = this.api();
            const info = api.page.info();
            const total = info.recordsDisplay ?? info.recordsTotal ?? 0;
            const start = total ? (info.start + 1) : 0;
            const end = info.end;
            if ($info) {
                $info.textContent = `共 ${total} 位用戶 · 顯示 ${start}–${end}`;
            }
            if (window.lucide && lucide.createIcons) {
                lucide.createIcons();
            }
        }
    });

    return table;
}

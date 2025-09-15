document.addEventListener('DOMContentLoaded', () => {
    initTheme();
    if (window.lucide) window.lucide.createIcons();
    bindTabButtons();
    initPreviewFeature();
    initEditFeature();
    wireSidebar();
    wireUserMenu();
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

// 檔案總管：檔案預覽功能
function initPreviewFeature() {
    const modalEl = document.getElementById('pvModal');
    const PV_URL = modalEl?.dataset.previewUrl || '';
    if (!PV_URL) { window.fmPreview = () => { alert('預覽未初始化：缺少 Preview URL'); return false; }; return; }

    function curPath() { return new URLSearchParams(location.search).get('path') || '/'; }
    function esc(s) { return (s || '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c])); }

    window.fmPreview = async function (name) {
        const url = `${PV_URL}?path=${encodeURIComponent(curPath())}&name=${encodeURIComponent(name)}`;
        const ext = (name.split('.').pop() || '').toLowerCase();
        const box = document.getElementById('pvContainer');
        document.getElementById('pvTitle').textContent = name;
        document.getElementById('pvOpenNew').href = url;
        box.innerHTML = '<div class="p-4 text-muted">載入中…</div>';

        try {
            if (['png', 'jpg', 'jpeg', 'gif', 'webp', 'avif', 'svg'].includes(ext)) box.innerHTML = `<img src="${url}" class="img-fluid d-block mx-auto" alt="">`;
            else if (['mp4', 'webm', 'ogg'].includes(ext)) box.innerHTML = `<video controls class="w-100" style="max-height:75vh"><source src="${url}"></video>`;
            else if (['mp3', 'wav', 'm4a', 'oga', 'flac'].includes(ext)) box.innerHTML = `<audio controls class="w-100"><source src="${url}"></audio>`;
            else if (ext === 'pdf') box.innerHTML = `<iframe src="${url}" class="w-100" style="height:75vh;border:0;"></iframe>`;
            else if (['txt', 'log', 'json', 'xml', 'md', 'csv'].includes(ext)) box.innerHTML = `<pre class="m-0 p-3" style="max-height:75vh;overflow:auto;white-space:pre-wrap;">${esc(await (await fetch(url)).text())}</pre>`;
            else if (ext === 'docx') { const buf = await (await fetch(url)).arrayBuffer(); const html = (await window.mammoth.convertToHtml({ arrayBuffer: buf })).value; box.innerHTML = `<div class="p-3">${html}</div>`; }
            else if (['xlsx', 'xls'].includes(ext)) { const buf = await (await fetch(url)).arrayBuffer(); const wb = XLSX.read(buf, { type: 'array' }); const sh = wb.Sheets[wb.SheetNames[0]]; box.innerHTML = `<div class="p-3 table-responsive">${XLSX.utils.sheet_to_html(sh)}</div>`; }
            else box.innerHTML = `<div class="p-4">暫不支援此檔案格式的預覽。</div>`;
        } catch (e) { box.innerHTML = `<div class="p-4 text-danger">預覽失敗：${esc(e?.message || e)}</div>`; }

        new bootstrap.Modal(modalEl).show();
        return false;
    };
}

// 檔案總管：編輯初始化
function initEditFeature() {
    const modalEl = document.getElementById('edModal');
    const READ_URL = modalEl?.dataset.readUrl || '';
    const SAVE_URL = modalEl?.dataset.saveUrl || '';
    function curPath() { return new URLSearchParams(location.search).get('path') || '/'; }
    function token() { const el = document.querySelector('input[name="__RequestVerificationToken"]'); return el ? el.value : ''; }
    function isTextExt(name) {
        const ext = (name.split('.').pop() || '').toLowerCase();
        return ['txt', 'log', 'md', 'json', 'xml', 'csv', 'yml', 'yaml', 'ini', 'cfg', 'conf', 'js', 'ts', 'css', 'html', 'cs', 'cshtml'].includes(ext);
    }

    window.fmEdit = async function (name) {
        if (!isTextExt(name)) { alert('此格式不支援線上編輯'); return false; }
        document.getElementById('edTitle').textContent = name;
        document.getElementById('edHiddenName').value = name;
        document.getElementById('edHiddenPath').value = curPath();
        document.getElementById('edInput').value = '載入中…';

        // 讀取內容（POST）
        const fd = new FormData();
        fd.append('__RequestVerificationToken', token());
        fd.append('path', curPath());
        fd.append('name', name);
        const res = await fetch(READ_URL, { method: 'POST', body: fd, credentials: 'same-origin' });
        if (!res.ok) { alert('讀取失敗'); return false; }
        document.getElementById('edInput').value = await res.text();

        new bootstrap.Modal(modalEl).show();
        return false;
    };

    window.fmEditSave = async function () {
        const name = document.getElementById('edHiddenName').value;
        const path = document.getElementById('edHiddenPath').value;
        const content = document.getElementById('edInput').value;

        const fd = new FormData();
        fd.append('__RequestVerificationToken', token());
        fd.append('path', path); fd.append('name', name); fd.append('content', content);

        const res = await fetch(SAVE_URL, { method: 'POST', body: fd, credentials: 'same-origin' });
        if (!res.ok) { alert('儲存失敗'); return false; }
        const r = await res.json();
        if (!r?.ok) { alert(r?.message || '儲存失敗'); return false; }

        bootstrap.Modal.getInstance(modalEl)?.hide();
        if (typeof loadList === 'function') loadList();
        return false;
    };
}


/** Easing: cubic-out */
function easeOutCubic(t) { return 1 - Math.pow(1 - t, 3); }

/** 數字動畫：可多次呼叫、可客製 */
function initCounters(opts = {}) {
    const selector = opts.selector ?? '[data-count-target]';
    const duration = opts.duration ?? 2000;
    const els = document.querySelectorAll(selector);
    if (!els.length) return;

    els.forEach(el => {
        const raw = (el.getAttribute('data-count-target') || '').trim();
        const m = raw.match(/^(\D*?)(-?\d+(?:\.\d+)?)(.*)$/);
        const prefix = m ? m[1] : '';
        const targetN = m ? parseFloat(m[2]) : 0;
        const suffix = m ? m[3] : '';
        const fromN = parseFloat(el.getAttribute('data-count-from') || 0);
        const decimals = parseInt(el.getAttribute('data-count-decimals') || 0, 10);

        let start = null;
        function tick(ts) {
            if (start === null) start = ts;
            const p = Math.min(1, (ts - start) / duration);
            const v = fromN + (targetN - fromN) * easeOutCubic(p);
            el.textContent = `${prefix}${v.toFixed(decimals)}${suffix}`;
            if (p < 1) requestAnimationFrame(tick);
            else el.textContent = `${prefix}${targetN.toFixed(decimals)}${suffix}`;
        }
        requestAnimationFrame(tick);
    });
}

/** 分頁切換：不硬編面板，掃描所有 id="tab-*" 的 pane */
function initTabs(opts = {}) {
    const btnSelector = opts.btnSelector ?? '.tab-btn';
    const paneSelector = opts.paneSelector ?? '.tab-pane';
    const activeClass = opts.activeClass ?? ['bg-white', 'text-blue-600', 'border-blue-600'];
    const defaultTab = opts.defaultTab ?? null;

    const btns = Array.from(document.querySelectorAll(btnSelector));
    const panes = Array.from(document.querySelectorAll(paneSelector));
    if (!btns.length || !panes.length) return;

    const paneMap = Object.fromEntries(
        panes
            .filter(p => p.id && p.id.startsWith('tab-'))
            .map(p => [p.id.slice(4), p])
    );

    function setActive(name) {
        if (!name) return;
        btns.forEach(b => {
            const isActive = b.getAttribute('data-tab') === name;
            b.classList.toggle(activeClass[0], isActive);
            b.classList.toggle(activeClass[1], isActive);
            b.classList.toggle(activeClass[2], isActive);
        });
        Object.entries(paneMap).forEach(([key, el]) => {
            el.classList.toggle('hidden', key !== name);
        });
        window.lucide?.createIcons?.();
    }

    btns.forEach(b => b.addEventListener('click', () => {
        const name = b.getAttribute('data-tab');
        if (name) setActive(name);
    }));

    const hashMatch = location.hash.match(/^#tab-(.+)$/);
    const byHash = hashMatch?.[1];
    const byAttr = btns.find(b => b.getAttribute('data-tab-default') === 'true')?.getAttribute('data-tab');
    const byParam = defaultTab;
    const byFirst = btns[0]?.getAttribute('data-tab');
    setActive(byHash || byAttr || byParam || byFirst);
}

/** lucide：單獨抽出，必要時可手動重繪 */
function initLucide() {
    window.lucide?.createIcons?.();
}

document.addEventListener('DOMContentLoaded', () => {
    initTabs();
    initCounters();
    initLucide();
});
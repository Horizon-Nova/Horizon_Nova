/**
 * HNAdmin — Layout.js
 * Sidebar, header, dropdown, toast
 */

'use strict';

const HNA_SIDEBAR_KEY = 'hna_sidebar_collapsed';

// ── Sidebar ───────────────────────────────────────────────
function hna_getSidebar() { return document.getElementById('hna-sidebar'); }

function hna_saveSidebarState(collapsed) {
    try { sessionStorage.setItem(HNA_SIDEBAR_KEY, collapsed ? '1' : '0'); } catch {}
}

function hna_loadSidebarState() {
    try {
        const v = sessionStorage.getItem(HNA_SIDEBAR_KEY);
        return v === null ? false : v === '1';   // 預設展開
    } catch { return false; }
}

function hna_applySidebarState(collapsed) {
    const sidebar = hna_getSidebar();
    if (!sidebar) return;
    sidebar.setAttribute('data-collapsed', collapsed ? 'true' : 'false');
}

function hna_ToggleSidebar() {
    const sidebar = hna_getSidebar();
    if (!sidebar) return;
    const now = sidebar.getAttribute('data-collapsed') === 'true';
    hna_applySidebarState(!now);
    hna_saveSidebarState(!now);
}

function hna_OpenMobileSidebar() {
    const sidebar  = hna_getSidebar();
    const overlay  = document.getElementById('hna-overlay');
    if (sidebar) sidebar.classList.add('mobile-open');
    if (overlay) overlay.classList.add('open');
    document.body.style.overflow = 'hidden';
}

function hna_CloseMobileSidebar() {
    const sidebar = hna_getSidebar();
    const overlay = document.getElementById('hna-overlay');
    if (sidebar) sidebar.classList.remove('mobile-open');
    if (overlay) overlay.classList.remove('open');
    document.body.style.overflow = '';
}

// ── Active nav ────────────────────────────────────────────
function hna_setActiveNav() {
    const path = window.location.pathname.toLowerCase();
    let bestEl = null, bestLen = 0;
    document.querySelectorAll('.hna-nav-item[href]').forEach(el => {
        const href = (el.getAttribute('href') || '').toLowerCase();
        if (!href || href === '#') return;
        if (path === href || (path.startsWith(href) && href.length > 1)) {
            if (href.length > bestLen) { bestEl = el; bestLen = href.length; }
        }
    });
    if (bestEl) bestEl.classList.add('active');
}

// ── User dropdown ─────────────────────────────────────────
function hna_ToggleUserMenu(force) {
    const menu = document.getElementById('hna-user-menu');
    const btn  = document.getElementById('hna-user-btn');
    if (!menu) return;
    const open = typeof force === 'boolean' ? force : !menu.classList.contains('open');
    menu.classList.toggle('open', open);
    if (btn) btn.setAttribute('aria-expanded', open ? 'true' : 'false');
}

// ── Toast ─────────────────────────────────────────────────
const HNA_TOAST_ICONS = { success: 'check-circle', error: 'x-circle', info: 'info' };

function showToast(message, type = 'info', duration = 3500) {
    const container = document.getElementById('hna-toasts');
    if (!container) return;
    const t = HNA_TOAST_ICONS[type] ? type : 'info';
    const id = `hnat-${Date.now()}`;
    const el = document.createElement('div');
    el.id = id;
    el.className = `hna-toast ${t}`;
    el.innerHTML = `
        <span class="hna-toast-icon"><i data-lucide="${HNA_TOAST_ICONS[t]}" style="width:16px;height:16px;"></i></span>
        <span class="hna-toast-msg">${message}</span>
        <button class="hna-toast-close" onclick="hna_closeToast('${id}')">
            <i data-lucide="x" style="width:14px;height:14px;"></i>
        </button>`;
    container.prepend(el);
    if (typeof lucide !== 'undefined') lucide.createIcons();
    if (duration > 0) setTimeout(() => hna_closeToast(id), duration);
}

function hna_closeToast(id) {
    const el = document.getElementById(id);
    if (!el) return;
    el.style.opacity = '0';
    el.style.transform = 'translateX(12px)';
    el.style.transition = 'all 0.2s ease';
    setTimeout(() => el.remove(), 200);
}

window.showToast    = showToast;
window.closeToast   = hna_closeToast;

// ── Init ──────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', function () {
    // Apply saved sidebar state
    const collapsed = hna_loadSidebarState();
    hna_applySidebarState(collapsed);

    // Active nav
    hna_setActiveNav();

    // Close dropdown on outside click
    document.addEventListener('click', function (e) {
        const menu = document.getElementById('hna-user-menu');
        const btn  = document.getElementById('hna-user-btn');
        if (!menu || !menu.classList.contains('open')) return;
        if (!menu.contains(e.target) && !btn?.contains(e.target)) {
            hna_ToggleUserMenu(false);
        }
    });

    // ESC key
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            hna_CloseMobileSidebar();
            hna_ToggleUserMenu(false);
        }
    });

    // Lucide
    if (typeof lucide !== 'undefined') lucide.createIcons();
});

window.hna_ToggleSidebar      = hna_ToggleSidebar;
window.hna_OpenMobileSidebar  = hna_OpenMobileSidebar;
window.hna_CloseMobileSidebar = hna_CloseMobileSidebar;
window.hna_ToggleUserMenu     = hna_ToggleUserMenu;

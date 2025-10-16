'use strict';

// Global DataTable helper for Horizon Nova Backoffice
// Usage: HNBDataTable.init('#tableId', { /* optional overrides */ })
window.HNBDataTable = (function () {
    function init(selector, options) {
        if (typeof $ === 'undefined' || !$.fn.DataTable) {
            console.warn('[HNBDataTable] jQuery DataTables not found. Ensure CDN is loaded in _Layout.');
            return null;
        }

        var defaults = {
            stateSave: true,
            order: [[4, 'desc']],
            pageLength: 25,
            lengthMenu: [10, 25, 50, 100],
            language: { url: 'https://cdn.datatables.net/plug-ins/1.13.7/i18n/zh-HANT.json' },
            columnDefs: [
                { targets: 0, className: 'whitespace-nowrap' },
                { targets: 1, className: 'text-slate-700' },
                { targets: 2, className: 'text-slate-700' },
                { targets: 3, className: 'text-slate-700' },
                { targets: 4, className: 'text-slate-700' },
                { targets: 5, className: 'text-right' }
            ],
            drawCallback: function () {
                if (typeof lucide !== 'undefined' && lucide.createIcons) {
                    lucide.createIcons();
                }
            }
        };

        var cfg = $.extend(true, {}, defaults, options || {});
        return $(selector).DataTable(cfg);
    }

    return { init: init };
})();



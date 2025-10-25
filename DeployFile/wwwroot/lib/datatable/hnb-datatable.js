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
            order: [[0, 'asc']],
            pageLength: 25,
            lengthMenu: [10, 25, 50, 100, 200],
            dom: '<"d-flex align-items-center justify-content-between mb-3"l<"ms-auto"f>>rt<"d-flex align-items-center justify-content-between mt-3"i<"ms-auto"p>>',
            language: {
                url: 'https://cdn.datatables.net/plug-ins/1.13.7/i18n/zh-HANT.json',
                search: '搜尋:',
                lengthMenu: '顯示 _MENU_ 筆',
                info: '顯示第 _START_ 至 _END_ 筆，共 _TOTAL_ 筆',
                infoEmpty: '沒有資料',
                infoFiltered: '(從 _MAX_ 筆中篩選)',
                paginate: {
                    first: '首頁',
                    last: '末頁',
                    next: '下一頁',
                    previous: '上一頁'
                }
            },
            drawCallback: function () {
                // 安全調用 Lucide icons（使用可選鏈）
                try {
                    if (typeof lucide !== 'undefined' && typeof lucide.createIcons === 'function') {
                        lucide.createIcons();
                    }
                } catch (e) {
                    // 忽略 Lucide 錯誤，不影響 DataTable 功能
                }
            }
        };

        var cfg = $.extend(true, {}, defaults, options || {});
        return $(selector).DataTable(cfg);
    }

    return { init: init };
})();



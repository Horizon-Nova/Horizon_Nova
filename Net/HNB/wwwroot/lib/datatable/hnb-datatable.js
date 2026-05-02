'use strict';

// Global DataTable helper for Horizon Nova Backoffice
// Usage: HNBDataTable.init('#tableId', { /* optional overrides */ })
window.HNBDataTable = (function () {
    function init(target, options) {
        if (typeof $ === 'undefined' || !$.fn.DataTable) {
            console.warn('[HNBDataTable] jQuery DataTables not found. Ensure CDN is loaded in _Layout.');
            return null;
        }

        var $table = $(target);
        if (!$table.length || $.fn.DataTable.isDataTable($table[0])) {
            return $table.length ? $table.DataTable() : null;
        }

        var defaults = {
            stateSave: true,
            pageLength: 25,
            dom:
                "<'row align-items-center g-2'<'col ms-auto'f>>" +
                "<'row'<'col-12'tr>>" +
                "<'row align-items-center g-2'<'col ms-auto'p>>",
            drawCallback: function () {
                try {
                    if (typeof lucide !== 'undefined' && typeof lucide.createIcons === 'function') {
                        lucide.createIcons();
                    }
                } catch (e) {
                    /* ignore */
                }
            }
        };

        var cfg = $.extend(true, {}, defaults, options || {});
        return $table.DataTable(cfg);
    }

    function autoInit(context) {
        var $root = context ? $(context) : $(document);
        $root.find('table[data-hnb-datatable], table.js-hnb-datatable').each(function () {
            var $table = $(this);
            if ($.fn.DataTable.isDataTable(this)) {
                return;
            }

            var inlineOptions = {};
            var rawOptions = $table.attr('data-hnb-options');
            if (rawOptions) {
                try {
                    inlineOptions = JSON.parse(rawOptions);
                } catch (err) {
                    console.warn('[HNBDataTable] Failed to parse data-hnb-options JSON:', err);
                }
            }

            init($table, inlineOptions);
        });
    }

    $(document).ready(function () {
        autoInit();
    });

    return { init: init, autoInit: autoInit };
})();



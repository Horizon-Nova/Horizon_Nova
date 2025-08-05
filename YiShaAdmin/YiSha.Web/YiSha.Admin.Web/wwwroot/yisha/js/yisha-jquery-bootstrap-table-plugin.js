; (function ($) {
    "use strict";
    $.fn.ysTable = function (option, param) {
        //如果是調用方法
        if (typeof option == 'string') {
            return $.fn.ysTable.methods[option](this, param);
        }

        //如果是初始化組件
        var _option = $.extend({}, $.fn.ysTable.defaults, option || {});
        var target = $(this);
        target.bootstrapTable(_option);
        return target;
    };

    $.fn.ysTable.methods = {
        search: function (target) {
            // 從第一頁開始
            target.bootstrapTable('refresh', { pageNumber: 1 });
        },
        getPagination: function (target, params) {
            var pagination = {
                pageSize: params.limit,                         //頁面大小
                pageIndex: (params.offset / params.limit) + 1,   //頁碼
                sort: params.sort,      //排序列名
                sortType: params.order //排位命令（desc，asc）
            };
            return pagination;
        }
    };

    $.fn.ysTable.defaults = {
        method: 'GET',                      // 請求方式（*）
        toolbar: '#toolbar',                // 工具按鈕用哪個容器
        striped: true,                      // 是否顯示行間隔色
        cache: false,                       // 是否使用緩存，默認為true，所以一般情况下需要設置一下這個属性（*）
        pagination: true,                   // 是否顯示分頁（*）
        sortable: true,                     // 是否啟用排序
        sortStable: true,                   // 設置為 true 將獲得稳定的排序
        sortName: 'Id',                     // 排序列名稱
        sortOrder: "desc",                  // 排序方式
        sidePagination: "server",           // 分頁方式：client客户端分頁，server服務端分頁（*）
        pageNumber: 1,                      // 初始化加載第一頁，默認第一頁,並記錄
        pageSize: 10,                       // 每頁的記錄行數（*）
        pageList: "10, 25, 50, 100",        // 可供選擇的每頁的行數（*）
        search: false,                      // 是否顯示表格查詢
        strictSearch: true,
        showColumns: true,                  // 是否顯示所有的列（選擇顯示的列）
        showRefresh: true,                  // 是否顯示刷新按鈕
        showToggle: true,                   // 是否顯示詳细視圖和列表視圖的切換按鈕
        minimumCountColumns: 2,             // 最少允許的列數
        clickToSelect: true,                // 是否啟用點击選中行
        height: undefined,                  // 行高，如果沒有設置height属性，表格自動根据記錄條數覺得表格高度
        uniqueId: "Id",                     // 每一行的唯一標識，一般為主鍵列
        cardView: false,                    // 是否顯示詳细視圖
        detailView: false,                  // 是否顯示父子表
        totalField: 'Total',
        dataField: 'Data',
        columns: [],
        queryParams: {},
        onLoadSuccess: function (obj) {
            if (obj) {
                if (obj.Tag != 1) {
                    ys.alertError(obj.Message);
                }
            }
        },
        onLoadError: function (status, s) {
            if (s.statusText != "abort") {
                ys.alertError("資料加載失敗！");
            }
        }
    };
})(window.jQuery);
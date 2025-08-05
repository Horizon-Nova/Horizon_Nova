/**
 * bootstrapTreeTable
 *
 * @author swifly YiShaAdmin
 */
(function ($) {
    "use strict";

    $.fn.bootstrapTreeTable = function (options, param) {
        var target = $(this).data('bootstrap.tree.table');
        target = target ? target : $(this);
        // 如果是調用方法
        if (typeof options == 'string') {
            return $.fn.bootstrapTreeTable.methods[options](target, param);
        }
        // 如果是初始化組件
        options = $.extend({}, $.fn.bootstrapTreeTable.defaults, options || {});
        target.hasSelectItem = false;// 是否有radio或checkbox
        target.data_list = null; //用於緩存格式化後的資料-按父分組
        target.data_obj = null; //用於緩存格式化後的資料-按id存對象
        target.hiddenColumns = []; //用於存放被隱藏列的field
        target.lastAjaxParams; //使用者最後一次請求的參數
        target.isFixWidth = false; //是否有固定寬度
        // 初始化
        var init = function () {
            // 初始化容器
            initContainer();
            // 初始化工具欄
            initToolbar();
            // 初始化表頭
            initHeader();
            // 初始化表體
            initBody();
            // 初始化資料服務
            initServer();
            // 動態設置表頭寬度
            autoTheadWidth(true);
            // 緩存target對象
            target.data('bootstrap.tree.table', target);
        }
        // 初始化容器
        var initContainer = function () {
            // 在外層包装一下div，樣式用的bootstrap-table的
            var $main_div = $("<div class='bootstrap-tree-table'></div>");
            var $treetable = $("<div class='treetable-table'></div>");
            target.before($main_div);
            $main_div.append($treetable);
            $treetable.append(target);
            target.addClass("table");
            if (options.striped) {
                target.addClass('table-striped');
            }
            if (options.bordered) {
                target.addClass('table-bordered');
            }
            if (options.hover) {
                target.addClass('table-hover');
            }
            if (options.condensed) {
                target.addClass('table-condensed');
            }
            target.html("");
        }
        // 初始化工具欄
        var initToolbar = function () {
            var $toolbar = $("<div class='treetable-bars'></div>");
            if (options.toolbar) {
                $(options.toolbar).addClass('tool-left');
                $toolbar.append($(options.toolbar));
            }
            var $rightToolbar = $('<div class="btn-group tool-right">');
            $toolbar.append($rightToolbar);
            target.parent().before($toolbar);
            // 是否顯示刷新按鈕
            if (options.showRefresh) {
                var $refreshBtn = $('<button class="btn btn-default btn-outline" type="button" aria-label="refresh" title="刷新"><i class="glyphicon glyphicon-repeat"></i></button>');
                $rightToolbar.append($refreshBtn);
                registerRefreshBtnClickEvent($refreshBtn);
            }
            // 是否顯示列選項
            if (options.showColumns) {
                var $columns_div = $('<div class="btn-group pull-right" title="列"><button type="button" aria-label="columns" class="btn btn-default btn-outline dropdown-toggle" data-toggle="dropdown" aria-expanded="false"><i class="glyphicon glyphicon-list"></i> <span class="caret"></span></button></div>');
                var $columns_ul = $('<ul class="dropdown-menu columns" role="menu"></ul>');
                $.each(options.columns, function (i, column) {
                    if (column.field != 'selectItem') {
                        var _li = null;
                        if (typeof column.visible == "undefined" || column.visible == true) {
                            _li = $('<li role="menuitem"><label><input type="checkbox" checked="checked" data-field="' + column.field + '" value="' + column.field + '" > ' + column.title + '</label></li>');
                        } else {
                            _li = $('<li role="menuitem"><label><input type="checkbox" data-field="' + column.field + '" value="' + column.field + '" > ' + column.title + '</label></li>');
                            target.hiddenColumns.push(column.field);
                        }
                        $columns_ul.append(_li);
                    }
                });
                $columns_div.append($columns_ul);
                $rightToolbar.append($columns_div);
                // 注册列選項事件
                registerColumnClickEvent();
            } else {
                $.each(options.columns, function (i, column) {
                    if (column.field != 'selectItem') {
                        if (!(typeof column.visible == "undefined" || column.visible == true)) {
                            target.hiddenColumns.push(column.field);
                        }
                    }
                });
            }
        }
        // 初始化隱藏列
        var initHiddenColumns = function () {
            $.each(target.hiddenColumns, function (i, field) {
                target.find("." + field + "_cls").hide();
            });
        }
        // 初始化表頭
        var initHeader = function () {
            var $thr = $('<tr></tr>');
            $.each(options.columns, function (i, column) {
                var $th = null;
                // 判斷有沒有選擇列
                if (i == 0 && column.field == 'selectItem') {
                    target.hasSelectItem = true;
                    $th = $('<th style="width:36px;"></th>');
                } else {
                    $th = $('<th style="' + ((column.width) ? ('width:' + column.width) : '') + '" class="' + column.field + '_cls"></th>');
                }
                if ((!target.isFixWidth) && column.width) {
                    target.isFixWidth = column.width.indexOf("px") > -1 ? true : false;
                }
                $th.text(column.title);
                $thr.append($th);
            });
            var $thead = $('<thead class="treetable-thead"></thead>');
            $thead.append($thr);
            target.append($thead);
        }
        // 初始化表體
        var initBody = function () {
            var $tbody = $('<tbody class="treetable-tbody"></tbody>');
            target.append($tbody);
            // 默認高度
            if (options.height) {
                $tbody.css("height", options.height);
            }
        }
        // 初始化資料服務
        var initServer = function (parms) {
            // 加載資料前先清空
            target.data_list = {};
            target.data_obj = {};
            var $tbody = target.find("tbody");
            // 添加加載loading
            var $loading = '<tr><td colspan="' + options.columns.length + '"><div style="display: block;text-align: center;">正在努力地加載資料中，請稍候……</div></td></tr>'
            $tbody.html($loading);
            if (options.url) {
                $.ajax({
                    type: options.method,
                    url: options.url,
                    data: parms ? parms : options.ajaxParams,
                    dataType: "JSON",
                    success: function (data, textStatus, jqXHR) {
                        var dataJson = data;
                        if (data.Data) {
                            dataJson = data.Data;
                        }
                        renderTable(dataJson);
                    },
                    error: function (xhr, textStatus) {
                        var _errorMsg = '<tr><td colspan="' + options.columns.length + '"><div style="display: block;text-align: center;">' + xhr.responseText + '</div></td></tr>'
                        $tbody.html(_errorMsg);
                    },
                });
            } else {
                renderTable(options.data);
            }
        }
        // 加載完資料後渲染表格
        var renderTable = function (data) {
            var $tbody = target.find("tbody");
            // 先清空
            $tbody.html("");
            if (!data || data.length <= 0) {
                var _empty = '<tr><td colspan="' + options.columns.length + '"><div style="display: block;text-align: center;">沒有找到匹配的記錄</div></td></tr>'
                $tbody.html(_empty);
                return;
            }
            // 緩存並格式化資料
            formatData(data);
            // 獲取所有根節點
            var rootNode = target.data_list["_root_"];
            // 開始绘制
            if (rootNode) {
                $.each(rootNode, function (i, item) {
                    var _child_row_id = "row_id_" + i;
                    recursionNode(item, 1, _child_row_id, "row_root");
                });
            }
            // 下邊的操作主要是為了查询時讓一些沒有根節點的節點顯示
            $.each(data, function (i, item) {
                var _defaultRootFlag = target.getRootFlag(item);
                if (_defaultRootFlag) {
                    if (!item.isShow) {
                        var tr = renderRow(item, false, 1, "", "");
                        $tbody.append(tr);
                    }
                }
            });
            target.append($tbody);
            registerExpanderEvent();
            registerRowClickEvent();
            initHiddenColumns();
            if ($.isFunction(options.onLoadSuccess)) {
                options.onLoadSuccess();
            }
            // 動態設置表頭寬度
            autoTheadWidth();
        }
        // 動態設置表頭寬度
        var autoTheadWidth = function (initFlag) {
            if (options.height > 0) {
                var $thead = target.find("thead");
                var $tbody = target.find("tbody");
                var borderWidth = parseInt(target.css("border-left-width")) + parseInt(target.css("border-right-width"))

                $thead.css("width", $tbody.children(":first").width());
                if (initFlag) {
                    var resizeWaiter = false;
                    $(window).resize(function () {
                        if (!resizeWaiter) {
                            resizeWaiter = true;
                            setTimeout(function () {
                                if (!target.isFixWidth) {
                                    $tbody.css("width", target.parent().width() - borderWidth);
                                }
                                $thead.css("width", $tbody.children(":first").width());
                                resizeWaiter = false;
                            }, 300);
                        }
                    });
                }
            }

        }
        // 緩存並格式化資料
        var formatData = function (data) {
            if (!target.data_list["_root_"]) {
                target.data_list["_root_"] = [];
            }
            var tempRoot = [];  // 根目錄根据資料的原始順序排序
            var _root = options.rootIdValue ? options.rootIdValue : null
            $.each(data, function (index, item) {
                // 添加一個默認属性，用來判斷當前節點有沒有被顯示
                item.isShow = false;
                // 這里兼容几種常见Root節點寫法
                // 默認的几種判斷
                var _defaultRootFlag = target.getRootFlag(item);
                if (!item[options.parentCode] || (_root ? (item[options.parentCode] == options.rootIdValue) : _defaultRootFlag)) {
                    if (!target.data_obj["id_" + item[options.code]]) {
                        tempRoot.push({ 'index': index, 'node': item })
                    }
                } else {
                    var rootNode = recursionQueryRootNode(data, 0, item);
                    if (rootNode && rootNode.node) {
                        if (!target.data_obj["id_" + rootNode.node[options.code]]) {
                            target.data_obj["id_" + rootNode.node[options.code]] = rootNode.node

                            tempRoot.push(rootNode)
                        }
                    }
                    if (!target.data_list["_n_" + item[options.parentCode]]) {
                        target.data_list["_n_" + item[options.parentCode]] = [];
                    }
                    if (!target.data_obj["id_" + item[options.code]]) {
                        target.data_list["_n_" + item[options.parentCode]].push(item);
                    }
                }
                if (!target.data_obj["id_" + item[options.code]]) {
                    target.data_obj["id_" + item[options.code]] = item;
                }
            });
            tempRoot = tempRoot.sort(function (a, b) {
                return a.index - b.index
            });
            $.each(tempRoot, function (index, item) {
                target.data_list["_root_"].push(item.node);
            })
        }
        // 递归獲取節點的根節點
        var recursionQueryRootNode = function (data, index, node) {
            for (let i = 0; i < data.length; i++) {
                if (data[i][options.code] == node[options.parentCode]) {
                    return recursionQueryRootNode(data, i, data[i]);
                }
                if (i == data.length - 1) {
                    return { 'index': index, 'node': node };
                }
            }
        }
        // 递归獲取子節點並且設置子節點
        var recursionNode = function (parentNode, lv, row_id, p_id) {
            var $tbody = target.find("tbody");
            var _ls = target.data_list["_n_" + parentNode[options.code]];
            var $tr = renderRow(parentNode, _ls ? true : false, lv, row_id, p_id);
            $tbody.append($tr);
            if (_ls) {
                if (options.expandAll) {
                    $.each(_ls, function (i, item) {
                        var _child_row_id = row_id + "_" + i;
                        recursionNode(item, (lv + 1), _child_row_id, row_id);
                    });
                }
                else if (options.expandFirst) {
                    if (lv == 1) {
                        $.each(_ls, function (i, item) {
                            var _child_row_id = row_id + "_" + i;
                            recursionNode(item, (lv + 1), _child_row_id, row_id);
                        });
                    }
                }
            }
        };
        // 绘制行
        var renderRow = function (item, isP, lv, row_id, p_id, isShow) {
            // 標記已顯示
            item.isShow = true;
            item.row_id = row_id;
            item.p_id = p_id;
            item.lv = lv;
            var $tr = $('<tr id="' + row_id + '" pid="' + p_id + '"></tr>');
            var _icon = options.expanderCollapsedClass;
            if (options.expandAll) {
                $tr.css("display", "table");
                _icon = options.expanderExpandedClass;
            } else if (isShow) {
                $tr.css("display", "table");
                _icon = options.expanderCollapsedClass;
            }
            else if (lv == 1) {
                $tr.css("display", "table");
                _icon = (options.expandFirst) ? options.expanderExpandedClass : options.expanderCollapsedClass;
            } else if (lv == 2) {
                if (options.expandFirst) {
                    $tr.css("display", "table");
                } else {
                    $tr.css("display", "none");
                }
                _icon = options.expanderCollapsedClass;
            } else {
                $tr.css("display", "none");
                _icon = options.expanderCollapsedClass;
            }
            $.each(options.columns, function (index, column) {
                // 判斷有沒有選擇列
                if (column.field == 'selectItem') {
                    target.hasSelectItem = true;
                    var $td = $('<td style="text-align:center;width:36px;"></td>');
                    if (column.radio) {
                        var _ipt = $('<input name="select_item" type="radio" value="' + item[options.code] + '"></input>');
                        $td.append(_ipt);
                    }
                    if (column.checkbox) {
                        var _ipt = $('<input name="select_item" type="checkbox" value="' + item[options.code] + '"></input>');
                        $td.append(_ipt);
                    }
                    $tr.append($td);
                } else {
                    var $td = $('<td name="' + column.field + '" class="' + column.field + '_cls"></td>');
                    if (column.width) {
                        $td.css("width", column.width);
                    }
                    if (column.align) {
                        $td.css("text-align", column.align);
                    }
                    if (options.expandColumn == index) {
                        $td.css("text-align", "left");
                    }
                    if (column.valign) {
                        $td.css("vertical-align", column.valign);
                    }
                    if (options.showTitle) {
                        $td.addClass("ellipsis");
                    }
                    // 增加formatter渲染
                    if (column.formatter) {
                        $td.html(column.formatter.call(this, item[column.field], item, index));
                    } else {
                        if (options.showTitle) {
                            // 只在字段沒有formatter時才添加title属性
                            $td.attr("title", item[column.field]);
                        }
                        $td.text(item[column.field]);
                    }
                    if (options.expandColumn == index) {
                        if (!isP) {
                            $td.prepend('<span class="treetable-expander"></span>')
                        } else {
                            $td.prepend('<span class="treetable-expander ' + _icon + '"></span>')
                        }
                        for (var int = 0; int < (lv - 1); int++) {
                            $td.prepend('<span class="treetable-indent"></span>')
                        }
                    }
                    $tr.append($td);
                }
            });
            return $tr;
        }
        // 注册刷新按鈕點击事件
        var registerRefreshBtnClickEvent = function (btn) {
            $(btn).off('click').on('click', function () {
                target.refresh();
            });
        }
        // 注册列選項事件
        var registerColumnClickEvent = function () {
            $(".bootstrap-tree-table .treetable-bars .columns label input").off('click').on('click', function () {
                var $this = $(this);
                if ($this.prop('checked')) {
                    target.showColumn($(this).val());
                } else {
                    target.hideColumn($(this).val());
                }
            });
        }
        // 注册行點击選中事件
        var registerRowClickEvent = function () {
            target.find("tbody").find("tr").unbind();
            target.find("tbody").find("tr").click(function () {
                target.rowClickHandler(this);
            });
        }
        // 注册小圖標點击事件--展開縮起
        var registerExpanderEvent = function () {
            target.find("tbody").find("tr").find(".treetable-expander").unbind();
            target.find("tbody").find("tr").find(".treetable-expander").click(function () {
                target.rowExpandHandler(this);
            });
        }
        target.getRootFlag = function (item) {
            var _defaultRootFlag = item[options.parentCode] == '0' ||
                item[options.parentCode] == 0 ||
                item[options.parentCode] == null ||
                item[options.parentCode] == '';
            return _defaultRootFlag;
        }
        // 行點击選中事件
        target.rowClickHandler = function (tr) {
            if (target.hasSelectItem) {
                var _ipt = $(tr).find("input[name='select_item']");
                if (_ipt.attr("type") == "radio") {
                    _ipt.prop('checked', true);
                    target.find("tbody").find("tr").removeClass("treetable-selected");
                    $(tr).addClass("treetable-selected");
                } else {
                    if (_ipt.prop('checked')) {
                        _ipt.prop('checked', false);
                        $(tr).removeClass("treetable-selected");
                    } else {
                        _ipt.prop('checked', true);
                        $(tr).addClass("treetable-selected");
                    }
                }
            }
        }
        // 小圖標點击事件
        target.rowExpandHandler = function (span) {
            var _isExpanded = $(span).hasClass(options.expanderExpandedClass);
            var _isCollapsed = $(span).hasClass(options.expanderCollapsedClass);
            if (_isExpanded || _isCollapsed) {
                var tr = $(span).parent().parent();
                var row_id = tr.attr("id");
                var _ls = target.find("tbody").find("tr[id^='" + row_id + "_']"); //下所有
                if (_isExpanded) {
                    $(span).removeClass(options.expanderExpandedClass);
                    $(span).addClass(options.expanderCollapsedClass);
                    if (_ls && _ls.length > 0) {
                        $.each(_ls, function (index, item) {
                            $(item).css("display", "none");
                        });
                    }
                } else {
                    $(span).removeClass(options.expanderCollapsedClass);
                    $(span).addClass(options.expanderExpandedClass);
                    if (_ls && _ls.length == 0) {
                        var _key = tr.find("input[type='radio']").val();
                        var lv = row_id.replace("row_id_", "").split('_').length;
                        var _children = target.data_list["_n_" + _key];
                        var lastRow = null;
                        $.each(_children, function (i, item) {
                            var _child_row_id = row_id + "_" + i;
                            var hasChildren = target.data_list["_n_" + item[options.code]] ? true : false;
                            var $tr = renderRow(item, hasChildren, lv + 1, _child_row_id, row_id, true);
                            if (lastRow == null) {
                                tr.after($tr);
                            }
                            else {
                                if (i > 0) {
                                    lastRow = $("#" + row_id + "_" + (i - 1).toString());
                                }
                                lastRow.after($tr);
                            }
                            var childRow = $("#" + _child_row_id);
                            childRow.click(function () {
                                target.rowClickHandler(this);
                            });
                            childRow.find(".treetable-expander").click(function () {
                                target.rowExpandHandler(this);
                            });
                            if (lastRow == null) {
                                lastRow = childRow;
                            }
                        });
                    } else {
                        if (_ls && _ls.length > 0) {
                            $.each(_ls, function (index, item) {
                                // 父icon
                                var _p_icon = $("#" + $(item).attr("pid")).children().eq(options.expandColumn).find(".treetable-expander");
                                if (_p_icon.hasClass(options.expanderExpandedClass)) {
                                    $(item).css("display", "table");
                                }
                            });
                        }
                    }
                }
            }
        }
        // 刷新資料
        target.refresh = function (parms) {
            if (parms) {
                target.lastAjaxParams = parms;
            }
            initServer(target.lastAjaxParams);
        }
        // 添加資料刷新表格
        target.appendData = function (data) {
            // 下邊的操作主要是為了查询時讓一些沒有根節點的節點顯示
            $.each(data, function (i, item) {
                var _data = target.data_obj["id_" + item[options.code]];
                var _p_data = target.data_obj["id_" + item[options.parentCode]];
                var _c_list = target.data_list["_n_" + item[options.parentCode]];
                var row_id = ""; //行id
                var p_id = ""; //父行id
                var _lv = 1; //如果沒有父就是1默認顯示
                var tr; //要添加行的對象
                if (_data && _data.row_id && _data.row_id != "") {
                    row_id = _data.row_id; // 如果已经存在了，就直接引用原來的
                }
                if (_p_data) {
                    p_id = _p_data.row_id;
                    if (row_id == "") {
                        var _tmp = 0
                        if (_c_list && _c_list.length > 0) {
                            _tmp = _c_list.length;
                        }
                        row_id = _p_data.row_id + "_" + _tmp;
                    }
                    _lv = _p_data.lv + 1; //如果有父
                    // 绘制行
                    tr = renderRow(item, false, _lv, row_id, p_id);

                    var _p_icon = $("#" + _p_data.row_id).children().eq(options.expandColumn).find(".treetable-expander");
                    var _isExpanded = _p_icon.hasClass(options.expanderExpandedClass);
                    var _isCollapsed = _p_icon.hasClass(options.expanderCollapsedClass);
                    // 父節點有沒有展開收縮按鈕
                    if (_isExpanded || _isCollapsed) {
                        // 父節點展開狀態顯示新加行
                        if (_isExpanded) {
                            tr.css("display", "table");
                        }
                    } else {
                        // 父節點沒有展開收縮按鈕则添加
                        _p_icon.addClass(options.expanderCollapsedClass);
                    }

                    if (_data) {
                        $("#" + _data.row_id).before(tr);
                        $("#" + _data.row_id).remove();
                    } else {
                        // 計算父的同級下一行
                        var _tmp_ls = _p_data.row_id.split("_");
                        var _p_next = _p_data.row_id.substring(0, _p_data.row_id.length - 1) + (parseInt(_tmp_ls[_tmp_ls.length - 1]) + 1);
                        // 画上
                        $("#" + _p_next).before(tr);
                    }
                } else {
                    tr = renderRow(item, false, _lv, row_id, p_id);
                    if (_data) {
                        $("#" + _data.row_id).before(tr);
                        $("#" + _data.row_id).remove();
                    } else {
                        // 画上
                        var tbody = target.find("tbody");
                        tbody.append(tr);
                    }
                }
                item.isShow = true;
                // 緩存並格式化資料
                formatData([item]);
            });
            registerExpanderEvent();
            registerRowClickEvent();
            initHiddenColumns();
        }
        // 展開/折叠指定的行
        target.toggleRow = function (id) {
            var _rowData = target.data_obj["id_" + id];
            var $row_expander = $("#" + _rowData.row_id).find(".treetable-expander");
            $row_expander.trigger("click");
        }
        // 展開指定的行
        target.expandRow = function (id) {
            var destArr = [];
            ys.recursion(target.data_obj, id, destArr, options.code, options.parentCode);
            // 一層一層展開
            for (var i = destArr.length - 1; i >= 0; i--) {
                if (destArr[i].row_id) {
                    var $row_expander = $("#" + destArr[i].row_id).find(".treetable-expander");
                    var _isCollapsed = $row_expander.hasClass(options.expanderCollapsedClass);
                    if (_isCollapsed) {
                        $row_expander.trigger("click");
                    }
                }
            }
        }
        // 折叠 指定的行
        target.collapseRow = function (id) {
            var _rowData = target.data_obj["id_" + id];
            var $row_expander = $("#" + _rowData.row_id).find(".treetable-expander");
            var _isExpanded = $row_expander.hasClass(options.expanderExpandedClass);
            if (_isExpanded) {
                $row_expander.trigger("click");
            }
        }
        // 展開所有的行
        target.expandAll = function () {
            target.find("tbody").find("tr").find(".treetable-expander").each(function (i, n) {
                var _isCollapsed = $(n).hasClass(options.expanderCollapsedClass);
                if (_isCollapsed) {
                    $(n).trigger("click");
                }
            })
        }
        // 折叠所有的行
        target.collapseAll = function () {
            target.find("tbody").find("tr").find(".treetable-expander").each(function (i, n) {
                var _isExpanded = $(n).hasClass(options.expanderExpandedClass);
                if (_isExpanded) {
                    $(n).trigger("click");
                }
            })
        }
        // 顯示指定列
        target.showColumn = function (field, flag) {
            var _index = $.inArray(field, target.hiddenColumns);
            if (_index > -1) {
                target.hiddenColumns.splice(_index, 1);
            }
            target.find("." + field + "_cls").show();
            //是否更新列選項狀態
            if (flag && options.showColumns) {
                var $input = $(".bootstrap-tree-table .treetable-bars .columns label").find("input[value='" + field + "']")
                $input.prop("checked", 'checked');
            }
        }
        // 隱藏指定列
        target.hideColumn = function (field, flag) {
            target.hiddenColumns.push(field);
            target.find("." + field + "_cls").hide();
            //是否更新列選項狀態
            if (flag && options.showColumns) {
                var $input = $(".bootstrap-tree-table .treetable-bars .columns label").find("input[value='" + field + "']")
                $input.prop("checked", '');
            }
        }
        // 初始化
        init();
        return target;
    };

    // 組件方法封装........
    $.fn.bootstrapTreeTable.methods = {
        // 為了兼容bootstrap-table的寫法，统一返回數組，這里返回了表格顯示列的資料
        getSelections: function (target, data) {
            // 所有被選中的記錄input
            var _ipt = target.find("tbody").find("tr").find("input[name='select_item']:checked");
            var chk_value = [];
            // 如果是radio
            if (_ipt.attr("type") == "radio") {
                var _data = target.data_obj["id_" + _ipt.val()];
                chk_value.push(_data);
            } else {
                _ipt.each(function (_i, _item) {
                    var _data = target.data_obj["id_" + $(_item).val()];
                    chk_value.push(_data);
                });
            }
            return chk_value;
        },
        // 刷新記錄
        refresh: function (target, parms) {
            if (parms) {
                target.refresh(parms);
            } else {
                target.refresh();
            }
        },
        // 添加資料到表格
        appendData: function (target, data) {
            if (data) {
                target.appendData(data);
            }
        },
        // 展開/折叠指定的行
        toggleRow: function (target, id) {
            target.toggleRow(id);
        },
        // 展開指定的行
        expandRow: function (target, id) {
            target.expandRow(id);
        },
        // 折叠 指定的行
        collapseRow: function (target, id) {
            target.collapseRow(id);
        },
        // 展開所有的行
        expandAll: function (target) {
            target.expandAll();
        },
        // 折叠所有的行
        collapseAll: function (target) {
            target.collapseAll();
        },
        // 顯示指定列
        showColumn: function (target, field) {
            target.showColumn(field, true);
        },
        // 隱藏指定列
        hideColumn: function (target, field) {
            target.hideColumn(field, true);
        }
        // 組件的其他方法也可以進行類似封装........
    };

    $.fn.bootstrapTreeTable.defaults = {
        code: 'code',              // 選取記錄返回的值,用於設置父子關系
        parentCode: 'parentCode',  // 用於設置父子關系
        rootIdValue: null,         // 設置根節點id值----可指定根節點，默認為null,"",0,"0"
        data: null,                // 構造table的資料集合
        method: "GET",               // 請求資料的ajax類型
        url: null,                 // 請求資料的ajax的url
        ajaxParams: {},            // 請求資料的ajax的data属性
        expandColumn: 0,           // 在哪一列上面顯示展開按鈕
        expandAll: false,          // 是否全部展開
        expandFirst: true,         // 是否默認第一級展開--expandAll為false時生效
        striped: false,            // 是否各行渐變色
        bordered: true,            // 是否顯示邊框
        hover: true,               // 是否鼠標悬停
        condensed: false,          // 是否紧縮表格
        columns: [],               // 列
        toolbar: null,             // 頂部工具條
        height: 0,                 // 表格高度
        showTitle: true,           // 是否采用title属性顯示字段內容（被formatter格式化的字段不会顯示）
        showColumns: true,         // 是否顯示內容列下拉框
        showRefresh: true,         // 是否顯示刷新按鈕
        expanderExpandedClass: 'glyphicon glyphicon-chevron-down', // 展開的按鈕的圖標
        expanderCollapsedClass: 'glyphicon glyphicon-chevron-right', // 縮起的按鈕的圖標
        onLoadSuccess: null          // 加載完成後調用
    };
})(jQuery);
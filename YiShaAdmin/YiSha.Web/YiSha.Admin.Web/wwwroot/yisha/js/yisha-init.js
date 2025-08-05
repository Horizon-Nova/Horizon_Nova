/*頁面的ready函數執行之後再執行*/
$(function () {
    // checkbox 事件绑定
    if ($(".check-box").length > 0) {
        $(".check-box").iCheck({
            checkboxClass: 'icheckbox-blue',
            radioClass: 'iradio-blue',
        });
    }

    // radio 事件绑定
    if ($(".radio-box").length > 0) {
        $(".radio-box").iCheck({
            checkboxClass: 'icheckbox-blue',
            radioClass: 'iradio-blue',
        });
    }

    // laydate 時間控件绑定
    if ($(".select-time").length > 10) {
        layui.use('laydate', function () {
            var laydate = layui.laydate;
            var startDate = laydate.render({
                elem: '#startTime',
                max: $('#endTime').val(),
                theme: 'molv',
                trigger: 'click',
                done: function (value, date) {
                    // 結束時間大於開始時間
                    if (value !== '') {
                        endDate.config.min.year = date.year;
                        endDate.config.min.month = date.month - 1;
                        endDate.config.min.date = date.date;
                    } else {
                        endDate.config.min.year = '';
                        endDate.config.min.month = '';
                        endDate.config.min.date = '';
                    }
                }
            });
            var endDate = laydate.render({
                elem: '#endTime',
                min: $('#startTime').val(),
                theme: 'molv',
                trigger: 'click',
                done: function (value, date) {
                    // 開始時間小於結束時間
                    if (value !== '') {
                        startDate.config.max.year = date.year;
                        startDate.config.max.month = date.month - 1;
                        startDate.config.max.date = date.date;
                    } else {
                        startDate.config.max.year = '';
                        startDate.config.max.month = '';
                        startDate.config.max.date = '';
                    }
                }
            });
        });
    }

    // tree 關鍵字查詢绑定
    if ($("#keyword").length > 0) {
        $("#keyword").bind("focus", function focusKey(e) {
            if ($("#keyword").hasClass("empty")) {
                $("#keyword").removeClass("empty");
            }
        }).bind("blur", function blurKey(e) {
            if ($("#keyword").val() === "") {
                $("#keyword").addClass("empty");
            }
            $.tree.searchNode(e);
        }).bind("input propertychange", $.tree.searchNode);
    }

    // bootstrap table tree 表格树 展開/折叠
    var expandFlag = false;
    $("#btnExpandAll").click(function () {
        if (expandFlag) {
            $('#gridTable').bootstrapTreeTable('expandAll');
        } else {
            $('#gridTable').bootstrapTreeTable('collapseAll');
        }
        expandFlag = expandFlag ? false : true;
    });


    // bootstraple table 行選中按鈕樣式狀態變更
    $("#gridTable").on("check.bs.table uncheck.bs.table check-all.bs.table uncheck-all.bs.table", function () {
        var ids = $("#gridTable").bootstrapTable("getSelections");
        if ($('#btnDelete')) {
            $('#btnDelete').toggleClass('disabled', !ids.length);
        }
        if ($('#btnEdit')) {
            $('#btnEdit').toggleClass('disabled', ids.length != 1);
        }
    });

    // select2复選框事件绑定
    if ($.fn.select2 !== undefined) {
        $("select.form-control.select2").each(function () {
            $(this).select2().on("change", function () {
                $(this).valid();
            });
        });
    }

    $("#searchDiv").keyup(function (e) {
        if (e.which === 13) {
            $("#btnSearch").click();
        }
    });

    // 校驗按鈕權限，沒有權限的按鈕就隱藏
    if (top.getButtonAuthority) {
        var buttonList = [];
        $('#toolbar').find('a').each(function (i, ele) {
            buttonList.push(ele.id);
        });
        $('.toolbar').find('a').each(function (i, ele) {
            buttonList.push(ele.id);
        });
        var removeButtonList = top.getButtonAuthority(window.location.href, buttonList);
        if (removeButtonList) {
            $.each(removeButtonList, function (i, val) {
                $("#" + val).remove();
            });
        }
    }

    // input,select 的id赋值給name，因為jquery.validation驗證組件使用的是name
    $("input:text, input:password, input:radio, select").each(function (i, ele) {
        if (ele.id) {
            $(ele).attr("name", ele.id);
        }
    });
});

// 查询事件調用，給按鈕添加disabled
function resetToolbarStatus() {
    if ($('#btnDelete')) {
        $('#btnDelete').addClass('disabled');
    }
    if ($('#btnEdit')) {
        $('#btnEdit').addClass('disabled');
    }
}

function createMenuItem(dataUrl, menuName) {
    var dataIndex = ys.getGuid,
        flag = true;
    if (dataUrl == undefined || $.trim(dataUrl).length == 0) return false;
    var topWindow = $(window.parent.document);
    // 選項卡選單已存在
    $('.menuTab', topWindow).each(function () {
        if ($(this).data('id') == dataUrl) {
            if (!$(this).hasClass('active')) {
                $(this).addClass('active').siblings('.menuTab').removeClass('active');
                $('.page-tabs-content').animate({ marginLeft: "" }, "fast");
                // 顯示tab對應的內容區
                $('.mainContent .YiSha_iframe', topWindow).each(function () {
                    if ($(this).data('id') == dataUrl) {
                        $(this).show().siblings('.YiSha_iframe').hide();
                        return false;
                    }
                });
            }
            flag = false;
            return false;
        }
    });
    // 選項卡選單不存在
    if (flag) {
        var str = '<a href="javascript:;" class="active menuTab" data-id="' + dataUrl + '">' + menuName + ' <i class="fa fa-times-circle"></i></a>';
        $('.menuTab', topWindow).removeClass('active');

        // 添加選項卡對應的iframe
        var str1 = '<iframe class="YiSha_iframe" name="iframe' + dataIndex + '" width="100%" height="100%" src="' + dataUrl + '" frameborder="0" data-id="' + dataUrl + '" seamless></iframe>';
        $('.mainContent', topWindow).find('iframe.YiSha_iframe').hide().parents('.mainContent').append(str1);

        // 添加選項卡
        $('.menuTabs .page-tabs-content', topWindow).append(str);
    }
    return false;
}

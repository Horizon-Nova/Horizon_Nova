/**
 * 首頁方法封装處理
 * Copyright (c) 2019 ruoyi yisha
 */
$(function () {
    // MetsiMenu
    $('#side-menu').metisMenu();

    //固定選單欄
    $(function () {
        $('.sidebar-collapse').slimScroll({
            height: '100%',
            railOpacity: 0.9,
            size: '4px'
        });
    });

    // 選單切換
    $('.navbar-minimalize').click(function () {
        $("body").toggleClass("mini-navbar");
        SmoothlyMenu();
    });

    $('#side-menu>li').click(function () {
        if ($('body').hasClass('mini-navbar')) {
            NavToggle();
        }
    });
    $('#side-menu>li li a').click(function () {
        if ($(window).width() < 769) {
            NavToggle();
        }
    });

    $('.nav-close').click(NavToggle);

    //ios瀏覽器兼容性處理
    if (/(iPhone|iPad|iPod|iOS)/i.test(navigator.userAgent)) {
        $('#content-main').css('overflow-y', 'auto');
    }
});

$(window).bind("load resize", function () {
    if ($(this).width() < 769) {
        $('body').addClass('mini-navbar');
        $('.navbar-static-side').fadeIn();
    }
});

function NavToggle() {
    $('.navbar-minimalize').trigger('click');
}

function SmoothlyMenu() {
    if (!$('body').hasClass('mini-navbar')) {
        $('#side-menu').hide();
        setTimeout(function () {
            $('#side-menu').fadeIn(500);
        }, 100);
    } else if ($('body').hasClass('fixed-sidebar')) {
        $('#side-menu').hide();
        setTimeout(function () {
            $('#side-menu').fadeIn(500);
        }, 300);
    } else {
        $('#side-menu').removeAttr('style');
    }
}

/**
 * iframe處理
 */
$(function () {
    //計算元素集合的總寬度
    function calSumWidth(elements) {
        var width = 0;
        $(elements).each(function () {
            width += $(this).outerWidth(true);
        });
        return width;
    }

    //滚動到指定選項卡
    function scrollToTab(element) {
        var marginLeftVal = calSumWidth($(element).prevAll()),
            marginRightVal = calSumWidth($(element).nextAll());
        // 可視區域非tab寬度
        var tabOuterWidth = calSumWidth($(".content-tabs").children().not(".menuTabs"));
        //可視區域tab寬度
        var visibleWidth = $(".content-tabs").outerWidth(true) - tabOuterWidth;
        //實際滚動寬度
        var scrollVal = 0;
        if ($(".page-tabs-content").outerWidth() < visibleWidth) {
            scrollVal = 0;
        } else if (marginRightVal <= (visibleWidth - $(element).outerWidth(true) - $(element).next().outerWidth(true))) {
            if ((visibleWidth - $(element).next().outerWidth(true)) > marginRightVal) {
                scrollVal = marginLeftVal;
                var tabElement = element;
                while ((scrollVal - $(tabElement).outerWidth()) > ($(".page-tabs-content").outerWidth() - visibleWidth)) {
                    scrollVal -= $(tabElement).prev().outerWidth();
                    tabElement = $(tabElement).prev();
                }
            }
        } else if (marginLeftVal > (visibleWidth - $(element).outerWidth(true) - $(element).prev().outerWidth(true))) {
            scrollVal = marginLeftVal - $(element).prev().outerWidth(true);
        }
        $('.page-tabs-content').animate({ marginLeft: 0 - scrollVal + 'px' }, "fast");
    }

    // 滚動到指定選單
    function scrollToMenu(element) {
        var menuTabUrl = $(element).data('id');
        $(".nav ul, .nav li").removeClass("selected").removeClass("active").removeClass("in");
        $(".nav ul, .nav li").each(function () {
            if ($(this).children().length > 0) {
                var link = $(this).children()[0];
                if (link) {
                    var menuUrl = $(link).data('url');
                    if (menuUrl == menuTabUrl) {
                        var dataType = "[data-type=menu]";
                        var parent1_li = $(link).parent(dataType);
                        parent1_li.addClass("active");

                        var parent2_ul = parent1_li.parent(dataType);
                        parent2_ul.addClass("in").addClass("active");

                        var parent3_li = parent2_ul.parent(dataType);
                        parent3_li.addClass("active");

                        var parent4_ul = parent3_li.parent(dataType);
                        if (parent4_ul) {
                            parent4_ul.addClass("in").addClass("active");

                            var parent5_li = parent4_ul.parent(dataType);
                            parent5_li.addClass("active");
                        }
                        return false; // 終止循环
                    }
                }
            }
        });
    }

    //查看左側隱藏的選項卡
    function scrollTabLeft() {
        var marginLeftVal = Math.abs(parseInt($('.page-tabs-content').css('margin-left')));
        // 可視區域非tab寬度
        var tabOuterWidth = calSumWidth($(".content-tabs").children().not(".menuTabs"));
        //可視區域tab寬度
        var visibleWidth = $(".content-tabs").outerWidth(true) - tabOuterWidth;
        //實際滚動寬度
        var scrollVal = 0;
        if ($(".page-tabs-content").width() < visibleWidth) {
            return false;
        } else {
            var tabElement = $(".menuTab:first");
            var offsetVal = 0;
            while ((offsetVal + $(tabElement).outerWidth(true)) <= marginLeftVal) { //找到离當前tab最近的元素
                offsetVal += $(tabElement).outerWidth(true);
                tabElement = $(tabElement).next();
            }
            offsetVal = 0;
            if (calSumWidth($(tabElement).prevAll()) > visibleWidth) {
                while ((offsetVal + $(tabElement).outerWidth(true)) < (visibleWidth) && tabElement.length > 0) {
                    offsetVal += $(tabElement).outerWidth(true);
                    tabElement = $(tabElement).prev();
                }
                scrollVal = calSumWidth($(tabElement).prevAll());
            }
        }
        $('.page-tabs-content').animate({ marginLeft: 0 - scrollVal + 'px' }, "fast");
    }

    //查看右側隱藏的選項卡
    function scrollTabRight() {
        var marginLeftVal = Math.abs(parseInt($('.page-tabs-content').css('margin-left')));
        // 可視區域非tab寬度
        var tabOuterWidth = calSumWidth($(".content-tabs").children().not(".menuTabs"));
        //可視區域tab寬度
        var visibleWidth = $(".content-tabs").outerWidth(true) - tabOuterWidth;
        //實際滚動寬度
        var scrollVal = 0;
        if ($(".page-tabs-content").width() < visibleWidth) {
            return false;
        } else {
            var tabElement = $(".menuTab:first");
            var offsetVal = 0;
            while ((offsetVal + $(tabElement).outerWidth(true)) <= marginLeftVal) { //找到离當前tab最近的元素
                offsetVal += $(tabElement).outerWidth(true);
                tabElement = $(tabElement).next();
            }
            offsetVal = 0;
            while ((offsetVal + $(tabElement).outerWidth(true)) < (visibleWidth) && tabElement.length > 0) {
                offsetVal += $(tabElement).outerWidth(true);
                tabElement = $(tabElement).next();
            }
            scrollVal = calSumWidth($(tabElement).prevAll());
            if (scrollVal > 0) {
                $('.page-tabs-content').animate({ marginLeft: 0 - scrollVal + 'px' }, "fast");
            }
        }
    }

    // 通過遍历給選單項加上data-index属性
    $(".menuItem").each(function (index) {
        if (!$(this).attr('data-index')) {
            $(this).attr('data-index', index);
        }
    });

    $('.menuItem').on('click', function menuItem() {
        // 獲取標識資料
        var dataUrl = $(this).data('url'),
            dataIndex = $(this).data('index'),
            menuName = $.trim($(this).text()),
            href = $(this).attr("href"),
            addMenuTab = true;

        if (href !== "#" && href !== "") {
            ys.openLink(href, '_blank');
            return false;
        }
        if (dataUrl == undefined || $.trim(dataUrl).length == 0) {
            return false;
        }
        // 選項卡選單已存在
        $('.menuTab').each(function () {
            if ($(this).data('id') == dataUrl) {
                if (!$(this).hasClass('active')) {
                    $(this).addClass('active').siblings('.menuTab').removeClass('active');
                    scrollToTab(this);
                    // 顯示tab對應的內容區
                    $('.mainContent .YiSha_iframe').each(function () {
                        if ($(this).data('id') == dataUrl) {
                            $(this).show().siblings('.YiSha_iframe').hide();
                            return false;
                        }
                    });
                }
                addMenuTab = false;
                return false;
            }
        });
        // 選項卡選單不存在
        if (addMenuTab) {
            var menuTabStr = '<a href="javascript:;" class="active menuTab" data-id="' + dataUrl + '">' + menuName + ' <i class="fa fa-times-circle"></i></a>';
            $('.menuTab').removeClass('active');

            // 添加選項卡對應的iframe
            var ifrmaeStr = '<iframe class="YiSha_iframe" name="iframe' + dataIndex + '" width="100%" height="100%" src="' + dataUrl + '" frameborder="0" data-id="' + dataUrl + '" seamless></iframe>';
            $('.mainContent').find('iframe.YiSha_iframe').hide().parents('.mainContent').append(ifrmaeStr);
            ys.showLoading("資料加載中，請稍後...");

            $('.mainContent iframe:visible').load(function () {
                ys.closeLoading();
            });

            // 添加選項卡
            $('.menuTabs .page-tabs-content').append(menuTabStr);
            scrollToTab($('.menuTab.active'));
        }
        scrollToMenu($('.menuTab.active'));
        return false;
    });

    // 關閉選項卡選單
    function closeTab() {
        var closeTabId = $(this).parents('.menuTab').data('id');
        var currentWidth = $(this).parents('.menuTab').width();

        // 當前元素處於活動狀態
        if ($(this).parents('.menuTab').hasClass('active')) {

            // 當前元素後面有同辈元素，使後面的一個元素處於活動狀態
            if ($(this).parents('.menuTab').next('.menuTab').size()) {

                var activeId = $(this).parents('.menuTab').next('.menuTab:eq(0)').data('id');
                $(this).parents('.menuTab').next('.menuTab:eq(0)').addClass('active');

                $('.mainContent .YiSha_iframe').each(function () {
                    if ($(this).data('id') == activeId) {
                        $(this).show().siblings('.YiSha_iframe').hide();
                        return false;
                    }
                });

                var marginLeftVal = parseInt($('.page-tabs-content').css('margin-left'));
                if (marginLeftVal < 0) {
                    $('.page-tabs-content').animate({
                        marginLeft: (marginLeftVal + currentWidth) + 'px'
                    }, "fast");
                }

                //  移除當前選項卡
                $(this).parents('.menuTab').remove();

                // 移除tab對應的內容區
                $('.mainContent .YiSha_iframe').each(function () {
                    if ($(this).data('id') == closeTabId) {
                        $(this).remove();
                        return false;
                    }
                });
            }

            // 當前元素後面沒有同辈元素，使當前元素的上一個元素處於活動狀態
            if ($(this).parents('.menuTab').prev('.menuTab').size()) {
                var activeId = $(this).parents('.menuTab').prev('.menuTab:last').data('id');
                $(this).parents('.menuTab').prev('.menuTab:last').addClass('active');
                $('.mainContent .YiSha_iframe').each(function () {
                    if ($(this).data('id') == activeId) {
                        $(this).show().siblings('.YiSha_iframe').hide();
                        return false;
                    }
                });

                //  移除當前選項卡
                $(this).parents('.menuTab').remove();

                // 移除tab對應的內容區
                $('.mainContent .YiSha_iframe').each(function () {
                    if ($(this).data('id') == closeTabId) {
                        $(this).remove();
                        return false;
                    }
                });
            }
        }
        // 當前元素不處於活動狀態
        else {
            //  移除當前選項卡
            $(this).parents('.menuTab').remove();

            // 移除相應tab對應的內容區
            $('.mainContent .YiSha_iframe').each(function () {
                if ($(this).data('id') == closeTabId) {
                    $(this).remove();
                    return false;
                }
            });
            scrollToTab($('.menuTab.active'));
        }
        return false;
    }

    $('.menuTabs').on('click', '.menuTab i', closeTab);

    //關閉其他選項卡
    $('.tabCloseOther').on('click', function closeOtherTabs() {
        $('.page-tabs-content').children("[data-id]").not(":first").not(".active").each(function () {
            $('.YiSha_iframe[data-id="' + $(this).data('id') + '"]').remove();
            $(this).remove();
        });
        $('.page-tabs-content').css("margin-left", "0");
    });

    //滚動到已激活的選項卡
    $('.tabShowActive').on('click', function showActiveTab() {
        scrollToTab($('.menuTab.active'));
    });

    // 點击選項卡選單
    $('.menuTabs').on('click', '.menuTab', function activeTab() {
        if (!$(this).hasClass('active')) {
            var currentId = $(this).data('id');
            // 顯示tab對應的內容區
            $('.mainContent .YiSha_iframe').each(function () {
                if ($(this).data('id') == currentId) {
                    $(this).show().siblings('.YiSha_iframe').hide();
                    return false;
                }
            });
            $(this).addClass('active').siblings('.menuTab').removeClass('active');
            scrollToTab(this);
        }
        scrollToMenu(this);
    });

    //刷新iframe
    function refreshTab() {
        var currentId = $('.page-tabs-content').find('.active').attr('data-id');
        var target = $('.YiSha_iframe[data-id="' + currentId + '"]');
        var url = target.attr('src');
        target.attr('src', url).ready();
    }

    // 全螢幕顯示
    $('#fullScreen').on('click', function () {
        $('#wrapper').fullScreen();
    });

    // 刷新按鈕
    $('.tabReload').on('click', refreshTab);

    $('.menuTabs').on('dblclick', '.menuTab', refreshTab);

    // 左移按扭
    $('.tabLeft').on('click', scrollTabLeft);

    // 右移按扭
    $('.tabRight').on('click', scrollTabRight);

    // 關閉當前
    $('.tabCloseCurrent').on('click', function () {
        $('.page-tabs-content').find('.active i').trigger("click");
    });

    // 關閉全部
    $('.tabCloseAll').on('click', function () {
        $('.page-tabs-content').children("[data-id]").not(":first").each(function () {
            $('.YiSha_iframe[data-id="' + $(this).data('id') + '"]').remove();
            $(this).remove();
        });
        $('.page-tabs-content').children("[data-id]:first").each(function () {
            $('.YiSha_iframe[data-id="' + $(this).data('id') + '"]').show();
            $(this).addClass("active");
        });
        $('.page-tabs-content').css("margin-left", "0");
    });
});
document.addEventListener('DOMContentLoaded', () => {
    initSidebarToggle();
    initSidebarMenuHighlight();
    initTabs();
    initSidebarHoverPopup();

    $('.tabLeft').on('click', scrollTabLeft);
    $('.tabRight').on('click', scrollTabRight);

    $('.dropdown-menu .dropdown-item').on('click', function () {
        const action = $(this).data('action');
        handleTabOperation(action);
    });

    $('#tabRefresh').on('click', () => {
        const $activeTab = $('.page-tabs-content').find('.menuTab.active');
        if ($activeTab.length) {
            const id = $activeTab.data('id');
            const $iframe = $(`.Nova_iframe[data-id="${id}"]`);
            if ($iframe.length) {
                const url = $iframe.attr('src');
                $iframe.attr('src', url);
            }
        }
    });
    $('#fullscreenToggle').on('click', toggleFullscreen);

});

function initSidebarToggle() {
    $('#sidebarToggle').on('click', function () {
        const $sidebar = $('#sidebar');
        $sidebar.toggleClass('sidebar-mini');

        $('.sidebar-popup').remove(); 

        if ($sidebar.hasClass('sidebar-mini')) {
            $('#sidebar .collapse.show').collapse('hide');
        }
    });
}

function initSidebarMenuHighlight() {
    $('#sidebar .nav-link[data-bs-toggle="collapse"]').on('click', function (e) {
        const $sidebar = $('#sidebar');
        const $link = $(this);
        const target = $link.attr('href'); 

        if ($sidebar.hasClass('sidebar-mini')) {
            $sidebar.removeClass('sidebar-mini');
            $('.sidebar-popup').remove();
        }
        $('#sidebar .collapse.show').not(target).collapse('hide');

        $('#sidebar .nav-link').removeClass('active');
        $link.addClass('active');
    });

    $('#sidebar .collapse .nav-link').on('click', function () {
        const $link = $(this);

        // 清除所有 active
        $('#sidebar .nav-link').removeClass('active');

        // 給對應主選單和自己加上 active
        $link.closest('.collapse').prev('.nav-link').addClass('active');
        $link.addClass('active');
    });
}

function initTabs() {
    const $tabsContainer = $('.page-tabs-content');
    const $mainContent = $('.mainContent');

    // 點擊左側選單或 popup 內容
    $(document).on('click', '.menuItem', function () {
        const url = $(this).data('url');
        const id = $(this).data('id');
        const text = $(this).text().trim();

        if (!id || !url) return;

        const $existingTab = $tabsContainer.find(`.menuTab[data-id="${id}"]`);
        if ($existingTab.length) {
            activateTab($existingTab);
        } else {
            const $tab = $(`
                <a href="javascript:;" class="menuTab" data-id="${id}">
                    ${text} <span class="close-tab">&times;</span>
                </a>`);
            $tabsContainer.append($tab);

            const $iframe = $(`
                <iframe class="content-iframe" 
                        name="iframe_${id}" 
                        width="100%" height="100%" 
                        src="${url}" frameborder="0" 
                        data-id="${id}"></iframe>`);
            $mainContent.find('.content-iframe').hide();
            $mainContent.append($iframe);

            activateTab($tab);
        }
    });

    // 點擊頁籤
    $tabsContainer.on('click', '.menuTab', function (e) {
        if ($(e.target).hasClass('close-tab')) return;
        activateTab($(this));
    });

    // 關閉頁籤
    $tabsContainer.on('click', '.close-tab', function () {
        const $tab = $(this).closest('.menuTab');
        const id = $tab.data('id');
        const isActive = $tab.hasClass('active');
        $tab.remove();
        $(`.content-iframe[data-id="${id}"]`).remove();

        if (isActive) {
            const $last = $tabsContainer.find('.menuTab').last();
            if ($last.length) activateTab($last);
            else $mainContent.empty();
        }
    });

    function activateTab($tab) {
        const id = $tab.data('id');
        $tabsContainer.find('.menuTab').removeClass('active');
        $tab.addClass('active');

        $mainContent.find('.content-iframe').hide();
        $mainContent.find(`.content-iframe[data-id="${id}"]`).show();
    }
}

function initSidebarHoverPopup() {
    let hideTimeout;

    $('#sidebar .nav-link').hover(
        function () {
            const $sidebar = $('#sidebar');
            if (!$sidebar.hasClass('sidebar-mini')) return;

            clearTimeout(hideTimeout);
            $('.sidebar-popup').remove();

            const collapseId = $(this).attr('href')?.replace('#', '');
            const $collapse = $('#' + collapseId).clone().addClass('show').css('display', 'block');

            const offset = $(this).offset();

            const $popup = $('<div class="sidebar-popup"></div>')
                .append(`<div class="popup-title">${$(this).find('.menu-text').text().trim()}</div>`)
                .append($collapse)
                .css({
                    top: offset.top,
                    left: offset.left + $(this).outerWidth() + 10,
                    position: 'absolute',
                    background: '#162029',
                    padding: '10px',
                    'min-width': '200px',
                    'z-index': 9999,
                    color: '#fff'
                })
                .appendTo('body');

            $popup.hover(
                function () {
                    clearTimeout(hideTimeout);
                },
                function () {
                    hideTimeout = setTimeout(() => {
                        $(this).remove();
                    }, 200);
                }
            );
        },
        function () {
            hideTimeout = setTimeout(() => {
                $('.sidebar-popup').remove();
            }, 200);
        }
    );
}

function handleTabOperation(action) {
    const $tabsContainer = $('.page-tabs-content');
    const $mainContent = $('.mainContent');
    const $activeTab = $tabsContainer.find('.menuTab.active');

    switch (action) {
        case 'close-current':
            if ($activeTab.length) {
                $activeTab.find('.close-tab').click();
            }
            break;

        case 'close-others':
            $tabsContainer.find('.menuTab').not('.active').each(function () {
                const id = $(this).data('id');
                $(`.Nova_iframe[data-id="${id}"]`).remove();
                $(this).remove();
            });
            break;

        case 'close-all':
            $tabsContainer.find('.menuTab').each(function () {
                const id = $(this).data('id');
                $(`.Nova_iframe[data-id="${id}"]`).remove();
                $(this).remove();
            });
            $mainContent.html('');
            break;
    }
}

function calSumWidth($elements) {
    let width = 0;
    $elements.each(function () {
        width += $(this).outerWidth(true);
    });
    return width;
}

function scrollTabLeft() {
    const $content = $('.page-tabs-content');
    const $container = $('.tab-bar');
    const marginLeftVal = Math.abs(parseInt($content.css('margin-left')) || 0);
    const tabOuterWidth = calSumWidth($container.children().not('.menuTabs'));
    const visibleWidth = $container.outerWidth(true) - tabOuterWidth;

    if ($content.width() <= visibleWidth) return;

    let scrollVal = 0;
    let offsetVal = 0;
    let $tab = $('.menuTab:first');

    while ((offsetVal + $tab.outerWidth(true)) <= marginLeftVal) {
        offsetVal += $tab.outerWidth(true);
        $tab = $tab.next();
    }

    if (calSumWidth($tab.prevAll()) > visibleWidth) {
        offsetVal = 0;
        while ((offsetVal + $tab.outerWidth(true)) < visibleWidth && $tab.length > 0) {
            offsetVal += $tab.outerWidth(true);
            $tab = $tab.prev();
        }
        scrollVal = calSumWidth($tab.prevAll());
    }

    $content.stop().animate({ marginLeft: -scrollVal + 'px' }, 'fast');
}

function scrollTabRight() {
    const $content = $('.page-tabs-content');
    const $container = $('.tab-bar');
    const marginLeftVal = Math.abs(parseInt($content.css('margin-left')) || 0);
    const tabOuterWidth = calSumWidth($container.children().not('.menuTabs'));
    const visibleWidth = $container.outerWidth(true) - tabOuterWidth;

    if ($content.width() <= visibleWidth) return;

    let scrollVal = 0;
    let offsetVal = 0;
    let $tab = $('.menuTab:first');

    while ((offsetVal + $tab.outerWidth(true)) <= marginLeftVal) {
        offsetVal += $tab.outerWidth(true);
        $tab = $tab.next();
    }

    offsetVal = 0;
    while ((offsetVal + $tab.outerWidth(true)) < visibleWidth && $tab.length > 0) {
        offsetVal += $tab.outerWidth(true);
        $tab = $tab.next();
    }

    scrollVal = calSumWidth($tab.prevAll());
    if (scrollVal > 0) {
        $content.stop().animate({ marginLeft: -scrollVal + 'px' }, 'fast');
    }
}

function toggleFullscreen() {
    if (!document.fullscreenElement) {
        document.documentElement.requestFullscreen().catch(err => {
            console.error(`無法切換全螢幕: ${err.message}`);
        });
    } else {
        document.exitFullscreen();
    }
}
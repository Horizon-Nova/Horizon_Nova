// 初始化
document.addEventListener('DOMContentLoaded', () => {
    initSidebarToggle();
    initSidebarMenuHighlight();
    initTabs();
    initSidebarHoverPopup();
});


function initSidebarToggle() {
    $('#sidebarToggle').on('click', function () {
        $('#sidebar').toggleClass('sidebar-mini');
    });
}

function initSidebarMenuHighlight() {
    $('#sidebar .nav-link[data-bs-toggle="collapse"]').on('click', function () {
        const $link = $(this);

        // 清除所有 active
        $('#sidebar .nav-link').removeClass('active');

        // 給當前加上 active
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
    const $tabsContainer = $('.tabs-container');
    const $iframe = $('#content-frame');

    // 點選左側選單
    $('.menuItem').on('click', function () {
        const url = $(this).data('url');
        const id = $(this).data('id');
        const text = $(this).text();

        if (!id || !url) return;

        const $existingTab = $tabsContainer.find(`.tab[data-id="${id}"]`);
        if ($existingTab.length) {
            activateTab($existingTab);
        } else {
            const $tab = $(`<a href="#" class="tab" data-id="${id}">${text} <span class="close-tab">&times;</span></a>`);
            $tabsContainer.append($tab);
            activateTab($tab);
        }
    });

    // 點選頁籤
    $tabsContainer.on('click', '.tab', function (e) {
        if ($(e.target).hasClass('close-tab')) return;
        activateTab($(this));
    });

    // 關閉頁籤
    $tabsContainer.on('click', '.close-tab', function () {
        const $tab = $(this).closest('.tab');
        const isActive = $tab.hasClass('active');
        $tab.remove();

        if (isActive) {
            const $last = $tabsContainer.find('.tab').last();
            if ($last.length) activateTab($last);
            else $iframe.attr('src', '/Home/index');
        }
    });

    // 頁籤操作
    $('.dropdown-menu .dropdown-item').on('click', function () {
        const action = $(this).text().trim();
        const $activeTab = $tabsContainer.find('.tab.active');

        if (action === '关闭当前' && $activeTab.length) {
            $activeTab.find('.close-tab').click();
        } else if (action === '关闭其他') {
            $tabsContainer.find('.tab').not('.active').remove();
        } else if (action === '全部关闭') {
            $tabsContainer.find('.tab').remove();
            $iframe.attr('src', '/Home/index');
        }
    });

    // 刷新
    $('#tabRefresh').on('click', function () {
        const $activeTab = $tabsContainer.find('.tab.active');
        const id = $activeTab.data('id');
        if (!id) return;

        const $menuItem = $(`.menuItem[data-id="${id}"]`);
        if ($menuItem.length) {
            const url = $menuItem.data('url');
            $iframe.attr('src', url);
        }
    });

    // 左右滾動
    $('#tabLeft').on('click', function () {
        $tabsContainer.scrollLeft($tabsContainer.scrollLeft() - 200);
    });
    $('#tabRight').on('click', function () {
        $tabsContainer.scrollLeft($tabsContainer.scrollLeft() + 200);
    });

    function activateTab($tab) {
        const id = $tab.data('id');
        const $menuItem = $(`.menuItem[data-id="${id}"]`);
        if ($menuItem.length) {
            const url = $menuItem.data('url');
            $iframe.attr('src', url);
        }
        $tabsContainer.find('.tab').removeClass('active');
        $tab.addClass('active');
    }
}

function initSidebarHoverPopup() {
    let hideTimeout;

    $('#sidebar .nav-link').hover(
        function () {
            const $sidebar = $('#sidebar');
            if (!$sidebar.hasClass('sidebar-mini')) {
                return;
            }

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


$(function () {
    if (window.lucide?.createIcons) {
        window.lucide.createIcons();
    }

    // 畫面切換功能
    function getScreenUrl(screenName) {
        return '/NovaAPP/DialVision/LoadScreen?screen=' + screenName;
    }

    function switchScreen(screenName) {
        const screenContainer = $('#screenContainer');
        const screenUrl = getScreenUrl(screenName);
        const phoneScreen = $('#phoneScreen');
        
        screenContainer.addClass('fade-out');
        
        setTimeout(function() {
            $.ajax({
                url: screenUrl,
                method: 'GET',
                success: function(html) {
                    screenContainer.html(html).removeClass('fade-out').addClass('fade-in');
                    
                    // 根據畫面切換背景
                    if (screenName === 'dashboard') {
                        phoneScreen.addClass('dashboard-bg');
                    } else {
                        phoneScreen.removeClass('dashboard-bg');
                    }
                    
                    if (window.lucide?.createIcons) {
                        window.lucide.createIcons();
                    }
                    
                    setTimeout(function() {
                        screenContainer.removeClass('fade-in');
                    }, 300);
                },
                error: function() {
                    screenContainer.removeClass('fade-out');
                }
            });
        }, 150);
    }

    // 暴露為全域，提供 inline onclick 及其他模組呼叫
    window.switchScreen = switchScreen;
    window.getScreenUrl = getScreenUrl;

    // 登入按鈕點擊事件
    $(document).on('click', '#btnLogin', function() {
        switchScreen('dashboard');
    });
});


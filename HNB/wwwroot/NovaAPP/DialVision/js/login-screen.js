// LoginScreen 專用 JavaScript
$(function () {
    // 密碼顯示/隱藏切換
    $(document).on('click', '.password-toggle', function() {
        const $input = $(this).siblings('.form-input');
        const $icon = $(this).find('i');
        
        if ($input.attr('type') === 'password') {
            $input.attr('type', 'text');
            $icon.attr('data-lucide', 'eye');
        } else {
            $input.attr('type', 'password');
            $icon.attr('data-lucide', 'eye-off');
        }
        
        if (window.lucide?.createIcons) {
            window.lucide.createIcons();
        }
    });
});


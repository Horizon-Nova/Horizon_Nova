'use strict';

(function () {
    var iconMap = {
        success: 'check-circle',
        error: 'x-circle',
        warning: 'alert-triangle',
        info: 'info'
    };

    function createToastElement(type) {
        var $template = $('#toast-template');
        if ($template.length === 0) {
            console.warn('[Toast] 找不到 toast-template');
            return null;
        }

        var $toast = $template.clone();
        $toast.removeAttr('data-toast-template');
        $toast.removeClass('d-none toast-success toast-error toast-warning toast-info');
        $toast.removeAttr('id');
        $toast.addClass('toast-' + type);

        return $toast;
    }

    function setToastContent($toast, message, type) {
        var icon = iconMap[type] || iconMap.info;
        $toast.find('[data-toast-role=\"message\"]').text(message || '');
        var $icon = $toast.find('[data-toast-role=\"icon\"]');
        if ($icon.length > 0) {
            $icon.attr('data-lucide', icon);
        }
    }

    function showToast(message, type, duration) {
        if (type === void 0) { type = 'info'; }
        if (duration === void 0) { duration = 3000; }

        var $container = $('#toast-container');
        if ($container.length === 0) {
            console.warn('[Toast] 找不到 toast-container');
            return;
        }

        var resolvedType = iconMap[type] ? type : 'info';
        var toastId = 'toast-' + Date.now() + '-' + Math.random().toString(36).substr(2, 6);

        var $toast = createToastElement(resolvedType);
        if (!$toast) {
            return;
        }

        $toast.attr('id', toastId);
        setToastContent($toast, message, resolvedType);

        $container.append($toast);

        if (window.lucide && typeof window.lucide.createIcons === 'function') {
            window.lucide.createIcons();
        }

        requestAnimationFrame(function () {
            $toast.addClass('show');
        });

        if (duration > 0) {
            setTimeout(function () {
                closeToastById(toastId);
            }, duration);
        }
    }

    function closeToast(trigger) {
        var $toast = $(trigger).closest('.toast');
        if ($toast.length === 0) {
            return;
        }
        closeToastById($toast.attr('id'));
    }

    function closeToastById(toastId) {
        if (!toastId) {
            return;
        }

        var $toast = $('#' + toastId);
        if ($toast.length === 0) {
            return;
        }

        $toast.removeClass('show');
        setTimeout(function () {
            $toast.remove();
        }, 200);
    }

    window.showToast = showToast;
    window.closeToast = closeToast;
    window.closeToastById = closeToastById;
})();


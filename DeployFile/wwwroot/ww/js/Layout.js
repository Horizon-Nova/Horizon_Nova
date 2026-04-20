(function () {

    function initializeLucideIcons() {
        window.lucide ? lucide.createIcons() : null;
    }

    function initializeLayout() {
        initializeLucideIcons();
    }

    document.readyState === "loading"
        ? document.addEventListener("DOMContentLoaded", initializeLayout)
        : initializeLayout();

})();
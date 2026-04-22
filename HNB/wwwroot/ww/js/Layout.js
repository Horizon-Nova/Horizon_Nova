(function () {

    function clamp(value, min, max) {
        return Math.max(min, Math.min(max, value));
    }

    function updateWwScale() {
        if (!document.body || !document.body.classList.contains("ww-preview")) {
            document.documentElement.style.setProperty("--ww-scale", "1");
            return;
        }

        var appWidth = 1180;
        var appHeight = 730;
        var padding = 64;

        var availableWidth = Math.max(0, window.innerWidth - padding);
        var availableHeight = Math.max(0, window.innerHeight - padding);

        var scaleX = availableWidth / appWidth;
        var scaleY = availableHeight / appHeight;
        var scale = clamp(Math.min(scaleX, scaleY, 1), 0.25, 1);

        document.documentElement.style.setProperty("--ww-scale", scale.toFixed(4));
    }

    function queryById(id) {
        return document.getElementById(id);
    }

    function closeLocationPopup() {
        var popup = queryById("loc-popup");
        popup ? popup.classList.remove("open") : null;
    }

    window.wwToggleLocPopup = function () {
        var popup = queryById("loc-popup");
        if (!popup) {
            return;
        }

        popup.classList.toggle("open");

        if (popup.classList.contains("open")) {
            window.setTimeout(function () {
                var input = queryById("loc-inp");
                input ? input.focus() : null;
            }, 50);
        }
    };

    window.wwConfirmLoc = function () {
        var input = queryById("loc-inp");
        var text = queryById("loc-text");

        if (!input || !text) {
            return;
        }

        var value = (input.value || "").trim();

        if (value) {
            text.textContent = value;
        }

        input.value = "";
        closeLocationPopup();
    };

    function initializeLayout() {
        updateWwScale();
        window.addEventListener("resize", updateWwScale);

        document.addEventListener("click", function (event) {
            var pill = queryById("loc-pill");
            var popup = queryById("loc-popup");

            if (!pill || !popup) {
                return;
            }

            pill.contains(event.target) ? null : popup.classList.remove("open");
        });
    }

    document.readyState === "loading"
        ? document.addEventListener("DOMContentLoaded", initializeLayout)
        : initializeLayout();

})();

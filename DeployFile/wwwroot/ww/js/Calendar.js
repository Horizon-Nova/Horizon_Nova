(function () {

    var MONTHS = [
        "January",
        "February",
        "March",
        "April",
        "May",
        "June",
        "July",
        "August",
        "September",
        "October",
        "November",
        "December"
    ];
    var currentMonth = 3;
    var currentYear = 2026;
    var outfitDays = [1, 2, 3, 4, 6, 7, 8, 9, 10, 11, 13, 14, 15, 16, 17, 18, 19, 20];

    function renderCalendar() {
        var monthLabel = document.getElementById("month-label");
        if (monthLabel) {
            monthLabel.textContent = MONTHS[currentMonth] + " " + currentYear;
        }

        var grid = document.getElementById("cal-grid");
        if (!grid) {
            return;
        }

        grid.innerHTML = "";

        ["S", "M", "T", "W", "T", "F", "S"].forEach(function (d) {
            var el = document.createElement("div");
            el.className = "cal-dow";
            el.textContent = d;
            grid.appendChild(el);
        });

        var firstDay = new Date(currentYear, currentMonth, 1).getDay();
        var daysInMonth = new Date(currentYear, currentMonth + 1, 0).getDate();

        var today = 20;

        for (var i = 0; i < firstDay; i++) {
            var dim = document.createElement("div");
            dim.className = "cal-day dim";
            grid.appendChild(dim);
        }

        for (var day = 1; day <= daysInMonth; day++) {
            var el = document.createElement("div");
            var hasOutfit = outfitDays.indexOf(day) >= 0 && currentMonth === 3;
            var isToday = day === today && currentMonth === 3;
            var isFuture = day > today && currentMonth === 3;

            el.className = "cal-day" + (isToday ? " today" : "") + (isFuture ? " dim" : "");
            el.innerHTML = '<span class="cal-num">' + day + "</span>" + (hasOutfit ? '<div class="cal-dot"></div>' : "");

            if (day === 20) {
                el.classList.add("sel");
            }

            if (hasOutfit && !isFuture) {
                el.addEventListener("click", function () {
                    document.querySelectorAll(".cal-day").forEach(function (c) { c.classList.remove("sel"); });
                    this.classList.add("sel");
                });
            }

            grid.appendChild(el);
        }
    }

    function changeMonth(dir) {
        currentMonth += dir;
        if (currentMonth < 0) {
            currentMonth = 11;
            currentYear--;
        }
        if (currentMonth > 11) {
            currentMonth = 0;
            currentYear++;
        }
        renderCalendar();
    }

    function showDay(element) {
        document.querySelectorAll(".js-recent-item").forEach(function (i) { i.classList.remove("active"); });
        element.classList.add("active");

        var date = element.getAttribute("data-date") || "";
        var weekday = element.getAttribute("data-weekday") || "";
        var weather = element.getAttribute("data-weather") || "";
        var occ = element.getAttribute("data-occ") || "";
        var name = element.getAttribute("data-name") || "";
        var summary = element.getAttribute("data-summary") || "";

        var dayNum = (date.split(" ")[1] || "").trim();
        var logDateBig = document.getElementById("log-date-big");
        if (logDateBig) {
            logDateBig.textContent = weekday + ", " + MONTHS[currentMonth] + " " + dayNum;
        }

        var logWeather = document.getElementById("log-weather");
        if (logWeather) {
            logWeather.textContent = weather;
        }

        var logOcc = document.getElementById("log-occ");
        if (logOcc) {
            logOcc.textContent = occ;
        }

        var heroBadge = document.getElementById("hero-badge");
        if (heroBadge) {
            heroBadge.textContent = occ;
        }

        var heroWeather = document.getElementById("hero-weather");
        if (heroWeather) {
            heroWeather.textContent = weather.split("·")[0].trim();
        }

        var aiName = document.getElementById("ai-summary-name");
        if (aiName) {
            aiName.textContent = name;
        }

        var aiText = document.getElementById("ai-summary-text");
        if (aiText) {
            aiText.textContent = summary;
        }

        var likeBtn = document.getElementById("like-btn");
        if (likeBtn) {
            likeBtn.classList.remove("liked");
        }
    }

    function toggleLike() {
        var likeBtn = document.getElementById("like-btn");
        if (likeBtn) {
            likeBtn.classList.toggle("liked");
        }
    }

    function initializeRecentStrip() {
        document.querySelectorAll(".js-recent-item").forEach(function (el) {
            el.addEventListener("click", function () { showDay(el); });
        });
    }

    window.wwCalendarChangeMonth = changeMonth;
    window.wwCalendarToggleLike = toggleLike;

    document.readyState === "loading"
        ? document.addEventListener("DOMContentLoaded", function () {
            renderCalendar();
            initializeRecentStrip();
        })
        : (renderCalendar(), initializeRecentStrip());

})();

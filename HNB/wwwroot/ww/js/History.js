(function () {

    var state = {
        filter: "all",
        query: "",
        records: [],
        selectedDay: null,
        activeGroupId: null
    };

    function normalizeRecord(r) {
        if (!r) {
            return {
                id: "",
                createdAt: "",
                typeKey: "",
                typeName: "",
                title: "",
                description: "",
                badgeText: ""
            };
        }

        // System.Text.Json 直接序列化時預設是 PascalCase；前端使用 camelCase，這裡做一次相容。
        return {
            id: r.id || r.Id || "",
            createdAt: r.createdAt || r.CreatedAt || "",
            typeKey: r.typeKey || r.TypeKey || "",
            typeName: r.typeName || r.TypeName || "",
            title: r.title || r.Title || "",
            description: r.description || r.Description || "",
            badgeText: r.badgeText || r.BadgeText || ""
        };
    }

    function initializeStateFromBootstrap() {
        var bootstrap = window.__wwHistoryBootstrap || {};
        state.records = (bootstrap.records || []).map(normalizeRecord);
    }

    function normalizeText(text) {
        return (text || "").toString().trim().toLowerCase();
    }

    function matchQuery(record, query) {
        if (!query) {
            return true;
        }

        var q = normalizeText(query);
        var haystacks = [
            record.typeName,
            record.title,
            record.description,
            record.badgeText
        ].map(normalizeText);

        return haystacks.some(function (t) { return t.indexOf(q) >= 0; });
    }

    function matchFilter(record, filter) {
        if (filter === "all") {
            return true;
        }
        return record.typeKey === filter;
    }

    function queryVisibleRecords() {
        return state.records.filter(function (record) {
            return matchFilter(record, state.filter) && matchQuery(record, state.query);
        });
    }

    function setActiveFilterButton() {
        document.querySelectorAll(".js-history-filter").forEach(function (el) {
            var isActive = (el.getAttribute("data-filter") || "") === state.filter;
            isActive ? el.classList.add("active") : el.classList.remove("active");
        });
    }

    function updateSubtitle() {
        var subtitle = document.getElementById("ww-history-subtitle");
        if (!subtitle) {
            return;
        }

        var label = "全部";
        var activeButton = document.querySelector(".js-history-filter.active");
        if (activeButton) {
            var text = (activeButton.textContent || "").trim();
            label = text.length > 0 ? text.replace(/\s+/g, " ") : label;
        }

        subtitle.textContent = state.query
            ? "以「" + label + "」顯示，並套用搜尋：" + state.query
            : "以「" + label + "」顯示";
    }

    function updateCounts() {
        var counts = {
            all: state.records.length
        };

        state.records.forEach(function (r) {
            var key = r.typeKey || "";
            counts[key] = (counts[key] || 0) + 1;
        });

        setText("ww-history-count-all", counts.all || 0);
        setText("ww-history-count-import", counts.import || 0);
        setText("ww-history-count-extract", counts.extract || 0);
        setText("ww-history-count-ai_outfit", counts.ai_outfit || 0);
        setText("ww-history-count-save", counts.save || 0);
    }

    function setText(id, value) {
        var el = document.getElementById(id);
        if (el) {
            el.textContent = String(value);
        }
    }

    function formatTimeOnly(value) {
        try {
            var date = new Date(value);
            if (isNaN(date.getTime())) {
                return "";
            }
            var hh = String(date.getHours()).padStart(2, "0");
            var mi = String(date.getMinutes()).padStart(2, "0");
            return hh + ":" + mi;
        } catch {
            return "";
        }
    }

    function formatDateOnly(value) {
        try {
            var date = new Date(value);
            if (isNaN(date.getTime())) {
                return "";
            }
            var yyyy = date.getFullYear();
            var mm = String(date.getMonth() + 1).padStart(2, "0");
            var dd = String(date.getDate()).padStart(2, "0");
            return yyyy + "-" + mm + "-" + dd;
        } catch {
            return "";
        }
    }

    function escapeHtml(text) {
        return (text || "").toString()
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/\"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function queryTypeIconName(typeKey) {
        if (typeKey === "import") { return "image-plus"; }
        if (typeKey === "extract") { return "wand-2"; }
        if (typeKey === "ai_outfit") { return "sparkles"; }
        if (typeKey === "save") { return "bookmark"; }
        return "dot";
    }

    function querySelectedDayKey() {
        if (state.selectedDay) {
            return state.selectedDay;
        }
        var now = new Date();
        return formatDateOnly(now.toISOString());
    }

    function queryDayRecords(dayKey) {
        return queryVisibleRecords()
            .filter(function (r) { return formatDateOnly(r.createdAt) === dayKey; })
            .slice()
            .sort(function (a, b) { return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(); });
    }

    function queryDayCounts(records) {
        var counts = { import: 0, extract: 0, ai_outfit: 0, save: 0, all: records.length };
        records.forEach(function (r) {
            if (counts[r.typeKey] !== undefined) {
                counts[r.typeKey]++;
            }
        });
        return counts;
    }

    function buildGroupsForDay(records) {
        // 以「使用者自己回看」為核心：把當日事件收斂成少數「操作卡」。
        var groups = [];
        var used = {};

        function markUsed(id) { used[id] = true; }
        function isUsed(id) { return used[id] === true; }

        var importExtract = records.filter(function (r) { return r.typeKey === "import" || r.typeKey === "extract"; });
        if (importExtract.length > 0) {
            importExtract.forEach(function (r) { markUsed(r.id); });
            groups.push({
                id: "g-import-extract",
                typeKey: "import",
                title: "衣櫃更新",
                pills: [
                    "匯入 " + String(importExtract.filter(function (r) { return r.typeKey === "import"; }).length),
                    "提取 " + String(importExtract.filter(function (r) { return r.typeKey === "extract"; }).length)
                ],
                records: importExtract
            });
        }

        var aiRecords = records.filter(function (r) { return r.typeKey === "ai_outfit"; });
        aiRecords.forEach(function (ai) {
            if (isUsed(ai.id)) {
                return;
            }

            var aiTime = new Date(ai.createdAt).getTime();
            var relatedSaves = records.filter(function (r) {
                if (r.typeKey !== "save" || isUsed(r.id)) {
                    return false;
                }
                var t = new Date(r.createdAt).getTime();
                return Math.abs(t - aiTime) <= (60 * 60 * 1000);
            });

            markUsed(ai.id);
            relatedSaves.forEach(function (r) { markUsed(r.id); });

            groups.push({
                id: "g-ai-" + ai.id,
                typeKey: "ai_outfit",
                title: ai.title || "AI 生成穿搭",
                pills: [
                    "生成 1",
                    relatedSaves.length > 0 ? ("保存 " + String(relatedSaves.length)) : "未保存"
                ],
                records: [ai].concat(relatedSaves)
            });
        });

        var leftover = records.filter(function (r) { return !isUsed(r.id); });
        leftover.forEach(function (r) {
            markUsed(r.id);
            groups.push({
                id: "g-" + r.id,
                typeKey: r.typeKey || "other",
                title: r.title || "活動",
                pills: [r.typeName || "活動"],
                records: [r]
            });
        });

        groups.sort(function (a, b) {
            var ta = Math.max.apply(null, a.records.map(function (r) { return new Date(r.createdAt).getTime(); }));
            var tb = Math.max.apply(null, b.records.map(function (r) { return new Date(r.createdAt).getTime(); }));
            return tb - ta;
        });

        return groups;
    }

    function queryGroupTimeText(group) {
        var times = group.records.map(function (r) { return new Date(r.createdAt).getTime(); }).filter(function (t) { return !isNaN(t); });
        if (times.length === 0) {
            return "";
        }
        var max = Math.max.apply(null, times);
        return formatTimeOnly(new Date(max).toISOString());
    }

    function buildGroupCard(group) {
        var isActive = state.activeGroupId === group.id;
        var cls = isActive ? "ww-group-card active" : "ww-group-card";

        var pillsHtml = (group.pills || []).map(function (t) {
            return "<span class=\"ww-group-pill\">" + escapeHtml(t) + "</span>";
        }).join("");

        return [
            "<div class=\"" + cls + "\" data-group-id=\"" + escapeHtml(group.id) + "\" onclick=\"wwHistoryPickGroup('" + escapeHtml(group.id) + "')\">",
            "  <div class=\"ww-group-top\">",
            "    <div class=\"ww-group-topl\">",
            "      <span class=\"ww-group-ico\"><i data-lucide=\"" + queryTypeIconName(group.typeKey) + "\" class=\"w-[14px] h-[14px]\"></i></span>",
            "      <div class=\"ww-group-title\">" + escapeHtml(group.title) + "</div>",
            "    </div>",
            "    <div class=\"ww-group-time\">" + escapeHtml(queryGroupTimeText(group)) + "</div>",
            "  </div>",
            "  <div class=\"ww-group-body\">",
            "    <div class=\"ww-group-row\">" + pillsHtml + "</div>",
            "  </div>",
            "</div>"
        ].join("");
    }

    function queryActiveGroup(dayKey) {
        var records = queryDayRecords(dayKey);
        var groups = buildGroupsForDay(records);
        return groups.find(function (g) { return g.id === state.activeGroupId; }) || null;
    }

    function buildDetailHtml(group, dayKey) {
        if (!group) {
            return "";
        }

        var typeKey = group.typeKey;

        if (typeKey === "ai_outfit") {
            var ai = group.records.find(function (r) { return r.typeKey === "ai_outfit"; }) || group.records[0];
            var userText = "今天想要通勤但會下雨，風格俐落一點。";
            var aiText = (ai && ai.description) ? ai.description : "已產生穿搭建議與原因說明。";

            return [
                "<div class=\"ww-detail-hero\">",
                "  <span class=\"ww-detail-badge\">AI 生成</span>",
                "  <div class=\"text-[12px] text-[#A89F96]\">產出圖片（示意）</div>",
                "</div>",
                "<div class=\"ww-detail-actions\">",
                "  <button type=\"button\" class=\"ww-detail-btn\"><i data-lucide=\"download\" class=\"w-[14px] h-[14px]\"></i>下載</button>",
                "  <button type=\"button\" class=\"ww-detail-btn\"><i data-lucide=\"copy\" class=\"w-[14px] h-[14px]\"></i>複製</button>",
                "  <button type=\"button\" class=\"ww-detail-btn pri\"><i data-lucide=\"rotate-ccw\" class=\"w-[14px] h-[14px]\"></i>再生成</button>",
                "</div>",
                "<div class=\"ww-detail-section\">",
                "  <div class=\"ww-detail-section-hdr\">",
                "    <div class=\"ww-detail-section-title\">對話</div>",
                "    <div class=\"text-[10px] text-[#A89F96]\">" + escapeHtml(dayKey) + "</div>",
                "  </div>",
                "  <div class=\"ww-detail-section-body\">",
                "    <div class=\"ww-chat\">",
                "      <div class=\"ww-bubble user\">" + escapeHtml(userText) + "</div>",
                "      <div class=\"ww-bubble ai\">" + escapeHtml(aiText) + "</div>",
                "    </div>",
                "  </div>",
                "</div>",
                "<div class=\"ww-detail-section\">",
                "  <div class=\"ww-detail-section-hdr\">",
                "    <div class=\"ww-detail-section-title\">產出摘要</div>",
                "    <div class=\"text-[10px] text-[#A89F96]\">建議（示意）</div>",
                "  </div>",
                "  <div class=\"ww-detail-section-body\">",
                "    <div class=\"ww-kv\">",
                "      <div class=\"ww-kv-item\"><div class=\"ww-kv-k\">上衣</div><div class=\"ww-kv-v\">防潑水外套</div></div>",
                "      <div class=\"ww-kv-item\"><div class=\"ww-kv-k\">下身</div><div class=\"ww-kv-v\">深色長褲</div></div>",
                "      <div class=\"ww-kv-item\"><div class=\"ww-kv-k\">鞋子</div><div class=\"ww-kv-v\">好走防滑鞋</div></div>",
                "      <div class=\"ww-kv-item\"><div class=\"ww-kv-k\">重點</div><div class=\"ww-kv-v\">保持乾爽</div></div>",
                "    </div>",
                "  </div>",
                "</div>"
            ].join("");
        }

        if (typeKey === "import" || typeKey === "extract") {
            var importCount = group.records.filter(function (r) { return r.typeKey === "import"; }).length;
            var extractCount = group.records.filter(function (r) { return r.typeKey === "extract"; }).length;

            return [
                "<div class=\"ww-detail-hero\">",
                "  <span class=\"ww-detail-badge\">衣櫃更新</span>",
                "  <div class=\"text-[12px] text-[#A89F96]\">匯入/提取（示意）</div>",
                "</div>",
                "<div class=\"ww-detail-actions\">",
                "  <button type=\"button\" class=\"ww-detail-btn\"><i data-lucide=\"download\" class=\"w-[14px] h-[14px]\"></i>下載原始檔</button>",
                "  <button type=\"button\" class=\"ww-detail-btn\"><i data-lucide=\"shirt\" class=\"w-[14px] h-[14px]\"></i>前往衣櫃</button>",
                "  <button type=\"button\" class=\"ww-detail-btn pri\"><i data-lucide=\"wand-2\" class=\"w-[14px] h-[14px]\"></i>再提取</button>",
                "</div>",
                "<div class=\"ww-detail-section\">",
                "  <div class=\"ww-detail-section-hdr\">",
                "    <div class=\"ww-detail-section-title\">批次摘要</div>",
                "    <div class=\"text-[10px] text-[#A89F96]\">" + escapeHtml(dayKey) + "</div>",
                "  </div>",
                "  <div class=\"ww-detail-section-body\">",
                "    <div class=\"ww-kv\">",
                "      <div class=\"ww-kv-item\"><div class=\"ww-kv-k\">匯入</div><div class=\"ww-kv-v\">" + String(importCount) + " 次</div></div>",
                "      <div class=\"ww-kv-item\"><div class=\"ww-kv-k\">提取</div><div class=\"ww-kv-v\">" + String(extractCount) + " 次</div></div>",
                "      <div class=\"ww-kv-item\"><div class=\"ww-kv-k\">原始檔</div><div class=\"ww-kv-v\">照片 5 張</div></div>",
                "      <div class=\"ww-kv-item\"><div class=\"ww-kv-k\">結果</div><div class=\"ww-kv-v\">待確認 2 件</div></div>",
                "    </div>",
                "  </div>",
                "</div>",
                "<div class=\"ww-detail-section\">",
                "  <div class=\"ww-detail-section-hdr\">",
                "    <div class=\"ww-detail-section-title\">提取前後</div>",
                "    <div class=\"text-[10px] text-[#A89F96]\">示意</div>",
                "  </div>",
                "  <div class=\"ww-detail-section-body\">",
                "    <div class=\"ww-kv\">",
                "      <div class=\"ww-kv-item\"><div class=\"ww-kv-k\">原始檔</div><div class=\"ww-kv-v\">IMG_1021.jpg</div></div>",
                "      <div class=\"ww-kv-item\"><div class=\"ww-kv-k\">服裝</div><div class=\"ww-kv-v\">外套 / 深藍 / 秋冬</div></div>",
                "      <div class=\"ww-kv-item\"><div class=\"ww-kv-k\">原始檔</div><div class=\"ww-kv-v\">IMG_1022.jpg</div></div>",
                "      <div class=\"ww-kv-item\"><div class=\"ww-kv-k\">服裝</div><div class=\"ww-kv-v\">鞋子 / 白 / 四季</div></div>",
                "    </div>",
                "  </div>",
                "</div>"
            ].join("");
        }

        var first = group.records[0];
        return [
            "<div class=\"ww-detail-section\">",
            "  <div class=\"ww-detail-section-hdr\">",
            "    <div class=\"ww-detail-section-title\">詳細資訊</div>",
            "    <div class=\"text-[10px] text-[#A89F96]\">" + escapeHtml(dayKey) + "</div>",
            "  </div>",
            "  <div class=\"ww-detail-section-body\">",
            "    <div class=\"text-[11px] text-[#6B6460] leading-[1.6]\">" + escapeHtml((first && first.description) || "") + "</div>",
            "  </div>",
            "</div>"
        ].join("");
    }

    function renderMiddlePanel() {
        var titleEl = document.getElementById("ww-history-day-title");
        var subEl = document.getElementById("ww-history-day-sub");
        var groupsEl = document.getElementById("ww-history-groups");

        if (!titleEl || !subEl || !groupsEl) {
            return;
        }

        var dayKey = querySelectedDayKey();
        var records = queryDayRecords(dayKey);
        var counts = queryDayCounts(records);
        var groups = buildGroupsForDay(records);

        titleEl.textContent = dayKey;
        subEl.textContent = "共 " + String(counts.all) + " 筆 · 匯入 " + String(counts.import) + " · 提取 " + String(counts.extract) + " · 生成 " + String(counts.ai_outfit) + " · 保存 " + String(counts.save);

        groupsEl.innerHTML = groups.length > 0
            ? groups.map(buildGroupCard).join("")
            : "<div class=\"text-[11px] text-[#A89F96]\">此日期沒有紀錄</div>";

        window.lucide ? lucide.createIcons() : null;

        if (!state.activeGroupId && groups.length > 0) {
            state.activeGroupId = groups[0].id;
            renderMiddlePanel();
            renderDetailPanel();
        }
    }

    function renderDetailPanel() {
        var emptyEl = document.getElementById("ww-history-detail-empty");
        var detailEl = document.getElementById("ww-history-detail");

        if (!emptyEl || !detailEl) {
            return;
        }

        var dayKey = querySelectedDayKey();
        var group = queryActiveGroup(dayKey);

        if (!group) {
            emptyEl.classList.remove("hidden");
            detailEl.classList.add("hidden");
            detailEl.innerHTML = "";
            return;
        }

        emptyEl.classList.add("hidden");
        detailEl.classList.remove("hidden");
        detailEl.innerHTML = buildDetailHtml(group, dayKey);

        window.lucide ? lucide.createIcons() : null;
    }

    function renderAll() {
        updateCounts();
        renderMiddlePanel();
        renderDetailPanel();
    }

    function bindQueryInput() {
        var queryEl = document.getElementById("ww-history-query");
        if (!queryEl) {
            return;
        }

        queryEl.addEventListener("input", function () {
            state.query = queryEl.value || "";
            updateSubtitle();
            state.activeGroupId = null;
            renderAll();
        });
    }

    function bindDateInput() {
        var dateEl = document.getElementById("ww-history-date");
        if (!dateEl) {
            return;
        }

        dateEl.addEventListener("change", function () {
            var value = dateEl.value || "";
            state.selectedDay = value ? value : null;
            state.activeGroupId = null;
            renderAll();
        });
    }

    function pickQuickDate(kind) {
        var now = new Date();
        if (kind === "yesterday") {
            now = new Date(now.getFullYear(), now.getMonth(), now.getDate() - 1);
        }
        state.selectedDay = formatDateOnly(now.toISOString());
        state.activeGroupId = null;

        var dateEl = document.getElementById("ww-history-date");
        if (dateEl) {
            dateEl.value = state.selectedDay;
        }

        renderAll();
    }

    function pickFilter(filter) {
        state.filter = filter;
        state.activeGroupId = null;
        setActiveFilterButton();
        updateSubtitle();
        renderAll();
    }

    function clearQuery() {
        state.query = "";
        var queryEl = document.getElementById("ww-history-query");
        if (queryEl) {
            queryEl.value = "";
        }
        updateSubtitle();
        state.activeGroupId = null;
        renderAll();
    }

    function reset() {
        state.filter = "all";
        state.query = "";
        state.selectedDay = querySelectedDayKey();
        state.activeGroupId = null;

        var queryEl = document.getElementById("ww-history-query");
        if (queryEl) {
            queryEl.value = "";
        }

        var dateEl = document.getElementById("ww-history-date");
        if (dateEl) {
            dateEl.value = state.selectedDay;
        }

        setActiveFilterButton();
        updateSubtitle();
        renderAll();
    }

    function pickGroup(groupId) {
        state.activeGroupId = groupId;
        renderAll();
    }

    function initialize() {
        initializeStateFromBootstrap();
        bindDateInput();
        bindQueryInput();

        var dateEl = document.getElementById("ww-history-date");
        if (dateEl) {
            dateEl.value = querySelectedDayKey();
            state.selectedDay = dateEl.value;
        }

        updateCounts();
        setActiveFilterButton();
        updateSubtitle();
        renderAll();
        window.lucide ? lucide.createIcons() : null;
    }

    window.wwHistoryPickFilter = pickFilter;
    window.wwHistoryClearQuery = clearQuery;
    window.wwHistoryReset = reset;
    window.wwHistoryPickGroup = pickGroup;
    window.wwHistoryPickQuickDate = pickQuickDate;

    document.readyState === "loading"
        ? document.addEventListener("DOMContentLoaded", initialize)
        : initialize();

})();

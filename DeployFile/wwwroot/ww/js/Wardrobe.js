(function () {

    var state = {
        filter: "all",
        query: "",
        view: "grid",
        categories: [],
        items: [],
        imports: []
    };

    function normalizeCategory(c) {
        if (!c) {
            return { key: "", name: "", count: 0 };
        }

        return {
            key: c.key || c.Key || "",
            name: c.name || c.Name || "",
            count: c.count || c.Count || 0
        };
    }

    function normalizeItem(i) {
        if (!i) {
            return {
                id: "",
                name: "",
                type: "",
                categoryKey: "",
                categoryName: "",
                primaryColor: "",
                season: "",
                occasion: "",
                tags: [],
                isPendingReview: false,
                isUncategorized: false,
                imageDataUrl: null
            };
        }

        return {
            id: i.id || i.Id || "",
            name: i.name || i.Name || "",
            type: i.type || i.Type || "",
            categoryKey: i.categoryKey || i.CategoryKey || "",
            categoryName: i.categoryName || i.CategoryName || "",
            primaryColor: i.primaryColor || i.PrimaryColor || "",
            season: i.season || i.Season || "",
            occasion: i.occasion || i.Occasion || "",
            tags: i.tags || i.Tags || [],
            isPendingReview: (i.isPendingReview !== undefined ? i.isPendingReview : i.IsPendingReview) === true,
            isUncategorized: (i.isUncategorized !== undefined ? i.isUncategorized : i.IsUncategorized) === true,
            imageDataUrl: i.imageDataUrl || i.ImageDataUrl || null
        };
    }

    function initializeStateFromBootstrap() {
        var bootstrap = window.__wwWardrobeBootstrap || {};
        state.categories = (bootstrap.categories || []).map(normalizeCategory);
        state.items = (bootstrap.items || []).map(normalizeItem);
    }

    function normalizeText(text) {
        return (text || "").toString().trim().toLowerCase();
    }

    function matchQuery(item, query) {
        if (!query) {
            return true;
        }

        var q = normalizeText(query);
        var haystacks = [
            item.name,
            item.type,
            item.categoryName,
            item.primaryColor,
            item.season,
            item.occasion,
            (item.tags || []).join(" ")
        ].map(normalizeText);

        return haystacks.some(function (t) { return t.indexOf(q) >= 0; });
    }

    function matchFilter(item, filter) {
        if (filter === "all") {
            return true;
        }

        if (filter === "pending") {
            return item.isPendingReview === true;
        }

        if (filter === "uncategorized") {
            return item.isUncategorized === true;
        }

        if (filter.indexOf("category:") === 0) {
            var key = filter.substring("category:".length);
            return item.categoryKey === key && item.isPendingReview !== true;
        }

        return true;
    }

    function queryVisibleItems() {
        return state.items.filter(function (item) {
            return matchFilter(item, state.filter) && matchQuery(item, state.query);
        });
    }

    function setActiveFilterButton() {
        document.querySelectorAll(".js-wardrobe-filter").forEach(function (el) {
            var isActive = (el.getAttribute("data-filter") || "") === state.filter;
            isActive ? el.classList.add("active") : el.classList.remove("active");
        });
    }

    function updateSubtitle() {
        var subtitle = document.getElementById("ww-result-subtitle");
        if (!subtitle) {
            return;
        }

        var label = "全部";

        if (state.filter === "pending") {
            label = "待確認";
        } else if (state.filter === "uncategorized") {
            label = "未分類";
        } else if (state.filter.indexOf("category:") === 0) {
            var key = state.filter.substring("category:".length);
            var category = state.categories.find(function (c) { return c.key === key; });
            label = category ? category.name : "分類";
        }

        subtitle.textContent = state.query
            ? "以「" + label + "」顯示，並套用查詢：" + state.query
            : "以「" + label + "」顯示";
    }

    function updateCounts() {
        var allCount = state.items.filter(function (i) { return i.isPendingReview !== true; }).length;
        var pendingCount = state.items.filter(function (i) { return i.isPendingReview === true; }).length;
        var uncategorizedCount = state.items.filter(function (i) { return i.isUncategorized === true && i.isPendingReview !== true; }).length;

        var allEl = document.getElementById("ww-count-all");
        var pendingEl = document.getElementById("ww-count-pending");
        var uncategorizedEl = document.getElementById("ww-count-uncategorized");

        if (allEl) { allEl.textContent = String(allCount); }
        if (pendingEl) { pendingEl.textContent = String(pendingCount); }
        if (uncategorizedEl) { uncategorizedEl.textContent = String(uncategorizedCount); }

        state.categories.forEach(function (category) {
            var count = state.items.filter(function (i) {
                return i.categoryKey === category.key && i.isPendingReview !== true;
            }).length;

            var categoryCountEl = document.getElementById("ww-count-category-" + category.key);
            if (categoryCountEl) {
                categoryCountEl.textContent = String(count);
            }
        });
    }

    function buildCardMeta(item) {
        var parts = [];
        if (item.type) { parts.push(item.type); }
        if (item.primaryColor) { parts.push(item.primaryColor); }
        if (item.season && item.season !== "未知") { parts.push(item.season); }
        if (item.occasion && item.occasion !== "未知") { parts.push(item.occasion); }
        return parts;
    }

    function buildCardTag(item) {
        if (item.isPendingReview) {
            return "待確認";
        }
        if (item.isUncategorized) {
            return "未分類";
        }
        return item.categoryName || "服裝";
    }

    function buildItemCard(item) {
        var meta = buildCardMeta(item);
        var metaHtml = meta.map(function (m) { return "<span>" + escapeHtml(m) + "</span>"; }).join("");

        var imageHtml = item.imageDataUrl
            ? "<img alt=\"\" src=\"" + item.imageDataUrl + "\" />"
            : "<i data-lucide=\"shirt\" class=\"w-[26px] h-[26px] text-[#C4BAB0]\"></i>";

        var actionsHtml = "";
        if (item.isPendingReview) {
            actionsHtml = [
                "<div class=\"ww-card-actions\">",
                "<button type=\"button\" class=\"ww-mini-btn\" onclick=\"wwWardrobeMarkUncategorized('" + item.id + "')\">先放未分類</button>",
                "<button type=\"button\" class=\"ww-mini-btn pri\" onclick=\"wwWardrobeConfirmItem('" + item.id + "')\">確認入庫</button>",
                "</div>"
            ].join("");
        }

        return [
            "<div class=\"ww-card\" data-id=\"" + escapeHtml(item.id) + "\">",
            "  <div class=\"ww-card-top\">",
            "    <span class=\"ww-card-tag\">" + escapeHtml(buildCardTag(item)) + "</span>",
            "    " + imageHtml,
            "  </div>",
            "  <div class=\"ww-card-body\">",
            "    <div class=\"ww-card-name\">" + escapeHtml(item.name || "未命名") + "</div>",
            "    <div class=\"ww-card-meta\">" + metaHtml + "</div>",
            actionsHtml,
            "  </div>",
            "</div>"
        ].join("");
    }

    function renderGrid() {
        var gridEl = document.getElementById("ww-wardrobe-grid");
        var emptyEl = document.getElementById("ww-wardrobe-empty");

        if (!gridEl || !emptyEl) {
            return;
        }

        var items = queryVisibleItems();
        gridEl.innerHTML = items.map(buildItemCard).join("");

        if (items.length === 0) {
            emptyEl.classList.remove("hidden");
        } else {
            emptyEl.classList.add("hidden");
        }

        // 動態渲染後需要再建立圖示
        window.lucide ? lucide.createIcons() : null;
    }

    function escapeHtml(text) {
        return (text || "").toString()
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/\"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function renderImportQueue() {
        var queueEl = document.getElementById("ww-import-queue");
        var extractBtn = document.getElementById("ww-extract-btn");

        if (!queueEl || !extractBtn) {
            return;
        }

        queueEl.innerHTML = state.imports.map(function (imp, idx) {
            return [
                "<div class=\"ww-queue-item\">",
                "  <div class=\"ww-queue-left\">",
                "    <span class=\"ww-queue-dot\"></span>",
                "    <div class=\"ww-queue-name\">" + escapeHtml(imp.name) + "</div>",
                "  </div>",
                "  <button type=\"button\" class=\"ww-queue-remove\" onclick=\"wwWardrobeRemoveImport(" + idx + ")\">移除</button>",
                "</div>"
            ].join("");
        }).join("");

        extractBtn.disabled = state.imports.length === 0;
    }

    function bindImportInput() {
        var inputEl = document.getElementById("ww-wardrobe-import-input");
        if (!inputEl) {
            return;
        }

        inputEl.addEventListener("change", function () {
            var files = Array.from(inputEl.files || []);
            if (files.length === 0) {
                return;
            }

            files.forEach(function (file) {
                state.imports.push({
                    name: file.name,
                    file: file
                });
            });

            inputEl.value = "";
            renderImportQueue();
        });
    }

    function toDataUrl(file) {
        return new Promise(function (resolve) {
            var reader = new FileReader();
            reader.onload = function () { resolve(reader.result); };
            reader.onerror = function () { resolve(null); };
            reader.readAsDataURL(file);
        });
    }

    function guessExtractResult(fileName) {
        var name = (fileName || "").toLowerCase();
        if (name.indexOf("shoe") >= 0 || name.indexOf("鞋") >= 0) {
            return { type: "鞋子", categoryKey: "shoes", categoryName: "鞋子" };
        }
        if (name.indexOf("coat") >= 0 || name.indexOf("jacket") >= 0 || name.indexOf("外套") >= 0) {
            return { type: "外套", categoryKey: "outerwear", categoryName: "外套" };
        }
        if (name.indexOf("pant") >= 0 || name.indexOf("jean") >= 0 || name.indexOf("褲") >= 0) {
            return { type: "長褲", categoryKey: "bottoms", categoryName: "下身" };
        }
        if (name.indexOf("shirt") >= 0 || name.indexOf("tee") >= 0 || name.indexOf("上衣") >= 0) {
            return { type: "上衣", categoryKey: "tops", categoryName: "上衣" };
        }

        return { type: "待提取", categoryKey: "", categoryName: "待確認" };
    }

    async function extractImports() {
        var extractBtn = document.getElementById("ww-extract-btn");
        if (extractBtn) {
            extractBtn.disabled = true;
            extractBtn.textContent = "提取中...";
        }

        for (var i = 0; i < state.imports.length; i++) {
            var imp = state.imports[i];
            var imageDataUrl = await toDataUrl(imp.file);
            var guess = guessExtractResult(imp.name);
            var nowId = "it-u-" + Date.now().toString(36) + "-" + i;

            state.items.unshift({
                id: nowId,
                name: "待確認：" + (imp.name || "新匯入照片"),
                type: guess.type,
                categoryKey: guess.categoryKey,
                categoryName: guess.categoryName,
                primaryColor: "未知",
                season: "未知",
                occasion: "未知",
                tags: [],
                isPendingReview: true,
                isUncategorized: false,
                imageDataUrl: imageDataUrl
            });
        }

        state.imports = [];

        if (extractBtn) {
            extractBtn.textContent = "開始提取";
        }

        state.filter = "pending";
        state.query = "";

        var queryEl = document.getElementById("ww-wardrobe-query");
        if (queryEl) {
            queryEl.value = "";
        }

        renderImportQueue();
        updateCounts();
        setActiveFilterButton();
        updateSubtitle();
        renderGrid();
    }

    function confirmItem(id) {
        var item = state.items.find(function (i) { return i.id === id; });
        if (!item) {
            return;
        }

        item.isPendingReview = false;

        if (!item.categoryKey) {
            item.isUncategorized = true;
            item.categoryName = "未分類";
        }

        updateCounts();
        renderGrid();
    }

    function markUncategorized(id) {
        var item = state.items.find(function (i) { return i.id === id; });
        if (!item) {
            return;
        }

        item.isPendingReview = false;
        item.isUncategorized = true;
        item.categoryKey = "";
        item.categoryName = "未分類";

        updateCounts();
        renderGrid();
    }

    function pickFilter(filter) {
        state.filter = filter;
        setActiveFilterButton();
        updateSubtitle();
        renderGrid();
    }

    function clearQuery() {
        state.query = "";
        var queryEl = document.getElementById("ww-wardrobe-query");
        if (queryEl) {
            queryEl.value = "";
        }
        updateSubtitle();
        renderGrid();
    }

    function applyQuery(text) {
        state.query = (text || "").toString();
        var queryEl = document.getElementById("ww-wardrobe-query");
        if (queryEl) {
            queryEl.value = state.query;
        }
        updateSubtitle();
        renderGrid();
    }

    function reset() {
        state.filter = "all";
        state.query = "";

        var queryEl = document.getElementById("ww-wardrobe-query");
        if (queryEl) {
            queryEl.value = "";
        }

        setActiveFilterButton();
        updateSubtitle();
        renderGrid();
    }

    function removeImport(idx) {
        state.imports.splice(idx, 1);
        renderImportQueue();
    }

    function toggleView() {
        // 先保留擴充點：目前只有 grid，但維持工作台模式即可。
        state.view = state.view === "grid" ? "grid" : "grid";
    }

    function bindQueryInput() {
        var queryEl = document.getElementById("ww-wardrobe-query");
        if (!queryEl) {
            return;
        }

        queryEl.addEventListener("input", function () {
            state.query = queryEl.value || "";
            updateSubtitle();
            renderGrid();
        });
    }

    function initialize() {
        initializeStateFromBootstrap();
        bindQueryInput();
        bindImportInput();

        updateCounts();
        setActiveFilterButton();
        updateSubtitle();
        renderImportQueue();
        renderGrid();
    }

    // 導出給 onclick 使用
    window.wwWardrobePickFilter = pickFilter;
    window.wwWardrobeClearQuery = clearQuery;
    window.wwWardrobeApplyQuery = applyQuery;
    window.wwWardrobeExtract = extractImports;
    window.wwWardrobeRemoveImport = removeImport;
    window.wwWardrobeConfirmItem = confirmItem;
    window.wwWardrobeMarkUncategorized = markUncategorized;
    window.wwWardrobeReset = reset;
    window.wwWardrobeToggleView = toggleView;

    document.readyState === "loading"
        ? document.addEventListener("DOMContentLoaded", initialize)
        : initialize();

})();

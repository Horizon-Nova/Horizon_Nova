(function () {

    function filterCat(button, cat) {
        document.querySelectorAll(".bubble").forEach(function (b) { b.classList.remove("active"); });
        button.classList.add("active");

        var cards = document.querySelectorAll(".ccard");
        var count = 0;
        cards.forEach(function (card) {
            if (cat === "all" || card.dataset.cat === cat) {
                card.style.display = "";
                count++;
            } else {
                card.style.display = "none";
            }
        });

        var labels = {
            all: "All",
            tops: "Tops",
            bottoms: "Bottoms",
            outerwear: "Outerwear",
            shoes: "Shoes",
            accessories: "Accessories",
            unconfirmed: "Review"
        };

        var gridTitle = document.getElementById("grid-title");
        var gridSub = document.getElementById("grid-sub");
        if (gridTitle) {
            gridTitle.textContent = labels[cat] || "All";
        }
        if (gridSub) {
            gridSub.textContent = "Showing " + count + " items";
        }
    }

    function addNewCards() {
        var grid = document.getElementById("clothes-grid");
        if (!grid) {
            return;
        }

        var newItems = [
            { cat: "tops", bg: "#e8eef4,#d8e4ee", name: "Navy Stripe Tee", tags: ["Navy", "Casual"], icon: "top" },
            { cat: "bottoms", bg: "#3a4a5a,#2a3a4a", name: "Dark Denim", tags: ["Indigo", "Casual"], icon: "bottom" },
            { cat: "outerwear", bg: "#8a9a8a,#7a8a7a", name: "Olive Jacket", tags: ["Olive", "Layering"], icon: "jacket" },
            { cat: "accessories", bg: "#c8b070,#b8a060", name: "Brown Belt", tags: ["Brown", "Classic"], icon: "acc" }
        ];

        var iconPaths = {
            top: '<path d="M20.38 3.46L16 2a4 4 0 01-8 0L3.62 3.46a2 2 0 00-1.34 2.23l.58 3.57a1 1 0 00.99.84H6v10c0 1.1.9 2 2 2h8a2 2 0 002-2V10h2.15a1 1 0 00.99-.84l.58-3.57a2 2 0 00-1.34-2.23z"/>',
            bottom: '<path d="M6 2l-3 8h5v12h8V10h5l-3-8H6z"/>',
            jacket: '<path d="M3 5c0-1 .7-2 2-2l4 3 3-3 3 3 4-3c1.3 0 2 1 2 2v14a2 2 0 01-2 2H5a2 2 0 01-2-2V5z"/>',
            acc: '<rect x="2" y="7" width="20" height="14" rx="2"/><path d="M16 7V5a2 2 0 00-2-2h-4a2 2 0 00-2 2v2"/>'
        };

        newItems.forEach(function (item, index) {
            setTimeout(function () {
                var catTagLabel = "Item";
                if (item.cat === "tops") {
                    catTagLabel = "Top";
                } else if (item.cat === "bottoms") {
                    catTagLabel = "Bottom";
                } else if (item.cat === "outerwear") {
                    catTagLabel = "Outerwear";
                } else if (item.cat === "shoes") {
                    catTagLabel = "Shoes";
                } else if (item.cat === "accessories") {
                    catTagLabel = "Accessory";
                }

                var card = document.createElement("div");
                card.className = "ccard new";
                card.dataset.cat = item.cat;
                card.innerHTML =
                    '<div class="ccard-img" style="background:linear-gradient(145deg,' + item.bg + ');">' +
                    '  <svg viewBox="0 0 24 24">' + (iconPaths[item.icon] || "") + "</svg>" +
                    '  <div class="ccard-cat-tag">' + catTagLabel + "</div>" +
                    '  <div class="ccard-del" onclick="event.stopPropagation()"><svg viewBox="0 0 24 24"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg></div>' +
                    "</div>" +
                    '<div class="ccard-body">' +
                    '  <div class="ccard-name">' + item.name + "</div>" +
                    '  <div class="ccard-tags">' + item.tags.map(function (t) { return '<span class="ctag">' + t + "</span>"; }).join("") + "</div>" +
                    "</div>";
                grid.appendChild(card);
            }, index * 300);
        });
    }

    function startUpload() {
        var idle = document.getElementById("uz-idle");
        var scanning = document.getElementById("uz-scanning");
        if (!idle || !scanning) {
            return;
        }

        idle.style.display = "none";
        scanning.style.display = "flex";

        var steps = [
            { dot: "ss1", txt: "st1", dl: 500 },
            { dot: "ss2", txt: "st2", dl: 1100 },
            { dot: "ss3", txt: "st3", dl: 1700 },
            { dot: "ss4", txt: "st4", dl: 2300 }
        ];

        steps.forEach(function (step) {
            setTimeout(function () {
                var dot = document.getElementById(step.dot);
                var txt = document.getElementById(step.txt);
                if (dot) {
                    dot.className = "ss-dot cur";
                }
                if (txt) {
                    txt.className = "ss-text cur";
                }
            }, step.dl);

            setTimeout(function () {
                var dot = document.getElementById(step.dot);
                var txt = document.getElementById(step.txt);
                if (dot) {
                    dot.className = "ss-dot done";
                }
                if (txt) {
                    txt.className = "ss-text done";
                }
            }, step.dl + 550);
        });

        setTimeout(function () {
            scanning.style.display = "none";
            var done = document.getElementById("uz-done");
            if (done) {
                done.style.display = "flex";
            }

            addNewCards();

            var statTotal = document.getElementById("stat-total");
            if (statTotal) {
                statTotal.textContent = (parseInt(statTotal.textContent || "0", 10) + 4).toString();
            }
        }, 3400);
    }

    function resetUpload() {
        var idle = document.getElementById("uz-idle");
        var done = document.getElementById("uz-done");
        if (done) {
            done.style.display = "none";
        }
        if (idle) {
            idle.style.display = "flex";
        }

        ["ss1", "ss2", "ss3", "ss4"].forEach(function (id) {
            var el = document.getElementById(id);
            if (el) {
                el.className = "ss-dot";
            }
        });
        ["st1", "st2", "st3", "st4"].forEach(function (id) {
            var el = document.getElementById(id);
            if (el) {
                el.className = "ss-text";
            }
        });
    }

    function handleDrop(e) {
        e.preventDefault();
        var zone = document.getElementById("upload-zone");
        if (zone) {
            zone.classList.remove("drag-over");
        }
        if (e.dataTransfer && e.dataTransfer.files && e.dataTransfer.files.length > 0) {
            startUpload();
        }
    }

    window.wwClosetFilterCat = filterCat;
    window.wwClosetStartUpload = startUpload;
    window.wwClosetResetUpload = resetUpload;
    window.wwClosetHandleDrop = handleDrop;

})();

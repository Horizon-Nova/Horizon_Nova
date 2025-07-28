// 初始化呼叫
document.addEventListener("DOMContentLoaded", function () {
    initSidebarToggle();
    initTreeCollapseToggle();
});

function initSidebarToggle() {
    const sidebar = document.getElementById("sidebarPanel");
    const toggleBtn = document.getElementById("btnToggleSidebar");
    const icon = toggleBtn.querySelector("i");

    let isCollapsed = false;

    toggleBtn.addEventListener("click", () => {
        if (!isCollapsed) {
            sidebar.style.display = "none";
            icon.classList.replace("fa-chevron-left", "fa-chevron-right");
        } else {
            sidebar.style.display = "block";
            icon.classList.replace("fa-chevron-right", "fa-chevron-left");
        }
        isCollapsed = !isCollapsed;
    });
}

function initTreeCollapseToggle() {
    const btnCollapseTree = document.getElementById("btnCollapseTree");
    const btnExpandTree = document.getElementById("btnExpand");
    const sidebarContent = document.getElementById("sidebarContent");

    btnCollapseTree.addEventListener("click", () => {
        gsap.to(sidebarContent, {
            height: 0,
            paddingTop: 0,
            paddingBottom: 0,
            opacity: 0,
            duration: 0.3,
            onComplete: () => {
                sidebarContent.style.display = "none";
                btnCollapseTree.classList.add("d-none");
                btnExpandTree.classList.remove("d-none");
            }
        });
    });

    btnExpandTree.addEventListener("click", () => {
        sidebarContent.style.display = "block";
        gsap.fromTo(sidebarContent,
            { height: 0, paddingTop: 0, paddingBottom: 0, opacity: 0 },
            {
                height: "auto",
                paddingTop: "0.5rem",
                paddingBottom: "0.5rem",
                opacity: 1,
                duration: 0.3,
                onComplete: () => {
                    btnExpandTree.classList.add("d-none");
                    btnCollapseTree.classList.remove("d-none");
                }
            }
        );
    });
}



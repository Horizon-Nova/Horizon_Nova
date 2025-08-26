document.addEventListener('DOMContentLoaded', () => {
    initIcons();
    initCounters();
});

// 初始化 Lucide Icons
function initIcons() {
    if (window.lucide && typeof lucide.createIcons === "function") {
        lucide.createIcons();
    }
}

// 初始化 GSAP Counter 動畫
function initCounters(selector = ".gsap-counter") {
    if (!window.gsap || !window.ScrollTrigger) return;
    gsap.registerPlugin(ScrollTrigger);

    document.querySelectorAll(selector).forEach(counter => {
        let target = parseFloat(counter.dataset.target) || 0;
        let suffix = counter.dataset.suffix || "";
        let prefix = counter.dataset.prefix || "";
        let duration = parseFloat(counter.dataset.duration) || 2;

        gsap.fromTo(counter,
            { innerText: 0 },
            {
                innerText: target,
                duration: duration,
                ease: "power1.out",
                snap: { innerText: 1 },
                scrollTrigger: {
                    trigger: counter,
                    start: "top 80%",
                    once: true
                },
                onUpdate: function () {
                    counter.innerText = prefix + Math.floor(counter.innerText) + suffix;
                }
            });
    });
}

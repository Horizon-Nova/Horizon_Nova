document.addEventListener("DOMContentLoaded", function () {
    TeamHeaderAnimator.init();

     LayoutBackgroundManager.loadParticlesJS();
     //LayoutBackgroundManager.loadVantaNet();
     //LayoutBackgroundManager.loadVantaFog();
});


// team-header-animate
const TeamHeaderAnimator = (function () {
    let initialized = false;

    function init() {
        if (initialized) return;
        initialized = true;

        if (typeof gsap === "undefined") {
            console.error("GSAP not loaded!");
            return;
        }

        if (typeof ScrollTrigger !== "undefined") {
            gsap.registerPlugin(ScrollTrigger);
        }

        const tl = gsap.timeline({
            scrollTrigger: {
                trigger: ".team-header",
                start: "top 80%",
                toggleActions: "play none none none"
            }
        });

        tl.from(".team-header h1", { opacity: 0, y: 20, duration: 0.8, ease: "power2.out" })
            .from(".team-header p", { opacity: 0, y: 20, duration: 0.8, ease: "power2.out" }, "-=0.4")
            .from(".team-logos", { opacity: 0, y: 20, duration: 0.8, ease: "power2.out" }, "-=0.4");
    }

    return {
        init
    };
})();

// 背景樣式切換
const LayoutBackgroundManager = (function () {
    let currentEffect = null;

    function clearBackgroundEffect() {
        if (currentEffect && typeof currentEffect.destroy === "function") {
            currentEffect.destroy();
        }

        const target = document.getElementById("team-particles-bg");
        if (target) {
            target.innerHTML = "";
        }

        // 如果是 CanvasNest，自動清除
        if (window.__canvasNestInstance && typeof window.__canvasNestInstance.destroy === "function") {
            window.__canvasNestInstance.destroy();
            window.__canvasNestInstance = null;
        }
    }

    function loadParticlesJS() {
        clearBackgroundEffect();

        if (typeof particlesJS === "undefined") {
            console.error("particlesJS not loaded!");
            return;
        }

        particlesJS("team-particles-bg", {
            particles: {
                number: { value: 80 },
                color: { value: "#ffffff" },
                shape: { type: "circle" },
                opacity: { value: 0.5 },
                size: { value: 3 },
                move: { enable: true, speed: 2 }
            }
        });

        currentEffect = { destroy: function () { /* particles.js 無需特別 destroy */ } };
    }

    function loadVantaNet() {
        clearBackgroundEffect();

        if (typeof VANTA === "undefined" || typeof VANTA.NET !== "function") {
            console.error("VANTA.NET not loaded!");
            return;
        }

        currentEffect = VANTA.NET({
            el: "#team-particles-bg",
            color: 0xffffff,
            backgroundColor: 0x000000,
            points: 12,
            maxDistance: 20,
            spacing: 15
        });
    }

    function loadVantaFog() {
        clearBackgroundEffect();

        if (typeof VANTA === "undefined" || typeof VANTA.FOG !== "function") {
            console.error("VANTA.FOG not loaded!");
            return;
        }

        currentEffect = VANTA.FOG({
            el: "#team-particles-bg",
            highlightColor: 0xffffff,
            midtoneColor: 0x9999ff,
            lowlightColor: 0x000000,
            baseColor: 0x000000,
            blurFactor: 0.5,
            speed: 1.5
        });
    }

    return {
        loadParticlesJS,
        loadVantaNet,
        loadVantaFog,
    };
})();


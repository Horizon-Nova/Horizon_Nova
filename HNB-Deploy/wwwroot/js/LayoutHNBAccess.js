document.addEventListener("DOMContentLoaded", function () {
    //LayoutBackgroundManager.loadParticlesJS();
    //LayoutBackgroundManager.loadParticlesPolygon();
    //LayoutBackgroundManager.loadVantaNet();
    //LayoutBackgroundManager.loadVantaFog();
    LayoutBackgroundManager.loadThreeSphereLatLng();
});

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
            points: 14,
            maxDistance: 24,
            spacing: 16,
            showDots: true,
            showLines: true
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

    // 3D 線框球體動態
    function loadThreeSphereLatLng() {
        clearBackgroundEffect();
        const target = document.getElementById('team-particles-bg');
        target.innerHTML = "";

        const renderer = new THREE.WebGLRenderer({ alpha: true, antialias: true });
        renderer.setSize(window.innerWidth, window.innerHeight);
        target.appendChild(renderer.domElement);

        const scene = new THREE.Scene();
        const camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
        camera.position.z = 36;

        const radius = 24;
        const latNum = 8, lngNum = 12;

        // 建立一個 Group 做傾斜包裝
        const globeGroup = new THREE.Group();

        // 緯線
        for (let lat = 1; lat < latNum; lat++) {
            const phi = Math.PI * lat / latNum;
            const curve = [];
            for (let lng = 0; lng <= 64; lng++) {
                const theta = 2 * Math.PI * (lng / 64);
                curve.push(new THREE.Vector3(
                    radius * Math.sin(phi) * Math.cos(theta),
                    radius * Math.cos(phi),
                    radius * Math.sin(phi) * Math.sin(theta)
                ));
            }
            const geometry = new THREE.BufferGeometry().setFromPoints(curve);
            const line = new THREE.Line(geometry, new THREE.LineBasicMaterial({ color: 0xffffff }));
            globeGroup.add(line);
        }
        // 經線
        for (let lng = 0; lng < lngNum; lng++) {
            const theta = 2 * Math.PI * lng / lngNum;
            const curve = [];
            for (let lat = 0; lat <= 64; lat++) {
                const phi = Math.PI * (lat / 64);
                curve.push(new THREE.Vector3(
                    radius * Math.sin(phi) * Math.cos(theta),
                    radius * Math.cos(phi),
                    radius * Math.sin(phi) * Math.sin(theta)
                ));
            }
            const geometry = new THREE.BufferGeometry().setFromPoints(curve);
            const line = new THREE.Line(geometry, new THREE.LineBasicMaterial({ color: 0xffffff }));
            globeGroup.add(line);
        }

        // 先對整體做 45度傾斜（繞X、Y都可以微調）
        globeGroup.rotation.x = Math.PI / 6; // 約30度
        globeGroup.rotation.y = Math.PI / 4; // 45度
        scene.add(globeGroup);

        // Resize
        window.addEventListener('resize', function () {
            renderer.setSize(window.innerWidth, window.innerHeight);
            camera.aspect = window.innerWidth / window.innerHeight;
            camera.updateProjectionMatrix();
        });

        function animate() {
            requestAnimationFrame(animate);
            globeGroup.rotation.y += 0.0035; // 只繞偏轉後的Y自轉
            renderer.render(scene, camera);
        }
        animate();

        currentEffect = {
            destroy: function () {
                renderer.dispose();
                target.innerHTML = "";
            }
        };
    }

    return {
        loadParticlesJS,
        loadVantaNet,
        loadVantaFog,
        loadThreeSphereLatLng,
    };
})();


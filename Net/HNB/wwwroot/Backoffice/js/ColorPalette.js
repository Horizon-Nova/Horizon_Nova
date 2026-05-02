const ColorPalette = {
    colors: [],

    getPrimaryColor: function(varName) {
        const name = varName.toLowerCase();
        if (name.includes('white') || name === 'snow' || name === 'ivory' || name === 'beige' || 
            name.includes('cream') || name === 'azure' || name === 'alice-blue' || name === 'ghost-white' ||
            name === 'white-smoke' || name === 'seashell' || name === 'old-lace' || name === 'floral-white' ||
            name === 'linen' || name === 'antique-white' || name === 'cornsilk' || name === 'lavender-blush' ||
            name === 'misty-rose' || name === 'light-yellow' || name === 'lemon-chiffon' ||
            name === 'light-goldenrod-yellow' || name === 'light-cyan' || name === 'pale-turquoise' ||
            name === 'powder-blue' || name === 'light-sky-blue' || name === 'light-steel-blue' ||
            name === 'light-blue' || name === 'light-green' || name === 'pale-green' ||
            name === 'light-pink' || name === 'navajo-white' || name === 'bisque' ||
            name === 'blanched-almond' || name === 'peach-puff' || name === 'moccasin' ||
            name === 'papaya-whip' || name === 'light-salmon' || name === 'light-coral' ||
            name === 'light-slate-gray' || name === 'gainsboro' || name === 'light-gray') {
            return 'White';
        }
        
        if (name.includes('black') || name === 'dark-slate-gray' || name === 'dim-gray' ||
            name === 'slate-gray' || name === 'gray' || name === 'dark-gray') {
            return 'Black';
        }
        
        if (name.includes('red') || name === 'crimson' || name === 'fire-brick' ||
            name === 'indian-red' || name === 'salmon' || name === 'tomato' ||
            name === 'orange-red' || name === 'medium-violet-red' || name === 'pale-violet-red') {
            return 'Red';
        }
        
        if (name.includes('green') || name === 'lime' || name === 'forest-green' ||
            name === 'sea-green' || name === 'spring-green' || name === 'teal' ||
            name === 'dark-cyan' || name === 'aquamarine' || name === 'turquoise' ||
            name === 'olive' || name === 'yellow-green' || name === 'chartreuse' ||
            name === 'lawn-green' || name === 'green-yellow') {
            return 'Green';
        }
        
        if (name.includes('blue') || name === 'navy' || name === 'midnight-blue' ||
            name === 'royal-blue' || name === 'steel-blue' || name === 'dodger-blue' ||
            name === 'deep-sky-blue' || name === 'sky-blue' || name === 'cornflower-blue' ||
            name === 'medium-blue' || name === 'slate-blue' || name === 'medium-slate-blue') {
            return 'Blue';
        }
        
        if (name.includes('yellow') || name === 'gold' || name === 'khaki') {
            return 'Yellow';
        }
        
        if (name.includes('cyan') || name === 'aqua') {
            return 'Cyan';
        }
        
        if (name.includes('magenta') || name === 'fuchsia') {
            return 'Magenta';
        }
        
        if (name === 'silver') {
            return 'Gray';
        }
        
        if (name.includes('orange') || name === 'coral') {
            return 'Orange';
        }
        
        if (name.includes('purple') || name === 'indigo' || name === 'violet' ||
            name === 'blue-violet' || name === 'medium-purple' || name === 'plum' ||
            name === 'orchid' || name === 'medium-orchid' || name === 'dark-orchid' ||
            name === 'lavender') {
            return 'Purple';
        }
        
        if (name.includes('pink') || name === 'hot-pink' || name === 'deep-pink' ||
            name === 'medium-pink') {
            return 'Pink';
        }
        
        if (name.includes('brown') || name === 'saddle-brown' || name === 'sienna' ||
            name === 'chocolate' || name === 'peru' || name === 'sandy-brown' ||
            name === 'burly-wood' || name === 'tan' || name === 'rosy-brown' ||
            name === 'wheat') {
            return 'Brown';
        }
        
        return 'Other';
    },

    isColorValue: function(value) {
        return /^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$/.test(value);
    },

    loadColorsFromCSS: function() {
        const root = getComputedStyle(document.documentElement);
        const colors = [];
        const styleSheets = Array.from(document.styleSheets);
        const colorVariables = new Set();
        
        styleSheets.forEach(sheet => {
            try {
                const rules = Array.from(sheet.cssRules || []);
                rules.forEach(rule => {
                    if (rule.selectorText === ':root' && rule.style) {
                        for (let i = 0; i < rule.style.length; i++) {
                            const propName = rule.style[i];
                            if (propName.startsWith('--')) {
                                const propValue = rule.style.getPropertyValue(propName).trim();
                                if (this.isColorValue(propValue)) {
                                    colorVariables.add(propName);
                                }
                            }
                        }
                    }
                });
            } catch (e) {
            }
        });
        
        colorVariables.forEach(cssVarName => {
            const colorValue = root.getPropertyValue(cssVarName).trim();
            
            if (colorValue && this.isColorValue(colorValue)) {
                const varName = cssVarName.substring(2);
                const displayName = varName.split('-').map(word => 
                    word.charAt(0).toUpperCase() + word.slice(1).toLowerCase()
                ).join(' ');
                
                colors.push({
                    name: displayName,
                    code: colorValue,
                    varName: cssVarName,
                    primaryColor: this.getPrimaryColor(varName)
                });
            }
        });
        
        const primaryColorOrder = ['White', 'Black', 'Red', 'Green', 'Blue', 'Yellow', 'Cyan', 'Magenta', 'Gray', 'Orange', 'Purple', 'Pink', 'Brown', 'Other'];
        colors.sort((a, b) => {
            const aIndex = primaryColorOrder.indexOf(a.primaryColor);
            const bIndex = primaryColorOrder.indexOf(b.primaryColor);
            if (aIndex !== bIndex) {
                return aIndex - bIndex;
            }
            return a.name.localeCompare(b.name);
        });
        
        this.colors = colors;
        return colors;
    },

    init: function() {
        this.loadColorsFromCSS();
        this.renderColors();
        this.setupSearch();
    },

    renderColors: function(filteredColors = null) {
        const colorList = document.getElementById('colorList');
        const noResults = document.getElementById('noResults');
        const colors = filteredColors || this.colors;

        if (!colorList) return;

        if (colors.length === 0) {
            colorList.innerHTML = '';
            noResults.style.display = 'block';
            return;
        }

        noResults.style.display = 'none';
        
        const groupedColors = {};
        colors.forEach(color => {
            const primary = color.primaryColor || 'Other';
            if (!groupedColors[primary]) {
                groupedColors[primary] = [];
            }
            groupedColors[primary].push(color);
        });

        const primaryColorOrder = ['White', 'Black', 'Red', 'Green', 'Blue', 'Yellow', 'Cyan', 'Magenta', 'Gray', 'Orange', 'Purple', 'Pink', 'Brown', 'Other'];
        
        let html = '';
        primaryColorOrder.forEach(primary => {
            if (groupedColors[primary] && groupedColors[primary].length > 0) {
                html += `<div class="mb-3">`;
                html += `<h2 class="h6 mb-2 fw-bold border-bottom pb-1">${primary}</h2>`;
                html += `<div class="row g-2">`;
                
                const colors = groupedColors[primary].sort((a, b) => {
                    const hslA = this.hexToHsl(a.code);
                    const hslB = this.hexToHsl(b.code);
                    
                    if (Math.abs(hslA.h - hslB.h) > 1) {
                        return hslA.h - hslB.h;
                    }
                    if (Math.abs(hslA.s - hslB.s) > 0.01) {
                        return hslB.s - hslA.s;
                    }
                    return hslB.l - hslA.l;
                });
                colors.forEach(color => {
                    html += `<div class="col-6 col-sm-4 col-md-3 col-lg-2 col-xl-1">`;
                    html += this.createColorCell(color);
                    html += `</div>`;
                });
                
                html += `</div></div>`;
            }
        });

        colorList.innerHTML = html;

        if (typeof lucide !== 'undefined' && typeof lucide.createIcons === 'function') {
            lucide.createIcons();
        }
    },

    getRecommendedFont: function(color) {
        const hsl = this.hexToHsl(color.code);
        
        if (hsl.l < 0.3) {
            return { name: 'Sans Serif', var: '--font-primary', family: 'var(--font-primary)' };
        }
        else if (hsl.l > 0.7) {
            return { name: 'Serif', var: '--font-secondary', family: 'var(--font-secondary)' };
        }
        else {
            return { name: 'Brand', var: '--font-brand-italic', family: 'var(--font-brand-italic)' };
        }
    },

    createColorCell: function(color) {
        const font = this.getRecommendedFont(color);
        const textColor = this.getContrastColor(color.code);
        
        return `
            <div class="position-relative border rounded overflow-hidden" 
                 style="background-color: var(${color.varName}); width: 100%; aspect-ratio: 1; cursor: pointer;"
                 onclick="ColorPalette.copyToClipboard('var(${color.varName})')"
                 title="點擊複製 CSS 變數">
                <div class="position-absolute bottom-0 start-0 p-1" style="color: ${textColor}; font-family: ${font.family};">
                    <div class="small fw-medium" style="font-size: 0.7rem; line-height: 1.2; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;">
                        ${color.name}
                    </div>
                </div>
            </div>
        `;
    },

    hexToHsl: function(hex) {
        const hexClean = hex.replace('#', '');
        const r = parseInt(hexClean.substr(0, 2), 16) / 255;
        const g = parseInt(hexClean.substr(2, 2), 16) / 255;
        const b = parseInt(hexClean.substr(4, 2), 16) / 255;
        
        const max = Math.max(r, g, b);
        const min = Math.min(r, g, b);
        let h, s, l = (max + min) / 2;
        
        if (max === min) {
            h = s = 0;
        } else {
            const d = max - min;
            s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
            
            switch (max) {
                case r: h = ((g - b) / d + (g < b ? 6 : 0)) / 6; break;
                case g: h = ((b - r) / d + 2) / 6; break;
                case b: h = ((r - g) / d + 4) / 6; break;
            }
        }
        
        return {
            h: h * 360,
            s: s,
            l: l
        };
    },

    getContrastColor: function(hexColor) {
        const hex = hexColor.replace('#', '');
        
        const r = parseInt(hex.substr(0, 2), 16);
        const g = parseInt(hex.substr(2, 2), 16);
        const b = parseInt(hex.substr(4, 2), 16);
        
        const brightness = (r * 299 + g * 587 + b * 114) / 1000;
        
        return brightness > 128 ? '#000000' : '#FFFFFF';
    },

    setupSearch: function() {
        const searchInput = document.getElementById('colorSearchInput');
        const clearBtn = document.getElementById('clearSearchBtn');

        if (!searchInput) return;

        searchInput.addEventListener('input', (e) => {
            const query = e.target.value.trim().toLowerCase();
            this.searchColors(query);
            
            if (clearBtn) {
                clearBtn.style.display = query ? 'block' : 'none';
            }
        });

        if (clearBtn) {
            clearBtn.addEventListener('click', () => {
                searchInput.value = '';
                clearBtn.style.display = 'none';
                this.renderColors();
            });
        }
    },

    searchColors: function(query) {
        if (!query) {
            this.renderColors();
            return;
        }

        const queryLower = query.toLowerCase();
        
        const filtered = this.colors.filter(color => {
            const nameMatch = color.name.toLowerCase().includes(queryLower);
            const codeMatch = color.code.toLowerCase().includes(queryLower);
            
            return nameMatch || codeMatch;
        });

        const primaryColorOrder = ['White', 'Black', 'Red', 'Green', 'Blue', 'Yellow', 'Cyan', 'Magenta', 'Gray', 'Orange', 'Purple', 'Pink', 'Brown', 'Other'];
        filtered.sort((a, b) => {
            const aIndex = primaryColorOrder.indexOf(a.primaryColor);
            const bIndex = primaryColorOrder.indexOf(b.primaryColor);
            if (aIndex !== bIndex) {
                return aIndex - bIndex;
            }
            return a.name.localeCompare(b.name);
        });

        this.renderColors(filtered);
    },

    copyToClipboard: function(text) {
        if (navigator.clipboard && navigator.clipboard.writeText) {
            navigator.clipboard.writeText(text).then(() => {
                showToast('已複製顏色代碼: ' + text, 'success');
            }).catch(() => {
                this.fallbackCopy(text);
            });
        } else {
            this.fallbackCopy(text);
        }
    },

    fallbackCopy: function(text) {
        const textArea = document.createElement('textarea');
        textArea.value = text;
        textArea.style.position = 'fixed';
        textArea.style.opacity = '0';
        document.body.appendChild(textArea);
        textArea.select();
        
        try {
            document.execCommand('copy');
            showToast('已複製顏色代碼: ' + text, 'success');
        } catch (err) {
            showToast('複製失敗，請手動複製: ' + text, 'error');
        }
        
        document.body.removeChild(textArea);
    }
};


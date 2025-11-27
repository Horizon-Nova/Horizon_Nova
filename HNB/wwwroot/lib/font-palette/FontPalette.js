const FontPalette = {
    fonts: [],

    isFontValue: function(value) {
        return value && typeof value === 'string' && value.trim().length > 0;
    },

    getFontCategory: function(varName) {
        const name = varName.toLowerCase();
        
        if (name.includes('primary') || name.includes('main')) {
            return 'Primary';
        }
        
        if (name.includes('secondary')) {
            return 'Secondary';
        }
        
        if (name.includes('mono') || name.includes('monospace')) {
            return 'Monospace';
        }
        
        if (name.includes('brand')) {
            return 'Brand';
        }
        
        if (name.includes('lato')) {
            return 'Custom';
        }
        
        return 'Other';
    },

    loadFontsFromCSS: function() {
        const root = getComputedStyle(document.documentElement);
        const fonts = [];
        const styleSheets = Array.from(document.styleSheets);
        const fontVariables = new Set();
        
        styleSheets.forEach(sheet => {
            try {
                const rules = Array.from(sheet.cssRules || []);
                rules.forEach(rule => {
                    if (rule.selectorText === ':root' && rule.style) {
                        for (let i = 0; i < rule.style.length; i++) {
                            const propName = rule.style[i];
                            if (propName.startsWith('--font-')) {
                                const propValue = rule.style.getPropertyValue(propName).trim();
                                if (this.isFontValue(propValue)) {
                                    fontVariables.add(propName);
                                }
                            }
                        }
                    }
                });
            } catch (e) {
            }
        });
        
        fontVariables.forEach(cssVarName => {
            const fontValue = root.getPropertyValue(cssVarName).trim();
            
            if (fontValue && this.isFontValue(fontValue)) {
                const varName = cssVarName.substring(2);
                
                const displayName = varName.split('-').map(word => 
                    word.charAt(0).toUpperCase() + word.slice(1).toLowerCase()
                ).join(' ');
                
                fonts.push({
                    name: displayName,
                    value: fontValue,
                    varName: cssVarName,
                    category: this.getFontCategory(varName)
                });
            }
        });
        
        const categoryOrder = ['Primary', 'Secondary', 'Monospace', 'Brand', 'Custom', 'Other'];
        fonts.sort((a, b) => {
            const aIndex = categoryOrder.indexOf(a.category);
            const bIndex = categoryOrder.indexOf(b.category);
            if (aIndex !== bIndex) {
                return aIndex - bIndex;
            }
            return a.name.localeCompare(b.name);
        });
        
        this.fonts = fonts;
        return fonts;
    },

    init: function() {
        this.loadFontsFromCSS();
        this.renderFonts();
        this.setupSearch();
    },

    renderFonts: function(filteredFonts = null) {
        const fontList = document.getElementById('fontList');
        const noResults = document.getElementById('noResults');
        const fonts = filteredFonts || this.fonts;

        if (!fontList) return;

        if (fonts.length === 0) {
            fontList.innerHTML = '';
            noResults.style.display = 'block';
            return;
        }

        noResults.style.display = 'none';
        
        let html = '';
        html += `<div class="row g-2">`;
        
        fonts.forEach(font => {
            html += `<div class="col-6 col-sm-4 col-md-3 col-lg-2 col-xl-1">`;
            html += this.createFontCell(font);
            html += `</div>`;
        });
        
        html += `</div>`;

        fontList.innerHTML = html;

        if (typeof lucide !== 'undefined' && typeof lucide.createIcons === 'function') {
            lucide.createIcons();
        }
    },

    createFontCell: function(font) {
        return `
            <div class="text-center border rounded" 
                 style="cursor: pointer; min-height: 4rem; display: flex; align-items: center; justify-content: center; background-color: var(--Black); padding: 0.5rem;"
                 onclick="FontPalette.copyToClipboard('var(${font.varName})')"
                 title="點擊複製 CSS 變數">
                 <div class="small fw-medium" 
                      style="font-family: var(${font.varName}); 
                             font-size: 1.5rem; 
                             color: #FFFFFF;
                             word-break: break-word;
                             overflow-wrap: break-word;">
                     ${font.name}
                 </div>
            </div>
        `;
    },

    setupSearch: function() {
        const searchInput = document.getElementById('fontSearchInput');
        const clearBtn = document.getElementById('clearSearchBtn');

        if (!searchInput) return;

        searchInput.addEventListener('input', (e) => {
            const query = e.target.value.trim().toLowerCase();
            this.searchFonts(query);
            
            if (clearBtn) {
                clearBtn.style.display = query ? 'block' : 'none';
            }
        });

        if (clearBtn) {
            clearBtn.addEventListener('click', () => {
                searchInput.value = '';
                clearBtn.style.display = 'none';
                this.renderFonts();
            });
        }
    },

    searchFonts: function(query) {
        if (!query) {
            this.renderFonts();
            return;
        }

        const queryLower = query.toLowerCase();
        
        const filtered = this.fonts.filter(font => {
            const nameMatch = font.name.toLowerCase().includes(queryLower);
            const varMatch = font.varName.toLowerCase().includes(queryLower);
            const valueMatch = font.value.toLowerCase().includes(queryLower);
            
            return nameMatch || varMatch || valueMatch;
        });

        const categoryOrder = ['Primary', 'Secondary', 'Monospace', 'Brand', 'Custom', 'Other'];
        filtered.sort((a, b) => {
            const aIndex = categoryOrder.indexOf(a.category);
            const bIndex = categoryOrder.indexOf(b.category);
            if (aIndex !== bIndex) {
                return aIndex - bIndex;
            }
            return a.name.localeCompare(b.name);
        });

        this.renderFonts(filtered);
    },

    copyToClipboard: function(text) {
        if (navigator.clipboard && navigator.clipboard.writeText) {
            navigator.clipboard.writeText(text).then(() => {
                showToast('已複製字體變數: ' + text, 'success');
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
            showToast('已複製字體變數: ' + text, 'success');
        } catch (err) {
            showToast('複製失敗，請手動複製: ' + text, 'error');
        }
        
        document.body.removeChild(textArea);
    }
};


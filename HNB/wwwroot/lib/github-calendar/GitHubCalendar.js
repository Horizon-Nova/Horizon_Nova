/**
 * GitHub 風格活動日曆
 * 一個可重用的日曆組件，用於顯示年度活動數據
 */
class GitHubCalendar {
    constructor(container, options = {}) {
        this.container = typeof container === 'string' ? document.querySelector(container) : container;
        this.options = {
            year: new Date().getFullYear(),
            startDay: 1, // 週開始日 (0=週日, 1=週一)
            showTooltip: true,
            showLegend: true,
            showMonthLabels: true,
            showWeekLabels: true,
            ...options
        };
        this.activityData = {};
        this.init();
    }

    init() {
        if (!this.container) {
            console.error('GitHubCalendar: Container element not found');
            return;
        }
        this.generateCalendar();
        this.addEventListeners();
    }

    generateCalendar() {
        const currentYear = this.options.year;
        
        const startDate = new Date(currentYear, 0, 1);
        
        const startOfWeek = new Date(startDate);
        const dayOfWeek = startDate.getDay();
        const mondayOffset = dayOfWeek === 0 ? -6 : 1 - dayOfWeek;
        startOfWeek.setDate(startDate.getDate() + mondayOffset);
        
        const endDate = new Date(currentYear, 11, 31);
        const endOfWeek = new Date(endDate);
        const endDayOfWeek = endDate.getDay();
        const sundayOffset = endDayOfWeek === 0 ? 0 : 7 - endDayOfWeek;
        endOfWeek.setDate(endDate.getDate() + sundayOffset);
        
        const totalWeeks = Math.ceil((endOfWeek - startOfWeek) / (7 * 24 * 60 * 60 * 1000)) + 1;
        
        let monthLabels = '';
        if (this.options.showMonthLabels) {
            monthLabels = this.generateMonthLabels(totalWeeks);
        }
        
        let weekLabels = '';
        if (this.options.showWeekLabels) {
            weekLabels = this.generateWeekLabels();
        }
        
        const calendarGrid = this.generateCalendarGrid(startOfWeek, totalWeeks, currentYear);
        
        let legend = '';
        if (this.options.showLegend) {
            legend = this.generateLegend();
        }
        
        const calendarHTML = `
            <div class="github-calendar-container">
                ${monthLabels}
                <div class="github-calendar-grid">
                    <div class="github-calendar-flex">
                        ${weekLabels}
                        <div class="github-calendar-main" style="width: ${totalWeeks * 0.875}rem;">
                            ${calendarGrid}
                        </div>
                    </div>
                </div>
                ${legend}
            </div>
        `;
        
        this.container.innerHTML = calendarHTML;
    }

    generateMonthLabels(totalWeeks) {
        const monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
        let monthLabels = '';
        
        const weeksPerMonth = totalWeeks / 12;
        let currentWeek = 0;
        
        for (let i = 0; i < 12; i++) {
            const monthName = monthNames[i];
            let weekSpan;
            
            if (i === 11) {
                weekSpan = totalWeeks - currentWeek;
            } else {
                weekSpan = Math.floor(weeksPerMonth);
                if (i < totalWeeks % 12) {
                    weekSpan += 1;
                }
            }
            
            const labelWidth = (weekSpan * 0.875) + 'rem';
            monthLabels += `<div class="github-calendar-month-label" style="width: ${labelWidth};">${monthName}</div>`;
            currentWeek += weekSpan;
        }
        
        return `<div class="github-calendar-month-labels" style="width: ${totalWeeks * 0.875}rem;">${monthLabels}</div>`;
    }

    generateWeekLabels() {
        const weekNames = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
        let weekLabels = '';
        for (let i = 0; i < 7; i++) {
            weekLabels += `<div class="github-calendar-week-label">${weekNames[i]}</div>`;
        }
        return `<div class="github-calendar-week-labels">${weekLabels}</div>`;
    }

    generateCalendarGrid(startOfWeek, totalWeeks, currentYear) {
        let calendarGrid = '';
        for (let week = 0; week < totalWeeks; week++) {
            calendarGrid += '<div class="github-calendar-week">';
            
            for (let day = 0; day < 7; day++) {
                const currentDate = new Date(startOfWeek);
                currentDate.setDate(startOfWeek.getDate() + (week * 7) + day);
                
                const isCurrentYear = currentDate.getFullYear() === currentYear;
                
                if (isCurrentYear) {
                    const dateStr = currentDate.toISOString().split('T')[0];
                    const count = this.activityData[dateStr] || 0;
                    
                    let level = 0;
                    if (count >= 4) level = 4;
                    else if (count >= 3) level = 3;
                    else if (count >= 2) level = 2;
                    else if (count >= 1) level = 1;
                    
                    calendarGrid += `<div class="github-calendar-day level-${level}" data-date="${dateStr}" data-count="${count}"></div>`;
                } else {
                    calendarGrid += `<div class="github-calendar-day level-0" style="background-color: transparent; border: none;"></div>`;
                }
            }
            
            calendarGrid += '</div>';
        }
        return calendarGrid;
    }

    generateLegend() {
        return `
            <div class="github-calendar-legend">
                <span>Less</span>
                <div class="github-calendar-legend-item level-0"></div>
                <div class="github-calendar-legend-item level-1"></div>
                <div class="github-calendar-legend-item level-2"></div>
                <div class="github-calendar-legend-item level-3"></div>
                <div class="github-calendar-legend-item level-4"></div>
                <span>More</span>
            </div>
        `;
    }

    addEventListeners() {
        if (!this.options.showTooltip) return;
        
        document.addEventListener('mouseover', (e) => {
            if (e.target.classList.contains('github-calendar-day')) {
                this.showTooltip(e);
            }
        });

        document.addEventListener('mouseout', (e) => {
            if (e.target.classList.contains('github-calendar-day')) {
                this.hideTooltip();
            }
        });
    }

    showTooltip(event) {
        const day = event.target;
        const date = day.getAttribute('data-date');
        const count = day.getAttribute('data-count') || '0';
        
        this.hideTooltip();
        
        if (!date) return;
        
        const tooltip = document.createElement('div');
        tooltip.className = 'github-calendar-tooltip show';
        
        let displayText = '';
        if (count === '0') {
            displayText = `${date}: 無活動`;
        } else if (count === '1') {
            displayText = `${date}: ${count} 次活動`;
        } else {
            displayText = `${date}: ${count} 次活動`;
        }
        
        tooltip.textContent = displayText;
        
        const rect = day.getBoundingClientRect();
        tooltip.style.left = `${rect.left + rect.width / 2}px`;
        tooltip.style.top = `${rect.top - 30}px`;
        
        document.body.appendChild(tooltip);
    }

    hideTooltip() {
        const tooltip = document.querySelector('.github-calendar-tooltip');
        if (tooltip) {
            tooltip.remove();
        }
    }

    setActivityData(data) {
        this.activityData = data || {};
        this.generateCalendar();
    }

    addActivity(date, count = 1) {
        if (!this.activityData[date]) {
            this.activityData[date] = 0;
        }
        this.activityData[date] += count;
        this.generateCalendar();
    }

    removeActivity(date) {
        delete this.activityData[date];
        this.generateCalendar();
    }

    clearAllActivities() {
        this.activityData = {};
        this.generateCalendar();
    }

    getActivityStats() {
        const totalDays = Object.keys(this.activityData).length;
        const totalCount = Object.values(this.activityData).reduce((sum, count) => sum + count, 0);
        
        return {
            totalDays,
            totalCount,
            data: this.activityData
        };
    }

    updateOptions(newOptions) {
        this.options = { ...this.options, ...newOptions };
        this.generateCalendar();
    }

    destroy() {
        if (this.container) {
            this.container.innerHTML = '';
        }
    }
}

// 導出類別
if (typeof module !== 'undefined' && module.exports) {
    module.exports = GitHubCalendar;
} else if (typeof window !== 'undefined') {
    window.GitHubCalendar = GitHubCalendar;
}

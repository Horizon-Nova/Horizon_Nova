// 衣櫃管理 JavaScript
let wardrobeCalendar;

// 初始化
document.addEventListener('DOMContentLoaded', function() {
    wardrobeCalendar = new GitHubCalendar('.github-calendar', {
        year: new Date().getFullYear(),
        showTooltip: true,
        showLegend: true,
        showMonthLabels: true,
        showWeekLabels: true
    });

    loadTestData();
});

// 載入測試數據
function loadTestData() {
    if (!wardrobeCalendar) return;

    const testData = {
        '2024-12-01': 2,
        '2024-12-02': 1,
        '2024-12-03': 0,
        '2024-12-04': 3,
        '2024-12-05': 1,
        '2024-12-10': 2,
        '2024-12-15': 4,
        '2024-12-20': 1,
        '2024-12-25': 2,
        '2024-12-30': 1,
        '2025-01-01': 1,
        '2025-01-05': 3,
        '2025-01-10': 2,
        '2025-01-15': 1,
        '2025-01-20': 4,
        '2025-01-25': 2,
        '2025-01-30': 1
    };

    wardrobeCalendar.setActivityData(testData);
}

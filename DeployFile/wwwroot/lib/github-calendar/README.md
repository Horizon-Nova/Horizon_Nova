# GitHub Calendar 組件

一個可重用的 GitHub 風格活動日曆組件，用於顯示年度活動數據。

## 文件結構

```
github-calendar/
├── GitHubCalendar.js          # 主要組件文件
├── GitHubCalendar.css         # 樣式文件
├── GitHubCalendar-example.html # 使用示例
└── README.md                  # 說明文件
```

## 快速開始

### 1. 引入文件

```html
<!-- CSS -->
<link rel="stylesheet" href="~/lib/github-calendar/GitHubCalendar.css">

<!-- JavaScript -->
<script src="~/lib/github-calendar/GitHubCalendar.js"></script>
```

### 2. 基本使用

```html
<div id="my-calendar" class="github-calendar"></div>

<script>
const calendar = new GitHubCalendar('#my-calendar');
</script>
```

### 3. 載入數據

```javascript
const data = {
    '2025-01-01': 2,
    '2025-01-02': 1,
    '2025-01-03': 4
};

calendar.setActivityData(data);
```

## 配置選項

```javascript
const calendar = new GitHubCalendar('.container', {
    year: 2025,                    // 年份
    startDay: 1,                   // 週開始日 (0=週日, 1=週一)
    showTooltip: true,             // 顯示工具提示
    showLegend: true,              // 顯示圖例
    showMonthLabels: true,         // 顯示月份標籤
    showWeekLabels: true           // 顯示星期標籤
});
```

## API 方法

### 數據管理

```javascript
// 設置活動數據
calendar.setActivityData(data);

// 添加單個活動
calendar.addActivity('2025-01-01', 3);

// 移除活動
calendar.removeActivity('2025-01-01');

// 清空所有活動
calendar.clearAllActivities();
```

### 統計信息

```javascript
// 獲取活動統計
const stats = calendar.getActivityStats();
console.log(stats.totalDays);    // 總活動天數
console.log(stats.totalCount);   // 總活動次數
console.log(stats.data);         // 原始數據
```

### 配置更新

```javascript
// 更新選項
calendar.updateOptions({ year: 2024 });

// 銷毀實例
calendar.destroy();
```

## 主題支持

### 暗色主題

```html
<div class="github-calendar theme-dark"></div>
```

### 緊湊模式

```html
<div class="github-calendar compact"></div>
```

## 響應式設計

組件自動適配不同螢幕尺寸：

- **桌面**: 完整顯示所有功能
- **平板**: 適中的格子大小
- **手機**: 緊湊的顯示模式

## 自定義樣式

您可以通過 CSS 變量來自定義顏色：

```css
:root {
    --github-calendar-bg: #ffffff;
    --github-calendar-text: #24292f;
    --github-calendar-level-0: #ebedf0;
    --github-calendar-level-1: #9be9a8;
    --github-calendar-level-2: #40c463;
    --github-calendar-level-3: #30a14e;
    --github-calendar-level-4: #216e39;
}
```

## 瀏覽器支持

- Chrome 60+
- Firefox 55+
- Safari 12+
- Edge 79+

## 授權

MIT License

## 更新日誌

### v1.0.0
- 初始版本
- 支持基本日曆功能
- 支持自定義配置
- 支持響應式設計

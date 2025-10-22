# Bootstrap 5 使用規範

## 核心原則

1. **統一使用 Bootstrap 5** - 不使用其他 CSS 框架（如 Tailwind）
2. **使用 Utility Classes** - 優先使用 Bootstrap 內建 class
3. **避免自訂 CSS** - 除非 Bootstrap 無法實現
4. **語義化 HTML** - 使用正確的 HTML 標籤

---

## 常用 Class 對照

### 佈局

| 用途 | Class | 範例 |
|------|-------|------|
| 容器 | `container`, `container-fluid` | `<div class="container">` |
| 行 | `row` | `<div class="row">` |
| 列 | `col`, `col-md-6`, `col-lg-3` | `<div class="col-md-6">` |
| 間距 | `g-2`, `g-3`, `g-4` | `<div class="row g-3">` |
| Flex | `d-flex`, `align-items-center`, `justify-content-between` | `<div class="d-flex gap-2">` |
| 堆疊 | `vstack`, `hstack` | `<div class="vstack gap-3">` |

### 表單

| 元素 | Class | 範例 |
|------|-------|------|
| 輸入框 | `form-control`, `form-control-sm` | `<input class="form-control-sm">` |
| 選擇器 | `form-select`, `form-select-sm` | `<select class="form-select-sm">` |
| Label | `form-label`, `col-form-label` | `<label class="form-label">` |
| Checkbox | `form-check`, `form-check-input` | `<div class="form-check">` |
| 按鈕 Checkbox | `btn-check` | `<input class="btn-check">` |

### 按鈕

| 類型 | Class | 範例 |
|------|-------|------|
| 主要按鈕 | `btn btn-primary` | `<button class="btn btn-primary">` |
| 次要按鈕 | `btn btn-secondary` | `<button class="btn btn-secondary">` |
| 成功按鈕 | `btn btn-success` | `<button class="btn btn-success">` |
| 小型按鈕 | `btn btn-sm` | `<button class="btn btn-sm">` |
| 外框按鈕 | `btn btn-outline-primary` | `<button class="btn btn-outline-primary">` |

### 文字

| 用途 | Class | 範例 |
|------|-------|------|
| 標題 | `h1`~`h6`, `fs-1`~`fs-6` | `<h5 class="fs-5">` |
| 粗體 | `fw-bold`, `fw-semibold` | `<span class="fw-bold">` |
| 顏色 | `text-primary`, `text-muted` | `<span class="text-muted">` |
| 對齊 | `text-center`, `text-end` | `<p class="text-center">` |
| 大小 | `small` | `<span class="small">` |

### 間距

| 用途 | Class | 範例 |
|------|-------|------|
| Margin | `m-2`, `mt-3`, `mb-2` | `<div class="mb-3">` |
| Padding | `p-2`, `pt-3`, `pb-2` | `<div class="p-4">` |
| Gap | `gap-2`, `gap-3` | `<div class="d-flex gap-2">` |

### 邊框

| 用途 | Class | 範例 |
|------|-------|------|
| 邊框 | `border`, `border-bottom` | `<div class="border-bottom">` |
| 圓角 | `rounded`, `rounded-3`, `rounded-circle` | `<div class="rounded-3">` |

### 背景

| 用途 | Class | 範例 |
|------|-------|------|
| 背景色 | `bg-light`, `bg-white`, `bg-primary` | `<div class="bg-light">` |
| 透明度 | `bg-opacity-10`, `bg-opacity-25` | `<div class="bg-primary bg-opacity-10">` |

---

## 表單設計規範

### 標題和輸入框並行（推薦）

```html
<div class="row g-2 align-items-center">
    <div class="col-auto" style="width: 90px;">
        <label for="name" class="col-form-label col-form-label-sm mb-0">姓名<span class="text-danger">*</span></label>
    </div>
    <div class="col">
        <input type="text" id="name" class="form-control form-control-sm" placeholder="請輸入姓名" required>
    </div>
    <div class="col-auto" style="width: 90px;">
        <label for="email" class="col-form-label col-form-label-sm mb-0">電子郵件<span class="text-danger">*</span></label>
    </div>
    <div class="col">
        <input type="email" id="email" class="form-control form-control-sm" placeholder="example@email.com" required>
    </div>
</div>
```

**優點：**
- 節省垂直空間
- 填寫更快速
- 視覺對齊整齊

### 按鈕樣式的 Checkbox（推薦）

```html
<!-- 角色多選 -->
<div class="d-flex flex-wrap gap-2">
    <input type="checkbox" name="role_ids" value="1" class="btn-check" id="role_1" autocomplete="off">
    <label class="btn btn-outline-primary btn-sm" for="role_1">管理員</label>
    
    <input type="checkbox" name="role_ids" value="2" class="btn-check" id="role_2" autocomplete="off">
    <label class="btn btn-outline-primary btn-sm" for="role_2">一般使用者</label>
</div>
```

**優點：**
- 視覺更美觀
- 選中狀態更明顯（變成實心按鈕）
- 無需額外 JavaScript

---

## DataTable 整合

### HNBDataTable 模組

使用專案的自訂 DataTable 模組：

```javascript
// 初始化（不需要額外容器）
const table = HNBDataTable.init('#myTable', {
    order: [[0, 'desc']],
    columnDefs: [
        { orderable: false, targets: [5] }
    ]
});
```

**必要元素：**
```html
<table id="myTable" class="table table-hover mb-0">
    <thead>...</thead>
    <tbody>...</tbody>
    <!-- DataTable 元件（長度、搜尋、分頁、資訊）會自動插入於表格外層相鄰位置，無需手動包裝容器。-->
    <!-- 如需卡片視覺，可由外層頁面自行加上卡片樣式，不屬於 HNBDataTable 的必要條件。-->
</table>
```

**樣式檔：**
- `hnb-datatable.css`
- `hnb-datatable.js`

---

## 顏色系統

### 統一使用 Bootstrap 顏色

| 用途 | 顏色變數 | Hex | 使用場景 |
|------|---------|-----|---------|
| **主要色** | `primary` | `#0d6efd` | 主要按鈕、連結 |
| **成功色** | `success` | `#198754` | 成功訊息、啟用狀態 |
| **警告色** | `warning` | `#ffc107` | 警告訊息 |
| **危險色** | `danger` | `#dc3545` | 錯誤、刪除、必填標記 |
| **灰色** | `secondary` / `muted` | `#6c757d` | 次要文字、禁用狀態 |
| **淺灰背景** | `light` | `#f8f9fa` | 背景色 |
| **白色背景** | `white` | `#ffffff` | 卡片、Modal 背景 |

**使用範例：**
```html
<!-- 文字顏色 -->
<span class="text-primary">主要文字</span>
<span class="text-muted">次要文字</span>
<span class="text-danger">*</span>

<!-- 背景顏色 -->
<div class="bg-light">淺灰背景</div>
<div class="bg-white">白色背景</div>

<!-- 按鈕顏色 -->
<button class="btn btn-primary">主要按鈕</button>
<button class="btn btn-success">成功按鈕</button>
```

---

## Badge（徽章）使用

```html
<!-- 狀態徽章 -->
<span class="badge bg-success">啟用</span>
<span class="badge bg-secondary">停用</span>

<!-- 角色徽章 -->
<span class="badge bg-primary">管理員</span>
<span class="badge bg-info">一般使用者</span>
```

---

## 圖標使用（Lucide）

### 基本使用

```html
<i data-lucide="icon-name" style="width: 1rem; height: 1rem;"></i>
```

### 常用圖標

| 用途 | 圖標名稱 |
|------|---------|
| 使用者 | `user`, `user-plus`, `users` |
| 角色/權限 | `shield`, `key`, `lock` |
| 組織 | `building`, `building-2` |
| 操作 | `edit`, `trash-2`, `plus`, `x` |
| 說明 | `help-circle`, `info` |
| 導航 | `navigation`, `menu` |

### 圖標大小標準

| 位置 | 大小 | Style |
|------|------|-------|
| Modal Header 大圖標 | 1.5rem | `style="width: 1.5rem; height: 1.5rem;"` |
| 按鈕內圖標 | 1rem | `style="width: 1rem; height: 1rem;"` |
| 表格內圖標 | 1.25rem | `style="width: 1.25rem; height: 1.25rem;"` |

---

**相關文檔：**
- [Modal 使用指南](./modal.md)
- [表單設計規範](./forms.md)
- [DataTable 規範](./datatable.md)

---

最後更新：2025-10-22


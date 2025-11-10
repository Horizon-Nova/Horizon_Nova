# Bootstrap 5 使用規範

## 核心原則
1. 統一使用 Bootstrap 5（不混用其他 CSS 框架）
2. 優先使用 Utility Classes；避免自訂 CSS，除非 Bootstrap 無法實現
3. 語義化 HTML

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
| 按鈕型 Checkbox | `btn-check` | `<input class="btn-check">` |

### 按鈕
| 類型 | Class | 範例 |
|------|-------|------|
| 主要按鈕 | `btn btn-primary` | `<button class="btn btn-primary">` |
| 次要按鈕 | `btn btn-secondary` | `<button class="btn btn-secondary">` |
| 成功 | `btn btn-success` | `<button class="btn btn-success">` |
| 小型 | `btn btn-sm` | `<button class="btn btn-sm">` |
| 外框 | `btn btn-outline-primary` | `<button class="btn btn-outline-primary">` |

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

### 邊框與背景
| 用途 | Class | 範例 |
|------|-------|------|
| 邊框 | `border`, `border-bottom` | `<div class="border-bottom">` |
| 圓角 | `rounded`, `rounded-3`, `rounded-circle` | `<div class="rounded-3">` |
| 背景 | `bg-light`, `bg-white`, `bg-primary` | `<div class="bg-light">` |
| 透明度 | `bg-opacity-10`, `bg-opacity-25` | `<div class="bg-primary bg-opacity-10">` |

---

## 顏色系統
| 用途 | 變數 | Hex | 場景 |
|------|------|-----|------|
| 主要 | `primary` | `#0d6efd` | 主要按鈕、連結 |
| 成功 | `success` | `#198754` | 成功訊息、啟用狀態 |
| 警告 | `warning` | `#ffc107` | 警告訊息 |
| 危險 | `danger` | `#dc3545` | 錯誤、刪除、必填標記 |
| 次要 | `secondary` / `muted` | `#6c757d` | 次要文字、禁用狀態 |
| 淺灰 | `light` | `#f8f9fa` | 背景色 |
| 白色 | `white` | `#ffffff` | 卡片、Modal 背景 |

---

## 徽章（Badge）
```html
<span class="badge bg-success">啟用</span>
<span class="badge bg-secondary">停用</span>
```

---

## 圖標（Lucide）
```html
<i data-lucide="icon-name" style="width: 1rem; height: 1rem;"></i>
```

常用大小：
| 位置 | 大小 |
|------|------|
| Modal Header | 1.5rem |
| 按鈕內 | 1rem |
| 表格內 | 1.25rem |

---

## 相關文檔
- [Modal 原則與決策](./modal.md)
- [表單設計規範](./forms.md)
- [DataTable 規範](./datatable.md)

---

最後更新：2025-11-10
# Bootstrap 5 使用規範

（原 ui/bootstrap.md 遷移，路徑正規化至 frontend/）

最後更新：2025-11-10


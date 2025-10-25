# Modal 完整使用指南

> 快速速覽（必讀）
> - 所有 Modal 集中於單一檔：`_{PageName}Modal.cshtml`
> - 動態資料：以 `showModal('xxx', { url: 'LoadDetail', data })` 呼叫；Controller 回傳同一份 Partial（`_{PageName}Modal`）
> - 不需要也不應該提供額外 container（例如 `<div id="X"></div>`）
> - 只使用 `showModal()` / `closeModal()` API；前端一律用 jQuery API
> - 尺寸規範：表單=`modal-lg`、詳情=預設、說明=`modal-xl`
> - FileManager（無資料庫）屬特殊情況，也遵守以上規則
>
> 禁止事項（JS 不得組裝 HTML）：
> ```javascript
> // 錯誤：以 JS 拼接 Modal HTML
> const html = '<div class="modal">...'+ dynamic +'</div>'; 
> $('#container').html(html);
> showModal('temp');
> 
> // 正確：Razor 產出 Modal；JS 只觸發與傳參
> @await Html.PartialAsync("_UsersModal")
> showModal('userDetailModal', { url: '@Url.Action("LoadDetail")', data: { id: 1, type: 'user' } });
> ```

## 目錄
- [快速規範](#快速規範)
- [Modal.js API](#modaljs-api)
- [HTML 結構要求](#html-結構要求)
- [Modal 大小規範](#modal-大小規範)
- [使用場景](#使用場景)
- [完整範例](#完整範例)
- [常見問題](#常見問題)

---

## 快速規範

### 核心規則（強制）

| 項目 | 規範 |
|------|------|
| **API** | 只使用 `showModal()` 和 `closeModal()` |
| **HTML 結構** | 必須使用 Bootstrap Modal 標準結構 |
| **檔案命名** | `_{PageName}Modal.cshtml`（所有 Modal 集中一個文件） |
| **大小規範** | 表單=`modal-lg`、詳情=預設、說明=`modal-xl` |

### Modal 大小規範（強制）

| Modal 類型 | 使用尺寸 | Class | 寬度 | 使用時機 |
|-----------|---------|-------|------|---------|
| **表單 Modal**<br>（新增/編輯） | 大型 | `modal-lg` | 800px | 含多個輸入欄位的表單 |
| **詳情 Modal**<br>（查看資料） | 標準 | 無 | 500px | 唯讀資料展示 |
| **說明 Modal**<br>（操作指南） | 超大型 | `modal-xl` | 1140px | 說明文件、FAQ |
| **確認 Modal**<br>（刪除/警告） | 標準 | 無 | 500px | 簡單確認對話框 |

---

## Modal.js API

### 全域函數

#### `showModal(modalId, options)`
開啟指定 ID 的 Modal。

**參數：**
```javascript
showModal(
    'modalId',        // Modal 的 ID（必填）
    {
        url: '',      // AJAX 請求 URL（選填）
        method: 'GET', // HTTP 方法（選填，預設 GET）
        data: {},     // 請求資料（選填）
        container: '' // 內容容器 ID（選填，預設與 modalId 相同）
    }
)
```

**範例：**
```javascript
// 靜態 Modal（直接顯示）
showModal('helpModal');

// 動態 Modal（AJAX 載入）
showModal('entityFormModal', {
    url: '@Url.Action("LoadDetail", "Module")',
    method: 'GET',
    data: { id: 123, type: 'entity' },
    container: 'entityModal'
});
```

#### `closeModal(modalId)`
關閉指定 ID 的 Modal。

**範例：**
```javascript
closeModal('userFormModal');
```

---

## HTML 結構要求

### 標準結構（必須遵守）

```html
<div id="modalId" class="modal fade" tabindex="-1" aria-labelledby="modalLabel" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered [尺寸class]">
        <div class="modal-content">
            <!-- 標題列 -->
            <div class="modal-header">
                <h5 class="modal-title" id="modalLabel">標題</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="關閉"></button>
            </div>
            
            <!-- 內容區 -->
            <div class="modal-body">
                內容
            </div>
            
            <!-- 按鈕區 -->
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">取消</button>
                <button type="button" class="btn btn-primary">確認</button>
            </div>
        </div>
    </div>
</div>
```

### 結構說明

| 元素 | 類別 | 必要 | 說明 |
|------|------|------|------|
| 外層容器 | `.modal .fade` | ✓ | Modal 主容器，需有唯一 `id` |
| 對話框 | `.modal-dialog` | ✓ | 控制寬度與位置 |
| 內容容器 | `.modal-content` | ✓ | 實際內容區域 |
| 標題列 | `.modal-header` | - | 標題與關閉按鈕 |
| 內容區 | `.modal-body` | ✓ | 主要內容 |
| 按鈕區 | `.modal-footer` | - | 操作按鈕 |

---

## Modal 大小規範

### 表單 Modal（modal-lg）

**完整範例：**
```html
<div id="entityFormModal" class="modal fade" tabindex="-1" aria-labelledby="entityFormModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered modal-dialog-scrollable modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="entityFormModalLabel">@(ViewBag.Entity?.id > 0 ? "編輯項目" : "新增項目")</h5>
                <div class="d-flex gap-2">
                    <button type="button" onclick="showModal('entityFormHelpModal')" class="btn btn-sm btn-outline-secondary">
                        <i data-lucide="help-circle" style="width: 1rem; height: 1rem;"></i>
                    </button>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
            </div>
            <div class="modal-body p-4">
                <form>...</form>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">取消</button>
                <button type="button" class="btn btn-primary" onclick="submitEntityForm()">儲存</button>
            </div>
        </div>
    </div>
</div>
```

### 詳情 Modal（預設大小）

**完整範例：**
```html
<div id="entityDetailModal" class="modal fade" tabindex="-1" aria-labelledby="entityDetailModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="entityDetailModalLabel">項目詳細資訊</h5>
                <div class="d-flex gap-2">
                    <button type="button" onclick="showModal('entityDetailHelpModal')" class="btn btn-sm btn-outline-secondary">
                        <i data-lucide="help-circle" style="width: 1rem; height: 1rem;"></i>
                    </button>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
            </div>
            <div class="modal-body">
                <p>詳細資訊...</p>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">關閉</button>
                <button type="button" class="btn btn-primary">編輯</button>
            </div>
        </div>
    </div>
</div>
```

### 說明 Modal（modal-xl）

**完整範例：**
```html
<div id="entityFormHelpModal" class="modal fade" tabindex="-1" aria-labelledby="entityFormHelpModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered modal-dialog-scrollable modal-xl">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="entityFormHelpModalLabel">操作說明</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <!-- 左右兩欄佈局 -->
                <div class="row g-4">
                    <!-- 左側：說明內容 -->
                    <div class="col-md-6">
                        <h6 class="fw-bold mb-2 pb-2 border-bottom">操作說明</h6>
                        <p>...</p>
                    </div>
                    <!-- 右側：常見問題 -->
                    <div class="col-md-6">
                        <h6 class="fw-bold mb-2 pb-2 border-bottom">常見問題 (Q&A)</h6>
                        <p>...</p>
                    </div>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-primary" data-bs-dismiss="modal">我知道了</button>
            </div>
        </div>
    </div>
</div>
```

---

## 使用場景

### 場景 1：靜態 Modal（不需要數據）

**主頁面：**
```html
<!-- Modal 容器在頁面底部 -->
<div id="helpModal"></div>

<!-- 觸發按鈕 -->
<button onclick="showModal('helpModal')">說明</button>
```

### 場景 2：動態 Modal（AJAX 載入數據）

**主頁面：**
```html
<!-- Modal 空容器 -->
<div id="entityModal"></div>

<!-- 觸發按鈕（直接在 onclick 中調用）-->
<button onclick="showModal('entityFormModal', {url: '@Url.Action("LoadDetail")', method: 'GET', data: {id: null, type: 'entity'}, container: 'entityModal'})">
    新增項目
</button>

<!-- 表格行點擊 -->
<tr onclick="showModal('entityDetailModal', {url: '@Url.Action("LoadDetail")', method: 'GET', data: {id: @entity.id, type: 'entity'}, container: 'entityModal'})">
    ...
</tr>
```

**Controller：**
```csharp
public IActionResult LoadDetail(int? id = null, string? type = null)
{
    // 依據需求載入對應資料至 ViewBag（示例）
    // service.FillViewBag(ViewBag, id);

    return type switch
    {
        "entity" => PartialView("_EntityModal"),
        _ => PartialView("_EntityModal")
    };
}
```

**Modal Partial View (_EntityModal.cshtml)：**
```cshtml
@{
    Layout = null;
}

<!-- 表單 Modal -->
<div id="entityFormModal" class="modal fade">
    ...使用 ViewBag 渲染數據...
</div>

<!-- 詳情 Modal -->
<div id="entityDetailModal" class="modal fade">
    ...
</div>

<!-- 說明 Modal -->
<div id="entityFormHelpModal" class="modal fade">
    ...
</div>

<script>
function submitEntityForm() { ... }
</script>
```

---

## 完整範例

請參考以下匿名結構示例：
- `Views/Feature/Index.cshtml` - 主頁面
- `Views/Feature/_EntityModal.cshtml` - Modal 文件

---

## 常見問題

### Q：為什麼不直接把 Modal 寫在主頁面？

**A：** 使用 AJAX 動態載入 Modal 有以下優勢：
1. 頁面載入快（初始 HTML 小 70%）
2. Modal 資料即時（每次開啟都是最新的）
3. 可以使用 ViewBag 傳遞動態資料
4. Modal 可以獨立維護

### Q：為什麼不使用 @model？

**A：** Modal 需要包含：
- 當前實體資料（如：編輯的使用者）
- 下拉選單資料（如：組織列表、角色列表）
- 權限資料（如：導航權限列表）

這些資料結構不同，無法用單一 `@model` 涵蓋。使用 `ViewBag` 是「特殊情況」，符合 [ViewBag vs Model 規範](../architecture/viewbag-model.md)。

### Q：為什麼說明 Modal 要用 modal-xl？

**A：** 說明 Modal 使用左右兩欄佈局：
- 左側：操作說明、權限控制、欄位說明
- 右側：常見問題 (Q&A)、注意事項

需要較大寬度才能容納兩欄內容，避免擁擠。

### Q：如何在 Modal 內再開啟另一個 Modal？

**A：** 直接調用即可，Modal.js 會自動管理多層 Modal：
```javascript
// 在詳情 Modal 內開啟編輯 Modal
closeModal('userDetailModal'); 
showModal('userFormModal', {...});
```

### Q：為什麼 Modal Header 要加說明按鈕？

**A：** 產品級系統需要讓使用者自助解決問題：
```html
<div class="modal-header">
    <h5 class="modal-title">標題</h5>
    <div class="d-flex gap-2">
        <!-- 說明按鈕 -->
        <button onclick="showModal('helpModal')" class="btn btn-sm btn-outline-secondary">
            <i data-lucide="help-circle"></i>
        </button>
        <button class="btn-close" data-bs-dismiss="modal"></button>
    </div>
</div>
```

---

## 說明 Modal 內容結構（推薦）

### 左右兩欄佈局（modal-xl）

```html
<div class="modal-body">
    <div class="row g-4">
        <!-- 左側：說明內容 -->
        <div class="col-md-6">
            <div class="vstack gap-3">
                <!-- 1. 基本操作流程 -->
                <div>
                    <h6 class="fw-bold mb-2 pb-2 border-bottom">基本操作流程</h6>
                    <ol class="small mb-0">...</ol>
                </div>
                
                <!-- 2. 權限控制說明 -->
                <div>
                    <h6 class="fw-bold mb-2 pb-2 border-bottom">權限控制說明</h6>
                    <div class="small">...</div>
                </div>
                
                <!-- 3. 欄位說明（表格） -->
                <div>
                    <h6 class="fw-bold mb-2 pb-2 border-bottom">欄位說明</h6>
                    <table class="table table-sm table-bordered small">...</table>
                </div>
            </div>
        </div>
        
        <!-- 右側：常見問題 -->
        <div class="col-md-6">
            <div class="vstack gap-3">
                <!-- 常見問題 Q&A -->
                <div>
                    <h6 class="fw-bold mb-2 pb-2 border-bottom">常見問題 (Q&A)</h6>
                    <div class="small">
                        <p><strong>Q1：問題？</strong></p>
                        <p class="ms-3">A：解答...</p>
                    </div>
                </div>
                
                <!-- 注意事項 -->
                <div class="alert alert-warning small mb-0">
                    <h6 class="fw-bold mb-2">重要提醒</h6>
                    <ul>...</ul>
                </div>
            </div>
        </div>
    </div>
</div>
```

---

## 檔案結構範例

```
Views/Feature/
├── Index.cshtml              ← 主頁面（列表）
└── _EntityModal.cshtml       ← 所有 Modal 集中（4個）
    ├── entityFormModal       ← 表單 Modal (modal-lg)
    ├── entityDetailModal     ← 詳情 Modal (預設)
    ├── entityFormHelpModal   ← 表單說明 (modal-xl)
    └── entityDetailHelpModal ← 詳情說明 (modal-xl)
```

**主頁面（Users.cshtml）：**
```cshtml
@model List<vw_permission_user>

<!-- 頁面內容 -->
<table>...</table>

<!-- Modal 空容器 -->
<div id="userModal"></div>
```

---

## 進階技巧

### 1. Modal 間的切換

```javascript
// 從詳情 Modal 切換到編輯 Modal
closeModal('userDetailModal');
showModal('userFormModal', {
    url: '@Url.Action("LoadDetail")',
    data: { id: @ViewBag.User.id, type: 'user' },
    container: 'userModal'
});
```

### 2. 表單驗證

```javascript
function submitUserForm() {
    const formElement = document.getElementById('FormDataUser');
    
    // HTML5 原生驗證
    if (!formElement.checkValidity()) {
        formElement.reportValidity();
        return;
    }
    
    // AJAX 提交
    $.ajax({...});
}
```

### 3. 動態標題

```cshtml
<h5 class="modal-title">@(ViewBag.User?.id > 0 ? "編輯帳號" : "新增帳號")</h5>
```

---

## 自動功能

以下功能由 `Modal.js` 自動處理，無需額外程式碼：
- Lucide 圖標重新初始化
- Body 捲動鎖定/恢復
- ESC 鍵關閉最上層 Modal
- Bootstrap Modal 事件監聽
- 開啟 Modal 追蹤管理

---

## 最佳實踐

### 推薦做法
```javascript
// 直接在按鈕上調用 showModal
<button onclick="showModal('userFormModal', {...})">新增</button>
```

### 不推薦做法
```javascript
// 不要為簡單調用創建包裝函數
function showUserAdd() {
    showModal('userFormModal', {...});
}
<button onclick="showUserAdd()">新增</button>
```

---

**相關文檔：**
- [Bootstrap 使用規範](./bootstrap.md)
- [開發常見問題 (FAQ)](../faq/development-faq.md)

---

最後更新：2025-10-22


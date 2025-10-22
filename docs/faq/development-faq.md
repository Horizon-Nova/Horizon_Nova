# 開發常見問題 (FAQ)

## 目錄
- [架構相關](#架構相關)
- [Modal 開發](#modal-開發)
- [ViewBag vs Model](#viewbag-vs-model)
- [資料查詢](#資料查詢)
- [AJAX 請求](#ajax-請求)

---

## 架構相關

### Q1：為什麼要使用三層架構？

**回答：**  
三層架構（Controller → Service → Repository）的優勢：
1. **職責分離** - 每層只負責特定工作
2. **易於測試** - 可以獨立測試每一層
3. **易於維護** - 修改業務邏輯不影響資料訪問
4. **易於擴展** - 新增功能時結構清晰

**相關文檔：**
- [ARCHITECTURE.md - 概述](../../ARCHITECTURE.md#概述)

---

### Q2：為什麼不使用 async/await？

**回答：**  
專案選擇同步操作的原因：
1. **簡化錯誤處理** - 不需要處理 Task 相關錯誤
2. **代碼簡潔** - 使用 `=>` 表達式更簡潔
3. **專案規模** - 目前專案規模不需要高併發
4. **一致性** - 全專案統一使用同步操作

**注意：** 未來如需效能優化可考慮導入 async。

---

### Q3：為什麼不使用 try...catch？

**回答：**  
專案使用 `=>` 表達式和 `??` 運算子替代 try...catch：

```csharp
// 不推薦
public User GetUser(int id) {
    try {
        return db.users.Find(id);
    } catch {
        return null;
    }
}

// 推薦
public User? QueryUser(int id) 
    => db.users.Find(id) ?? null;
```

**優點：**
- 代碼更簡潔
- 強制處理 null 情況
- 使用 nullable 型別（`User?`）明確表達可能為 null

---

## Modal 開發

### Q1：為什麼 Modal 只能開一次？

**問題描述：**  
點擊按鈕開啟 Modal，關閉後再次點擊無法開啟。

**原因：**  
使用了錯誤的開啟/關閉方式（如 jQuery `.show()`/`.hide()`）。

**解決方案：**  
統一使用 `Modal.js` 提供的 API：
```javascript
// 正確
showModal('myModal');
closeModal('myModal');

// 錯誤
$('#myModal').show();  // 不要用
$('#myModal').hide();  // 不要用
```

**相關文檔：**
- [Modal 完整指南](../ui/modal.md)

---

### Q2：為什麼 Modal 關閉後頁面無法捲動？

**原因：**  
Modal 開啟時會鎖定 `body` 捲動（`overflow: hidden`），關閉時未正確恢復。

**解決方案：**  
使用 `Modal.js` 會自動處理 body 捲動鎖定/恢復。確保使用正確的關閉方式：
```html
<!-- 方式 1：data-bs-dismiss（推薦） -->
<button data-bs-dismiss="modal">關閉</button>

<!-- 方式 2：closeModal() -->
<button onclick="closeModal('myModal')">關閉</button>
```

---

### Q3：為什麼 Modal 內的圖標（Lucide）沒有顯示？

**原因：**  
Modal 透過 AJAX 動態載入時，Lucide 圖標未重新初始化。

**解決方案：**  
`Modal.js` 會在 Modal 完全顯示後（`shown.bs.modal` 事件）自動執行 `lucide.createIcons()`，無需手動處理。

確保使用 `showModal()` API 即可。

---

### Q4：Modal 應該寫在主頁面還是 Partial View？

**決策規則：**

| 場景 | 做法 | 原因 |
|------|------|------|
| **需要動態資料**（如表單、詳情） | Partial View + AJAX 載入 | 資料即時、頁面載入快 |
| **完全靜態**（如固定說明） | 寫在主頁面 | 無需 AJAX 請求 |

**範例：**
```cshtml
<!-- 主頁面 -->
<div id="userModal"></div>  ← AJAX 載入容器

<!-- 觸發 -->
<button onclick="showModal('userFormModal', {url: '...', container: 'userModal'})">
```

---

## ViewBag vs Model

### Q1：什麼時候用 ViewBag？什麼時候用 Model？

**快速判斷：**
```
需要 @foreach 迴圈？ → Model
需要 @if 條件判斷（多區塊）？ → Model
單一值/統計數字？ → ViewBag
```

**詳細規則：**
- [ViewBag vs Model 完整決策](../architecture/viewbag-model.md)

---

### Q2：為什麼 Modal 可以用 ViewBag 傳列表？

**回答：**  
這是「特殊情況」，因為：
1. Modal 透過 AJAX 載入，無法使用強型別 `@model`
2. Modal 需要多種類型資料（當前實體 + 多個列表）
3. 無法創建 ViewModel（專案規則禁止）

**相關文檔：**
- [ViewBag vs Model - 特殊情況](../architecture/viewbag-model.md#特殊情況詳解)

---

## 資料查詢

### Q1：為什麼不能用 Get 命名？

**回答：**  
專案規範統一使用 `Query*` / `Load*` 命名：
- Repository 層：`Query*` 
- Service 層：`Load*`

**原因：**
- 避免與 HTTP GET 混淆
- 語義更明確（查詢vs載入）

---

### Q2：為什麼查詢結果為 null 而不是空列表？

**回答：**  
使用 `FirstOrDefault()` 或 `Find()` 時，找不到資料會返回 null。

**處理方式：**
```csharp
// Repository
public User? QueryUser(int id) 
    => db.users.Find(id);  // 可能為 null

// Service
public User? LoadUser(int id) 
    => repo.QueryUser(id);  // 可能為 null

// View
@(ViewBag.User?.full_name ?? "")  // 使用 null-coalescing
```

---

## AJAX 請求

### Q1：為什麼 AJAX 請求失敗？

**常見原因：**
1. URL 錯誤
2. 資料格式錯誤
3. Controller Action 不存在或參數不匹配

**檢查步驟：**
```javascript
$.ajax({
    url: '@Url.Action("ActionName", "ControllerName")',  // 檢查 URL
    type: 'POST',
    data: { id: 123 },  // 檢查參數名稱
    success: function(response) {
        console.log(response);  // 檢查回應
    },
    error: function(xhr, status, error) {
        console.error(xhr.responseText);  // 查看錯誤訊息
    }
});
```

---

### Q2：為什麼表單提交後沒有驗證？

**回答：**  
使用 HTML5 原生驗證：

```javascript
function submitForm() {
    const formElement = document.getElementById('myForm');
    
    // HTML5 原生驗證
    if (!formElement.checkValidity()) {
        formElement.reportValidity();  // 顯示錯誤訊息
        return;
    }
    
    // 通過驗證後才提交
    $.ajax({...});
}
```

**必要屬性：**
```html
<input type="text" required>  ← 必填
<input type="email" required>  ← 必填且格式驗證
```

---

## 通用開發問題

### Q1：為什麼不能使用簡體中文或 Emoji？

**回答：**  
專案語言與文案規範：
- 統一使用繁體中文（zh-TW）
- 禁止使用 Emoji（避免跨平台顯示問題）
- 專業、正式的用詞

**相關文檔：**
- [語言與文案規範](../../ARCHITECTURE.md#語言與文案規範)

---

### Q2：為什麼禁止創建 ViewModel？

**回答：**  
專案規範明確禁止創建 ViewModel，原因：
1. 增加複雜度
2. 難以維護
3. ViewBag（特殊情況）或 Model 已足夠

**替代方案：**
- 主頁面列表 → 使用 `@model List<Entity>`
- Modal 動態載入 → 使用 `ViewBag`（特殊情況）

---

### Q3：如何為包裝函數命名？

**回答：**  
不要創建包裝函數，直接在元素上調用 API：

```html
<!-- 推薦：直接調用 -->
<button onclick="showModal('myModal', {...})">開啟</button>

<!-- 不推薦：創建包裝函數 -->
<script>
function showMyModal() { showModal('myModal', {...}); }
</script>
<button onclick="showMyModal()">開啟</button>
```

---

## 相關文檔

- [ARCHITECTURE.md](../../ARCHITECTURE.md) - 核心架構規範
- [ViewBag vs Model 決策](../architecture/viewbag-model.md)
- [Modal 完整指南](../ui/modal.md)
- [Bootstrap 使用規範](../ui/bootstrap.md)

---

最後更新：2025-10-22


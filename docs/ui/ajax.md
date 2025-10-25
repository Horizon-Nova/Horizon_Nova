# 前端 AJAX 規範

## 核心原則（強制）
- 統一使用 jQuery `$.ajax()`
- 禁止混用 `fetch()`、`XMLHttpRequest`、`$.get()`、`$.post()` 等其他方式（下載檔案例外）
- POST/PUT/DELETE 自動帶 Anti-Forgery Token（全域已啟用）
- 錯誤處理簡潔一致
- 嚴禁以 JS 組裝 HTML（彈窗、警告、表單、卡片）。UI 應由 Razor / Partial View 產出；JS 僅觸發與資料傳遞。

### 選取與操作（強制）
- 統一使用 jQuery 選擇器與 API：`$('#id')`、`$('.class')`、`.val()`、`.text()`、`.html()`、`.prop()`、`.on()`
- 禁止使用 `document.getElementById` / `querySelector` / `addEventListener`

```javascript
// 正確
const value = $('#searchInput').val();
$('#submitBtn').on('click', submitForm);

// 錯誤（不要用）
const value2 = document.getElementById('searchInput').value;
document.getElementById('submitBtn').addEventListener('click', submitForm);
```

---

## 表單與提交（合併指南）
- HTML5 驗證（強制）：使用 `required`、`type="email"`、`min/max`、`pattern` 等，不在 JS 手動檢查必填
- Modal 表單：使用 `modal-lg`，欄位採 `-sm` 尺寸（`form-control-sm`、`form-select-sm`、`btn-sm`）
- 送出前先檢查原生驗證：
```javascript
function submitEntity() {
    const form = document.getElementById('FormDataEntity');
    if (!form.checkValidity()) { form.reportValidity(); return; }
    const data = $(form).serialize();
    $.ajax({
        type: 'POST',
        url: '@Url.Action("SubmitEntity", "Entity")',
        data,
        success: (res) => res.success ? (alert('儲存成功'), location.reload()) : alert(res.message || '儲存失敗'),
        error: () => alert('系統發生錯誤')
    });
}
```

- JSON 提交：
```javascript
function submitJson() {
    const token = $('input[name="__RequestVerificationToken"]').val();
    const payload = { id: 1, name: '測試', __RequestVerificationToken: token };
    $.ajax({
        type: 'POST',
        url: '@Url.Action("Submit", "Entity")',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: (res) => res.success ? (alert('操作成功'), location.reload()) : alert(res.message || '操作失敗'),
        error: () => alert('系統發生錯誤')
    });
}
```

- 檔案上傳：
```javascript
function uploadFile() {
    const formData = new FormData();
    formData.append('file', $('#fileInput')[0].files[0]);
    formData.append('__RequestVerificationToken', $('input[name="__RequestVerificationToken"]').val());
    $.ajax({
        type: 'POST',
        url: '@Url.Action("Upload", "File")',
        data: formData,
        processData: false,
        contentType: false,
        success: (res) => res.success ? (alert('上傳成功'), location.reload()) : alert(res.message || '上傳失敗'),
        error: () => alert('上傳失敗')
    });
}
```

---

## 全域配置
在頁面加入 Anti-Forgery Token：
```cshtml
@Html.AntiForgeryToken()
```

Token 需在請求 body 中，非 headers。

---

## 標準範本（序列化）
```javascript
function submitForm() {
    const formData = $("#FormId").serialize();
    $.ajax({
        type: 'POST',
        url: '@Url.Action("ActionName", "ControllerName")',
        data: formData,
        success: (response) => {
            if (response.success) { alert('操作成功'); location.reload(); }
            else { alert(response.message || '操作失敗'); }
        },
        error: () => alert('系統發生錯誤')
    });
}
```

---

## 標準範本（JSON）
```javascript
function submitData() {
    const data = {
        id: 1,
        name: '測試',
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
    };
    $.ajax({
        type: 'POST',
        url: '@Url.Action("ActionName", "ControllerName")',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: (response) => {
            if (response.success) { alert('操作成功'); location.reload(); }
            else { alert(response.message || '操作失敗'); }
        },
        error: () => alert('系統發生錯誤')
    });
}
```

---

## 標準範本（檔案上傳）
```javascript
function uploadFile() {
    const formData = new FormData();
    formData.append('file', $('#fileInput')[0].files[0]);
    formData.append('__RequestVerificationToken', $('input[name="__RequestVerificationToken"]').val());

    $.ajax({
        type: 'POST',
        url: '@Url.Action("Upload", "File")',
        data: formData,
        processData: false,
        contentType: false,
        success: (response) => {
            if (response.success) { alert('上傳成功'); location.reload(); }
            else { alert(response.message || '上傳失敗'); }
        },
        error: () => alert('上傳失敗')
    });
}
```

---

## 訊息與警告（統一）
- 回傳 Json：`{ success: boolean, message?: string, data?: any }`
- JS 不組裝 HTML 提示；優先使用：
  - `alert()`（內建，簡單一致）
  - 或既有的 Modal/Toast 元件（由 Razor 產生結構，JS 只賦值/觸發）

---

## 錯誤處理（統一格式）
```csharp
// 成功
return Json(new { success = true, message = "操作成功", data = result });
// 失敗
return Json(new { success = false, message = "操作失敗：原因說明" });
```

---

## 禁止事項（DON'T）
- 不要在 JS 中手動驗證必填（請用 HTML5 `required`）
- 不要混用多種 AJAX 風格
- 不要省略錯誤處理
- 不要將 Token 放在 headers（應在 body）
- 不要在 JS 中組裝任何 HTML（含彈窗、警告、表單、卡片）

---

最後更新：2025-10-24

# 前端 AJAX 規範

## 核心原則（強制）
- 統一使用 jQuery `$.ajax()`
- 禁止混用 `fetch()`、`XMLHttpRequest`、`$.get()`、`$.post()` 等其他方式（下載檔案例外）
- POST/PUT/DELETE 自動帶 Anti-Forgery Token（全域已啟用）
- 錯誤處理簡潔一致

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

---

最後更新：2025-10-22

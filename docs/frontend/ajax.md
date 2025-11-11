# 前端 AJAX 規範

## 核心原則（強制）
- 一律使用 jQuery `$.ajax()`（下載檔案等特例除外）
- HTML 結構由 Razor/Partial 產生；JS 僅觸發事件與傳遞資料，嚴禁用 JS 組裝任何 HTML
- POST/PUT/DELETE 需攜帶 Anti-Forgery Token（全域已啟用；請確保表單含有 Token 元素）
- 錯誤處理必須一致、簡潔且可觀測
- 選取與事件一律禁止 `document.getElementById`、`addEventListener`

### 選取與事件（強制）
正確（HTML）：
```html
<button id="submitBtn" type="button" onclick="submitForm()">送出</button>
```

錯誤（JavaScript）：
```javascript
document.getElementById('submitBtn').addEventListener('click', submitForm);
```

---

## 表單與提交（合併指南）
- 使用 HTML5 原生驗證：`required`、`type="email"`、`min/max`、`pattern` 等
- 不在 JS 手動驗證；以 HTML5 `required` 為主，直接序列化提交
- Modal 表單一律 `modal-lg`；欄位採 `-sm` 尺寸

建議：將 Anti-Forgery Token 放在「各自的 form」內，確保每個表單各自擁有獨立的 Token。

```cshtml
<!-- 一般表單 -->
<form id="FormDataEntity" method="post">
  @Html.AntiForgeryToken()
  <!-- 欄位們 -->
  <button id="submitBtn" type="button">送出</button>
</form>

<!-- 上傳表單 -->
<form id="UploadForm" method="post" enctype="multipart/form-data">
  @Html.AntiForgeryToken()
  <input type="file" id="fileInput" name="file" required />
  <button id="uploadBtn" type="button">上傳</button>
</form>
```

```javascript
function submitEntity() {
  const data = $('#FormDataEntity').serialize();
  $.ajax({
    type: 'POST',
    url: '/entity/submit',
    data,
    success: (res) => res.success ? location.reload() : alert(res.message || '操作失敗'),
    error: () => alert('系統發生錯誤')
  });
}
```

### JSON 提交
```javascript
function submitJson() {
  const $form = $('#FormDataEntity');
  const token = $form.find('input[name="__RequestVerificationToken"]').val();
  const payload = { id: 1, name: '測試', __RequestVerificationToken: token };
  $.ajax({
    type: 'POST',
    url: '/entity/submit-json',
    contentType: 'application/json',
    data: JSON.stringify(payload),
    success: (res) => res.success ? location.reload() : alert(res.message || '操作失敗'),
    error: () => alert('系統發生錯誤')
  });
}
```

### 檔案上傳
```javascript
function uploadFile() {
  // 建議從表單建立，會自動帶入所有欄位與 Token
  const $form = $('#UploadForm');
  const formData = new FormData($form[0]);
  $.ajax({
    type: 'POST',
    url: '/file/upload',
    data: formData,
    processData: false,
    contentType: false,
    success: (res) => res.success ? location.reload() : alert(res.message || '上傳失敗'),
    error: () => alert('上傳失敗')
  });
}
```

---

## 全域配置與訊息格式
- 回傳 Json 統一格式：`{ success: boolean, message?: string, data?: any }`
- JS 不組裝 HTML 提示；優先 `alert()` 或既有的 Razor 產生之 Toast/Modal

```csharp
// 成功
return Json(new { success = true, message = "操作成功", data = result });
// 失敗
return Json(new { success = false, message = "操作失敗：原因說明" });
```

---

## 禁止事項（DON'T）
- 不要在 JS 中手動驗證必填（請用 HTML5）
- 不要混用多種 AJAX 風格（例如與 fetch 混用）
- 不要省略錯誤處理
- 不要將 Token 放在 headers（應在 body）
- 不要在 JS 中組裝任何 HTML（含彈窗、提示、表單、卡片）
- 不要多於判斷保持整潔
---

最後更新：2025-11-10


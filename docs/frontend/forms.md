# 表單設計規範（佈局建議）

本文件僅涵蓋 UI/佈局；提交與驗證請見：`frontend/ajax.md#表單與提交（合併指南）`。

---

## 佈局與 UI 建議
- 標題與輸入框並行（節省垂直空間），使用 Grid 與 Utility Classes
- 同一區塊欄位寬度一致；`col-auto` 對齊 label；欄位採 `form-control-sm`
- Modal 表單尺寸一律使用 `modal-lg`
- 使用 `g-2`/`g-3` 控制欄位間距
- 多選可用 `btn-check` + `btn btn-outline-primary btn-sm`

```html
<div class="row g-2 align-items-center">
  <div class="col-auto" style="width: 90px;">
    <label for="name" class="col-form-label col-form-label-sm mb-0">
      姓名<span class="text-danger">*</span>
    </label>
  </div>
  <div class="col">
    <input type="text" id="name" class="form-control form-control-sm" placeholder="請輸入姓名" required>
  </div>
  <div class="col-auto" style="width: 90px;">
    <label for="email" class="col-form-label col-form-label-sm mb-0">
      電子郵件<span class="text-danger">*</span>
    </label>
  </div>
  <div class="col">
    <input type="email" id="email" class="form-control form-control-sm" placeholder="example@email.com" required>
  </div>
</div>
```

## 按鈕與群組
- 尺寸：`btn-sm`；間距：`gap-2`；對齊：`d-flex justify-content-end`
```html
<div class="d-flex justify-content-end gap-2 mt-3">
  <button type="button" class="btn btn-secondary btn-sm">關閉</button>
  <button type="button" class="btn btn-primary btn-sm">儲存</button>
</div>
```

## 錯誤提示顯示
- 以 HTML5 內建提示為主；伺服端回傳錯誤再使用 Bootstrap `invalid-feedback` 顯示。
```html
<div class="col">
  <input type="text" id="code" name="code" class="form-control form-control-sm" required>
  <div class="invalid-feedback">代碼為必填</div>
  <!-- 於伺服端驗證失敗時加上 is-invalid -->
  <!-- <input class="form-control form-control-sm is-invalid"> -->
  <!-- <div class="invalid-feedback">伺服端錯誤訊息</div> -->
</div>
```

## 上傳欄位
- 表單需使用 `enctype="multipart/form-data"`；Token 放在各自的表單內（提交規範見 `frontend/ajax.md`）。
```html
<form id="UploadForm" method="post" enctype="multipart/form-data">
  @Html.AntiForgeryToken()
  <input type="file" id="fileInput" name="file" class="form-control form-control-sm" required>
  <div class="d-flex justify-content-end mt-3">
    <button type="button" class="btn btn-primary btn-sm" id="uploadBtn">上傳</button>
  </div>
</form>
```

---

最後更新：2025-11-10
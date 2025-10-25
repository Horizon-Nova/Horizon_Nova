# 表單設計規範（整合說明）

表單提交與驗證的完整規範已整合至：
- [前端 AJAX 規範 > 表單與提交（合併指南）](./ajax.md#表單與提交合併指南)

本檔僅保留佈局與 UI 建議（與提交邏輯無關）。

---

## 佈局與 UI 建議
- 標題與輸入框並行（節省垂直空間），使用 Bootstrap Grid 與 Utility Classes
- 同一區塊欄位寬度一致；使用 `col-auto` 對齊 label，欄位採 `form-control-sm`
- Modal 表單尺寸使用 `modal-lg`
- 使用 `g-2`/`g-3` 控制欄位間距
- 多選可用 `btn-check` + `btn btn-outline-primary btn-sm`

```html
<div class="row g-2 align-items-center">
  <div class="col-auto" style="width: 90px;"><label for="name" class="col-form-label col-form-label-sm mb-0">姓名<span class="text-danger">*</span></label></div>
  <div class="col"><input type="text" id="name" class="form-control form-control-sm" placeholder="請輸入姓名" required></div>
  <div class="col-auto" style="width: 90px;"><label for="email" class="col-form-label col-form-label-sm mb-0">電子郵件<span class="text-danger">*</span></label></div>
  <div class="col"><input type="email" id="email" class="form-control form-control-sm" placeholder="example@email.com" required></div>
</div>
```

---

最後更新：2025-10-24

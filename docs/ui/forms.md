# 表單設計規範

## 佈局原則
- 標題與輸入框並行（節省垂直空間）
- 使用 Bootstrap Grid 與 Utility Classes
- 同一區塊欄位寬度一致

### 範例：標題與輸入框並行
```html
<div class="row g-2 align-items-center">
    <div class="col-auto" style="width: 90px;"><label for="name" class="col-form-label col-form-label-sm mb-0">姓名<span class="text-danger">*</span></label></div>
    <div class="col"><input type="text" id="name" class="form-control form-control-sm" placeholder="請輸入姓名" required></div>
    <div class="col-auto" style="width: 90px;"><label for="email" class="col-form-label col-form-label-sm mb-0">電子郵件<span class="text-danger">*</span></label></div>
    <div class="col"><input type="email" id="email" class="form-control form-control-sm" placeholder="example@email.com" required></div>
</div>
```

---

## 必填與 HTML5 驗證（強制）
- 使用 `required`、`type="email"`、`min`、`max`、`pattern` 等原生驗證
- 不要在 JavaScript 手動驗證必填

```html
<input type="text" id="code" required minlength="3" maxlength="50" placeholder="請輸入代碼">
<input type="email" id="email" required placeholder="example@email.com">
```

---

## Placeholder 與提示
- 所有可輸入欄位需提供清楚的 placeholder
- 重要說明可使用 `small.text-muted` 置於欄位下方

```html
<input type="text" class="form-control form-control-sm" placeholder="請輸入完整名稱" required>
<small class="text-muted">名稱將顯示於清單與報表中</small>
```

---

## 欄位對齊與間距
- 使用 `g-2`/`g-3` 控制欄位間距
- 使用 `col-auto` 搭配固定寬度對齊標題
- 表單整體使用 `form-control-sm`、`form-select-sm`

---

## btn-check 用於多選
```html
<div class="d-flex flex-wrap gap-2">
    <input type="checkbox" name="role_ids" value="1" class="btn-check" id="role_1" autocomplete="off">
    <label class="btn btn-outline-primary btn-sm" for="role_1">管理員</label>
    <input type="checkbox" name="role_ids" value="2" class="btn-check" id="role_2" autocomplete="off">
    <label class="btn btn-outline-primary btn-sm" for="role_2">一般使用者</label>
</div>
```

---

## 表單尺寸
- 輸入元素一律使用 `-sm` 尺寸：`form-control-sm`、`form-select-sm`、`btn-sm`
- Modal 表單使用 `modal-lg`。

---

最後更新：2025-10-22

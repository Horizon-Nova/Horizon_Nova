# Controller 層規範

## 目標
- 控制 HTTP 請求與回應
- 僅做參數接收、呼叫 Service、回傳 View/PartialView/Json

---

## 結構與命名

### 查詢與載入
```csharp
[HttpGet]
public IActionResult Page()
{
    var model = service.LoadXxxList();
    return View(model);
}
```

### 載入 Modal（統一）
```csharp
[HttpGet]
public IActionResult LoadDetail(int? id = null)
{
    service.ViewBagModel(ViewBag, id);
    return PartialView("_XxxModal");
}
```

### 提交（統一）
```csharp
[HttpPost]
public IActionResult SubmitXxx(FormType form)
{
    var result = service.CreateXxx(form);
    return Json(new { success = result != null, message = result != null ? "儲存成功" : "儲存失敗" });
}
```

### 刪除（唯一）
```csharp
[HttpPost]
public IActionResult Delete(int id)
{
    var result = service.DeleteXxx(id);
    return Json(new { success = result, message = result ? "刪除成功" : "刪除失敗" });
}
```

---

## 規範
1. 不使用 `Get*` 命名；查詢頁面使用 `Page()` 或具體頁名
2. Modal 載入統一使用 `LoadDetail`
3. 提交動作統一 `Submit*`，由 Service/Repository 判斷新增或更新
4. 每個 Controller 僅允許一個 `Delete`
5. 不使用 `async/await`、不使用 `try...catch`

---

最後更新：2025-10-22



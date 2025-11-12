# Controller 層規範

## 目標
- 控制 HTTP 請求與回應
- 僅做參數接收、呼叫 Service、回傳 View/PartialView/Json

---

## 結構與命名

### 主畫面（Index）
```csharp
// 主頁一律使用 Index；簡單頁面可用表達式成員
public IActionResult Index() => View();
```

### 載入 Modal（統一）
```csharp
public IActionResult LoadDetail(int? id = null)
{
    service.ViewBagModel(ViewBag, id);
    return PartialView("Partials/{Feature}/Modal/_Xxx");
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
2. 主畫面動作使用 `Index()`；必要時保留其他頁名（例如 `Login`）
3. Modal 載入統一使用 `LoadDetail`，回傳對應 `Partials/{Feature}/Modal/_Name`
4. 簡單回傳頁面可使用表達式成員：`public IActionResult Index() => View();`
3. 提交動作統一 `Submit*`，由 Service/Repository 判斷新增或更新
4. 每個 Controller 僅允許一個 `Delete`
5. 不使用 `async/await`、不使用 `try...catch`

---

最後更新：2025-10-22



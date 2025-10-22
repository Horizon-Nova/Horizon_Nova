# Service 層規範

## 目標
- 將業務邏輯與資料訪問分離
- 提供清晰、可測試的介面給 Controller 使用

---

## 結構範本

```csharp
public class ExampleService(ExampleRepository repo)
{
    #region 統一的查詢方法
    public List<Table1> LoadTable1List(params...) => repo.QueryTable1List(params...);
    public Table1? LoadTable1(int? id = null) => repo.QueryTable1(id);
    #endregion

    #region ViewBag 設定方法
    public void ViewBagModel(dynamic viewBag, int? id = null)
    {
        viewBag.Id = id;
        viewBag.Table1s = LoadTable1List();
        viewBag.Table1 = id.HasValue ? LoadTable1(id.Value) : null;
    }
    #endregion

    #region 基本 CRUD 操作
    public Table CreateTable(Table data) => repo.InsertTable(data);
    public bool DeleteTable(int id) => repo.DeleteTable(id);
    #endregion
}
```

---

## 命名規則

| 類別 | 命名格式 | 範例 |
|------|----------|------|
| 列表載入 | `Load*表名*List` | `LoadUserList()` |
| 單一載入 | `Load*表名*` | `LoadUser(int id)` |
| 新增/更新 | `Create*表名*` | `CreateUser(user)` |
| 刪除 | `Delete*表名*` | `DeleteUser(int id)` |
| ViewBag | `ViewBagModel` | `ViewBagModel(viewBag, id)` |

---

## 規範

1. 不使用 `Get*` 命名；統一使用 `Load*`
2. 不使用 `async/await`
3. 不使用 `try...catch`（集中式錯誤捕捉處理例外）
4. Repository 僅提供查詢與儲存；業務邏輯放在 Service
5. 複雜業務邏輯請拆為輔助方法，避免大型函式

---

## 範例

```csharp
public (bool success, string message) ProcessBusiness(...)
{
    var entity = LoadTable1(id);
    if (entity == null) return (false, "資料不存在");
    // 業務處理...
    return (true, "完成");
}
```

---

最後更新：2025-10-22



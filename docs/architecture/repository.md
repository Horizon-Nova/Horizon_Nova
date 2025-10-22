# Repository 層規範

## 目標
- 提供乾淨、可預測的資料讀寫介面
- 僅進行簡單查詢與持久化，不包含業務邏輯

---

## 結構範本

```csharp
public class ExampleRepository(DbContext db)
{
    #region 統一的查詢來源
    private IQueryable<Table1> ValidTable1s => db.table1s.Where(x => !x.is_deleted).OrderBy(x => x.created_at);
    private IQueryable<Table2> ValidTable2s => db.table2s.Where(x => !x.is_deleted);
    #endregion

    #region 專用查詢方法
    public List<Table1> QueryTable1List(params...) => ValidTable1s.Where(...).ToList();
    public Table1? QueryTable1(int? id = null, string? name = null) { /* ... */ }

    public List<Table2> QueryTable2List(params...) => ValidTable2s.Where(...).ToList();
    public Table2? QueryTable2(int? id = null) => ValidTable2s.FirstOrDefault(...);
    #endregion

    #region 基本 CRUD 操作
    // 一張表只有一個 Insert 方法（同時負責新增/更新）
    public Table InsertTable(Table data)
    {
        var existing = db.tables.Find(data.id);
        if (existing == null)
        {
            data.created_at = DateTime.Now;
            db.tables.Add(data);
            db.SaveChanges();
            return data;
        }
        // 更新所有欄位 + 條件性更新（例如密碼）
        existing.field1 = data.field1;
        if (!string.IsNullOrEmpty(data.password_hash)) existing.password_hash = data.password_hash;
        existing.updated_at = DateTime.Now;
        db.SaveChanges();
        return existing;
    }

    public bool DeleteTable(int id)
    {
        var entity = db.tables.Find(id);
        if (entity == null) return false;
        db.tables.Remove(entity);
        db.SaveChanges();
        return true;
    }
    #endregion
}
```

---

## 命名規則

| 類別 | 命名格式 | 範例 |
|------|----------|------|
| 列表查詢 | `Query*表名*List` | `QueryUserList()` |
| 單一查詢 | `Query*表名*` | `QueryUser(int? id = null)` |
| 新增/更新 | `Insert*表名*` | `InsertUser(user)` |
| 刪除 | `Delete*表名*` | `DeleteUser(int id)` |

---

## 規範
- 不使用 `Get*` 命名
- 不使用 `async/await`
- 不使用 `try...catch`（由集中式錯誤捕捉處理例外）
- 查詢需要統一來源（`ValidXxx`）以避免重複條件
- Repository 僅負責資料查詢與持久化，不含業務規則

---

最後更新：2025-10-22

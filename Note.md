我腦袋有想法只是
1.建立一個資料表裡面就是存放 唯一性|...|使用者|創建時間|
>> 問題點 : 唯一性不知道要用甚麼判定，Railway 環境變數 不好設定我不採納Railway 環境變數
2.使用電腦名稱當作唯一性建立兩張表 |id(流水號)|code |...|創建時間|、|id(流水號)|使用者|可資料夾[]|創建時間
>> 這會有一個問題環境不同會導致失敗或沒有檔案

id | code | 檔案名稱 | Url | 使用者[] | 創建時間 | 檔案大小 | 上層階層(這就是code的用處) |

---

| 欄位名稱 | 資料型態 | 說明 | 範例值 |
|---------|---------|------|--------|
| id | BIGSERIAL PRIMARY KEY | 流水號 | 1, 2, 3... |
| code | VARCHAR(100) | 唯一識別碼 | file_abc123 |
| file_name | NVARCHAR(500) | 檔案名稱 | report.pdf |
| url | TEXT | 公開 URL | https://storage.../file.pdf |
| shared_users | TEXT[] | 使用者陣列 | {"user1", "user2"} |
| created_at | TIMESTAMP | 創建時間 | 2024-10-16 14:30:00 |
| file_size | BIGINT | 檔案大小（bytes） | 1024000 |
| parent_code | VARCHAR(100) | 上層資料夾 code | folder_xyz789 |
| item_type | VARCHAR(20) | 類型 | file / folder |
| owner_username | VARCHAR(100) | 擁有者 | john |
| mime_type | VARCHAR(100) | MIME 類型 | application/pdf |
| is_deleted | BOOLEAN DEFAULT FALSE | 軟刪除 | false |
| deleted_at | TIMESTAMP | 刪除時間 | NULL |
| updated_at | TIMESTAMP | 更新時間 | 2024-10-16 15:00:00 |

---

## SQL 建表語句

```sql
-- 建立檔案管理資料表
CREATE TABLE file_manager (
    id BIGSERIAL PRIMARY KEY,
    code VARCHAR(100) NOT NULL UNIQUE,
    file_name VARCHAR(500) NOT NULL,
    file_path TEXT,
    shared_users TEXT[],
    created_at TIMESTAMP DEFAULT NOW(),
    file_size BIGINT,
    parent_code VARCHAR(100),
    item_type VARCHAR(20) NOT NULL,
    owner_username VARCHAR(100) NOT NULL,
    mime_type VARCHAR(100),
    is_deleted BOOLEAN DEFAULT FALSE,
    deleted_at TIMESTAMP,
    updated_at TIMESTAMP
);

-- 欄位註解
COMMENT ON TABLE file_manager IS '檔案管理資料表';
COMMENT ON COLUMN file_manager.id IS '流水號主鍵';
COMMENT ON COLUMN file_manager.code IS '唯一識別碼';
COMMENT ON COLUMN file_manager.file_name IS '檔案或資料夾名稱';
COMMENT ON COLUMN file_manager.file_path IS '檔案路徑';
COMMENT ON COLUMN file_manager.shared_users IS '共享使用者陣列';
COMMENT ON COLUMN file_manager.created_at IS '建立時間';
COMMENT ON COLUMN file_manager.file_size IS '檔案大小（bytes）';
COMMENT ON COLUMN file_manager.parent_code IS '上層資料夾 code';
COMMENT ON COLUMN file_manager.item_type IS '類型（file/folder）';
COMMENT ON COLUMN file_manager.owner_username IS '擁有者使用者名稱';
COMMENT ON COLUMN file_manager.mime_type IS 'MIME 類型';
COMMENT ON COLUMN file_manager.is_deleted IS '是否已軟刪除';
COMMENT ON COLUMN file_manager.deleted_at IS '刪除時間';
COMMENT ON COLUMN file_manager.updated_at IS '更新時間';

-- 建立索引
CREATE INDEX idx_code ON file_manager(code);
CREATE INDEX idx_parent_code ON file_manager(parent_code);
CREATE INDEX idx_owner ON file_manager(owner_username);
CREATE INDEX idx_item_type ON file_manager(item_type);
CREATE INDEX idx_is_deleted ON file_manager(is_deleted);

-- 建立視圖
CREATE OR REPLACE VIEW vw_file_manager AS
SELECT 
    fm.id,
    fm.code,
    fm.file_name,
    fm.file_path,
    CASE 
        WHEN fm.file_path IS NOT NULL AND fm.item_type = 'file' 
        THEN (SELECT value FROM file_manager_settings WHERE key = 'storage_url_prefix') || fm.file_path
        ELSE NULL 
    END AS url,
    fm.shared_users,
    fm.created_at,
    fm.file_size,
    fm.parent_code,
    fm.item_type,
    fm.owner_username,
    fm.mime_type,
    fm.is_deleted,
    fm.deleted_at,
    fm.updated_at
FROM file_manager fm
WHERE fm.is_deleted = FALSE;

COMMENT ON VIEW vw_file_manager IS '檔案管理視圖（自動組合完整 URL，排除已刪除）';
```
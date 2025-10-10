# AI 配置資料表設計

**設計原則：**
1. 配置主表 + 參數表（用於 UI 表格管理）
2. Log 類型獨立一張表

---

## 表結構總覽

### ai_config（主表）
```
id, service_name, provider, scope, model_key, description, 
system_prompt, is_enabled, priority, daily_limit, 
created_at, updated_at, deleted_at
```

### ai_config_parameter（參數表）
```
id, ai_config_id, parameter_key, parameter_label, parameter_value, 
parameter_type, is_required, is_sensitive, sort_order, 
created_at, updated_at
```

### ai_config_usage_log（Log 表）
```
id, ai_config_id, user_id, request_type, request_data, response_data, 
status, response_time_ms, tokens_used, cost, error_message, 
created_at
```

---

## ai_config (AI 配置主表)

**說明：** AI 服務的主要配置資訊（包含提示詞）

| 欄位名稱 | 資料類型 | 必填 | 預設值 | 說明 |
|---------|---------|------|--------|------|
| id | BIGSERIAL | ✓ | AUTO | 主鍵，自動遞增 |
| service_name | VARCHAR(100) | ✓ | - | 服務顯示名稱 |
| provider | VARCHAR(50) | ✓ | - | AI 提供商（OpenAI, Anthropic, Google 等）|
| scope | VARCHAR(50) | ✓ | - | 服務作用域（chat, image, embedding 等）|
| model_key | VARCHAR(100) | ✓ | - | 模型識別鍵（gpt-4, claude-3 等）|
| description | TEXT | ✗ | NULL | 服務描述 |
| system_prompt | TEXT | ✗ | NULL | 系統提示詞（System Prompt）|
| is_enabled | BOOLEAN | ✓ | TRUE | 是否啟用 |
| priority | INTEGER | ✓ | 0 | 優先級（數字越小越優先）|
| daily_limit | INTEGER | ✗ | NULL | 每日使用限制（NULL 表示無限制）|
| created_at | TIMESTAMP | ✓ | NOW() | 創建時間 |
| updated_at | TIMESTAMP | ✗ | NULL | 更新時間 |
| deleted_at | TIMESTAMP | ✗ | NULL | 軟刪除時間 |

### 索引
```sql
CREATE INDEX idx_ai_config_provider ON ai_config(provider);
CREATE INDEX idx_ai_config_scope ON ai_config(scope);
CREATE INDEX idx_ai_config_enabled ON ai_config(is_enabled);
CREATE INDEX idx_ai_config_deleted_at ON ai_config(deleted_at);
CREATE UNIQUE INDEX idx_ai_config_unique ON ai_config(provider, scope, model_key) WHERE deleted_at IS NULL;
```

---

## ai_config_parameter (配置參數表)

**說明：** AI 配置的動態參數（在 UI 上以表格清單管理）

| 欄位名稱 | 資料類型 | 必填 | 預設值 | 說明 |
|---------|---------|------|--------|------|
| id | BIGSERIAL | ✓ | AUTO | 主鍵，自動遞增 |
| ai_config_id | BIGINT | ✓ | - | 關聯到 ai_config.id |
| parameter_key | VARCHAR(100) | ✓ | - | 參數鍵值（如：api_key, temperature）|
| parameter_label | VARCHAR(100) | ✓ | - | 顯示標籤（如：API 金鑰、溫度）|
| parameter_value | TEXT | ✗ | NULL | 參數值 |
| parameter_type | VARCHAR(20) | ✓ | 'text' | 參數類型（text, password, number, select, checkbox）|
| is_required | BOOLEAN | ✓ | FALSE | 是否必填 |
| is_sensitive | BOOLEAN | ✓ | FALSE | 是否敏感資訊（如密碼、API Key）|
| sort_order | INTEGER | ✓ | 0 | 排序順序 |
| created_at | TIMESTAMP | ✓ | NOW() | 創建時間 |
| updated_at | TIMESTAMP | ✗ | NULL | 更新時間 |

### 索引
```sql
CREATE INDEX idx_parameter_config_id ON ai_config_parameter(ai_config_id);
CREATE INDEX idx_parameter_sort ON ai_config_parameter(ai_config_id, sort_order);
CREATE UNIQUE INDEX idx_parameter_unique ON ai_config_parameter(ai_config_id, parameter_key);
```

---

## ai_config_usage_log (使用記錄表)

**說明：** 記錄 AI 服務的使用情況，用於統計、監控與成本分析

| 欄位名稱 | 資料類型 | 必填 | 預設值 | 說明 |
|---------|---------|------|--------|------|
| id | BIGSERIAL | ✓ | AUTO | 主鍵，自動遞增 |
| ai_config_id | BIGINT | ✓ | - | 關聯到 ai_config.id |
| user_id | INTEGER | ✗ | NULL | 使用者 ID |
| request_type | VARCHAR(50) | ✓ | - | 請求類型（如：chat, completion, embedding）|
| request_data | JSONB | ✗ | NULL | 請求資料 |
| response_data | JSONB | ✗ | NULL | 回應資料 |
| status | VARCHAR(20) | ✓ | 'pending' | 狀態（success, failed, pending）|
| response_time_ms | INTEGER | ✗ | NULL | 回應時間（毫秒）|
| tokens_used | INTEGER | ✗ | NULL | 使用的 Token 數 |
| cost | DECIMAL(10,4) | ✗ | NULL | 成本（美元）|
| error_message | TEXT | ✗ | NULL | 錯誤訊息 |
| created_at | TIMESTAMP | ✓ | NOW() | 創建時間 |

### 索引
```sql
CREATE INDEX idx_usage_log_config_id ON ai_config_usage_log(ai_config_id);
CREATE INDEX idx_usage_log_user_id ON ai_config_usage_log(user_id);
CREATE INDEX idx_usage_log_status ON ai_config_usage_log(status);
CREATE INDEX idx_usage_log_created_at ON ai_config_usage_log(created_at DESC);
CREATE INDEX idx_usage_log_request_type ON ai_config_usage_log(request_type);
```

---

## 外鍵約束
```sql
-- 參數表外鍵
ALTER TABLE ai_config_parameter 
ADD CONSTRAINT fk_parameter_config 
FOREIGN KEY (ai_config_id) 
REFERENCES ai_config(id) 
ON DELETE CASCADE;

-- 使用記錄表外鍵
ALTER TABLE ai_config_usage_log 
ADD CONSTRAINT fk_usage_log_config 
FOREIGN KEY (ai_config_id) 
REFERENCES ai_config(id) 
ON DELETE CASCADE;
```

---

## 使用範例

### 1. 新增 AI 配置（含參數）

```sql
-- 新增主配置
INSERT INTO ai_config (
    service_name, 
    provider, 
    scope, 
    model_key, 
    description,
    system_prompt
) VALUES (
    'GPT-4 聊天服務',
    'OpenAI',
    'chat',
    'gpt-4-turbo-preview',
    '用於一般對話的 GPT-4 服務',
    '你是一個專業的 AI 助理，請提供準確且有幫助的回答。'
) RETURNING id;
-- 假設返回 id = 1

-- 新增參數
INSERT INTO ai_config_parameter (
    ai_config_id,
    parameter_key,
    parameter_label,
    parameter_value,
    parameter_type,
    is_required,
    is_sensitive,
    sort_order
) VALUES
    (1, 'api_key', 'API 金鑰', 'sk-xxx', 'password', TRUE, TRUE, 1),
    (1, 'temperature', '溫度', '0.7', 'number', FALSE, FALSE, 2),
    (1, 'max_tokens', '最大 Token 數', '2000', 'number', FALSE, FALSE, 3);
```

### 2. 查詢啟用的配置（含參數）

```sql
-- 查詢主配置（包含系統提示詞）
SELECT 
    id,
    service_name,
    provider,
    scope,
    model_key,
    system_prompt,
    is_enabled,
    priority,
    daily_limit
FROM ai_config 
WHERE is_enabled = TRUE 
  AND deleted_at IS NULL
ORDER BY priority ASC;

-- 查詢特定配置的參數
SELECT 
    parameter_key,
    parameter_label,
    parameter_value,
    parameter_type,
    is_required,
    is_sensitive
FROM ai_config_parameter
WHERE ai_config_id = 1
ORDER BY sort_order;
```

### 3. 更新參數值

```sql
UPDATE ai_config_parameter
SET parameter_value = '0.8',
    updated_at = NOW()
WHERE ai_config_id = 1
  AND parameter_key = 'temperature';
```

### 4. 更新系統提示詞

```sql
UPDATE ai_config
SET system_prompt = '你是一個專業且友善的 AI 助理，請用繁體中文回答...',
    updated_at = NOW()
WHERE id = 1;
```

### 5. 記錄使用日誌

```sql
INSERT INTO ai_config_usage_log (
    ai_config_id,
    user_id,
    request_type,
    tokens_used,
    cost,
    status
) VALUES (
    1,
    123,
    'chat',
    1500,
    0.0300,
    'success'
);
```

### 6. 統計使用情況

```sql
-- 查詢今日使用量
SELECT 
    c.service_name,
    COUNT(*) as request_count,
    SUM(l.tokens_used) as total_tokens,
    SUM(l.cost) as total_cost
FROM ai_config_usage_log l
JOIN ai_config c ON l.ai_config_id = c.id
WHERE DATE(l.created_at) = CURRENT_DATE
GROUP BY c.id, c.service_name;

-- 檢查是否超過每日限制
SELECT 
    c.service_name,
    c.daily_limit,
    COUNT(*) as today_usage,
    CASE 
        WHEN c.daily_limit IS NULL THEN '無限制'
        WHEN COUNT(*) >= c.daily_limit THEN '已達上限'
        ELSE '正常'
    END as status
FROM ai_config c
LEFT JOIN ai_config_usage_log l 
    ON c.id = l.ai_config_id 
    AND DATE(l.created_at) = CURRENT_DATE
WHERE c.is_enabled = TRUE
GROUP BY c.id, c.service_name, c.daily_limit;
```
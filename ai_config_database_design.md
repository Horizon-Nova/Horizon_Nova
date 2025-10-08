# AI配置資料表設計

## ai_config (AI配置主表)

| 欄位名稱 | 資料類型 | 說明 |
|---------|---------|------|
| id | BIGSERIAL | 主鍵，自動遞增 |
| service_name | VARCHAR(100) | 服務顯示名稱 |
| provider | VARCHAR(50) | AI提供商 |
| scope | VARCHAR(50) | 服務作用域 |
| model_key | VARCHAR(50) | 模型識別鍵 |
| description | TEXT | 服務描述 |
| is_enabled | BOOLEAN | 是否啟用 |
| priority | INTEGER | 優先級 |
| daily_limit | INTEGER | 每日使用限制 |
| created_at | TIMESTAMP | 創建時間 |
| updated_at | TIMESTAMP | 更新時間 |
| deleted_at | TIMESTAMP | 軟刪除時間 |

## ai_config_parameters (配置參數表)

| 欄位名稱 | 資料類型 | 說明 |
|---------|---------|------|
| id | BIGSERIAL | 主鍵，自動遞增 |
| ai_config_id | BIGINT | 關聯到 ai_config.id |
| parameter_key | VARCHAR(100) | 參數鍵值 |
| parameter_label | VARCHAR(100) | 顯示標籤 |
| parameter_value | TEXT | 參數值 |
| parameter_type | VARCHAR(20) | 參數類型 |
| is_required | BOOLEAN | 是否必填 |
| sort_order | INTEGER | 排序順序 |
| created_at | TIMESTAMP | 創建時間 |
| updated_at | TIMESTAMP | 更新時間 |

## ai_config_prompts (提示詞模板表)

| 欄位名稱 | 資料類型 | 說明 |
|---------|---------|------|
| id | BIGSERIAL | 主鍵，自動遞增 |
| ai_config_id | BIGINT | 關聯到 ai_config.id |
| prompt_name | VARCHAR(100) | 模板名稱 |
| prompt_content | TEXT | 提示詞內容 |
| created_at | TIMESTAMP | 創建時間 |
| updated_at | TIMESTAMP | 更新時間 |

## ai_config_usage_log (使用記錄表)

| 欄位名稱 | 資料類型 | 說明 |
|---------|---------|------|
| id | BIGSERIAL | 主鍵，自動遞增 |
| ai_config_id | BIGINT | 關聯到 ai_config.id |
| request_type | VARCHAR(50) | 請求類型 |
| request_data | JSONB | 請求資料 |
| response_data | JSONB | 回應資料 |
| status | VARCHAR(20) | 狀態 |
| response_time_ms | INTEGER | 回應時間(毫秒) |
| tokens_used | INTEGER | 使用的Token數 |
| cost | DECIMAL(10,4) | 成本 |
| error_message | TEXT | 錯誤訊息 |
| created_at | TIMESTAMP | 創建時間 |
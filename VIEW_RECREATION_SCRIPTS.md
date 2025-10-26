# Permission Management Views 重建腳本

## 1. vw_permission_user

### 建立 View
```sql
CREATE OR REPLACE VIEW dbo.vw_permission_user AS
SELECT
    pm.id,
    pm.type,
    pm.name,
    pm.nickname,
    pm.full_name,
    pm.email,
    pm.phone,
    pm.bio,
    pm.avatar_url,
    pm.third_party_avatar,
    pm.gender,
    pm.birthday,
    pm.zodiac_sign,
    pm.favorite_color,
    pm.color_scheme,
    pm.location,
    pm.timezone,
    pm.login_method,
    pm.third_party_id,
    pm.is_email_verified,
    pm.is_phone_verified,
    pm.two_factor_enabled,
    pm.trusted_devices,
    pm.trusted_ips,
    pm.last_device_info,
    pm.subscription_products,
    pm.payment_methods,
    pm.subscription_status,
    pm.subscription_expires_at,
    pm.trial_ends_at,
    pm.billing_cycle,
    pm.auto_renew,
    pm.preferences,
    pm.notification_settings,
    pm.privacy_settings,
    pm.language,
    pm.theme,
    pm.login_count,
    pm.last_login_at,
    pm.last_activity_at,
    pm.total_session_time,
    pm.profile_completion_percentage,
    pm.permissions,
    pm.roles,
    pm.is_active,
    pm.is_online,
    pm.status,
    pm.status_reason,
    pm.created_at,
    pm.updated_at,
    pm.last_password_change_at,
    pm.password_expires_at,
    pm.last_login_ip,
    pm.last_login_user_agent,
    pm.tags,
    pm.notes,
    CASE WHEN pm.parent_id IS NOT NULL AND array_length(pm.parent_id, 1) >= 1 
         THEN (pm.parent_id[1])::int 
         ELSE NULL 
    END AS parent_id,
    pm.sort_order,
    pm.level,
    pm.created_by,
    pm.updated_by,
    pm.internal_notes,
    pm.password_hash,
    pm.salt,
    o.name AS organization_name,
    o.id AS organization_id,
    (SELECT r.name 
     FROM dbo.permission_management r 
     WHERE r.type = 'role' 
       AND r.id = CASE WHEN pm.roles IS NOT NULL AND array_length(pm.roles, 1) >= 1 
                       THEN (pm.roles[1])::int 
                       ELSE NULL 
                  END 
     LIMIT 1) AS role_name,
    pm.roles AS role_ids,
    0::bigint AS child_organizations_count,
    COALESCE(cardinality(pm.roles), 0) AS assigned_roles_count,
    CASE WHEN pm.last_login_at IS NULL 
         THEN NULL 
         ELSE EXTRACT(day FROM (now() - pm.last_login_at))::numeric 
    END AS last_login_days_ago
FROM dbo.permission_management pm
LEFT JOIN dbo.permission_management o 
    ON o.type = 'organization' 
   AND o.id = CASE WHEN pm.parent_id IS NOT NULL AND array_length(pm.parent_id, 1) >= 1 
                   THEN (pm.parent_id[1])::int 
                   ELSE NULL 
              END
WHERE pm.type = 'user';
```

### 加入註解
```sql
COMMENT ON COLUMN dbo.vw_permission_user.id IS '主鍵ID';
COMMENT ON COLUMN dbo.vw_permission_user.type IS '資料類型：user';
COMMENT ON COLUMN dbo.vw_permission_user.name IS '用戶名稱：從name欄位取得';
COMMENT ON COLUMN dbo.vw_permission_user.nickname IS '暱稱';
COMMENT ON COLUMN dbo.vw_permission_user.full_name IS '完整名稱';
COMMENT ON COLUMN dbo.vw_permission_user.email IS '電子郵件';
COMMENT ON COLUMN dbo.vw_permission_user.phone IS '電話號碼';
COMMENT ON COLUMN dbo.vw_permission_user.bio IS '個人簡介';
COMMENT ON COLUMN dbo.vw_permission_user.avatar_url IS '頭像網址';
COMMENT ON COLUMN dbo.vw_permission_user.third_party_avatar IS '第三方頭像';
COMMENT ON COLUMN dbo.vw_permission_user.gender IS '性別';
COMMENT ON COLUMN dbo.vw_permission_user.birthday IS '生日';
COMMENT ON COLUMN dbo.vw_permission_user.zodiac_sign IS '星座';
COMMENT ON COLUMN dbo.vw_permission_user.favorite_color IS '喜愛的顏色';
COMMENT ON COLUMN dbo.vw_permission_user.color_scheme IS '色彩主題';
COMMENT ON COLUMN dbo.vw_permission_user.location IS '所在地';
COMMENT ON COLUMN dbo.vw_permission_user.timezone IS '時區';
COMMENT ON COLUMN dbo.vw_permission_user.login_method IS '登入方式';
COMMENT ON COLUMN dbo.vw_permission_user.third_party_id IS '第三方ID';
COMMENT ON COLUMN dbo.vw_permission_user.is_email_verified IS '郵箱是否驗證';
COMMENT ON COLUMN dbo.vw_permission_user.is_phone_verified IS '電話是否驗證';
COMMENT ON COLUMN dbo.vw_permission_user.two_factor_enabled IS '是否啟用雙因子認證';
COMMENT ON COLUMN dbo.vw_permission_user.trusted_devices IS '信任設備陣列';
COMMENT ON COLUMN dbo.vw_permission_user.trusted_ips IS '信任IP陣列';
COMMENT ON COLUMN dbo.vw_permission_user.last_device_info IS '最後設備資訊';
COMMENT ON COLUMN dbo.vw_permission_user.subscription_products IS '訂閱產品';
COMMENT ON COLUMN dbo.vw_permission_user.payment_methods IS '付款方式';
COMMENT ON COLUMN dbo.vw_permission_user.subscription_status IS '訂閱狀態';
COMMENT ON COLUMN dbo.vw_permission_user.subscription_expires_at IS '訂閱到期時間';
COMMENT ON COLUMN dbo.vw_permission_user.trial_ends_at IS '試用期結束時間';
COMMENT ON COLUMN dbo.vw_permission_user.billing_cycle IS '計費週期';
COMMENT ON COLUMN dbo.vw_permission_user.auto_renew IS '自動續費';
COMMENT ON COLUMN dbo.vw_permission_user.preferences IS '用戶偏好';
COMMENT ON COLUMN dbo.vw_permission_user.notification_settings IS '通知設定';
COMMENT ON COLUMN dbo.vw_permission_user.privacy_settings IS '隱私設定';
COMMENT ON COLUMN dbo.vw_permission_user.language IS '語言設定';
COMMENT ON COLUMN dbo.vw_permission_user.theme IS '主題設定';
COMMENT ON COLUMN dbo.vw_permission_user.login_count IS '登入次數';
COMMENT ON COLUMN dbo.vw_permission_user.last_login_at IS '最後登入時間';
COMMENT ON COLUMN dbo.vw_permission_user.last_activity_at IS '最後活動時間';
COMMENT ON COLUMN dbo.vw_permission_user.total_session_time IS '總會話時間';
COMMENT ON COLUMN dbo.vw_permission_user.profile_completion_percentage IS '資料完成度';
COMMENT ON COLUMN dbo.vw_permission_user.permissions IS '權限陣列';
COMMENT ON COLUMN dbo.vw_permission_user.roles IS '角色ID陣列';
COMMENT ON COLUMN dbo.vw_permission_user.is_active IS '是否啟用';
COMMENT ON COLUMN dbo.vw_permission_user.is_online IS '是否在線';
COMMENT ON COLUMN dbo.vw_permission_user.status IS '狀態';
COMMENT ON COLUMN dbo.vw_permission_user.status_reason IS '狀態原因';
COMMENT ON COLUMN dbo.vw_permission_user.created_at IS '建立時間';
COMMENT ON COLUMN dbo.vw_permission_user.updated_at IS '更新時間';
COMMENT ON COLUMN dbo.vw_permission_user.last_password_change_at IS '最後密碼變更時間';
COMMENT ON COLUMN dbo.vw_permission_user.password_expires_at IS '密碼到期時間';
COMMENT ON COLUMN dbo.vw_permission_user.last_login_ip IS '最後登入IP';
COMMENT ON COLUMN dbo.vw_permission_user.last_login_user_agent IS '最後登入用戶代理';
COMMENT ON COLUMN dbo.vw_permission_user.tags IS '標籤陣列';
COMMENT ON COLUMN dbo.vw_permission_user.notes IS '備註';
COMMENT ON COLUMN dbo.vw_permission_user.parent_id IS '所屬組織ID';
COMMENT ON COLUMN dbo.vw_permission_user.sort_order IS '排序順序';
COMMENT ON COLUMN dbo.vw_permission_user.level IS '用戶層級';
COMMENT ON COLUMN dbo.vw_permission_user.created_by IS '建立者ID';
COMMENT ON COLUMN dbo.vw_permission_user.updated_by IS '更新者ID';
COMMENT ON COLUMN dbo.vw_permission_user.internal_notes IS '內部備註';
COMMENT ON COLUMN dbo.vw_permission_user.password_hash IS '密碼雜湊值';
COMMENT ON COLUMN dbo.vw_permission_user.salt IS '密碼鹽值';
COMMENT ON COLUMN dbo.vw_permission_user.organization_name IS '所屬組織名稱：從parent_id關聯取得';
COMMENT ON COLUMN dbo.vw_permission_user.organization_id IS '所屬組織ID：從parent_id取得';
COMMENT ON COLUMN dbo.vw_permission_user.role_name IS '主要角色名稱：從roles陣列取得第一個角色名稱';
COMMENT ON COLUMN dbo.vw_permission_user.role_ids IS '角色ID陣列：從roles欄位解析';
COMMENT ON COLUMN dbo.vw_permission_user.child_organizations_count IS '管理的子組織數量：該用戶管理的子組織數量';
COMMENT ON COLUMN dbo.vw_permission_user.assigned_roles_count IS '分配的角色數量：從roles陣列長度計算';
COMMENT ON COLUMN dbo.vw_permission_user.last_login_days_ago IS '距離上次登入天數：計算欄位';
```

---

## 2. vw_permission_role

### 建立 View
```sql
CREATE OR REPLACE VIEW dbo.vw_permission_role AS
SELECT
    pm.id,
    pm.type,
    pm.name,
    pm.description,
    pm.permissions,
    pm.is_active,
    pm.status,
    pm.created_at,
    pm.updated_at,
    pm.tags,
    pm.notes,
    CASE WHEN pm.parent_id IS NOT NULL AND array_length(pm.parent_id, 1) >= 1 
         THEN (pm.parent_id[1])::int 
         ELSE NULL 
    END AS parent_id,
    pm.sort_order,
    pm.level,
    pm.created_by,
    pm.updated_by,
    pm.internal_notes,
    o.name AS organization_name,
    o.id AS organization_id,
    (SELECT COUNT(*) 
     FROM dbo.permission_management u 
     WHERE u.type = 'user' 
       AND u.roles @> ARRAY[pm.id::text]) AS user_count,
    COALESCE(cardinality(pm.permissions), 0) AS permission_count,
    (SELECT array_agg(u.name) 
     FROM dbo.permission_management u 
     WHERE u.type = 'user' 
       AND u.roles @> ARRAY[pm.id::text]) AS user_names,
    pm.navigation_permissions,
    (SELECT array_agg(p.name) 
     FROM dbo.permission_management p 
     WHERE p.type = 'permission' 
       AND pm.permissions @> ARRAY[p.id::text]) AS permission_names,
    CASE WHEN pm.name ILIKE '%super%' THEN 'system' ELSE 'custom' END AS role_type,
    CASE WHEN pm.name ILIKE '%super%' THEN true ELSE false END AS is_system_role
FROM dbo.permission_management pm
LEFT JOIN dbo.permission_management o 
    ON o.type = 'organization' 
   AND o.id = CASE WHEN pm.parent_id IS NOT NULL AND array_length(pm.parent_id, 1) >= 1 
                   THEN (pm.parent_id[1])::int 
                   ELSE NULL 
              END
WHERE pm.type = 'role';
```

### 加入註解
```sql
COMMENT ON COLUMN dbo.vw_permission_role.id IS '主鍵ID';
COMMENT ON COLUMN dbo.vw_permission_role.type IS '資料類型：role';
COMMENT ON COLUMN dbo.vw_permission_role.name IS '角色名稱';
COMMENT ON COLUMN dbo.vw_permission_role.description IS '角色描述';
COMMENT ON COLUMN dbo.vw_permission_role.permissions IS '權限陣列';
COMMENT ON COLUMN dbo.vw_permission_role.is_active IS '是否啟用';
COMMENT ON COLUMN dbo.vw_permission_role.status IS '狀態';
COMMENT ON COLUMN dbo.vw_permission_role.created_at IS '建立時間';
COMMENT ON COLUMN dbo.vw_permission_role.updated_at IS '更新時間';
COMMENT ON COLUMN dbo.vw_permission_role.tags IS '標籤陣列';
COMMENT ON COLUMN dbo.vw_permission_role.notes IS '備註';
COMMENT ON COLUMN dbo.vw_permission_role.parent_id IS '所屬組織ID';
COMMENT ON COLUMN dbo.vw_permission_role.sort_order IS '排序順序';
COMMENT ON COLUMN dbo.vw_permission_role.level IS '角色層級';
COMMENT ON COLUMN dbo.vw_permission_role.created_by IS '建立者ID';
COMMENT ON COLUMN dbo.vw_permission_role.updated_by IS '更新者ID';
COMMENT ON COLUMN dbo.vw_permission_role.internal_notes IS '內部備註';
COMMENT ON COLUMN dbo.vw_permission_role.organization_name IS '所屬組織名稱';
COMMENT ON COLUMN dbo.vw_permission_role.organization_id IS '所屬組織ID';
COMMENT ON COLUMN dbo.vw_permission_role.user_count IS '擁有此角色的用戶數量';
COMMENT ON COLUMN dbo.vw_permission_role.permission_count IS '權限數量';
COMMENT ON COLUMN dbo.vw_permission_role.user_names IS '擁有此角色的用戶名稱陣列';
COMMENT ON COLUMN dbo.vw_permission_role.navigation_permissions IS '導航權限陣列';
COMMENT ON COLUMN dbo.vw_permission_role.permission_names IS '權限名稱陣列';
COMMENT ON COLUMN dbo.vw_permission_role.role_type IS '角色類型：system/custom';
COMMENT ON COLUMN dbo.vw_permission_role.is_system_role IS '是否為系統角色';
```

---

## 3. vw_permission_organization

### 建立 View
```sql
CREATE OR REPLACE VIEW dbo.vw_permission_organization AS
SELECT
    pm.id,
    pm.type,
    pm.name AS organization_name,
    pm.description AS organization_description,
    pm.level AS organization_level,
    pm.sort_order,
    pm.is_active,
    pm.status,
    pm.created_at,
    pm.updated_at,
    pm.notes AS public_notes,
    pm.internal_notes,
    CASE WHEN pm.parent_id IS NOT NULL AND array_length(pm.parent_id, 1) >= 1 
         THEN (pm.parent_id[1])::int 
         ELSE NULL 
    END AS parent_id,
    pm.created_by,
    pm.updated_by,
    (SELECT COUNT(*)::int 
     FROM dbo.permission_management r 
     WHERE r.type = 'role' 
       AND r.parent_id @> ARRAY[pm.id::text]) AS total_roles_count,
    (SELECT COUNT(*) 
     FROM dbo.permission_management u 
     WHERE u.type = 'user' 
       AND u.parent_id @> ARRAY[pm.id::text]) AS total_users_count,
    (SELECT COUNT(*) 
     FROM dbo.permission_management o2 
     WHERE o2.type = 'organization' 
       AND o2.parent_id @> ARRAY[pm.id::text]) AS total_sub_organizations_count,
    (SELECT array_agg(r.id::text) 
     FROM dbo.permission_management r 
     WHERE r.type = 'role' 
       AND r.parent_id @> ARRAY[pm.id::text]) AS organization_role_ids,
    (SELECT array_agg(r.name) 
     FROM dbo.permission_management r 
     WHERE r.type = 'role' 
       AND r.parent_id @> ARRAY[pm.id::text]) AS organization_role_names,
    (SELECT array_agg(u.id::text) 
     FROM dbo.permission_management u 
     WHERE u.type = 'user' 
       AND u.parent_id @> ARRAY[pm.id::text]) AS organization_user_ids,
    (SELECT array_agg(u.name) 
     FROM dbo.permission_management u 
     WHERE u.type = 'user' 
       AND u.parent_id @> ARRAY[pm.id::text]) AS organization_user_names,
    (SELECT array_agg(u.full_name) 
     FROM dbo.permission_management u 
     WHERE u.type = 'user' 
       AND u.parent_id @> ARRAY[pm.id::text]) AS organization_user_full_names,
    o.name AS parent_organization_name,
    o.id AS parent_organization_id,
    (SELECT array_agg(o2.name) 
     FROM dbo.permission_management o2 
     WHERE o2.type = 'organization' 
       AND o2.parent_id @> ARRAY[pm.id::text]) AS sub_organization_names,
    (SELECT array_agg(o2.id::text) 
     FROM dbo.permission_management o2 
     WHERE o2.type = 'organization' 
       AND o2.parent_id @> ARRAY[pm.id::text]) AS sub_organization_ids,
    CASE WHEN pm.parent_id IS NULL OR cardinality(pm.parent_id) = 0 
         THEN true 
         ELSE false 
    END AS is_root_organization,
    (SELECT COUNT(*)::int 
     FROM dbo.permission_management u 
     WHERE u.type = 'user' 
       AND u.parent_id @> ARRAY[pm.id::text]) AS total_organization_members_count
FROM dbo.permission_management pm
LEFT JOIN dbo.permission_management o 
    ON o.type = 'organization' 
   AND o.id = CASE WHEN pm.parent_id IS NOT NULL AND array_length(pm.parent_id, 1) >= 1 
                   THEN (pm.parent_id[1])::int 
                   ELSE NULL 
              END
WHERE pm.type = 'organization';
```

### 加入註解
```sql
COMMENT ON COLUMN dbo.vw_permission_organization.id IS '主鍵ID';
COMMENT ON COLUMN dbo.vw_permission_organization.type IS '資料類型：organization';
COMMENT ON COLUMN dbo.vw_permission_organization.organization_name IS '組織名稱';
COMMENT ON COLUMN dbo.vw_permission_organization.organization_description IS '組織描述';
COMMENT ON COLUMN dbo.vw_permission_organization.organization_level IS '組織層級';
COMMENT ON COLUMN dbo.vw_permission_organization.sort_order IS '排序順序';
COMMENT ON COLUMN dbo.vw_permission_organization.is_active IS '是否啟用';
COMMENT ON COLUMN dbo.vw_permission_organization.status IS '狀態';
COMMENT ON COLUMN dbo.vw_permission_organization.created_at IS '建立時間';
COMMENT ON COLUMN dbo.vw_permission_organization.updated_at IS '更新時間';
COMMENT ON COLUMN dbo.vw_permission_organization.public_notes IS '公開備註';
COMMENT ON COLUMN dbo.vw_permission_organization.internal_notes IS '內部備註';
COMMENT ON COLUMN dbo.vw_permission_organization.parent_id IS '上級組織ID';
COMMENT ON COLUMN dbo.vw_permission_organization.created_by IS '建立者ID';
COMMENT ON COLUMN dbo.vw_permission_organization.updated_by IS '更新者ID';
COMMENT ON COLUMN dbo.vw_permission_organization.total_roles_count IS '總角色數量';
COMMENT ON COLUMN dbo.vw_permission_organization.total_users_count IS '總用戶數量';
COMMENT ON COLUMN dbo.vw_permission_organization.total_sub_organizations_count IS '總子組織數量';
COMMENT ON COLUMN dbo.vw_permission_organization.organization_role_ids IS '組織角色ID陣列';
COMMENT ON COLUMN dbo.vw_permission_organization.organization_role_names IS '組織角色名稱陣列';
COMMENT ON COLUMN dbo.vw_permission_organization.organization_user_ids IS '組織用戶ID陣列';
COMMENT ON COLUMN dbo.vw_permission_organization.organization_user_names IS '組織用戶名稱陣列';
COMMENT ON COLUMN dbo.vw_permission_organization.organization_user_full_names IS '組織用戶完整名稱陣列';
COMMENT ON COLUMN dbo.vw_permission_organization.parent_organization_name IS '上級組織名稱';
COMMENT ON COLUMN dbo.vw_permission_organization.parent_organization_id IS '上級組織ID';
COMMENT ON COLUMN dbo.vw_permission_organization.sub_organization_names IS '子組織名稱陣列';
COMMENT ON COLUMN dbo.vw_permission_organization.sub_organization_ids IS '子組織ID陣列';
COMMENT ON COLUMN dbo.vw_permission_organization.is_root_organization IS '是否為根組織';
COMMENT ON COLUMN dbo.vw_permission_organization.total_organization_members_count IS '組織成員總數';
```

---

## 使用方式

1. 如果 View 已存在，先刪除：
```sql
DROP VIEW IF EXISTS dbo.vw_permission_user CASCADE;
DROP VIEW IF EXISTS dbo.vw_permission_role CASCADE;
DROP VIEW IF EXISTS dbo.vw_permission_organization CASCADE;
```

2. 依序執行上述三個 View 的建立 SQL

3. 依序執行上述三個 View 的註解 SQL

4. 驗證：
```sql
SELECT table_name 
FROM information_schema.views 
WHERE table_schema = 'dbo' 
  AND table_name LIKE 'vw_permission_%' 
ORDER BY table_name;
```


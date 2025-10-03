-- 檢查角色資料
SELECT 
    id,
    name,
    description,
    is_active,
    type
FROM permission_managements 
WHERE type = 'role' 
ORDER BY id;

-- 檢查角色視圖
SELECT 
    id,
    name,
    description,
    is_active,
    organization_name,
    role_name
FROM vw_permission_roles 
ORDER BY id;

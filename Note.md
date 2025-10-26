## Backoffice Authentication/Authorization and File Permission Flow

```mermaid
flowchart TD
    A[Request to Backoffice] --> B[Cookie Auth middleware]
    B -->|Unauthenticated| C[Redirect -> /Backoffice/Authorize/Login]
    B -->|Authenticated| D[Routing -> Controller Action]

    D --> E{PermissionAttribute on action/class?}
    E -- Yes --> F[Check IsAuthenticated + SidebarNavigationService for URL allowlist]
    F -- Deny --> G[Redirect -> /Backoffice/Authorize/AccessDenied]
    F -- Allow --> H[Proceed to action]
    E -- No --> H[Proceed to action]

    H --> I{FileManager action?}
    I -- Yes --> J[Controller inline check: User.Identity?.Name null -> 401]
    J --> K[DirectoryManagerUtilities.HasUserPermission(path, user)]
    K -- false --> L[401/403 (Unauthorized/Forbidden)]
    K -- true --> M[Execute operation (Load/List/CRUD/Share/Detail)]

    I -- No --> N[Other controllers handle their logic]

    %% File operations detail
    M --> P[List: Enumerate filesystem and filter by HasUserPermission]
    M --> Q[Detail: GetAppOwners for item; PrimaryOwner derived from owners]
    M --> R[Share: UpdateItemOwners → SetOwnersRecursive(folder) / SetAppOwners(file)]
```

### 補充：帳號/角色/組織（依你提供）
- **帳號**: 以部門/組織有領導者為假設，建立 `horizon-nova`、`horizon-nova-pg` 兩個組織。
- **角色**: 控制可見範圍與後續權限（例如活動、集體功能）。
- **組織**: 管理角色與帳號，也可作為功能集合。



# Feature-oriented Design: File/Folder Search

## Goals
- Composable features (spec-driven), not layer-led branching
- Single pipeline, re-entrant, testable, no JS-generated HTML

## SearchSpec (特性集合)
- rootVirtualPath: string (required)
- query:
  - nameContains?: string
  - pattern?: string (glob-like, optional)
  - type?: 'file' | 'folder' | 'both' (default: both)
- traversal:
  - recursiveMaxDepth?: int (default: 3)
- filters:
  - sizeRange?: (minBytes?: long, maxBytes?: long)
  - mimeTypes?: string[]
  - updatedRangeUtc?: (from?: DateTime, to?: DateTime)
- security:
  - currentUser: string (required)
  - ignoreProtectedPaths: bool (default: true)
  - requireOwnershipIfOwnersPresent: bool (default: true)
- result:
  - sortBy?: 'name' | 'lastWriteUtc' | 'size'
  - direction?: 'asc' | 'desc' (default: asc)
  - offset?: int (default: 0)
  - limit?: int (default: 200)

## Pipeline（單一路徑）
1) Normalize rootVirtualPath → absolute path (safety-checked)
2) Enumerate directories/files (bounded by recursiveMaxDepth)
3) Apply protection ignores (protected regex/patterns)
4) Permission filter (owners + currentUser rule)
5) Map → FileSystemItem (Name, Type, Size, MimeType, CreatedAt, UpdatedAt, VirtualPath, Owners, PrimaryOwner)
6) Apply spec filters (type/name/size/mime/time)
7) Sort + page (sortBy/direction/offset/limit)
8) Return strong-typed list (no HTML)

## Invariants（不變量）
- No JS-generated HTML; Razor renders, JS triggers
- Permissions enforced before projection
- Spec defaults are safe & bounded (depth/limit)
- Idempotent read (same spec → same result set when FS stable)
- Errors bubble to middleware; no try/catch in pipeline

## Interfaces（建議）
```csharp
public sealed class SearchSpec { /* fields above */ }
public sealed class FileSystemItem {
  public string Name; public string Type; public long? Size; public string? MimeType;
  public DateTime CreatedAt; public DateTime UpdatedAt; public string VirtualPath;
  public string[] Owners; public string PrimaryOwner;
}

// Service — pure function of (spec, user)
public List<FileSystemItem> SearchFileSystem(SearchSpec spec);

// Controller — pass-through (no branching)
public IActionResult Search(SearchSpec spec)
  => Json(new { success = true, items = svc.SearchFileSystem(spec) });
```

## Usage（View）
- DataTable 消費 JSON（items），不組裝 HTML
- 詳細/預覽等仍走 `showModal + LoadDetail`（同一 Partial）

## Extensibility
- 新增特性 = 擴充 SearchSpec + 過濾步驟，不改 Controller/View
- 可替換排序/比對器（大小寫、自然排序）

## Non-goals
- 不建立目錄樹快取（以安全與一致性優先）
- 不在 Controller/JS 做條件分支（全部用 spec 驅動）

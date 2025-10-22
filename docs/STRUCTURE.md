# 文檔架構說明

## 目錄結構

```
Horizon_Nova/
└── docs/
    ├── README.md                           文檔導覽（從這裡開始）
    ├── STRUCTURE.md                        本文件（文檔架構說明）
    │
    ├── architecture/                       架構詳細規範
    │   ├── viewbag-model.md               ViewBag vs Model 詳細決策
    │   ├── repository.md                  Repository 層規範
    │   ├── service.md                     Service 層規範
    │   └── controller.md                  Controller 層規範
    │
    ├── ui/                                UI/UX 規範
    │   ├── bootstrap.md                   Bootstrap 5 使用規範
    │   ├── modal.md                       Modal 完整使用指南
    │   ├── ajax.md                        前端 AJAX 規範
    │   ├── forms.md                       表單設計規範
    │   ├── datatable.md                   DataTable 與 HNBDataTable 規範
    │   └── naming.md                      前端命名與文案規範
    │
    └── faq/                               FAQ 與故障排除
        └── development-faq.md             通用開發問題與解決方案
```

---

## 文檔分類

### 1. 全域規範（強制）

**內容：**
- 語言與文案：全站統一使用繁體中文（zh-TW），禁止使用 Emoji
- 命名與風格：遵循各分文件（架構、UI、前端命名）

**適用對象：**
- 全體開發者、文件維護者

---

### 2. 架構詳細（docs/architecture/）

**內容：**
- 每個架構層的詳細規範
- 完整範例
- 決策流程圖
- 特殊情況處理

**適用對象：**
- 深入理解架構設計
- 遇到特殊情況時參考

**檔案大小：** 每個文件 500-1000 行

---

### 3. UI 規範（docs/ui/）

**內容：**
- Bootstrap、Modal、DataTable、表單設計
- 完整範例代碼
- 最佳實踐
- 常見問題

**適用對象：**
- 前端開發
- UI/UX 設計

**檔案大小：** 每個文件 300-800 行

---

<!-- 模組文檔章節已移除，統一以通用規範為主，不針對單一模組撰寫。 -->

### 5. FAQ（docs/faq/）

**內容：**
- 常見問題 Q&A
- 故障排除步驟
- 實用技巧

**適用對象：**
- 使用者自助解決問題
- 開發者疑難排解
- 3-5 年後維護參考

**檔案大小：** 彈性（隨問題累積增加）

---

## 使用情境對照表

| 我想要... | 看哪個文檔 | 章節 |
|----------|-----------|------|
| **判斷用 ViewBag 或 Model** | docs/architecture/viewbag-model.md | 決策流程圖 |
| **開發 Modal** | docs/ui/modal.md | 完整範例 |
| **設計表單** | docs/ui/forms.md | 佈局、驗證、placeholder |
| **AJAX 提交規範** | docs/ui/ajax.md | 標準範本 |
| **前端命名與文案** | docs/ui/naming.md | 函式/ID/按鈕文案 |
| **Repository/Service/Controller 範例** | docs/architecture/repository.md | 結構與命名 |
| **解決「建立後沒看到」** | docs/faq/development-faq.md | 資料查詢 Q1 |
| **解決「權限不生效」** | docs/faq/development-faq.md | ViewBag vs Model Q2 |

---

## 文檔維護原則

### 何時新增文檔？

1. **新增架構規範時** → 在 `docs/architecture/` 建立或更新
2. **新增 UI 元件時** → 在 `docs/ui/` 建立文檔
3. **累積 FAQ 時** → 在 `docs/faq/` 新增或更新

### 何時更新文檔？

1. **架構變更** → 更新 ARCHITECTURE.md
2. **規範調整** → 更新對應的子文檔
3. **遇到新問題** → 在 FAQ 中新增 QA
4. **模組功能異動** → 更新對應模組文檔

### 文檔撰寫標準

1. **標題清晰** - 使用 H2-H6 層級分明
2. **範例豐富** - 每個規則都有實際代碼範例
3. **包含 FAQ** - 記錄常見問題和解決方案
4. **維護日期** - 在文檔底部記錄「最後更新」日期
5. **交叉引用** - 使用相對連結連接相關文檔

---

## 文檔同步策略

### 子文檔（docs/）為唯一來源
- 包含詳細內容、範例、FAQ
- 定期審查和更新
- 確保與主文件規範一致

### FAQ 文檔
- 持續累積實際遇到的問題
- 每個 QA 包含：問題描述、原因分析、解決方案、相關代碼
- 按重要性和頻率排序

---

## 文檔統計

| 類別 | 檔案數 | 總行數（預估） |
|------|--------|---------------|
| **架構詳細** | 4 | ~900 |
| **UI 規範** | 6 | ~1200 |
| **FAQ** | 1 | 彈性 |
| **總計** | 11 | ~2100+ |

---

<!-- 取消『未來擴展計劃』段落，避免誤導與承諾未實作內容。 -->

## 最佳實踐

### 開發新功能時
1. 查 docs/architecture/（Repository/Service/Controller 規範）
2. 查 docs/ui/（UI、表單、AJAX、DataTable、命名）
3. 參考 FAQ（避免已知問題）

### 遇到問題時
1. 先查 FAQ（通用開發問題）
2. 查對應規範（architecture/ui）確認是否違反
3. 解決後將問題加入 FAQ

### 維護文檔時
1. 問題解決後立即更新 FAQ
2. 架構變更後立即更新規範
3. 定期審查文檔（每季一次）

---

最後更新：2025-10-22


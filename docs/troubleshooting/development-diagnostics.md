# 故障診斷（Diagnostics）

## 快速排錯（多層次分診）
1. 重現步驟：能穩定重現嗎？輸入、路徑、環境一致嗎？
2. 錯誤來源定位：
   - 前端：Console / Network（請求是否發出？回應 HTTP/JSON？）
   - 後端：中介軟體/全域例外、Controller 是否被打到？
   - 資料：Service/Repository 的輸入參數、回傳是否為 null？
3. 規範對齊：
   - Modal 是否走 `LoadDetail` 並回傳同一 Partial？
   - 有無在 JS 組裝 HTML？（禁止）
   - 列表是否用 `@model`？單值是否用 `ViewBag`？
4. 界面/流程：
   - 是否先畫面→Controller→Service→Repository 自上而下檢查？
   - 是否有遺漏 Anti-Forgery Token？
5. 記錄：貼上最小重現、請求/回應片段、相關日誌，避免單點猜測。

---

## 目錄
- [架構相關](#架構相關)
- [Modal 開發](#modal-開發)
- [ViewBag vs Model](#viewbag-vs-model)
- [資料查詢](#資料查詢)
- [AJAX 請求](#ajax-請求)

---

## 架構相關

### Q1：為什麼要使用三層架構？

回答：
1. 職責分離
2. 易於測試
3. 易於維護
4. 易於擴展

相關文檔：
- ARCHITECTURE.md - 概述

---

## Modal 開發

### Q1：為什麼 Modal 只能開一次？
原因：使用了錯誤的開關方式（如 `.show()`/`.hide()`）。

解決：只用 `showModal()/closeModal()`。

相關文檔：
- Modal 規範（原則與決策）

### Q2：為什麼 Modal 關閉後頁面無法捲動？
原因：未正確恢復 body 滾動。

解決：使用正確的關閉方式（data-bs-dismiss 或 `closeModal()`）。

### Q3：Modal 內圖標（Lucide）沒有顯示？
原因：AJAX 動態載入後未重新初始化。

解決：`Modal.js` 會在 `shown.bs.modal` 自動 `lucide.createIcons()`。

### Q4：Modal 應該寫在主頁面還是 Partial View？
決策規則：
| 場景 | 做法 | 原因 |
|------|------|------|
| 需要動態資料（表單、詳情） | Partial View + AJAX 載入 | 資料即時、頁面載入快 |
| 完全靜態（固定說明） | 主頁面 | 無需 AJAX 請求 |

範例（不需要額外 container）：
```cshtml
@await Html.PartialAsync("Partials/_EntityModal")
<button type="button" class="js-open-entity-form">開啟</button>
```

---

## ViewBag vs Model

快速判斷：
```
需要 @foreach？ → Model
需要多區塊條件判斷？ → Model
單一值/統計數字？ → ViewBag
```

詳細規則：
- ViewBag vs Model 完整決策

---

## 資料查詢

### Q1：為什麼不能用 Get 命名？
統一：Repository `Query*`，Service `Load*`。避免與 HTTP GET 混淆。

### Q2：為什麼查詢結果為 null 而不是空列表？
`Find()` 等找不到資料會是 null；於 View 使用 null-coalescing。

---

## AJAX 請求

### Q1：為什麼 AJAX 請求失敗？
常見原因：URL 錯、資料格式錯、Action 不存在或參數不匹配。

### Q2：為什麼表單提交後沒有驗證？
使用 HTML5 原生驗證（`required`、`type="email"` 等），通過後再 AJAX。

---

## 相關文檔
- [ARCHITECTURE.md](../../ARCHITECTURE.md)
- [ViewBag vs Model 決策](../architecture/viewbag-model.md)
- [Modal 規範（原則與決策）](../frontend/modal.md)
- [Bootstrap 使用規範](../frontend/bootstrap.md)

---

最後更新：2025-11-10


# Bootstrap 使用指南 - Weather Area

## 核心原則

### 1. 不要使用 @media queries
- **錯誤**：使用 `@media` queries 來處理響應式
- **正確**：依賴 Bootstrap 的 grid 系統和工具類別自動適應所有螢幕尺寸
- **原因**：既然已經使用 Bootstrap，就不需要手動寫 media queries

### 2. 不要使用響應式切換類別
- **錯誤**：使用 `d-none d-md-block` 和 `d-md-none` 來切換顯示/隱藏
- **錯誤**：為不同螢幕尺寸創建重複的元素
  ```html
  <!-- 錯誤範例 -->
  <h2 class="display-3 d-none d-md-block">Title</h2>
  <h2 class="display-5 d-md-none">Title</h2>
  ```
- **正確**：使用單一元素，讓 Bootstrap 自動調整
  ```html
  <!-- 正確範例 -->
  <h2 class="display-3">Title</h2>
  ```
- **原因**：Bootstrap 的 grid 系統和字體大小類別會自動適應，不需要手動切換

### 3. 正確使用 Grid 系統
- **正確結構**：
  ```html
  <div class="container">
      <div class="row g-4">
          <div class="col-12 col-md-6">
              <!-- 內容 -->
          </div>
          <div class="col-12 col-md-6">
              <!-- 內容 -->
          </div>
      </div>
  </div>
  ```
- **錯誤**：在 `row` 上使用 `h-100` 或 `d-flex`
- **錯誤**：在 `col` 內部使用不必要的 `d-flex` 容器
- **原因**：會干擾 Bootstrap grid 的正常運作，導致左右區塊無法正確並排

### 4. 避免不必要的 d-flex
- **錯誤**：在外層容器使用 `d-flex`
  ```html
  <!-- 錯誤範例 -->
  <div class="entry-page-container d-flex">
      <div class="container d-flex flex-column">
  ```
- **正確**：只在必要的地方使用 `d-flex`（例如按鈕內部居中）
- **原因**：`d-flex` 會導致畫面上下圖層出問題

### 5. 使用間距工具類別
- **正確**：使用 `row g-*` 處理左右區塊間距
- **正確**：使用 `p-*`、`m-*`、`mb-*`、`mt-*` 處理內部間距
- **正確**：使用 `gap-*` 處理 flex 容器內的間距（僅在必要時）
- **錯誤**：使用 `d-flex` 的 `gap-*` 來處理所有間距
- **範例**：
  ```html
  <!-- 正確：使用 mb-3 而不是 d-flex gap-3 -->
  <input class="form-control mb-3">
  <button class="btn">Submit</button>
  ```

### 6. CSS 只處理顏色
- **正確**：CSS 只用於定義顏色、背景圖片等非布局屬性
- **錯誤**：在 CSS 中寫布局相關的樣式（width、height、margin、padding、display 等）
- **範例**：
  ```css
  /* 正確 */
  .entry-page-container {
      background-color: #d9d4c0;
      background-image: url('/Weather/image/section-c-bg.png');
  }
  
  /* 錯誤 */
  .entry-page-container {
      display: flex;
      width: 100%;
      height: 100vh;
  }
  ```

### 7. 單一元素原則
- **正確**：每個內容只創建一個元素
- **錯誤**：為不同螢幕尺寸創建多個元素
- **原因**：Bootstrap 會自動處理響應式，不需要重複元素

### 8. 正確的層級結構
- **正確**：
  ```
  container
    └── row g-*
        ├── col-12 col-md-6
        │   └── 內容直接放在這裡
        └── col-12 col-md-6
            └── 內容直接放在這裡
  ```
- **錯誤**：在 `col` 內部添加不必要的 `d-flex` 容器

## 常見錯誤範例

### 錯誤 1：使用 @media queries
```css
/* 錯誤 */
@media (max-width: 768px) {
    .title {
        font-size: 2rem;
    }
}
```

### 錯誤 2：重複元素
```html
<!-- 錯誤 -->
<h2 class="display-3 d-none d-md-block">Title</h2>
<h2 class="display-5 d-md-none">Title</h2>
```

### 錯誤 3：在 row 上使用 d-flex
```html
<!-- 錯誤 -->
<div class="row g-4 h-100 d-flex">
```

### 錯誤 4：在 col 內部使用不必要的 flex
```html
<!-- 錯誤 -->
<div class="col-12 col-md-6">
    <div class="d-flex flex-column justify-content-between">
        <!-- 內容 -->
    </div>
</div>
```

## 正確範例

### 範例 1：兩欄布局
```html
<div class="container p-5">
    <div class="row g-4">
        <div class="col-12 col-md-6">
            <h2 class="display-3">Left Content</h2>
            <p class="mb-4">Description</p>
        </div>
        <div class="col-12 col-md-6">
            <div class="text-center">
                <p class="mb-3">Right Content</p>
            </div>
        </div>
    </div>
</div>
```

### 範例 2：按鈕內部居中（必要時使用 d-flex）
```html
<button class="upload-button d-flex flex-column align-items-center justify-content-center gap-2">
    <span class="upload-icon">+</span>
    <span>Upload</span>
</button>
```

### 範例 3：間距處理
```html
<!-- 正確：使用 mb-3 而不是 d-flex gap-3 -->
<div>
    <input class="form-control mb-3">
    <button class="btn w-100">Submit</button>
</div>
```



## 重要提醒

**這是商品級別的代碼，會影響 1000+ 使用者。**
- 結構必須正確
- 不能使用「能動就好」的做法
- 必須遵循 Bootstrap 的設計理念
- 必須通過所有檢查清單項目


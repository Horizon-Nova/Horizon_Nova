# HNB DropZone 使用說明

## 概述

HNB DropZone 是一個通用的拖放套件，支援檔案和資料夾的拖放上傳功能。使用 HTML 屬性配置，無需 JavaScript 代碼即可啟用拖放功能。

## 基本使用

### 1. 基本拖放區域

```html
<div class="dropZone" style="min-height: 120px;">
    <div class="dropZone-content">
        <i data-lucide="upload-cloud" class="dropZone-icon text-muted"></i>
        <div class="dropZone-text fw-semibold">拖曳檔案到這裡</div>
        <div class="dropZone-hint">支援單個或多個檔案</div>
    </div>
</div>
```

### 2. 自動初始化

套件會自動初始化所有具有 `class="dropZone"` 的元素，無需額外的 JavaScript 代碼。

## 配置選項

### HTML 屬性配置

| 屬性 | 說明 | 範例 |
|------|------|------|
| `data-max-files` | 最大檔案數量限制 | `data-max-files="5"` |
| `data-max-size` | 最大檔案大小限制（bytes） | `data-max-size="5242880"` |
| `data-allowed-types` | 允許的檔案類型 | `data-allowed-types="image/*,application/pdf"` |
| `data-upload-url` | 上傳 URL | `data-upload-url="/api/upload"` |
| `data-auto-upload` | 是否自動上傳 | `data-auto-upload="true"` |
| `data-disabled` | 是否禁用拖放 | `data-disabled="true"` |

### 回調函數配置

| 屬性 | 說明 | 範例 |
|------|------|------|
| `data-on-drop` | 拖放完成回調函數 | `data-on-drop="handleDrop"` |
| `data-on-error` | 錯誤處理回調函數 | `data-on-error="handleError"` |
| `data-on-upload-start` | 上傳開始回調函數 | `data-on-upload-start="handleUploadStart"` |
| `data-on-upload-progress` | 上傳進度回調函數 | `data-on-upload-progress="handleProgress"` |
| `data-on-upload-complete` | 上傳完成回調函數 | `data-on-upload-complete="handleComplete"` |
| `data-on-upload-error` | 上傳錯誤回調函數 | `data-on-upload-error="handleUploadError"` |

## 使用範例

### 1. 基本拖放區域

```html
<div class="dropZone" style="min-height: 120px;">
    <div class="dropZone-content">
        <i data-lucide="upload-cloud" class="dropZone-icon text-muted"></i>
        <div class="dropZone-text fw-semibold">拖曳檔案到這裡</div>
        <div class="dropZone-hint">支援單個或多個檔案</div>
    </div>
</div>
```

### 2. 限制檔案類型

```html
<div class="dropZone" 
     data-allowed-types="image/*" 
     style="min-height: 120px;">
    <div class="dropZone-content">
        <i data-lucide="image" class="dropZone-icon text-muted"></i>
        <div class="dropZone-text fw-semibold">只接受圖片檔案</div>
        <div class="dropZone-hint">支援 JPG, PNG, GIF 等格式</div>
    </div>
</div>
```

### 3. 限制檔案大小

```html
<div class="dropZone" 
     data-max-size="5242880" 
     style="min-height: 120px;">
    <div class="dropZone-content">
        <i data-lucide="file" class="dropZone-icon text-muted"></i>
        <div class="dropZone-text fw-semibold">檔案大小限制</div>
        <div class="dropZone-hint">單個檔案最大 5MB</div>
    </div>
</div>
```

### 4. 限制檔案數量

```html
<div class="dropZone" 
     data-max-files="3" 
     style="min-height: 120px;">
    <div class="dropZone-content">
        <i data-lucide="files" class="dropZone-icon text-muted"></i>
        <div class="dropZone-text fw-semibold">檔案數量限制</div>
        <div class="dropZone-hint">最多只能上傳 3 個檔案</div>
    </div>
</div>
```

### 5. 自動上傳

```html
<div class="dropZone" 
     data-auto-upload="true" 
     data-upload-url="/api/upload" 
     style="min-height: 120px;">
    <div class="dropZone-content">
        <i data-lucide="upload" class="dropZone-icon text-muted"></i>
        <div class="dropZone-text fw-semibold">自動上傳測試</div>
        <div class="dropZone-hint">拖放檔案自動上傳</div>
    </div>
</div>
```

### 6. 禁用狀態

```html
<div class="dropZone" 
     data-disabled="true" 
     style="min-height: 120px;">
    <div class="dropZone-content">
        <i data-lucide="upload-cloud" class="dropZone-icon text-muted"></i>
        <div class="dropZone-text fw-semibold">拖放功能已禁用</div>
        <div class="dropZone-hint">此區域無法使用拖放功能</div>
    </div>
</div>
```

## 進度顯示

### 進度 Modal

套件支援進度 Modal 顯示，需要在頁面中包含以下 HTML：

```html
<!-- 上傳進度 Modal -->
<div id="uploadProgress" class="modal fade" tabindex="-1" aria-labelledby="uploadProgressLabel" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="uploadProgressLabel">正在上傳</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="關閉"></button>
            </div>
            <div class="modal-body" style="max-height:300px; overflow:auto;">
                <div id="uploadItems" class="vstack gap-2">
                    <!-- 由 JavaScript 動態生成 -->
                </div>
            </div>
            <div class="modal-footer justify-content-between small text-muted">
                <span id="uploadSpeed">0 KB/s</span>
                <span id="uploadEta">計算中...</span>
            </div>
        </div>
    </div>
</div>
```

### 進度顯示行為

- **有進度 Modal**：顯示進度彈出視窗，包含檔案列表、進度條、速度和剩餘時間
- **無進度 Modal**：記錄到 console（開發模式）

## 回調函數

### 基本回調

```javascript
function handleDrop(filesWithPaths) {
    console.log('拖放完成，檔案數量：', filesWithPaths.length);
    filesWithPaths.forEach(fileWithPath => {
        console.log('檔案：', fileWithPath.path, '大小：', fileWithPath.file.size);
    });
}

function handleError(errorMessage) {
    console.error('拖放錯誤：', errorMessage);
    alert('錯誤：' + errorMessage);
}
```

### 上傳回調

```javascript
function handleUploadStart(filesWithPaths) {
    console.log('開始上傳，檔案數量：', filesWithPaths.length);
}

function handleProgress(progressData) {
    console.log('上傳進度：', progressData.percent + '%');
}

function handleComplete(results) {
    console.log('上傳完成：', results);
}

function handleUploadError(error) {
    console.error('上傳錯誤：', error);
}
```

## 樣式自定義

### CSS 類別

| 類別 | 說明 |
|------|------|
| `.dropZone` | 拖放區域基本樣式 |
| `.dropZone-active` | 拖拽時的樣式 |
| `.dropZone-uploading` | 上傳中的樣式 |
| `.dropZone-content` | 拖放區域內容容器 |
| `.dropZone-icon` | 圖示樣式 |
| `.dropZone-text` | 主要文字樣式 |
| `.dropZone-hint` | 提示文字樣式 |

### 自定義樣式範例

```css
.dropZone {
    border: 2px dashed #dee2e6;
    border-radius: 0.375rem;
    transition: all 0.3s ease;
}

.dropZone-active {
    border-color: #0d6efd;
    background-color: rgba(13, 110, 253, 0.1);
}

.dropZone-uploading {
    border-color: #198754;
    background-color: rgba(25, 135, 84, 0.1);
}
```

## 功能特色

### 1. 零 JavaScript 配置
- 完全使用 HTML 屬性配置
- 自動初始化所有 `dropZone` 元素
- 無需手動編寫 JavaScript 代碼

### 2. 拖放和點擊支援
- 支援拖拽檔案到區域
- 支援點擊區域選擇檔案
- 支援資料夾結構拖放

### 3. 檔案驗證
- 檔案數量限制
- 檔案大小限制
- 檔案類型限制
- 自定義驗證規則

### 4. 進度顯示
- 進度 Modal 支援
- 實時進度更新
- 上傳速度和剩餘時間
- 多檔案並行上傳

### 5. 回調函數
- 完整的回調函數支援
- 自定義錯誤處理
- 上傳生命週期回調

## 注意事項

1. **依賴套件**：需要 jQuery 和 Bootstrap 5
2. **進度 Modal**：需要 Bootstrap Modal 支援
3. **檔案驗證**：在拖放時進行驗證，不符合條件的檔案會被拒絕
4. **自動上傳**：只有設定 `data-auto-upload="true"` 和 `data-upload-url` 才會自動上傳
5. **回調函數**：回調函數必須是全域函數，可以通過 `data-on-*` 屬性指定

## 瀏覽器支援

- Chrome 60+
- Firefox 55+
- Safari 12+
- Edge 79+

## 更新日誌

### v1.0.0
- 初始版本發布
- 支援基本拖放功能
- 支援 HTML 屬性配置
- 支援進度 Modal 顯示
- 支援檔案驗證和回調函數

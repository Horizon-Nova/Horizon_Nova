# ObjectDetection API 測試

## 安裝依賴

```bash
pip install requests
```

## 使用方法

### 1. 準備測試圖片
將測試圖片命名為 `test_image.jpg` 並放在 PYTEST 目錄下，或修改 `test_object_detection_api.py` 中的 `TEST_IMAGE_PATH` 變數。

### 2. 修改 API URL
根據您的實際運行端口，修改 `BASE_URL`：
```python
BASE_URL = "https://localhost:7024"  # 修改為您的端口
```

### 3. 運行測試
```bash
cd PYTEST
python test_object_detection_api.py
```

## 測試項目

1. **test_status()** - 檢查服務狀態
2. **test_detect_image()** - 基礎檢測（上傳圖片）
3. **test_detect_base64()** - 基礎檢測（base64）
4. **test_detect_and_render_image()** - 檢測並渲染（圖片 → URL）
5. **test_detect_and_render_base64()** - 檢測並渲染（base64 → base64）

## 自動下載功能

**首次使用時，系統會自動下載 AI 模型：**
- 當首次調用檢測 API 時，如果模型不存在，系統會自動從遠程服務器下載
- 模型文件：`groundingdino.onnx` (662MB)
- 詞彙表：`vocab.txt` (256KB)
- 下載來源：https://horizon-nova.up.railway.app/storage/AI/

下載過程是異步的，在背景執行。首次運行時：
1. 調用任何檢測 API 會自動觸發下載
2. 使用 Status API 查看下載進度
3. 下載完成後，系統會自動初始化模型並可開始使用

## API 端點

### 狀態檢查
- `GET /api/ObjectDetectionApi/Status` - 檢查服務狀態（包含 isReady, message, isDownloading）

### 檢測功能（首次調用會自動下載模型）
- `POST /api/ObjectDetectionApi/Detect` - 基礎檢測（圖片文件）
- `POST /api/ObjectDetectionApi/DetectBase64` - 基礎檢測（base64）
- `POST /api/ObjectDetectionApi/DetectImage` - 檢測並渲染（圖片 → URL）
- `POST /api/ObjectDetectionApi/DetectImageBase64` - 檢測並渲染（base64 → base64）

### 手動下載（選用）
- `POST /api/ObjectDetectionApi/DownloadModels` - 手動觸發下載（異步）
- `POST /api/ObjectDetectionApi/DownloadModelsSync` - 手動觸發下載（同步）


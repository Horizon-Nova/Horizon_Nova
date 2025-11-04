"""
物件辨識 API 測試
測試 ObjectDetectionApi 的各個端點
"""
import requests
import base64
import os
from pathlib import Path

# API 基礎 URL
BASE_URL = "http://localhost:5255"  # 根據您的實際端口修改
API_URL = f"{BASE_URL}/api/ObjectDetectionApi"

# 測試圖片路徑（請根據實際情況調整）
TEST_IMAGE_PATH = "Test.jpg"

# 檢測目標（衣服、褲子）
TEXT_PROMPT = "shirt . clothes . pants . trousers . shorts ."


def test_status():
    """測試服務狀態"""
    print("=" * 60)
    print("測試 1: 檢查服務狀態")
    print("=" * 60)
    
    url = f"{API_URL}/Status"
    response = requests.get(url)
    
    print(f"請求 URL: {url}")
    print(f"狀態碼: {response.status_code}")
    
    if response.status_code == 200:
        print(f"回應內容: {response.json()}")
    else:
        print(f"錯誤: {response.text}")
    print()
    
    assert response.status_code == 200
    data = response.json()
    print(f"[OK] 服務狀態: {'就緒' if data.get('isReady') else '未就緒'}")
    print(f"     訊息: {data.get('message')}")
    print(f"     下載中: {'是' if data.get('isDownloading') else '否'}")
    print()
    
    return data


def test_detect_image():
    """測試圖片檢測（上傳文件）- 只返回檢測結果"""
    print("=" * 60)
    print("測試 2: 基礎檢測（上傳圖片文件）")
    print("=" * 60)
    
    if not os.path.exists(TEST_IMAGE_PATH):
        print(f"[!] 測試圖片不存在: {TEST_IMAGE_PATH}")
        print("    請準備一張測試圖片並更新路徑")
        return
    
    url = f"{API_URL}/Detect"
    files = {'image': open(TEST_IMAGE_PATH, 'rb')}
    data = {'textPrompt': TEXT_PROMPT}
    
    response = requests.post(url, files=files, data=data, )
    
    print(f"請求 URL: {url}")
    print(f"狀態碼: {response.status_code}")
    
    if response.status_code == 200:
        result = response.json()
        print(f"成功: {result.get('success')}")
        if result.get('success'):
            results = result.get('results', [])
            print(f"[OK] 檢測到 {len(results)} 個物件:")
            for i, obj in enumerate(results, 1):
                print(f"     {i}. {obj.get('label')} (信心度: {obj.get('score'):.2%})")
        else:
            print(f"[X] 錯誤: {result.get('error')}")
    else:
        print(f"[X] 請求失敗: {response.text}")
    print()


def test_detect_base64():
    """測試 base64 檢測 - 只返回檢測結果"""
    print("=" * 60)
    print("測試 3: 基礎檢測（base64）")
    print("=" * 60)
    
    if not os.path.exists(TEST_IMAGE_PATH):
        print(f"[!] 測試圖片不存在: {TEST_IMAGE_PATH}")
        return
    
    # 讀取圖片並轉換為 base64
    with open(TEST_IMAGE_PATH, 'rb') as f:
        image_data = f.read()
        image_base64 = base64.b64encode(image_data).decode('utf-8')
    
    url = f"{API_URL}/DetectBase64"
    payload = {
        'imageBase64': f"data:image/jpeg;base64,{image_base64}",
        'textPrompt': TEXT_PROMPT
    }
    
    response = requests.post(url, json=payload, )
    
    print(f"請求 URL: {url}")
    print(f"狀態碼: {response.status_code}")
    
    if response.status_code == 200:
        result = response.json()
        print(f"成功: {result.get('success')}")
        if result.get('success'):
            results = result.get('results', [])
            print(f"[OK] 檢測到 {len(results)} 個物件:")
            for i, obj in enumerate(results, 1):
                print(f"     {i}. {obj.get('label')} (信心度: {obj.get('score'):.2%})")
        else:
            print(f"[X] 錯誤: {result.get('error')}")
    else:
        print(f"[X] 請求失敗: {response.text}")
    print()


def test_detect_and_render_image():
    """測試檢測並渲染圖片（上傳文件）- 返回圖片 URL"""
    print("=" * 60)
    print("測試 4: 檢測並返回渲染圖片（上傳文件 → 返回 URL）")
    print("=" * 60)
    
    if not os.path.exists(TEST_IMAGE_PATH):
        print(f"[!] 測試圖片不存在: {TEST_IMAGE_PATH}")
        return
    
    url = f"{API_URL}/DetectImage"
    files = {'image': open(TEST_IMAGE_PATH, 'rb')}
    data = {'textPrompt': TEXT_PROMPT}
    
    response = requests.post(url, files=files, data=data, )
    
    print(f"請求 URL: {url}")
    print(f"狀態碼: {response.status_code}")
    
    if response.status_code == 200:
        result = response.json()
        print(f"成功: {result.get('success')}")
        if result.get('success'):
            detections = result.get('detections', [])
            image_url = result.get('imageUrl')
            print(f"[OK] 檢測到 {len(detections)} 個物件")
            print(f"[OK] 渲染圖片 URL: {BASE_URL}{image_url}")
            for i, obj in enumerate(detections, 1):
                print(f"     {i}. {obj.get('label')} (信心度: {obj.get('score'):.2%})")
        else:
            print(f"[X] 錯誤: {result.get('error')}")
    else:
        print(f"[X] 請求失敗: {response.text}")
    print()


def test_detect_and_render_base64():
    """測試檢測並渲染圖片（base64）- 返回 base64"""
    print("=" * 60)
    print("測試 5: 檢測並返回渲染圖片（base64 → 返回 base64）")
    print("=" * 60)
    
    if not os.path.exists(TEST_IMAGE_PATH):
        print(f"[!] 測試圖片不存在: {TEST_IMAGE_PATH}")
        return
    
    # 讀取圖片並轉換為 base64
    with open(TEST_IMAGE_PATH, 'rb') as f:
        image_data = f.read()
        image_base64 = base64.b64encode(image_data).decode('utf-8')
    
    url = f"{API_URL}/DetectImageBase64"
    payload = {
        'imageBase64': f"data:image/jpeg;base64,{image_base64}",
        'textPrompt': TEXT_PROMPT
    }
    
    response = requests.post(url, json=payload, )
    
    print(f"請求 URL: {url}")
    print(f"狀態碼: {response.status_code}")
    
    if response.status_code == 200:
        result = response.json()
        print(f"成功: {result.get('success')}")
        if result.get('success'):
            detections = result.get('detections', [])
            image_base64_result = result.get('imageBase64')
            print(f"[OK] 檢測到 {len(detections)} 個物件")
            print(f"[OK] 返回 base64 長度: {len(image_base64_result)} 字符")
            for i, obj in enumerate(detections, 1):
                print(f"     {i}. {obj.get('label')} (信心度: {obj.get('score'):.2%})")
            
            # 可選：保存渲染後的圖片
            save_path = "rendered_result.png"
            with open(save_path, 'wb') as f:
                f.write(base64.b64decode(image_base64_result))
            print(f"[OK] 渲染圖片已保存至: {save_path}")
        else:
            print(f"[X] 錯誤: {result.get('error')}")
    else:
        print(f"[X] 請求失敗: {response.text}")
    print()


def main():
    """執行所有測試"""
    print("\n")
    print("╔" + "═" * 58 + "╗")
    print("║" + " " * 12 + "物件辨識 API 測試套件" + " " * 24 + "║")
    print("╚" + "═" * 58 + "╝")
    print()
    
    # HTTP 不需要禁用 SSL 警告
    # import urllib3
    # urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)
    
    try:
        # 測試 1: 檢查服務狀態
        status = test_status()
        
        # 如果正在下載，提示用戶等待
        if status.get('isDownloading'):
            print("[提示] 模型正在自動下載中...")
            print("       groundingdino.onnx (662MB) - 需要較長時間")
            print("       請稍後再運行測試")
            return
        
        # 如果未就緒且未下載，說明需要觸發檢測來啟動自動下載
        if not status.get('isReady'):
            print("[提示] 模型尚未就緒")
            print("       首次調用檢測 API 時會自動從遠程下載模型")
            print("       請繼續運行測試...")
            print()
        
        # 測試 2: 基礎檢測（圖片文件）- 會自動觸發下載
        test_detect_image()
        
        # 測試 3: 基礎檢測（base64）
        test_detect_base64()
        
        # 測試 4: 檢測並渲染（圖片 → URL）
        test_detect_and_render_image()
        
        # 測試 5: 檢測並渲染（base64 → base64）
        test_detect_and_render_base64()
        
        print("=" * 60)
        print("[OK] 所有測試完成！")
        print("=" * 60)
        
    except Exception as e:
        print(f"\n[X] 測試過程中發生錯誤: {str(e)}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    main()


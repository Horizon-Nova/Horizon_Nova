"""
簡單測試下載 AI 模型文件
"""
import requests
from urllib.parse import quote

base = "https://horizon-nova.up.railway.app/storage/AI/"

print("測試路徑可訪問性:")
print(f"  vocab.txt: {requests.head(base + 'vocab.txt', timeout=10).status_code}")
print(f"  groundingdino.onnx: {requests.head(base + 'groundingdino.onnx', timeout=10).status_code}")
print(f"  groundingdino (1).onnx: {requests.head(base + quote('groundingdino (1).onnx'), timeout=10).status_code}")

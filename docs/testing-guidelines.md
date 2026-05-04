## 測試驗證記錄

### 使用方式
- **記錄格式**：`[時間] 測試項目 > 測試方法 > 步驟1 > 步驟2 > ... > 結果 > [失敗原因/修復方法]`
- **避免重複失敗**：測試前必須查看之前的測試記錄，避免重複使用已經失敗的測試方式
- **時間格式**：YYYY-MM-DD HH:MM 或 YYYY-MM-DD
### 範例：
[2024-01-01 10:00] 測試環境 > cmd > python --version > 成功
[2024-01-01 10:05] 測試環境 > cmd > pip install torch > 失敗 > 權限不足
[2024-01-01 10:10] 測試環境 > shell > pip install torch > 成功

### 測試記錄（時間軸格式）
[2026-05-04 19:06] /WW/Landing 匯入區塊(左側) > powershell > dotnet build Net/HNB/HNB.sln -c Release > [成功]
[2026-05-04 19:06] /WW/Landing 匯入區塊(左側) > powershell > dotnet run Net/HNB/HNB.csproj --urls http://localhost:5055 (背景) > Invoke-WebRequest http://localhost:5055/WW/Landing > [成功] 200 且包含 ww-landing-upload-zone
[2026-05-04 19:13] /WW/Closet 匯入區塊(左側) > powershell > dotnet build Net/HNB/HNB.sln -c Debug > [失敗] HNB.exe 被執行中程序鎖定（HNB 18572）
[2026-05-04 19:13] /WW/Closet 匯入區塊(左側) > powershell > dotnet build Net/HNB/HNB.sln -c Release > [成功]

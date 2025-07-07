@echo off
chcp 65001 >nul

::----------------------------------------
:: [步驟 0] 提示開始
::----------------------------------------
echo ==============================================
echo [Git Script] - Auto Commit and Force Push
echo ==============================================
echo.
::----------------------------------------
:: [步驟 1] 自動 Commit 並 Push 所有變更
::----------------------------------------
echo.
echo [步驟 1] 自動加入所有變更 (git add .)...
git add .

:: 使用 PowerShell 生成安全的提交訊息
for /f "usebackq tokens=*" %%i in (
  `powershell -NoProfile -Command "[DateTime]::Now.ToString('yyyy-MM-dd_HH-mm-ss')"`
) do set "commit_msg=%%i"

echo [步驟 2] Commit 所有變更，訊息為：%commit_msg%...
git commit -m "%commit_msg%" || echo [無變更需要提交]

echo [步驟 3] 將變更推送到遠端 (HNB-Deploy)...
git push origin HNB-Deploy --force

::----------------------------------------
:: [結束] - 統一在此結束腳本
::----------------------------------------

echo.
echo [所有動作完成！]
exit /b

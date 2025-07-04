@echo off
chcp 65001 >nul

REM ============================================================
REM [說明]
REM 這支批次檔用於匯入 Visual Studio 的個人化設定 (vssettings) 檔案。
REM 執行流程：
REM   1. 檢查指定的 .vssettings 檔案是否存在
REM   2. 呼叫 Visual Studio (devenv.exe) 以 /resetsettings 將設定匯入
REM   3. 透過 pause 暫停視窗，方便查看結果
REM ============================================================

echo 正在安裝 Visual Studio 設定...

REM ------------------------------------------------------------
REM [區域] 設定檔路徑檢查
REM ------------------------------------------------------------
set VSSETTINGS_FILE=./Exported-2025-01-13.vssettings

if not exist "%VSSETTINGS_FILE%" (
    echo 錯誤: 找不到設定檔 %VSSETTINGS_FILE%。
    pause
    exit /b 1
)

REM ------------------------------------------------------------
REM [區域] 匯入 Visual Studio 設定
REM ------------------------------------------------------------
echo 匯入 Visual Studio 設定...
devenv.exe /resetsettings "%VSSETTINGS_FILE%"

echo 設定已完成！
pause

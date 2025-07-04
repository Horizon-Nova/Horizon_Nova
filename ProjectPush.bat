@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

rem ===================== 使用者可調整區 =====================
set "BASE_DIR=%~dp0"
set "PROJECT_NAME=MISSA-Deploy"
set "GIT_REPO=https://github.com/Horizon-Nova/MISSA.git"
set "GIT_BRANCH=MISSA-Deploy"
set "CS_PROJ_PATH=%BASE_DIR%MISSA.csproj"
set "PUBLISH_DIR=%BASE_DIR%publish\Release"
set "DEPLOY_DIR=%BASE_DIR%%PROJECT_NAME%\MISSA-Deploy"
rem ==========================================================

rem [Step1] 檢查並更新 Git 儲存庫
echo [Step1] 檢查^ %PROJECT_NAME% 是否存在
if not exist "%BASE_DIR%%PROJECT_NAME%\.git" (
    echo [訊息] 專案目錄不存在或非 Git 倉庫，重新 clone...
    rd /s /q "%BASE_DIR%%PROJECT_NAME%" 2>nul
    git clone -b %GIT_BRANCH% %GIT_REPO% "%BASE_DIR%%PROJECT_NAME%"
) else (
    echo [訊息] Git 倉庫存在，執行 git pull...
    cd /d "%BASE_DIR%%PROJECT_NAME%" && git pull
)

rem [Step2] 清空發佈與部屬資料夾
echo [Step2] 清空目錄 %PUBLISH_DIR% 與 %DEPLOY_DIR%
rd /s /q "%PUBLISH_DIR%" 2>nul
mkdir "%PUBLISH_DIR%"
rd /s /q "%DEPLOY_DIR%" 2>nul
mkdir "%DEPLOY_DIR%"

rem [Step3] 執行 dotnet publish
echo [Step3] 執行 dotnet publish 到 %PUBLISH_DIR%
dotnet publish "%CS_PROJ_PATH%" -c Release -o "%PUBLISH_DIR%" || (
    echo [錯誤] dotnet publish 失敗！
    pause
    exit /b
)
echo [完成] dotnet publish 成功！

rem [Step4] 複製發佈檔案到部屬資料夾
echo [Step4] 複製檔案至 %DEPLOY_DIR%
robocopy "%PUBLISH_DIR%" "%DEPLOY_DIR%" /E /NFL /NDL /NJH /NJS /R:0
set ERRORLEVEL=%ERRORLEVEL%
if %ERRORLEVEL% GEQ 8 (
    echo [錯誤] robocopy 複製失敗！錯誤代碼：%ERRORLEVEL%
    pause
    exit /b
) else if %ERRORLEVEL% EQU 1 (
    echo [成功] 複製完成，有異動。
) else (
    echo [成功] 無異動，複製略過。
)

rem [Step5] 切換至專案並執行 GitPush
echo [Step5] 回到 %PROJECT_NAME% 並執行 GitPush.bat（如存在）
cd /d "%BASE_DIR%%PROJECT_NAME%"
if exist "GitPush.bat" (
    echo [訊息] 執行 GitPush.bat...
    call GitPush.bat
    echo [完成] GitPush.bat 已完成！
) else (
    echo [警告] GitPush.bat 不存在，略過。
)

rem [Step6] 可選：Fly.io 自動部署（如你使用 Fly）
:: echo [Step6] Fly.io 部屬
:: cd /d "%DEPLOY_DIR%"
:: fly deploy --config fly.toml --remote-only

echo [全部完成] 部屬流程完成！
pause

@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

:: ===================== 使用者可調整 =====================
set "MAX_BACKUPS=15"
:: =======================================================

:: ================== 初始化 ==================
call :init
:: =============== [步驟 0] 先進行備份 ===============
call :backup

:: =============== [步驟 1] 拉取 ===============
call :gitPull

:: =============== [步驟 2] 推送 ===============
call :gitPush

:: =============== [步驟 3] 重設 ===============
call :gitReset

:: =============== 結尾 ======================
goto :end

:: =====================================================
:init
echo [Init] 初始化變數...
set "script_dir=%~dp0"
set "script_dir=%script_dir:~0,-1%"

:: 取得目前時間 yyyyMMdd_HHmmss（作為版本）
for /f "usebackq tokens=*" %%i in (`powershell -NoProfile -Command "[DateTime]::Now.ToString('yyyyMMddHHmmss')"` ) do set "timestamp=%%i"

:: 取得目前分支
for /f "tokens=*" %%b in ('git rev-parse --abbrev-ref HEAD') do set "CUR_BRANCH=%%b"
echo [Init] 目前分支 = %CUR_BRANCH%

:: 專案與備份路徑
for %%f in ("%script_dir%") do set "project_name=%%~nxf"
for %%f in ("%script_dir%\..") do set "root_dir=%%~f"
set "backup_root=%root_dir%\.project-backup"
exit /b

:: =====================================================
:backup
:: 產生唯一檔名：專案_yyyyMMddHHmmss_rand.zip
set "bk_stamp=%timestamp%_%random%"
set "zip_name=%project_name%_%bk_stamp%.zip"
set "zip_path=%backup_root%\%zip_name%"

:: 建立備份根目錄（若不存在）
if not exist "%backup_root%" mkdir "%backup_root%"

echo [Backup] 使用 git archive 產生 "%zip_name%" ...

:: --prefix 可以讓壓縮包裡的檔案都放在一個資料夾下，避免展開後滿地檔案
git archive --format=zip --prefix=%project_name%/ HEAD -o "%zip_path%"
if %errorlevel% neq 0 (
    echo [錯誤] git archive 失敗，停止備份！
    exit /b 1
)

echo [成功] 備份完成：%zip_path%

:: 清理舊備份，只保留最新 %MAX_BACKUPS% 份
call :cleanupBackups
exit /b


:: =====================================================
:cleanupBackups
if not exist "%backup_root%" exit /b
set "count=0"
for /f "delims=" %%d in ('dir "%backup_root%\%project_name%_backup_*" /B /AD /O-D') do (
    set /a count+=1
    if !count! gtr %MAX_BACKUPS% (
        echo [清理] 刪除舊備份 %%d
        rmdir /s /q "%backup_root%\%%d"
    )
)
exit /b

:: =====================================================
:gitPull
echo [Git] 嘗試 git pull...
git config --global --add safe.directory "%cd%"
git pull origin %CUR_BRANCH%
if %errorlevel% neq 0 (
    echo [錯誤] Pull 失敗，進入強制同步
    call :forceSync
)
exit /b

:: =====================================================
:gitPush
echo [Git] 準備 commit ^& push...
set "hasCommits=false"
for /f "usebackq tokens=*" %%i in (`powershell -NoProfile -Command "[DateTime]::Now.ToString('yyyy-MM-dd_HH-mm-ss')"` ) do set "commit_msg=%%i"

git add .
git commit -m "%commit_msg%" >nul 2>&1
if %errorlevel% equ 0 (
    echo [Git] 有變更已提交
    set "hasCommits=true"
) else (
    echo [警告] 無變更可提交，略過 commit
)

git push origin %CUR_BRANCH%
if %errorlevel% neq 0 (
    echo [錯誤] Push 失敗，進入強制同步
    call :forceSync
)

exit /b

:: =====================================================
:gitReset
echo [Git] 重設本地至遠端最新狀態...
git fetch origin || call :forceSync
git reset --hard origin/%CUR_BRANCH% || call :forceSync
git pull origin %CUR_BRANCH% || call :forceSync
exit /b

:: =====================================================
:forceSync
echo.
echo [強制同步] 採用遠端狀態，放棄本地更動...

git fetch origin || goto :fatalError
git reset --hard origin/%CUR_BRANCH% || goto :fatalError
git clean -fdx
git pull origin %CUR_BRANCH% || goto :fatalError

echo [成功] 強制同步完成
exit /b

:: =====================================================
:fatalError
echo [嚴重錯誤] 無法完成同步，請手動處理！
pause
exit /b

:: =====================================================
:end
echo.
echo [成功] 所有流程完成！
pause
exit /b

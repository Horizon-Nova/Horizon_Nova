@echo off
chcp 65001 >nul
cd /d %~dp0

echo 正在從 appsettings.json 讀取連線字串與資料表...

:: 讀取 ConnectionStrings.TestConnection
for /f "delims=" %%c in ('powershell -NoProfile -Command ^
  "$conn = (Get-Content \"appsettings.json\" -Raw | ConvertFrom-Json).ConnectionStrings.TestConnection; if ($conn) { $conn } else { '' }"') do (
    set "CONNSTR=%%c"
)

:: 讀取 ScaffoldSettings.Tables 並組成 --table 清單
for /f "delims=" %%t in ('powershell -NoProfile -Command ^
  "$list = (Get-Content \"appsettings.json\" -Raw | ConvertFrom-Json).ScaffoldSettings.Tables; if ($list) { '--table ' + ($list -join ' --table ') } else { '' }"') do (
    set "TABLES=%%t"
)

:: 錯誤處理
if not defined CONNSTR (
    echo [錯誤] 無法讀取 appsettings.json 內的 ConnectionStrings.TestConnection！
    pause
    exit /b
)
if not defined TABLES (
    echo [錯誤] 無法讀取 appsettings.json 內的 ScaffoldSettings.Tables！
    pause
    exit /b
)

echo 匯出資料表：%TABLES%
echo 使用連線：%CONNSTR%

dotnet ef dbcontext scaffold ^
  "%CONNSTR%" ^
  Npgsql.EntityFrameworkCore.PostgreSQL ^
  -o Models ^
  %TABLES% --force ^
  --no-onconfiguring

pause

<div align="center">

# Horizon Nova 系統交接文件

<!-- 如果有 Logo 圖片，可以使用以下格式 -->
<!-- <img src="./assets/logo.png" alt="Horizon Nova Logo" width="300"> -->

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4?style=for-the-badge&logo=asp.net-core)](https://dotnet.microsoft.com/apps/aspnet)
[![Entity Framework](https://img.shields.io/badge/EF%20Core-8.0-512BD4?style=for-the-badge&logo=entity-framework)](https://learn.microsoft.com/ef/core/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16+-336791?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)](https://getbootstrap.com/)

**功能完整的企業級後台管理系統**

[專案概述](#專案概述) • [快速開始](#快速開始) • [技術架構](#技術架構) • [功能模組](#功能模組)

</div>

---

## 專案概述

Horizon Nova 是一個功能完整的企業級後台管理系統，提供全方位的管理工具與權限控制功能。

### 核心功能

| 功能模組 | 說明 |
|---------|------|
| ![使用者管理](https://img.shields.io/badge/使用者管理-權限控制-4285F4?logo=users&logoColor=white) | 完整的權限管理體系，支援使用者、角色、組織的多層級權限管理 |
| ![檔案管理](https://img.shields.io/badge/檔案管理-多使用者隔離-34A853?logo=file&logoColor=white) | 檔案上傳、下載、分享、預覽等功能，支援多使用者隔離 |
| ![系統監控](https://img.shields.io/badge/系統監控-硬體監控-FF6D00?logo=server&logoColor=white) | 硬體監控（CPU、GPU、記憶體、網路、儲存、電源）、日誌管理、快取清理 |
| ![AI 功能](https://img.shields.io/badge/AI_功能-ONNX_DALL--E3-9C27B0?logo=robot&logoColor=white) | 整合物件辨識（ONNX）、圖像生成（DALL-E 3）、向量資料庫（Qdrant） |
| ![導航管理](https://img.shields.io/badge/導航管理-動態側邊欄-00BCD4?logo=navigation&logoColor=white) | 動態側邊欄導航，可根據使用者權限顯示不同的導航項目 |

### 技術架構

<div align="center">

| 技術項目 | 版本/說明 |
|---------|----------|
| ![後端框架](https://img.shields.io/badge/後端框架-ASP.NET%20Core%208.0-512BD4?logo=dotnet) | ASP.NET Core 8.0 (.NET 8) |
| ![資料存取](https://img.shields.io/badge/資料存取-EF%20Core%208.0-512BD4?logo=entity-framework) | Entity Framework Core 8.0 |
| ![資料庫](https://img.shields.io/badge/資料庫-PostgreSQL-336791?logo=postgresql&logoColor=white) | PostgreSQL、多資料庫支援（Hnbdata、HnbHnbBackoffice） |
| ![前端技術](https://img.shields.io/badge/前端技術-Bootstrap%205-7952B3?logo=bootstrap&logoColor=white) | Razor Views、jQuery、Bootstrap 5、Lucide Icons |
| ![AI 框架](https://img.shields.io/badge/AI_框架-ONNX_DALL--E3-9C27B0?logo=robot) | ONNX Runtime、DALL-E 3 API、Qdrant |
| ![驗證方式](https://img.shields.io/badge/驗證方式-Cookie-FF5722?logo=security) | Cookie 驗證 |

</div>

### 文件導覽

1. [專案概述](#專案概述)
2. [快速開始](#快速開始)
3. [環境設定](#環境設定)
4. [專案結構](#專案結構)
5. [資料庫結構](#資料庫結構)
6. [功能模組說明](#功能模組說明)
7. [重要功能詳解](#重要功能詳解)
8. [資料處理規則](#資料處理規則)
9. [故障排除與常見問題](#故障排除與常見問題)
10. [部署與維護](#部署與維護)

---

## 快速開始

### 前置需求

- [x] .NET 8.0 SDK
- [x] PostgreSQL 16+
- [x] Visual Studio 2022 / VS Code / Rider
- [x] Node.js (用於前端套件管理，如需要)

### 安裝步驟

```bash
# 1. 複製專案
git clone https://github.com/your-username/Horizon_Nova.git
cd Horizon_Nova

# 2. 還原套件
dotnet restore

# 3. 設定資料庫連線
# 編輯 appsettings.json 中的資料庫連線字串

# 4. 執行資料庫遷移
dotnet ef database update

# 5. 執行專案
dotnet run
```

---

## 詳細文件

詳細的系統文件請參考 [READMEO.md](./READMEO.md)，包含完整的：
- 系統架構設計
- API 文件
- 資料庫結構說明
- 開發規範
- 部署指南

---

## 如何在 README 中加入 Icon 和 Logo

### 方法 1: 使用 Shields.io Badges（技術標籤）

這是最常用的方式，可以直接顯示技術棧、版本等資訊：

```markdown
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4?style=for-the-badge&logo=asp.net-core)](https://dotnet.microsoft.com/apps/aspnet)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16+-336791?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
```

實際效果：
- ![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet) 
- ![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4?logo=asp.net-core)
- ![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16+-336791?logo=postgresql&logoColor=white)

### 方法 2: 使用圖片（Logo）

```markdown
<!-- 使用專案內的圖片 -->
<img src="./assets/logo.png" alt="Horizon Nova Logo" width="200">

<!-- 或使用 Markdown 語法 -->
![Horizon Nova Logo](./assets/logo.png)
```

### 方法 3: 使用 Lucide Icons（SVG 圖示）

[Lucide](https://lucide.dev/) 提供超過 1600 個精美的 SVG 圖示，可以在 GitHub README 中直接使用：

#### 方式 A: 直接內嵌 SVG 代碼

從 [Lucide 官網](https://lucide.dev/icons) 選擇圖示，複製 SVG 代碼後直接內嵌：

```html
<!-- 例如：使用 User 圖示 -->
<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
  <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
  <circle cx="12" cy="7" r="4"></circle>
</svg>
```

實際效果：
<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
  <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
  <circle cx="12" cy="7" r="4"></circle>
</svg>
使用者管理

#### 方式 B: 從 Lucide GitHub 倉庫引用

```markdown
<!-- 使用 Lucide 的原始 SVG 文件 -->
<img src="https://raw.githubusercontent.com/lucide-icons/lucide/main/icons/user.svg" alt="User Icon" width="24" height="24">
```

實際效果：
<img src="https://raw.githubusercontent.com/lucide-icons/lucide/main/icons/user.svg" alt="User Icon" width="24" height="24">
使用者

#### 方式 C: 下載 SVG 到專案中

```markdown
<!-- 下載 SVG 到 ./assets/icons/ 目錄後使用 -->
<img src="./assets/icons/user.svg" alt="User Icon" width="24" height="24">
```

#### 常用 Lucide 圖示示範

| 圖示 | 用途 | SVG 代碼來源 |
|------|------|-------------|
| <img src="https://raw.githubusercontent.com/lucide-icons/lucide/main/icons/users.svg" alt="Users" width="20" height="20"> | 使用者管理 | [users.svg](https://lucide.dev/icons/users) |
| <img src="https://raw.githubusercontent.com/lucide-icons/lucide/main/icons/folder.svg" alt="Folder" width="20" height="20"> | 檔案管理 | [folder.svg](https://lucide.dev/icons/folder) |
| <img src="https://raw.githubusercontent.com/lucide-icons/lucide/main/icons/server.svg" alt="Server" width="20" height="20"> | 系統監控 | [server.svg](https://lucide.dev/icons/server) |
| <img src="https://raw.githubusercontent.com/lucide-icons/lucide/main/icons/brain.svg" alt="Brain" width="20" height="20"> | AI 功能 | [brain.svg](https://lucide.dev/icons/brain) |
| <img src="https://raw.githubusercontent.com/lucide-icons/lucide/main/icons/navigation.svg" alt="Navigation" width="20" height="20"> | 導航管理 | [navigation.svg](https://lucide.dev/icons/navigation) |

### 方法 4: 使用 SVG（向量圖形）

```markdown
<img src="https://raw.githubusercontent.com/username/repo/main/assets/logo.svg" alt="Logo" width="300">
```

### 方法 5: 居中對齊的 Logo 標題

```html
<div align="center">
  <img src="./assets/logo.png" alt="Horizon Nova" width="300">
  <h1>Horizon Nova 系統交接文件</h1>
</div>
```

### 方法 6: 專案統計 Badges

```markdown
![GitHub stars](https://img.shields.io/github/stars/username/repo?style=social)
![GitHub forks](https://img.shields.io/github/forks/username/repo?style=social)
![GitHub issues](https://img.shields.io/github/issues/username/repo)
![GitHub license](https://img.shields.io/github/license/username/repo)
```

---

## 專案統計

![GitHub stars](https://img.shields.io/github/stars/username/repo?style=social)
![GitHub forks](https://img.shields.io/github/forks/username/repo?style=social)
![GitHub issues](https://img.shields.io/github/issues/username/repo)
![GitHub license](https://img.shields.io/github/license/username/repo)

---

## 貢獻

歡迎提交 Issue 和 Pull Request！

## 授權

本專案採用 MIT 授權條款。

---

<div align="center">

**Horizon Nova** - 企業級後台管理系統

Made with ![Love](https://img.shields.io/badge/Made%20with-Love-FF1744?logo=heart&logoColor=white) by Horizon Nova Team

</div>

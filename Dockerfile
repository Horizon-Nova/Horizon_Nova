# 使用 ASP.NET 8.0 映像
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# 安裝繁體字體、OpenCV 與必要依賴
RUN apt-get update && apt-get install -y --no-install-recommends \
    libfontconfig1 \
    libfreetype6 \
    libjpeg62-turbo \
    libpng16-16 \
    libx11-6 \
    libxext6 \
    libxrender1 \
    fonts-noto-cjk \
    locales \
    libgdiplus \
    libopencv-dev \
 && locale-gen zh_TW.UTF-8 \
 && ldconfig \
 && apt-get clean \
 && rm -rf /var/lib/apt/lists/*

# 設定中文語系環境變數
ENV LANG=zh_TW.UTF-8
ENV LANGUAGE=zh_TW:zh
ENV LC_ALL=zh_TW.UTF-8

# 複製 dotnet publish 輸出的內容
COPY DeployFile . 

# 設定啟動指令
ENTRYPOINT ["dotnet", "HNB.dll"]

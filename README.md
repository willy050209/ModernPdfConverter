# Modern PDF Converter (.NET 10)

一個基於 **.NET 10** 與 **C# 14** 開發的現代化、安全且高效的 PDF 轉換工具。支援影像、Word、PowerPoint、Markdown 與純文字轉換為 PDF，並具備強大的 PDF 合併功能。

本專案採用 **Avalonia UI** 實作跨平台圖形化介面，並透過 **多專案架構 (Multi-Project Architecture)** 實現高內聚低耦合的開發標準。

## 🌟 核心功能

- **多專案架構**: 採用模式 A (Avalonia Shared/Desktop) 設計，核心邏輯與 UI 框架完全解耦，具備擴展至 Android/iOS/Web 的潛力。
- **雙模運作**: 同時支援 **GUI (圖形介面)** 與 **CLI (命令列)** 模式。
- **現代化語法**: 全面套用 **C# 14** 特性，包含主建構函式、集合表達式與 `readonly record struct` 優化效能。
- **影像轉換**: 使用 [QuestPDF](https://www.questpdf.com/) 高效將 JPG, PNG, BMP 轉換為 PDF 頁面。
- **Markdown 支援**: 整合 [Markdig](https://github.com/xoofx/markdig) 解析 Markdown 並轉換為精美的 PDF。
- **純文字支援**: 將 `.txt` 檔案直接轉換為 PDF 格式。
- **Office 轉換**: 整合 **LibreOffice CLI**，確保 Word (`.docx`) 與 PowerPoint (`.pptx`) 轉換後排版不跑位。
- **安全性優化**: 實作 **Fail-Fast (ArgumentNullException.ThrowIfNull)** 邊界防禦與強型別 `Result<T>` 模式。

## 🛠️ 技術棧

- **UI Framework**: Avalonia UI 11.0.11 (MVVM 模式)
- **MVVM Toolkit**: CommunityToolkit.Mvvm 8.2.2
- **IoC Container**: Microsoft.Extensions.DependencyInjection 10.0.5
- **Markdown Parser**: Markdig 1.1.2
- **Testing Framework**: xUnit, Moq, FluentAssertions
- **Runtime**: .NET 10.0
- **PDF Engine**: QuestPDF & PDFsharp 6.2.4
- **Image Processing**: SkiaSharp 2.88.7

## 📋 環境需求

1. **.NET 10 SDK**
2. **LibreOffice** (僅 Office 轉換需要):
   - 下載位址: [LibreOffice 官網](https://www.libreoffice.org/download/download/)
   - **重要**: 安裝後請將 `soffice` 命令所在路徑（通常是 `C:\Program Files\LibreOffice\program`）加入系統環境變數 **PATH** 中。

## 🚀 快速開始

### 安裝與編譯
```powershell
git clone https://github.com/willy050209/ModernPdfConverter.git
cd ModernPdfConverter
dotnet build ModernPdfConverter.slnx
```

### 🧪 執行測試
```powershell
dotnet test ModernPdfConverter.slnx
```

### 🖥️ 使用 GUI 模式 (推薦)
直接執行 Desktop 專案啟動圖形介面：
```powershell
dotnet run --project ModernPdfConverter.Desktop
```
**操作說明：**
1. **選擇來源**：點選「單檔」、「多檔」或「目錄」按鈕。
2. **選擇儲存路徑**：點選「瀏覽」選取輸出檔案名稱或目錄。
3. **開始轉換**：點選「開始轉換」並查看下方日誌與進度條。

### ⌨️ 使用 CLI 模式
```powershell
dotnet run --project ModernPdfConverter.Desktop -- [來源路徑] [目的路徑]
```
- **單一檔案**: `dotnet run --project ModernPdfConverter.Desktop -- "test.txt" "result.pdf"`
- **目錄合併**: `dotnet run --project ModernPdfConverter.Desktop -- "C:\Images" "all_images.pdf"`

## 📂 專案結構
- `ModernPdfConverter.Shared/`: 核心邏輯與 UI 共用專案。
    - `Views/`: Avalonia UI 視圖檔案。
    - `ViewModels/`: UI 展示邏輯與狀態管理。
    - `Core/`: 定義轉換介面與高效能資料模型 (`readonly record struct`)。
    - `Services/`: 跨平台轉換服務實作 (影像、Markdown、純文字、Office)。
    - `Assets/`: 專案靜態資源 (Icon, Svg)。
- `ModernPdfConverter.Desktop/`: 桌面端進入點專案。
    - `Program.cs`: 系統進入點、DI 容器設定與 CLI 路由。
    - `Services/`: 實作桌面專屬服務 (如基於原生視窗的 `AvaloniaDialogService`)。
- `ModernPdfConverter.Tests/`: 單元測試專案，負責驗證核心邏輯與 API 防禦機制。

## 🛡️ 安全性說明
本專案已針對 `NU1903`/`NU1904` 漏洞進行加固。架構上採用強型別與防禦性編程，確保每個 API 呼叫的安全性。所有轉換邏輯均脫離 UI Context 獨立運行，易於測試與維護。

## 📄 授權
本專案採用 MIT 授權。QuestPDF 在本專案中使用 Community 授權。

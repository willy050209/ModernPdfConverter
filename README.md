# Modern PDF Converter (.NET 10)

一個基於 **.NET 10** 與 **C# 14** 開發的現代化、安全且高效的 PDF 轉換工具。支援影像、Word、PowerPoint 轉換為 PDF，並具備強大的 PDF 合併功能。

本工具現已支援 **Avalonia UI** 圖形化介面，提供更直覺的操作體驗！

## 🌟 核心功能

- **雙模運作**: 同時支援 **GUI (圖形介面)** 與 **CLI (命令列)** 模式。
- **影像轉換**: 使用 [QuestPDF](https://www.questpdf.com/) 高效將 JPG, PNG, BMP 轉換為 PDF 頁面。
- **Office 轉換**: 整合 **LibreOffice CLI**，確保 Word (`.docx`) 與 PowerPoint (`.pptx`) 轉換後排版不跑位。
- **多樣化轉換模式**:
    - **單檔轉換**: 轉換單個檔案為 PDF。
    - **多檔批次 (GUI 獨有)**: 一次選取多個檔案，並分別轉換為獨立的 PDF。
    - **目錄合併**: 轉換目錄下所有支援的檔案並合併成單一 PDF。
- **安全性優化**: 已修正 `NU1903`/`NU1904` 弱點，完全移除了對舊版有風險之 `System.Drawing.Common` 的依賴。

## 🛠️ 技術棧

- **UI Framework**: Avalonia UI 11.0.11 (MVVM 模式)
- **MVVM Toolkit**: CommunityToolkit.Mvvm 8.2.2
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
dotnet build
```

### 🖥️ 使用 GUI 模式 (推薦)
直接執行程式，不帶任何參數即可啟動圖形介面：
```powershell
dotnet run
```
**操作說明：**
1. **選擇來源**：點選「單檔」、「多檔」或「目錄」按鈕。
2. **選擇儲存路徑**：點選「瀏覽」選取輸出檔案名稱或目錄。
3. **開始轉換**：點選「開始轉換」並查看下方日誌與進度條。

### ⌨️ 使用 CLI 模式
```powershell
dotnet run -- [來源路徑] [目的路徑]
```
- **單一檔案**: `dotnet run -- "test.docx" "result.pdf"`
- **目錄合併**: `dotnet run -- "C:\Images" "all_images.pdf"`

## 📂 專案結構
- `Views/`: Avalonia UI 視圖檔案。
- `ViewModels/`: UI 展示邏輯與狀態管理。
- `Core/`: 定義轉換介面與結果模式 (`Result<T>`)。
- `Services/`: 實作影像、Office 轉換與 PDF 合併邏輯。
- `Program.cs`: 混合式進入點 (CLI/GUI 偵測)。

## 🛡️ 安全性說明
本專案已針對 `NU1903`/`NU1904` 漏洞進行加固，並透過隱藏特定 Linux 依賴警告確保建置日誌整潔，適合用於對安全性有要求的生產環境。

## 📄 授權
本專案採用 MIT 授權。QuestPDF 在本專案中使用 Community 授權。

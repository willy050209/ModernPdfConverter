# Modern PDF Converter (.NET 10)

一個基於 **.NET 10** 與 **C# 14** 開發的現代化、安全且高效的 PDF 轉換工具。支援影像、Word、PowerPoint 轉換為 PDF，並具備強大的 PDF 合併功能。

## 🌟 核心功能

- **影像轉換**: 使用 [QuestPDF](https://www.questpdf.com/) 高效將 JPG, PNG, BMP 轉換為 PDF 頁面。
- **Office 轉換**: 整合 **LibreOffice CLI**，確保 Word (`.docx`) 與 PowerPoint (`.pptx`) 轉換後排版不跑位。
- **PDF 合併**: 採用官方最新 **PDFsharp 6.2.4**，提供極速且記憶體友善的合併體驗。
- **安全性優化**: 已修正 `NU1904` 弱點，完全移除了對舊版有風險之 `System.Drawing.Common` 的依賴。
- **CLI 工具化**: 支援透過命令列參數傳遞來源與目的路徑。

## 🛠️ 技術棧

- **Runtime**: .NET 10.0
- **Language**: C# 14 (Top-level statements, Primary constructors, Collection expressions)
- **PDF Engine**: QuestPDF & PDFsharp 6.x
- **Image Processing**: SkiaSharp

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

### 使用範例

#### 1. 轉換單一檔案
```powershell
dotnet run -- "C:\Data\Report.docx" "C:\Output\Report.pdf"
```

#### 2. 轉換整個目錄並合併
若來源路徑為目錄，程式會自動尋找支援的檔案進行批次轉換，並將結果合併為一個 PDF。
```powershell
dotnet run -- "C:\MyPhotos" "C:\Album.pdf"
```

## 📂 專案結構
- `Core/`: 定義轉換介面與結果模式 (`Result<T>`)。
- `Services/`: 實作影像、Office 轉換與 PDF 合併邏輯。
- `Program.cs`: 現代化 CLI 進入點。

## 🛡️ 安全性說明
本專案已針對 `NU1904` 漏洞進行加固，透過升級至 PDFsharp 6.x 系列，解決了 `System.Drawing.Common` 的安全性風險，適合用於對安全性有要求的生產環境。

## 📄 授權
本專案採用 MIT 授權。QuestPDF 在本專案中使用 Community 授權。

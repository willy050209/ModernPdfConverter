// filepath: D:/program/CS/ModernPdfConverter/Services/OfficeConverterService.cs
using System.Diagnostics;
using ModernPdfConverter.Core;

namespace ModernPdfConverter.Services;

/// <summary>
/// 使用 LibreOffice CLI 將 Office 檔案轉換為 PDF 的服務。
/// </summary>
public sealed class OfficeConverterService : IFileConverter
{
    public IReadOnlyList<string> SupportedExtensions { get; } = [".docx", ".doc", ".pptx", ".ppt", ".odt", ".rtf"];

    public async Task<Result<string>> ConvertAsync(ConversionRequest request)
    {
        try
        {
            var outputDir = Path.GetDirectoryName(request.DestinationPath) ?? ".";
            
            // LibreOffice 轉換指令範例: soffice --headless --convert-to pdf --outdir [dir] [file]
            var startInfo = new ProcessStartInfo
            {
                FileName = "soffice",
                Arguments = $"--headless --convert-to pdf --outdir \"{outputDir}\" \"{request.SourcePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            // 支援逾時與取消
            await process.WaitForExitAsync(request.CancellationToken);

            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync(request.CancellationToken);
                return Result<string>.Failure($"LibreOffice 轉換失敗: {error}");
            }

            // LibreOffice 會自動根據檔名生成 .pdf，我們將其重新命名為使用者指定的名稱
            var generatedFile = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(request.SourcePath) + ".pdf");
            if (File.Exists(generatedFile) && generatedFile != request.DestinationPath)
            {
                File.Move(generatedFile, request.DestinationPath, overwrite: true);
            }

            return Result<string>.Success(request.DestinationPath);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Office 轉換失敗: {ex.Message} (請確保已安裝 LibreOffice 並將其路徑加入 PATH)");
        }
    }
}

/// <summary>
/// 使用 PdfSharp 合併多個 PDF 檔案的服務。
/// </summary>
public static class PdfMergerService
{
    /// <summary>
    /// 合併多個 PDF 檔案。
    /// </summary>
    public static async Task<Result<string>> MergeAsync(IReadOnlyList<string> sourcePaths, string destinationPath, CancellationToken ct = default)
    {
        try
        {
            using var outputDocument = new PdfDocument();

            foreach (var path in sourcePaths)
            {
                if (ct.IsCancellationRequested) break;

                await Task.Run(() =>
                {
                    // 在 PDFsharp 6.x 中，PdfReader.Open 回傳的是一個可以處置的物件
                    using var inputDocument = PdfReader.Open(path, PdfDocumentOpenMode.Import);
                    var count = inputDocument.PageCount;
                    for (var idx = 0; idx < count; idx++)
                    {
                        // 使用 AddPage 並從來源文件的頁面匯入
                        outputDocument.AddPage(inputDocument.Pages[idx]);
                    }
                }, ct);
            }

            outputDocument.Save(destinationPath);
            return Result<string>.Success(destinationPath);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"PDF 合併失敗: {ex.Message}");
        }
    }
}

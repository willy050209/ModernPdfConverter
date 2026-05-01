namespace ModernPdfConverter.Services;

/// <summary>
/// 使用 LibreOffice CLI 將 Office 檔案轉換為 PDF 的服務。
/// </summary>
public sealed class OfficeConverterService(IProcessRunner processRunner) : IFileConverter
{
    /// <inheritdoc/>
    public IReadOnlyList<string> SupportedExtensions { get; } = [".docx", ".doc", ".pptx", ".ppt", ".odt", ".rtf"];

    /// <inheritdoc/>
    public async Task<Result<string>> ConvertAsync(ConversionRequest request)
    {
        try
        {
            var outputDir = Path.GetDirectoryName(request.DestinationPath) ?? ".";
            
            // LibreOffice 轉換指令範例: soffice --headless --convert-to pdf --outdir [dir] [file]
            var result = await processRunner.RunAsync("soffice", 
                $"--headless --convert-to pdf --outdir \"{outputDir}\" \"{request.SourcePath}\"", 
                request.CancellationToken);

            if (result.ExitCode != 0)
            {
                return Result<string>.Failure($"LibreOffice 轉換失敗: {result.StandardError}");
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

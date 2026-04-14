namespace ModernPdfConverter.Services;

/// <summary>
/// 使用 PdfSharp 合併多個 PDF 檔案的服務。
/// </summary>
public static class PdfMergerService
{
    /// <summary>
    /// 合併多個 PDF 檔案。
    /// </summary>
    /// <param name="sourcePaths">來源檔案路徑列表。</param>
    /// <param name="destinationPath">目的檔案路徑。</param>
    /// <param name="ct">取消權杖。</param>
    /// <returns>合併結果。</returns>
    /// <exception cref="ArgumentNullException">當 sourcePaths 或 destinationPath 為 null 時擲出。</exception>
    public static async Task<Result<string>> MergeAsync(IReadOnlyList<string> sourcePaths, string destinationPath, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(sourcePaths);
        ArgumentNullException.ThrowIfNull(destinationPath);

        try
        {
            // 使用 PDF 1.7 以支援現代化功能
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

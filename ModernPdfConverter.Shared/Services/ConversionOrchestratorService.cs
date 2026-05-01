namespace ModernPdfConverter.Services;

/// <summary>
/// 實作 <see cref="IConversionOrchestrator"/> 以協調轉換作業。
/// </summary>
public sealed class ConversionOrchestratorService(
    IEnumerable<IFileConverter> converters,
    IPdfMergerService pdfMerger) : IConversionOrchestrator
{
    private readonly IReadOnlyList<IFileConverter> _converters = converters.ToList();

    public event Func<string, Task>? OnLogAsync;
    public event Action<double>? OnProgressChanged;

    public async Task RunSingleFileConversionAsync(string source, string dest, CancellationToken ct)
    {
        var ext = Path.GetExtension(source).ToLower();
        var converter = _converters.FirstOrDefault(c => c.SupportedExtensions.Contains(ext));

        if (converter is null)
        {
            await LogAsync($"[錯誤] 不支援的格式: {ext}");
            return;
        }

        await LogAsync($"[執行] 正在轉換: {Path.GetFileName(source)}...");
        var result = await converter.ConvertAsync(new ConversionRequest(source, dest, ct));

        if (result.IsSuccess)
            await LogAsync($"[成功] 已儲存至: {dest}");
        else
            await LogAsync($"[失敗] {result.ErrorMessage}");
    }

    public async Task RunBatchIndividualConversionAsync(IReadOnlyList<string> files, string outputFolder, CancellationToken ct)
    {
        if (!Directory.Exists(outputFolder))
        {
            await LogAsync($"[錯誤] 輸出目錄不存在: {outputFolder}");
            return;
        }

        await LogAsync($"[批次] 準備獨立轉換 {files.Count} 個檔案...");

        for (int i = 0; i < files.Count; i++)
        {
            if (ct.IsCancellationRequested) break;

            var file = files[i];
            var dest = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(file) + ".pdf");
            
            await RunSingleFileConversionAsync(file, dest, ct);
            
            OnProgressChanged?.Invoke((double)(i + 1) / files.Count * 100);
        }
    }

    public async Task RunDirectoryConversionAsync(string sourceDir, string finalPdf, CancellationToken ct)
    {
        var files = Directory.GetFiles(sourceDir)
            .Where(f => !f.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f)
            .ToList();

        if (files.Count == 0)
        {
            await LogAsync("[跳過] 目錄中沒有可轉換的檔案。");
            return;
        }

        await LogAsync($"[批次] 找到 {files.Count} 個檔案，準備轉換並合併...");

        var tempPdfFiles = new List<string>();
        var tempDir = Path.Combine(Path.GetTempPath(), "ModernPdfConverter", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            for (int i = 0; i < files.Count; i++)
            {
                if (ct.IsCancellationRequested) break;

                var file = files[i];
                var tempDest = Path.Combine(tempDir, Path.GetFileNameWithoutExtension(file) + ".pdf");
                var ext = Path.GetExtension(file).ToLower();
                var converter = _converters.FirstOrDefault(c => c.SupportedExtensions.Contains(ext));

                if (converter is null) continue;

                await LogAsync($"  -> 處理中 ({i + 1}/{files.Count}): {Path.GetFileName(file)}...");
                var result = await converter.ConvertAsync(new ConversionRequest(file, tempDest, ct));
                
                if (result.IsSuccess) tempPdfFiles.Add(tempDest);
                
                OnProgressChanged?.Invoke((double)(i + 1) / files.Count * 80);
            }

            if (tempPdfFiles.Count > 0 && !ct.IsCancellationRequested)
            {
                await LogAsync($"[合併] 正在合併 {tempPdfFiles.Count} 個暫存檔案...");
                var mergeResult = await pdfMerger.MergeAsync(tempPdfFiles, finalPdf, ct);
                
                if (mergeResult.IsSuccess)
                    await LogAsync($"[全部完成] 最終檔案已產出: {finalPdf}");
                else
                    await LogAsync($"[合併失敗] {mergeResult.ErrorMessage}");
            }
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    private async Task LogAsync(string message)
    {
        if (OnLogAsync != null)
        {
            await OnLogAsync(message);
        }
    }
}

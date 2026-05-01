namespace ModernPdfConverter.Core;

/// <summary>
/// 定義負責協調與執行批次或複雜檔案轉換作業的編排器介面。
/// </summary>
public interface IConversionOrchestrator
{
    /// <summary>
    /// 當日誌訊息產生時觸發的事件。
    /// </summary>
    event Func<string, Task>? OnLogAsync;

    /// <summary>
    /// 當進度改變時觸發的事件 (0-100)。
    /// </summary>
    event Action<double>? OnProgressChanged;

    /// <summary>
    /// 轉換單一檔案。
    /// </summary>
    Task RunSingleFileConversionAsync(string source, string dest, CancellationToken ct);

    /// <summary>
    /// 轉換多個檔案到指定目錄。
    /// </summary>
    Task RunBatchIndividualConversionAsync(IReadOnlyList<string> files, string outputFolder, CancellationToken ct);

    /// <summary>
    /// 轉換目錄下的所有檔案並合併成單一 PDF。
    /// </summary>
    Task RunDirectoryConversionAsync(string sourceDir, string finalPdf, CancellationToken ct);
}

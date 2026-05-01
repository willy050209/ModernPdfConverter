namespace ModernPdfConverter.Core;

/// <summary>
/// 定義合併 PDF 檔案的服務合約。
/// </summary>
public interface IPdfMergerService
{
    /// <summary>
    /// 將多個 PDF 檔案合併成一個。
    /// </summary>
    /// <param name="sourcePaths">來源檔案路徑列表。</param>
    /// <param name="destinationPath">目的檔案路徑。</param>
    /// <param name="ct">取消權杖。</param>
    /// <returns>包含成功或錯誤訊息的結果。</returns>
    Task<Result<string>> MergeAsync(IReadOnlyList<string> sourcePaths, string destinationPath, CancellationToken ct = default);
}

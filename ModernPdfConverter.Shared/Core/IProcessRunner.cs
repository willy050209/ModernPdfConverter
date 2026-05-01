namespace ModernPdfConverter.Core;

/// <summary>
/// 執行外部處理程序（如 CLI 工具）的介面。
/// </summary>
public interface IProcessRunner
{
    /// <summary>
    /// 執行指令並等待結束。
    /// </summary>
    /// <param name="fileName">執行檔名稱或路徑。</param>
    /// <param name="arguments">指令參數。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>包含 ExitCode、Stdout 與 Stderr 的結果。</returns>
    Task<ProcessResult> RunAsync(string fileName, string arguments, CancellationToken cancellationToken = default);
}

/// <summary>
/// 外部處理程序執行的結果。
/// </summary>
public record ProcessResult(int ExitCode, string StandardOutput, string StandardError);

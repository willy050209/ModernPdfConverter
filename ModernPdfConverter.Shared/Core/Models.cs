// filepath: D:/program/CS/ModernPdfConverter/Core/Models.cs
namespace ModernPdfConverter.Core;

/// <summary>
/// 表示作業結果的泛型紀錄。
/// </summary>
public readonly record struct Result<T>(T? Value, string? ErrorMessage, bool IsSuccess)
{
    public static Result<T> Success(T value) => new(value, null, true);
    public static Result<T> Failure(string errorMessage) => new(default, errorMessage, false);
}

/// <summary>
/// 檔案轉換要求的資訊。
/// </summary>
public readonly record struct ConversionRequest
{
    public string SourcePath { get; init; }
    public string DestinationPath { get; init; }
    public CancellationToken CancellationToken { get; init; }

    /// <summary>
    /// 初始化 <see cref="ConversionRequest"/>。
    /// </summary>
    /// <param name="sourcePath">來源檔案或目錄路徑。</param>
    /// <param name="destinationPath">目的檔案或目錄路徑。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <exception cref="ArgumentException">當路徑為 null 或空白時擲出。</exception>
    public ConversionRequest(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);

        SourcePath = sourcePath;
        DestinationPath = destinationPath;
        CancellationToken = cancellationToken;
    }
}

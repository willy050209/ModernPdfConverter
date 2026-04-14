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
public readonly record struct ConversionRequest(
    string SourcePath,
    string DestinationPath,
    CancellationToken CancellationToken = default
);

// filepath: D:/program/CS/ModernPdfConverter/Core/IFileConverter.cs
namespace ModernPdfConverter.Core;

/// <summary>
/// 定義檔案轉換器的合約。
/// </summary>
public interface IFileConverter
{
    /// <summary>
    /// 支援的檔案副檔名列表（例如：.docx, .pptx, .jpg）。
    /// </summary>
    IReadOnlyList<string> SupportedExtensions { get; }

    /// <summary>
    /// 執行轉換作業。
    /// </summary>
    Task<Result<string>> ConvertAsync(ConversionRequest request);
}

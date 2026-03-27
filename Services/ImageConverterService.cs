// filepath: D:/program/CS/ModernPdfConverter/Services/ImageConverterService.cs
using ModernPdfConverter.Core;

namespace ModernPdfConverter.Services;

/// <summary>
/// 使用 QuestPDF 將影像轉換為 PDF 的服務。
/// </summary>
public sealed class ImageConverterService : IFileConverter
{
    static ImageConverterService()
    {
        // 設定 QuestPDF 授權
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public IReadOnlyList<string> SupportedExtensions { get; } = [".jpg", ".jpeg", ".png", ".bmp"];

    public async Task<Result<string>> ConvertAsync(ConversionRequest request)
    {
        try
        {
            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(0);
                        page.Content().Image(request.SourcePath);
                    });
                }).GeneratePdf(request.DestinationPath);
            }, request.CancellationToken);

            return Result<string>.Success(request.DestinationPath);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"影像轉換失敗: {ex.Message}");
        }
    }
}

namespace ModernPdfConverter.Services;

/// <summary>
/// 使用 QuestPDF 將純文字檔案 (.txt) 轉換為 PDF 的服務。
/// </summary>
public sealed class PlainTextConverterService : IFileConverter
{
    /// <inheritdoc/>
    public IReadOnlyList<string> SupportedExtensions { get; } = [".txt"];

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">當 request 為 null 時擲出。</exception>
    public async Task<Result<string>> ConvertAsync(ConversionRequest request)
    {
        try
        {
            var content = await File.ReadAllTextAsync(request.SourcePath, request.CancellationToken);

            await Task.Run(() =>
            {
                QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(50);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Verdana));

                        page.Content().Text(content);

                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("第 ");
                            x.CurrentPageNumber();
                            x.Span(" 頁");
                        });
                    });
                }).GeneratePdf(request.DestinationPath);
            }, request.CancellationToken);

            return Result<string>.Success(request.DestinationPath);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"純文字轉換失敗: {ex.Message}");
        }
    }
}

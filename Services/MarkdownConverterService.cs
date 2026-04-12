using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using ModernPdfConverter.Core;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ModernPdfConverter.Services;

/// <summary>
/// 使用 Markdig 與 QuestPDF 將 Markdown 轉換為 PDF 的服務。
/// </summary>
public sealed class MarkdownConverterService : IFileConverter
{
    public IReadOnlyList<string> SupportedExtensions { get; } = [".md", ".markdown"];

    public async Task<Result<string>> ConvertAsync(ConversionRequest request)
    {
        try
        {
            var content = await File.ReadAllTextAsync(request.SourcePath, request.CancellationToken);
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var document = Markdown.Parse(content, pipeline);

            await Task.Run(() =>
            {
                QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(50);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12).FontFamily(Fonts.Verdana));

                        page.Content().Column(column =>
                        {
                            foreach (var block in document)
                            {
                                RenderBlock(column, block);
                            }
                        });

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
            return Result<string>.Failure($"Markdown 轉換失敗: {ex.Message}");
        }
    }

    private void RenderBlock(ColumnDescriptor column, Block block)
    {
        switch (block)
        {
            case HeadingBlock heading:
                column.Item().PaddingTop(10).PaddingBottom(5).Text(t =>
                {
                    var size = heading.Level switch
                    {
                        1 => 24,
                        2 => 20,
                        3 => 18,
                        _ => 16
                    };
                    t.DefaultTextStyle(x => x.FontSize(size).Bold());
                    RenderInlines(t, heading.Inline);
                });
                break;

            case ParagraphBlock paragraph:
                column.Item().PaddingBottom(5).Text(t =>
                {
                    RenderInlines(t, paragraph.Inline);
                });
                break;

            case ListBlock listBlock:
                foreach (var item in listBlock)
                {
                    if (item is ListItemBlock listItem)
                    {
                        column.Item().PaddingLeft(20).Text(t =>
                        {
                            t.Span("• ").Bold();
                            foreach (var subBlock in listItem)
                            {
                                if (subBlock is LeafBlock leaf)
                                    RenderInlines(t, leaf.Inline);
                            }
                        });
                    }
                }
                break;

            case CodeBlock codeBlock:
                column.Item().PaddingVertical(5).Background(Colors.Grey.Lighten4).Padding(5).Text(t =>
                {
                    t.Span(GetCodeBlockText(codeBlock)).FontFamily(Fonts.CourierNew).FontSize(10);
                });
                break;
        }
    }

    private void RenderInlines(TextDescriptor text, ContainerInline? container)
    {
        if (container == null) return;
        foreach (var inline in container)
        {
            switch (inline)
            {
                case LiteralInline literal:
                    text.Span(literal.Content.ToString());
                    break;
                case LineBreakInline:
                    text.Span("\n");
                    break;
                case EmphasisInline emphasis:
                    var content = GetInlineText(emphasis);
                    var span = text.Span(content);
                    if (emphasis.DelimiterCount == 2) span.Bold();
                    else span.Italic();
                    break;
                case CodeInline code:
                    text.Span(code.Content).FontFamily(Fonts.CourierNew).BackgroundColor(Colors.Grey.Lighten3);
                    break;
            }
        }
    }

    private string GetInlineText(ContainerInline? container)
    {
        if (container == null) return string.Empty;
        var sb = new System.Text.StringBuilder();
        foreach (var inline in container)
        {
            if (inline is LiteralInline literal) sb.Append(literal.Content.ToString());
            else if (inline is ContainerInline subContainer) sb.Append(GetInlineText(subContainer));
            else if (inline is CodeInline code) sb.Append(code.Content);
        }
        return sb.ToString();
    }

    private string GetCodeBlockText(CodeBlock codeBlock)
    {
        var lines = codeBlock.Lines.Lines;
        if (lines == null) return string.Empty;
        return string.Join("\n", lines.Select(l => l.ToString()));
    }
}

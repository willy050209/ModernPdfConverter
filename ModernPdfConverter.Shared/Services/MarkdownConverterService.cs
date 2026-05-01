using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System.Text;

namespace ModernPdfConverter.Services;

/// <summary>
/// 使用 Markdig 與 QuestPDF 將 Markdown 轉換為 PDF 的服務。
/// </summary>
public sealed class MarkdownConverterService : IFileConverter
{
    /// <inheritdoc/>
    public IReadOnlyList<string> SupportedExtensions { get; } = [".md", ".markdown"];

    /// <inheritdoc/>
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

    private static void RenderBlock(ColumnDescriptor column, Block block)
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
                int index = 1;
                foreach (var item in listBlock)
                {
                    if (item is ListItemBlock listItem)
                    {
                        column.Item().PaddingLeft(20).Row(row =>
                        {
                            var bullet = listBlock.IsOrdered ? $"{index++}." : "•";
                            row.ConstantItem(20).Text(bullet).Bold();
                            row.RelativeItem().Column(itemColumn =>
                            {
                                foreach (var subBlock in listItem)
                                {
                                    RenderBlock(itemColumn, subBlock);
                                }
                            });
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

            case ThematicBreakBlock:
                column.Item().PaddingVertical(10).LineHorizontal(1);
                break;

            case QuoteBlock quote:
                column.Item().PaddingLeft(10).BorderLeft(2).BorderColor(Colors.Grey.Lighten2).PaddingLeft(10).Column(qCol =>
                {
                    foreach (var subBlock in quote)
                    {
                        RenderBlock(qCol, subBlock);
                    }
                });
                break;
        }
    }

    private static void RenderInlines(TextDescriptor text, ContainerInline? container, Action<TextSpanDescriptor>? styleAction = null)
    {
        if (container == null) return;
        foreach (var inline in container)
        {
            switch (inline)
            {
                case LiteralInline literal:
                    var span = text.Span(literal.Content.ToString());
                    styleAction?.Invoke(span);
                    break;
                case LineBreakInline:
                    text.Span("\n");
                    break;
                case EmphasisInline emphasis:
                    Action<TextSpanDescriptor> newStyle = s =>
                    {
                        styleAction?.Invoke(s);
                        if (emphasis.DelimiterCount == 2) s.Bold();
                        else s.Italic();
                    };
                    RenderInlines(text, emphasis, newStyle);
                    break;
                case CodeInline code:
                    var codeSpan = text.Span(code.Content).FontFamily(Fonts.CourierNew).BackgroundColor(Colors.Grey.Lighten3);
                    styleAction?.Invoke(codeSpan);
                    break;
                case LinkInline link:
                    var linkSpan = text.Span(GetInlineText(link)).FontColor(Colors.Blue.Medium).Underline();
                    styleAction?.Invoke(linkSpan);
                    break;
                case ContainerInline containerInline:
                    RenderInlines(text, containerInline, styleAction);
                    break;
            }
        }
    }

    private static string GetInlineText(ContainerInline? container)
    {
        if (container == null) return string.Empty;
        var sb = new StringBuilder();
        foreach (var inline in container)
        {
            if (inline is LiteralInline literal) sb.Append(literal.Content.ToString());
            else if (inline is ContainerInline subContainer) sb.Append(GetInlineText(subContainer));
            else if (inline is CodeInline code) sb.Append(code.Content);
        }
        return sb.ToString();
    }

    private static string GetCodeBlockText(CodeBlock codeBlock)
    {
        var lines = codeBlock.Lines.Lines;
        if (lines == null) return string.Empty;
        return string.Join("\n", lines.Select(l => l.ToString()));
    }
}

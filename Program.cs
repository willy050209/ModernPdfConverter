using ModernPdfConverter.Core;
using ModernPdfConverter.Services;
using Avalonia;
using ModernPdfConverter;

Console.OutputEncoding = System.Text.Encoding.UTF8;
QuestPDF.Settings.License = LicenseType.Community;

if (args.Length == 0)
{
    // 啟動 GUI
    BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
}
else
{
    // 執行 CLI
    await RunCliAsync(args);
}

static AppBuilder BuildAvaloniaApp()
    => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .LogToTrace();

static async Task RunCliAsync(string[] args)
{
    Console.WriteLine("=== 現代化 PDF 轉換器 (CLI 模式) ===");

    if (args.Length < 2)
    {
        Console.WriteLine("\n用法:");
        Console.WriteLine("  dotnet run -- [來源路徑] [目的路徑]");
        return;
    }

    string sourcePath = args[0];
    string destinationPath = args[1];

    IReadOnlyList<IFileConverter> converters = 
    [
        new ImageConverterService(),
        new OfficeConverterService(),
        new MarkdownConverterService()
    ];

    if (File.Exists(sourcePath))
    {
        await ConvertSingleFileAsync(sourcePath, destinationPath, converters);
    }
    else if (Directory.Exists(sourcePath))
    {
        await ConvertDirectoryAndMergeAsync(sourcePath, destinationPath, converters);
    }
    else
    {
        Console.WriteLine($"[錯誤] 找不到路徑: {sourcePath}");
    }
}

static async Task ConvertSingleFileAsync(string source, string dest, IReadOnlyList<IFileConverter> converters)
{
    var ext = Path.GetExtension(source).ToLower();
    var converter = converters.FirstOrDefault(c => c.SupportedExtensions.Contains(ext));

    if (converter is null)
    {
        Console.WriteLine($"[錯誤] 不支援的格式: {ext}");
        return;
    }

    Console.WriteLine($"[執行] 正在轉換: {Path.GetFileName(source)} -> {Path.GetFileName(dest)}...");
    var result = await converter.ConvertAsync(new ConversionRequest(source, dest));

    if (result.IsSuccess)
        Console.WriteLine("[成功]");
    else
        Console.WriteLine($"[失敗] {result.ErrorMessage}");
}

static async Task ConvertDirectoryAndMergeAsync(string sourceDir, string finalPdf, IReadOnlyList<IFileConverter> converters)
{
    var files = Directory.GetFiles(sourceDir)
        .Where(f => !f.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        .OrderBy(f => f)
        .ToList();

    if (files.Count == 0)
    {
        Console.WriteLine("[跳過] 目錄中沒有可轉換的檔案。");
        return;
    }

    Console.WriteLine($"[批次] 在目錄中找到 {files.Count} 個檔案，準備轉換並合併至 {Path.GetFileName(finalPdf)}...");

    var tempPdfFiles = new List<string>();
    var tempDir = Path.Combine(Path.GetTempPath(), "ModernPdfConverter", Guid.NewGuid().ToString());
    Directory.CreateDirectory(tempDir);

    try
    {
        foreach (var file in files)
        {
            var tempDest = Path.Combine(tempDir, Path.GetFileNameWithoutExtension(file) + ".pdf");
            var ext = Path.GetExtension(file).ToLower();
            var converter = converters.FirstOrDefault(c => c.SupportedExtensions.Contains(ext));

            if (converter is null) continue;

            Console.WriteLine($"  -> 處理中: {Path.GetFileName(file)}...");
            var result = await converter.ConvertAsync(new ConversionRequest(file, tempDest));
            
            if (result.IsSuccess) tempPdfFiles.Add(tempDest);
        }

        if (tempPdfFiles.Count > 0)
        {
            Console.WriteLine($"[合併] 正在合併 {tempPdfFiles.Count} 個暫存檔案...");
            var mergeResult = await PdfMergerService.MergeAsync(tempPdfFiles, finalPdf);
            
            if (mergeResult.IsSuccess)
                Console.WriteLine($"[全部完成] 最終檔案已產出: {finalPdf}");
            else
                Console.WriteLine($"[合併失敗] {mergeResult.ErrorMessage}");
        }
    }
    finally
    {
        if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
    }
}

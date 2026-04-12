using Microsoft.Extensions.DependencyInjection;

namespace ModernPdfConverter;

/// <summary>
/// 應用程式進入點。
/// </summary>
public static class Program
{
    /// <summary>
    /// 主進入點。
    /// </summary>
    /// <param name="args">命令列參數。</param>
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        QuestPDF.Settings.License = LicenseType.Community;

        var services = ConfigureServices();
        using var serviceProvider = services.BuildServiceProvider();

        if (args.Length == 0)
        {
            // 啟動 GUI
            BuildAvaloniaApp(serviceProvider).StartWithClassicDesktopLifetime(args);
        }
        else
        {
            // 執行 CLI
            await RunCliAsync(args, serviceProvider);
        }
    }

    /// <summary>
    /// 設定依賴注入服務。
    /// </summary>
    /// <returns>服務集合。</returns>
    public static IServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();

        // 註冊轉換器
        services.AddSingleton<IFileConverter, ImageConverterService>();
        services.AddSingleton<IFileConverter, OfficeConverterService>();
        services.AddSingleton<IFileConverter, MarkdownConverterService>();

        // 註冊 ViewModel
        services.AddTransient<MainViewModel>();

        return services;
    }

    /// <summary>
    /// 建立 Avalonia 應用程式實例。
    /// </summary>
    /// <param name="sp">服務提供者。</param>
    /// <returns>AppBuilder。</returns>
    public static AppBuilder BuildAvaloniaApp(IServiceProvider sp)
        => AppBuilder.Configure(() => new App(sp))
            .UsePlatformDetect()
            .LogToTrace();

    /// <summary>
    /// 執行命令列介面邏輯。
    /// </summary>
    /// <param name="args">命令列參數。</param>
    /// <param name="sp">服務提供者。</param>
    private static async Task RunCliAsync(string[] args, IServiceProvider sp)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(sp);

        Console.WriteLine("=== 現代化 PDF 轉換器 (CLI 模式) ===");

        if (args.Length < 2)
        {
            Console.WriteLine("\n用法:");
            Console.WriteLine("  dotnet run -- [來源路徑] [目的路徑]");
            return;
        }

        string sourcePath = args[0];
        string destinationPath = args[1];

        var converters = sp.GetServices<IFileConverter>().ToList();

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            Console.WriteLine("\n正在取消作業...");
            cts.Cancel();
            e.Cancel = true;
        };

        if (File.Exists(sourcePath))
        {
            await ConvertSingleFileAsync(sourcePath, destinationPath, converters, cts.Token);
        }
        else if (Directory.Exists(sourcePath))
        {
            await ConvertDirectoryAndMergeAsync(sourcePath, destinationPath, converters, cts.Token);
        }
        else
        {
            Console.WriteLine($"[錯誤] 找不到路徑: {sourcePath}");
        }
    }

    /// <summary>
    /// 轉換單一檔案。
    /// </summary>
    /// <param name="source">來源路徑。</param>
    /// <param name="dest">目的路徑。</param>
    /// <param name="converters">轉換器列表。</param>
    /// <param name="ct">取消權杖。</param>
    private static async Task ConvertSingleFileAsync(string source, string dest, IEnumerable<IFileConverter> converters, CancellationToken ct)
    {
        var ext = Path.GetExtension(source).ToLower();
        var converter = converters.FirstOrDefault(c => c.SupportedExtensions.Contains(ext));

        if (converter is null)
        {
            Console.WriteLine($"[錯誤] 不支援的格式: {ext}");
            return;
        }

        Console.WriteLine($"[執行] 正在轉換: {Path.GetFileName(source)} -> {Path.GetFileName(dest)}...");
        var result = await converter.ConvertAsync(new ConversionRequest(source, dest, ct));

        if (result.IsSuccess)
            Console.WriteLine("[成功]");
        else
            Console.WriteLine($"[失敗] {result.ErrorMessage}");
    }

    /// <summary>
    /// 轉換目錄並合併。
    /// </summary>
    /// <param name="sourceDir">來源目錄。</param>
    /// <param name="finalPdf">目的 PDF 路徑。</param>
    /// <param name="converters">轉換器列表。</param>
    /// <param name="ct">取消權杖。</param>
    private static async Task ConvertDirectoryAndMergeAsync(string sourceDir, string finalPdf, IEnumerable<IFileConverter> converters, CancellationToken ct)
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
                if (ct.IsCancellationRequested) break;

                var tempDest = Path.Combine(tempDir, Path.GetFileNameWithoutExtension(file) + ".pdf");
                var ext = Path.GetExtension(file).ToLower();
                var converter = converters.FirstOrDefault(c => c.SupportedExtensions.Contains(ext));

                if (converter is null) continue;

                Console.WriteLine($"  -> 處理中: {Path.GetFileName(file)}...");
                var result = await converter.ConvertAsync(new ConversionRequest(file, tempDest, ct));
                
                if (result.IsSuccess) tempPdfFiles.Add(tempDest);
            }

            if (tempPdfFiles.Count > 0 && !ct.IsCancellationRequested)
            {
                Console.WriteLine($"[合併] 正在合併 {tempPdfFiles.Count} 個暫存檔案...");
                var mergeResult = await PdfMergerService.MergeAsync(tempPdfFiles, finalPdf, ct);
                
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
}

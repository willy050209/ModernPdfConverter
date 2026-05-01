using Microsoft.Extensions.DependencyInjection;
using ModernPdfConverter.Desktop.Services;
using ModernPdfConverter.Core;
using ModernPdfConverter.Services;
using ModernPdfConverter.ViewModels;
using Avalonia;
using Avalonia.Controls;
using QuestPDF.Infrastructure;

namespace ModernPdfConverter.Desktop;

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

        // 註冊基礎服務
        services.AddSingleton<IProcessRunner, SystemProcessRunner>();
        services.AddSingleton<IPdfMergerService, PdfMergerService>();

        // 註冊轉換器
        services.AddSingleton<IFileConverter, ImageConverterService>();
        services.AddSingleton<IFileConverter, OfficeConverterService>();
        services.AddSingleton<IFileConverter, MarkdownConverterService>();
        services.AddSingleton<IFileConverter, PlainTextConverterService>();

        // 註冊編排器
        services.AddTransient<IConversionOrchestrator, ConversionOrchestratorService>();

        // 註冊平台專屬 DialogService 工廠
        services.AddSingleton<Func<Window, IDialogService>>(w => new AvaloniaDialogService(w));

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

        var orchestrator = sp.GetRequiredService<IConversionOrchestrator>();
        orchestrator.OnLogAsync += async msg => await Task.Run(() => Console.WriteLine(msg));

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            Console.WriteLine("\n正在取消作業...");
            cts.Cancel();
            e.Cancel = true;
        };

        if (File.Exists(sourcePath))
        {
            await orchestrator.RunSingleFileConversionAsync(sourcePath, destinationPath, cts.Token);
        }
        else if (Directory.Exists(sourcePath))
        {
            await orchestrator.RunDirectoryConversionAsync(sourcePath, destinationPath, cts.Token);
        }
        else
        {
            Console.WriteLine($"[錯誤] 找不到路徑: {sourcePath}");
        }
    }
}

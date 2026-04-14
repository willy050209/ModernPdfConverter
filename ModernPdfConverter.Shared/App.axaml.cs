using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ModernPdfConverter.ViewModels;
using ModernPdfConverter.Views;
using ModernPdfConverter.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ModernPdfConverter;

/// <summary>
/// 應用程式主類別。
/// </summary>
public partial class App : Application
{
    private readonly IServiceProvider? _serviceProvider;

    /// <summary>
    /// 初始化 App 類別。
    /// </summary>
    /// <param name="serviceProvider">服務提供者。</param>
    public App(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 無參數建構函式供 XAML 工具使用。
    /// </summary>
    public App() { }

    /// <inheritdoc/>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <inheritdoc/>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow();
            
            if (_serviceProvider != null)
            {
                // 從 DI 取得 IDialogService 工廠 (以 Window 為參數)
                var dialogServiceFactory = _serviceProvider.GetRequiredService<Func<Window, IDialogService>>();
                var dialogService = dialogServiceFactory(mainWindow);

                // 使用 ActivatorUtilities 手動建立 MainViewModel，並注入已建立的 dialogService
                var viewModel = ActivatorUtilities.CreateInstance<MainViewModel>(_serviceProvider, dialogService);
                mainWindow.DataContext = viewModel;
            }
            
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}

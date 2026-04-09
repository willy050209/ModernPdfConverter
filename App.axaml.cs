using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ModernPdfConverter.ViewModels;
using ModernPdfConverter.Views;
using ModernPdfConverter.Services;

namespace ModernPdfConverter;

public partial class App : Application
{
    public override void Initialize()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow();
            var dialogService = new AvaloniaDialogService(mainWindow);
            mainWindow.DataContext = new MainViewModel(dialogService);
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}

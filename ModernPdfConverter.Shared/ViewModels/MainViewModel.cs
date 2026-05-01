namespace ModernPdfConverter.ViewModels;

/// <summary>
/// 主視圖模型，負責管理檔案選取與轉換流程。
/// </summary>
/// <param name="dialogService">用於開啟對話視窗的服務。</param>
/// <param name="orchestrator">轉換編排器。</param>
/// <param name="converters">可用的轉換器集合（用於獲取支援的副檔名）。</param>
public partial class MainViewModel(
    IDialogService dialogService,
    IConversionOrchestrator orchestrator,
    IEnumerable<IFileConverter> converters) : ObservableObject
{
    static MainViewModel()
    {
        // 靜態建構函式用於初始化全域設定，例如 QuestPDF 授權
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// 無參數建構函式，供設計時使用。
    /// </summary>
    public MainViewModel() : this(null!, null!, []) { }

    [ObservableProperty]
    private string _sourcePath = string.Empty;

    [ObservableProperty]
    private string _destinationPath = string.Empty;

    [ObservableProperty]
    private bool _isMultipleFiles;

    private List<string> _selectedFiles = [];

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private string _statusText = "準備就緒";

    [ObservableProperty]
    private double _progressValue;

    /// <summary>
    /// 轉換作業的日誌訊息集合。
    /// </summary>
    public ObservableCollection<string> LogMessages { get; } = [];

    private readonly IReadOnlyList<IFileConverter> _converters = converters.ToList();

    /// <summary>
    /// 選擇單一來源檔案。
    /// </summary>
    [RelayCommand]
    private async Task SelectSourceFileAsync()
    {
        ArgumentNullException.ThrowIfNull(dialogService);
        var extensions = _converters.SelectMany(c => c.SupportedExtensions).ToList();
        var result = await dialogService.OpenFileAsync("選擇來源檔案", extensions);
        if (result != null)
        {
            SourcePath = result;
            IsMultipleFiles = false;
            _selectedFiles = [result];
        }
    }

    /// <summary>
    /// 選擇多個來源檔案。
    /// </summary>
    [RelayCommand]
    private async Task SelectSourceFilesAsync()
    {
        ArgumentNullException.ThrowIfNull(dialogService);
        var extensions = _converters.SelectMany(c => c.SupportedExtensions).ToList();
        var result = await dialogService.OpenFilesAsync("選擇多個來源檔案", extensions);
        if (result != null && result.Count > 0)
        {
            _selectedFiles = result.ToList();
            SourcePath = $"已選擇 {result.Count} 個檔案";
            IsMultipleFiles = true;
        }
    }

    /// <summary>
    /// 選擇來源資料夾。
    /// </summary>
    [RelayCommand]
    private async Task SelectSourceFolderAsync()
    {
        ArgumentNullException.ThrowIfNull(dialogService);
        var result = await dialogService.OpenFolderAsync("選擇來源資料夾");
        if (result != null)
        {
            SourcePath = result;
            IsMultipleFiles = false;
            _selectedFiles = [];
        }
    }

    /// <summary>
    /// 選擇儲存目的地。
    /// </summary>
    [RelayCommand]
    private async Task SelectDestinationAsync()
    {
        ArgumentNullException.ThrowIfNull(dialogService);
        if (IsMultipleFiles || (!string.IsNullOrEmpty(SourcePath) && Directory.Exists(SourcePath)))
        {
            var result = await dialogService.OpenFolderAsync("選擇儲存目錄");
            if (result != null) DestinationPath = result;
        }
        else
        {
            var result = await dialogService.SaveFileAsync("選擇儲存路徑", "result.pdf", ".pdf");
            if (result != null) DestinationPath = result;
        }
    }

    /// <summary>
    /// 啟動轉換作業。
    /// </summary>
    /// <param name="ct">取消權杖。</param>
    [RelayCommand]
    private async Task ConvertAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(SourcePath) || string.IsNullOrWhiteSpace(DestinationPath))
        {
            StatusText = "請選擇來源與目的路徑";
            return;
        }

        IsProcessing = true;
        ProgressValue = 0;
        LogMessages.Clear();
        StatusText = "正在處理...";

        orchestrator.OnLogAsync += AddLogAsync;
        orchestrator.OnProgressChanged += p => ProgressValue = p;

        try
        {
            if (IsMultipleFiles)
            {
                await orchestrator.RunBatchIndividualConversionAsync(_selectedFiles, DestinationPath, ct);
            }
            else if (File.Exists(SourcePath))
            {
                await orchestrator.RunSingleFileConversionAsync(SourcePath, DestinationPath, ct);
            }
            else if (Directory.Exists(SourcePath))
            {
                await orchestrator.RunDirectoryConversionAsync(SourcePath, DestinationPath, ct);
            }
            else
            {
                await AddLogAsync($"[錯誤] 找不到路徑: {SourcePath}");
            }
        }
        catch (Exception ex)
        {
            await AddLogAsync($"[錯誤] 發生未預期的錯誤: {ex.Message}");
        }
        finally
        {
            orchestrator.OnLogAsync -= AddLogAsync;
            orchestrator.OnProgressChanged -= p => ProgressValue = p;
            IsProcessing = false;
            ProgressValue = 100;
            StatusText = "處理完成";
        }
    }

    private async Task AddLogAsync(string message)
    {
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            LogMessages.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
        });
    }
}

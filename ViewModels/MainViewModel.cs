using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModernPdfConverter.Core;
using ModernPdfConverter.Services;
using Avalonia.Platform.Storage;
using System.Text;

namespace ModernPdfConverter.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IDialogService _dialogService;

    public MainViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    // Default constructor for design-time (optional, better to have a design-time data source)
    public MainViewModel() : this(null!) { }

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

    public ObservableCollection<string> LogMessages { get; } = [];

    private readonly IReadOnlyList<IFileConverter> _converters = 
    [
        new ImageConverterService(),
        new OfficeConverterService(),
        new MarkdownConverterService()
    ];

    [RelayCommand]
    private async Task SelectSourceFileAsync()
    {
        var extensions = _converters.SelectMany(c => c.SupportedExtensions).ToList();
        var result = await _dialogService.OpenFileAsync("選擇來源檔案", extensions);
        if (result != null)
        {
            SourcePath = result;
            IsMultipleFiles = false;
            _selectedFiles = [result];
        }
    }

    [RelayCommand]
    private async Task SelectSourceFilesAsync()
    {
        var extensions = _converters.SelectMany(c => c.SupportedExtensions).ToList();
        var result = await _dialogService.OpenFilesAsync("選擇多個來源檔案", extensions);
        if (result != null && result.Count > 0)
        {
            _selectedFiles = result.ToList();
            SourcePath = $"已選擇 {result.Count} 個檔案";
            IsMultipleFiles = true;
        }
    }

    [RelayCommand]
    private async Task SelectSourceFolderAsync()
    {
        var result = await _dialogService.OpenFolderAsync("選擇來源資料夾");
        if (result != null)
        {
            SourcePath = result;
            IsMultipleFiles = false;
            _selectedFiles = [];
        }
    }

    [RelayCommand]
    private async Task SelectDestinationAsync()
    {
        if (IsMultipleFiles || (!string.IsNullOrEmpty(SourcePath) && Directory.Exists(SourcePath)))
        {
            var result = await _dialogService.OpenFolderAsync("選擇儲存目錄");
            if (result != null) DestinationPath = result;
        }
        else
        {
            var result = await _dialogService.SaveFileAsync("選擇儲存路徑", "result.pdf", ".pdf");
            if (result != null) DestinationPath = result;
        }
    }

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

        try
        {
            if (IsMultipleFiles)
            {
                await RunBatchIndividualConversion(_selectedFiles, DestinationPath, ct);
            }
            else if (File.Exists(SourcePath))
            {
                await RunSingleFileConversion(SourcePath, DestinationPath, ct);
            }
            else if (Directory.Exists(SourcePath))
            {
                await RunDirectoryConversion(SourcePath, DestinationPath, ct);
            }
            else
            {
                AddLog($"[錯誤] 找不到路徑: {SourcePath}");
            }
        }
        catch (Exception ex)
        {
            AddLog($"[錯誤] 發生未預期的錯誤: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
            ProgressValue = 100;
            StatusText = "處理完成";
        }
    }

    private async Task RunBatchIndividualConversion(List<string> files, string outputFolder, CancellationToken ct)
    {
        if (!Directory.Exists(outputFolder))
        {
            AddLog($"[錯誤] 輸出目錄不存在: {outputFolder}");
            return;
        }

        AddLog($"[批次] 準備獨立轉換 {files.Count} 個檔案...");

        for (int i = 0; i < files.Count; i++)
        {
            if (ct.IsCancellationRequested) break;

            var file = files[i];
            var dest = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(file) + ".pdf");
            
            await RunSingleFileConversion(file, dest, ct);
            
            ProgressValue = (double)(i + 1) / files.Count * 100;
        }
    }

    private async Task RunSingleFileConversion(string source, string dest, CancellationToken ct)
    {
        var ext = Path.GetExtension(source).ToLower();
        var converter = _converters.FirstOrDefault(c => c.SupportedExtensions.Contains(ext));

        if (converter is null)
        {
            AddLog($"[錯誤] 不支援的格式: {ext}");
            return;
        }

        AddLog($"[執行] 正在轉換: {Path.GetFileName(source)}...");
        var result = await converter.ConvertAsync(new ConversionRequest(source, dest, ct));

        if (result.IsSuccess)
            AddLog($"[成功] 已儲存至: {dest}");
        else
            AddLog($"[失敗] {result.ErrorMessage}");
    }

    private async Task RunDirectoryConversion(string sourceDir, string finalPdf, CancellationToken ct)
    {
        var files = Directory.GetFiles(sourceDir)
            .Where(f => !f.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f)
            .ToList();

        if (files.Count == 0)
        {
            AddLog("[跳過] 目錄中沒有可轉換的檔案。");
            return;
        }

        AddLog($"[批次] 找到 {files.Count} 個檔案，準備轉換並合併...");

        var tempPdfFiles = new List<string>();
        var tempDir = Path.Combine(Path.GetTempPath(), "ModernPdfConverter", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            for (int i = 0; i < files.Count; i++)
            {
                if (ct.IsCancellationRequested) break;

                var file = files[i];
                var tempDest = Path.Combine(tempDir, Path.GetFileNameWithoutExtension(file) + ".pdf");
                var ext = Path.GetExtension(file).ToLower();
                var converter = _converters.FirstOrDefault(c => c.SupportedExtensions.Contains(ext));

                if (converter is null) continue;

                AddLog($"  -> 處理中 ({i + 1}/{files.Count}): {Path.GetFileName(file)}...");
                var result = await converter.ConvertAsync(new ConversionRequest(file, tempDest, ct));
                
                if (result.IsSuccess) tempPdfFiles.Add(tempDest);
                
                ProgressValue = (double)(i + 1) / files.Count * 80; // 轉換佔 80%
            }

            if (tempPdfFiles.Count > 0 && !ct.IsCancellationRequested)
            {
                AddLog($"[合併] 正在合併 {tempPdfFiles.Count} 個暫存檔案...");
                var mergeResult = await PdfMergerService.MergeAsync(tempPdfFiles, finalPdf, ct);
                
                if (mergeResult.IsSuccess)
                    AddLog($"[全部完成] 最終檔案已產出: {finalPdf}");
                else
                    AddLog($"[合併失敗] {mergeResult.ErrorMessage}");
            }
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    private void AddLog(string message)
    {
        LogMessages.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
    }
}

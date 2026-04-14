using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ModernPdfConverter.Services;

namespace ModernPdfConverter.Desktop.Services;

public class AvaloniaDialogService(Window parent) : IDialogService
{
    private readonly Window _parent = parent;

    public async Task<string?> OpenFileAsync(string title, IReadOnlyList<string>? extensions = null)
    {
        var result = await OpenFilesInternalAsync(title, extensions, false);
        return result?.FirstOrDefault();
    }

    public async Task<IReadOnlyList<string>?> OpenFilesAsync(string title, IReadOnlyList<string>? extensions = null)
    {
        return await OpenFilesInternalAsync(title, extensions, true);
    }

    private async Task<IReadOnlyList<string>?> OpenFilesInternalAsync(string title, IReadOnlyList<string>? extensions, bool allowMultiple)
    {
        var options = new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = allowMultiple
        };

        if (extensions != null)
        {
            options.FileTypeFilter = [new FilePickerFileType("支援的檔案") { Patterns = extensions.Select(e => $"*{e}").ToArray() }];
        }

        var result = await _parent.StorageProvider.OpenFilePickerAsync(options);
        return result.Select(f => f.Path.LocalPath).ToList();
    }

    public async Task<string?> OpenFolderAsync(string title)
    {
        var result = await _parent.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        });
        return result.FirstOrDefault()?.Path.LocalPath;
    }

    public async Task<string?> SaveFileAsync(string title, string defaultName, string extension)
    {
        var result = await _parent.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = defaultName,
            DefaultExtension = extension,
            FileTypeChoices = [new FilePickerFileType("PDF 檔案") { Patterns = ["*.pdf"] }]
        });
        return result?.Path.LocalPath;
    }
}

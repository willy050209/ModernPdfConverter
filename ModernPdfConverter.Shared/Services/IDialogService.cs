using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModernPdfConverter.Services;

public interface IDialogService
{
    Task<string?> OpenFileAsync(string title, IReadOnlyList<string>? extensions = null);
    Task<IReadOnlyList<string>?> OpenFilesAsync(string title, IReadOnlyList<string>? extensions = null);
    Task<string?> OpenFolderAsync(string title);
    Task<string?> SaveFileAsync(string title, string defaultName, string extension);
}

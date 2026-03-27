// filepath: D:/program/CS/ModernPdfConverter/Program.cs
using ModernPdfConverter.Core;
using ModernPdfConverter.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=== 現代化 PDF 轉換器 (.NET 10) ===");

if (args.Length < 2)
{
    Console.WriteLine("\n用法:");
    Console.WriteLine("  dotnet run -- [來源路徑] [目的路徑]");
    Console.WriteLine("\n範例:");
    Console.WriteLine("  1. 轉換單一檔案: dotnet run -- \"test.docx\" \"result.pdf\"");
    Console.WriteLine("  2. 轉換目錄並合併: dotnet run -- \"C:\\Images\" \"all_images.pdf\"");
    return;
}

string sourcePath = args[0];
string destinationPath = args[1];

// 初始化服務
IReadOnlyList<IFileConverter> converters = 
[
    new ImageConverterService(),
    new OfficeConverterService()
];

// 處理邏輯
if (File.Exists(sourcePath))
{
    await ConvertSingleFileAsync(sourcePath, destinationPath);
}
else if (Directory.Exists(sourcePath))
{
    await ConvertDirectoryAndMergeAsync(sourcePath, destinationPath);
}
else
{
    Console.WriteLine($"[錯誤] 找不到路徑: {sourcePath}");
}

async Task ConvertSingleFileAsync(string source, string dest)
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

async Task ConvertDirectoryAndMergeAsync(string sourceDir, string finalPdf)
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
        // 清理暫存目錄
        if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
    }
}

using ModernPdfConverter.Core;
using ModernPdfConverter.Services;
using Xunit;
using System.IO;

namespace ModernPdfConverter.Tests;

public class MathSupportTests
{
    [Fact]
    public async Task ConvertAsync_WithMathFormula_ShouldSucceed()
    {
        // Arrange
        var service = new MarkdownConverterService();
        var markdown = @"
# 效能評估 (Performance Evaluation)

Amdahl's Law 描述了在部分循序、部分並行的程式中，增加處理器數量所能獲得的加速比上限。

公式：$Speedup \le \frac{1}{S + \frac{(1 - S)}{N}}$ （$S$ 為不可平行的循序比例，$N$ 為核心數）。
";
        var sourcePath = Path.Combine(Path.GetTempPath(), "math_test.md");
        var destPath = Path.Combine(Path.GetTempPath(), "math_test.pdf");
        
        try
        {
            await File.WriteAllTextAsync(sourcePath, markdown);
            var request = new ConversionRequest(sourcePath, destPath);

            // Act
            var result = await service.ConvertAsync(request);

            // Assert
            Assert.True(result.IsSuccess, $"Conversion failed: {result.ErrorMessage}");
            Assert.True(File.Exists(destPath));
            var fileInfo = new FileInfo(destPath);
            Assert.True(fileInfo.Length > 0);
            
            // Note: Since we can't easily "read" the PDF content to verify math rendering, 
            // we manually check if the file was generated. 
            // In a real scenario, we might use a library to inspect the PDF.
        }
        finally
        {
            if (File.Exists(sourcePath)) File.Delete(sourcePath);
            // Keep the PDF for manual inspection if needed, or delete it
            if (File.Exists(destPath)) File.Delete(destPath);
        }
    }
}

using ModernPdfConverter.Core;
using ModernPdfConverter.Services;
using Xunit;
using System.IO;

namespace ModernPdfConverter.Tests;

public class MarkdownConverterTests
{
    [Fact]
    public async Task ConvertAsync_WithNestedLists_ShouldSucceed()
    {
        // Arrange
        var service = new MarkdownConverterService();
        var markdown = @"
*   **定義**：
    *   作為使用者與電腦硬體之間的**媒介 (Intermediary)**。
    *   **資源分配者 (Resource Allocator)**：管理並分配硬體資源（CPU, 記憶體, I/O 等）。
    *   **控制程式 (Control Program)**：控制使用者程式的執行，防止錯誤與不當使用。
    *   **核心 (Kernel)**：唯一隨時執行於電腦上的程式。
*   **目標**：
    *   **使用者角度**：方便 (Convenience)、易用、高效能。
    *   **系統角度**：資源利用率 (Resource utilization) 極大化。
";
        var sourcePath = Path.GetTempFileName() + ".md";
        var destPath = Path.GetTempFileName() + ".pdf";
        
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
        }
        finally
        {
            if (File.Exists(sourcePath)) File.Delete(sourcePath);
            if (File.Exists(destPath)) File.Delete(destPath);
        }
    }

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
        }
        finally
        {
            if (File.Exists(sourcePath)) File.Delete(sourcePath);
            if (File.Exists(destPath)) File.Delete(destPath);
        }
    }
}

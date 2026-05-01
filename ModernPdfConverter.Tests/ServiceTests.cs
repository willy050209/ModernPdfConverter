using ModernPdfConverter.Core;
using ModernPdfConverter.Services;
using Moq;
using Xunit;

namespace ModernPdfConverter.Tests;

public class ServiceTests
{
    [Fact]
    public async Task OfficeConverterService_ConvertAsync_ShouldSucceed_WhenProcessRunnerReturnsZero()
    {
        // Arrange
        var mockRunner = new Mock<IProcessRunner>();
        mockRunner.Setup(r => r.RunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new ProcessResult(0, "Success", ""));

        var service = new OfficeConverterService(mockRunner.Object);
        var source = Path.GetTempFileName() + ".docx";
        var dest = Path.GetTempFileName() + ".pdf";
        
        // Simulate LibreOffice output file creation
        var generatedPdf = Path.Combine(Path.GetDirectoryName(dest)!, Path.GetFileNameWithoutExtension(source) + ".pdf");
        await File.WriteAllTextAsync(source, "test");
        await File.WriteAllTextAsync(generatedPdf, "fake pdf");

        try
        {
            var request = new ConversionRequest(source, dest);

            // Act
            var result = await service.ConvertAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(File.Exists(dest));
            mockRunner.Verify(r => r.RunAsync("soffice", It.Is<string>(s => s.Contains(source)), It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            if (File.Exists(source)) File.Delete(source);
            if (File.Exists(dest)) File.Delete(dest);
            if (File.Exists(generatedPdf)) File.Delete(generatedPdf);
        }
    }

    [Fact]
    public async Task OfficeConverterService_ConvertAsync_ShouldFail_WhenProcessRunnerReturnsNonZero()
    {
        // Arrange
        var mockRunner = new Mock<IProcessRunner>();
        mockRunner.Setup(r => r.RunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new ProcessResult(1, "", "Error message"));

        var service = new OfficeConverterService(mockRunner.Object);
        var source = Path.GetTempFileName() + ".docx";
        var dest = Path.GetTempFileName() + ".pdf";
        await File.WriteAllTextAsync(source, "test");

        try
        {
            var request = new ConversionRequest(source, dest);

            // Act
            var result = await service.ConvertAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Error message", result.ErrorMessage);
        }
        finally
        {
            if (File.Exists(source)) File.Delete(source);
            if (File.Exists(dest)) File.Delete(dest);
        }
    }
}

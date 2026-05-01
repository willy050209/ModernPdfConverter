using ModernPdfConverter.Core;
using ModernPdfConverter.Services;
using ModernPdfConverter.ViewModels;
using Moq;
using FluentAssertions;
using Xunit;

namespace ModernPdfConverter.Tests;

public class DefenseTests
{
    [Fact]
    public async Task MainViewModel_SelectSourceFileAsync_ShouldThrowArgumentNullException_WhenDialogServiceIsNull()
    {
        // Arrange
        var viewModel = new MainViewModel(null!, null!, []);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await viewModel.SelectSourceFileCommand.ExecuteAsync(null));
    }

    [Theory]
    [InlineData(null, "dest.pdf")]
    [InlineData("", "dest.pdf")]
    [InlineData(" ", "dest.pdf")]
    [InlineData("source.md", null)]
    [InlineData("source.md", "")]
    [InlineData("source.md", " ")]
    public void ConversionRequest_Constructor_ShouldThrowArgumentException_WhenPathIsInvalid(string? source, string? dest)
    {
        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => new ConversionRequest(source!, dest!));
    }
}

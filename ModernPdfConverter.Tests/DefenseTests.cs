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
        var viewModel = new MainViewModel(null!, []);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await viewModel.SelectSourceFileCommand.ExecuteAsync(null));
    }

    [Fact]
    public async Task PdfMergerService_MergeAsync_ShouldThrowArgumentNullException_WhenPathsIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await PdfMergerService.MergeAsync(null!, "out.pdf"));
    }
}

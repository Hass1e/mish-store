using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Mish.Api.Controllers;
using Mish.Api.Dtos;
using Mish.Api.Services;
using Xunit;

namespace Mish.Api.Tests.Controllers;

/// <summary>
/// Тесты CategoriesController.
/// Проверяет API контроллер для работы с деревом категорий.
/// </summary>
public class CategoriesControllerTests
{
    /// <summary>
    /// Тест 6: Метод GetTree возвращает HTTP 200 с деревом категорий от сервиса.
    /// </summary>
    [Fact]
    public async Task GetTree_ShouldReturnOkResult_WithTreeFromService()
    {
        // Arrange (Подготовка)
        var mockService = new Mock<ICategoriesService>();
        var expectedTree = new List<CategoryTreeDto>
        {
            new() { Id = 1, Name = "Electronics", Slug = "electronics", Children = new List<CategoryTreeDto>() }
        };
        
        mockService
            .Setup(s => s.GetTreeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTree);

        var controller = new CategoriesController(mockService.Object);

        // Act (Действие)
        var result = await controller.GetTree(CancellationToken.None);

        // Assert (Проверка)
        result.Result.Should().BeOfType<OkObjectResult>();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedTree);
        
        // Проверяем, что метод сервиса был вызван ровно один раз
        mockService.Verify(s => s.GetTreeAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Тест 7: Возвращает пустой список когда сервис возвращает пустой результат.
    /// </summary>
    [Fact]
    public async Task GetTree_ShouldReturnEmptyList_WhenServiceReturnsEmpty()
    {
        // Arrange
        var mockService = new Mock<ICategoriesService>();
        mockService.Setup(s => s.GetTreeAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<CategoryTreeDto>());
        var controller = new CategoriesController(mockService.Object);

        // Act
        var result = await controller.GetTree(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        (okResult.Value as IReadOnlyList<CategoryTreeDto>).Should().BeEmpty();
    }
}
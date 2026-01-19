using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ProductionAnalysis.Application.Implementation.Dictionaries;
using ProductionAnalysis.Application.Tests.Infrastructure;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Tests.Dictionaries;

public class OperationsServiceTests : BaseIntegrationTest
{
    [Test]
    public async Task CreateOperationAsync_WithValidRequest_ShouldCreateOperation()
    {
        // Arrange
        var request = new CreateOperationRequest
        {
            Name = "Test Operation",
            DurationInSeconds = 300,
            BasedOnType = OperationBasedOnType.Nothing,
            BasedOperationId = null,
            BasedProductId = null
        };

        // Act
        var result = await GetService<IOperationsService>().CreateOperationAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Test Operation");
        result.Value.Duration.Should().Be(TimeSpan.FromSeconds(300));
        result.Value.BasedOnType.Should().Be(OperationBasedOnType.Nothing);

        var created = await DbContext.Operations.FirstOrDefaultAsync(o => o.Id == result.Value.Id);
        created.Should().NotBeNull();
        created!.Name.Should().Be("Test Operation");
        created.DurationInSeconds.Should().Be(300);
    }

    [Test]
    public async Task CreateOperationAsync_WithBasedOnOperation_ShouldCreateOperation()
    {
        // Arrange
        var parentOperation = await DataBuilder.CreateOperationAsync(
            id: 1,
            name: "Parent Operation",
            basedOnType: 1);

        var request = new CreateOperationRequest
        {
            Name = "Child Operation",
            DurationInSeconds = 200,
            BasedOnType = OperationBasedOnType.Operation,
            BasedOperationId = parentOperation.Id,
            BasedProductId = null
        };

        // Act
        var result = await GetService<IOperationsService>().CreateOperationAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Child Operation");
        result.Value.BasedOnType.Should().Be(OperationBasedOnType.Operation);
        result.Value.BasedOperationId.Should().Be(parentOperation.Id);
    }

    [Test]
    public async Task CreateOperationAsync_WithBasedOnProduct_ShouldCreateOperation()
    {
        // Arrange
        var enterprise = await DataBuilder.CreateEnterpriseAsync();
        var product = await DataBuilder.CreateProductAsync(enterpriseId: enterprise.Id);

        var request = new CreateOperationRequest
        {
            Name = "Product Operation",
            DurationInSeconds = 150,
            BasedOnType = OperationBasedOnType.Product,
            BasedOperationId = null,
            BasedProductId = product.Id
        };

        // Act
        var result = await GetService<IOperationsService>().CreateOperationAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Product Operation");
        result.Value.BasedOnType.Should().Be(OperationBasedOnType.Product);
        result.Value.BasedProductId.Should().Be(product.Id);
    }

    [Test]
    public async Task CreateOperationAsync_WithNonExistentBasedOperation_ShouldReturnNotFound()
    {
        // Arrange
        var request = new CreateOperationRequest
        {
            Name = "Child Operation",
            DurationInSeconds = 200,
            BasedOnType = OperationBasedOnType.Operation,
            BasedOperationId = 99999,
            BasedProductId = null
        };

        // Act
        var result = await GetService<IOperationsService>().CreateOperationAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Operation");
        result.Error.Message.Should().Contain("not found");
    }

    [Test]
    public async Task CreateOperationAsync_WithNonExistentBasedProduct_ShouldReturnNotFound()
    {
        // Arrange
        var request = new CreateOperationRequest
        {
            Name = "Product Operation",
            DurationInSeconds = 150,
            BasedOnType = OperationBasedOnType.Product,
            BasedOperationId = null,
            BasedProductId = 99999
        };

        // Act
        var result = await GetService<IOperationsService>().CreateOperationAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Product");
        result.Error.Message.Should().Contain("not found");
    }

    [Test]
    public async Task UpdateOperationAsync_WithValidRequest_ShouldUpdateOperation()
    {
        // Arrange
        var operation = await DataBuilder.CreateOperationAsync(
            id: 1,
            name: "Test Operation",
            durationInSeconds: 300);

        var request = new UpdateOperationRequest
        {
            Name = "Updated Operation",
            DurationInSeconds = 400,
            BasedOnType = OperationBasedOnType.Nothing,
            BasedOperationId = null,
            BasedProductId = null
        };

        // Act
        var result = await GetService<IOperationsService>().UpdateOperationAsync(operation.Id, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Updated Operation");
        result.Value.Duration.Should().Be(TimeSpan.FromSeconds(400));

        var updated = await DbContext.Operations.FirstOrDefaultAsync(o => o.Id == operation.Id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Operation");
        updated.DurationInSeconds.Should().Be(400);
    }

    [Test]
    public async Task UpdateOperationAsync_WithNonExistentOperation_ShouldReturnNotFound()
    {
        // Arrange
        var request = new UpdateOperationRequest
        {
            Name = "Updated Operation",
            DurationInSeconds = 400,
            BasedOnType = OperationBasedOnType.Nothing,
            BasedOperationId = null,
            BasedProductId = null
        };

        // Act
        var result = await GetService<IOperationsService>().UpdateOperationAsync(99999, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Operation");
        result.Error.Message.Should().Contain("not found");
    }

    [Test]
    public async Task DeleteOperationAsync_WithExistingOperation_ShouldDeleteOperation()
    {
        // Arrange
        var operation = await DataBuilder.CreateOperationAsync(
            id: 1,
            name: "Test Operation");

        // Act
        var result = await GetService<IOperationsService>().DeleteOperationAsync(operation.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deleted = await DbContext.Operations.FirstOrDefaultAsync(o => o.Id == operation.Id);
        deleted.Should().BeNull();
    }

    [Test]
    public async Task DeleteOperationAsync_WithNonExistentOperation_ShouldReturnNotFound()
    {
        // Act
        var result = await GetService<IOperationsService>().DeleteOperationAsync(99999);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Operation");
        result.Error.Message.Should().Contain("not found");
    }
}
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ProductionAnalysis.Application.Implementation.Dictionaries;
using ProductionAnalysis.Application.Tests.Infrastructure;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Tests.Dictionaries;

public class ProductsServiceTests : BaseIntegrationTest
{
    [Test]
    public async Task CreateProductAsync_WithValidRequest_ShouldCreateProduct()
    {
        // Arrange
        var enterprise = await DataBuilder.CreateEnterpriseAsync();
        var request = new CreateProductRequest
        {
            Name = "Test Product",
            TactTimeInSeconds = 60,
            EnterpriseId = enterprise.Id
        };

        // Act
        var result = await GetService<IProductsService>().CreateProductAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Test Product");
        result.Value.TactTime.Should().Be(TimeSpan.FromSeconds(60));
        result.Value.EnterpriseId.Should().Be(enterprise.Id);

        var created = await DbContext.Products.FirstOrDefaultAsync(p => p.Id == result.Value.Id);
        created.Should().NotBeNull();
        created!.Name.Should().Be("Test Product");
        created.TactTimeInSeconds.Should().Be(60);
    }

    [Test]
    public async Task CreateProductAsync_WithNonExistentEnterprise_ShouldReturnNotFound()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "Test Product",
            TactTimeInSeconds = 60,
            EnterpriseId = 99999
        };

        // Act
        var result = await GetService<IProductsService>().CreateProductAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Enterprise");
        result.Error.Message.Should().Contain("not found");
    }

    [Test]
    public async Task UpdateProductAsync_WithValidRequest_ShouldUpdateProduct()
    {
        // Arrange
        var enterprise = await DataBuilder.CreateEnterpriseAsync();
        var product = await DataBuilder.CreateProductAsync(enterpriseId: enterprise.Id);
        var newEnterprise = await DataBuilder.CreateEnterpriseAsync(id: 2, name: "New Enterprise");

        var request = new UpdateProductRequest
        {
            Name = "Updated Product",
            TactTimeInSeconds = 120,
            EnterpriseId = newEnterprise.Id
        };

        // Act
        var result = await GetService<IProductsService>().UpdateProductAsync(product.Id, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Updated Product");
        result.Value.TactTime.Should().Be(TimeSpan.FromSeconds(120));
        result.Value.EnterpriseId.Should().Be(newEnterprise.Id);

        var updated = await DbContext.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Product");
        updated.TactTimeInSeconds.Should().Be(120);
    }

    [Test]
    public async Task UpdateProductAsync_WithNonExistentProduct_ShouldReturnNotFound()
    {
        // Arrange
        var enterprise = await DataBuilder.CreateEnterpriseAsync();
        var request = new UpdateProductRequest
        {
            Name = "Updated Product",
            TactTimeInSeconds = 120,
            EnterpriseId = enterprise.Id
        };

        // Act
        var result = await GetService<IProductsService>().UpdateProductAsync(99999, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Product");
        result.Error.Message.Should().Contain("not found");
    }

    [Test]
    public async Task UpdateProductAsync_WithNonExistentEnterprise_ShouldReturnNotFound()
    {
        // Arrange
        var enterprise = await DataBuilder.CreateEnterpriseAsync();
        var product = await DataBuilder.CreateProductAsync(enterpriseId: enterprise.Id);

        var request = new UpdateProductRequest
        {
            Name = "Updated Product",
            TactTimeInSeconds = 120,
            EnterpriseId = 99999
        };

        // Act
        var result = await GetService<IProductsService>().UpdateProductAsync(product.Id, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Enterprise");
        result.Error.Message.Should().Contain("not found");
    }

    [Test]
    public async Task DeleteProductAsync_WithExistingProduct_ShouldDeleteProduct()
    {
        // Arrange
        var enterprise = await DataBuilder.CreateEnterpriseAsync();
        var product = await DataBuilder.CreateProductAsync(enterpriseId: enterprise.Id);

        // Act
        var result = await GetService<IProductsService>().DeleteProductAsync(product.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deleted = await DbContext.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
        deleted.Should().BeNull();
    }

    [Test]
    public async Task DeleteProductAsync_WithNonExistentProduct_ShouldReturnNotFound()
    {
        // Act
        var result = await GetService<IProductsService>().DeleteProductAsync(99999);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Product");
        result.Error.Message.Should().Contain("not found");
    }
}
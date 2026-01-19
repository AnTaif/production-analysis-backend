using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ProductionAnalysis.Application.Implementation.Dictionaries;
using ProductionAnalysis.Application.Tests.Infrastructure;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Tests.Dictionaries;

public class EnterprisesServiceTests : BaseIntegrationTest
{
    [Test]
    public async Task CreateEnterpriseAsync_WithValidRequest_ShouldCreateEnterprise()
    {
        // Arrange
        var request = new CreateEnterpriseRequest
        {
            Name = "Test Enterprise"
        };

        // Act
        var result = await GetService<IEnterprisesService>().CreateEnterpriseAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Test Enterprise");

        var created = await DbContext.Enterprises.FirstOrDefaultAsync(e => e.Id == result.Value.Id);
        created.Should().NotBeNull();
        created!.Name.Should().Be("Test Enterprise");
    }

    [Test]
    public async Task UpdateEnterpriseAsync_WithValidRequest_ShouldUpdateEnterprise()
    {
        // Arrange
        var enterprise = await DataBuilder.CreateEnterpriseAsync();
        var request = new UpdateEnterpriseRequest
        {
            Name = "Updated Enterprise"
        };

        // Act
        var result = await GetService<IEnterprisesService>().UpdateEnterpriseAsync(enterprise.Id, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Updated Enterprise");

        var updated = await DbContext.Enterprises.FirstOrDefaultAsync(e => e.Id == enterprise.Id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Enterprise");
    }

    [Test]
    public async Task UpdateEnterpriseAsync_WithNonExistentEnterprise_ShouldReturnNotFound()
    {
        // Arrange
        var request = new UpdateEnterpriseRequest
        {
            Name = "Updated Enterprise"
        };

        // Act
        var result = await GetService<IEnterprisesService>().UpdateEnterpriseAsync(99999, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Enterprise");
        result.Error.Message.Should().Contain("not found");
    }

    [Test]
    public async Task DeleteEnterpriseAsync_WithExistingEnterprise_ShouldDeleteEnterprise()
    {
        // Arrange
        var enterprise = await DataBuilder.CreateEnterpriseAsync();

        // Act
        var result = await GetService<IEnterprisesService>().DeleteEnterpriseAsync(enterprise.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deleted = await DbContext.Enterprises.FirstOrDefaultAsync(e => e.Id == enterprise.Id);
        deleted.Should().BeNull();
    }

    [Test]
    public async Task DeleteEnterpriseAsync_WithNonExistentEnterprise_ShouldReturnNotFound()
    {
        // Act
        var result = await GetService<IEnterprisesService>().DeleteEnterpriseAsync(99999);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Enterprise");
        result.Error.Message.Should().Contain("not found");
    }
}
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ProductionAnalysis.Application.Implementation.Dictionaries;
using ProductionAnalysis.Application.Tests.Infrastructure;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Tests.Dictionaries;

public class AuxiliaryOperationsServiceTests : BaseIntegrationTest
{
    [Test]
    public async Task CreateAuxiliaryOperationAsync_WithValidRequest_ShouldCreateAuxiliaryOperation()
    {
        // Arrange
        var request = new CreateAuxiliaryOperationRequest
        {
            Name = "Test Auxiliary Operation",
            DurationInSeconds = 1800
        };

        // Act
        var result = await GetService<IAuxiliaryOperationsService>().CreateAuxiliaryOperationAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Test Auxiliary Operation");
        result.Value.Duration.Should().Be(TimeSpan.FromSeconds(1800));

        var created = await DbContext.AuxiliaryOperations.FirstOrDefaultAsync(a => a.Id == result.Value.Id);
        created.Should().NotBeNull();
        created!.Name.Should().Be("Test Auxiliary Operation");
        created.DurationInSeconds.Should().Be(1800);
    }

    [Test]
    public async Task UpdateAuxiliaryOperationAsync_WithValidRequest_ShouldUpdateAuxiliaryOperation()
    {
        // Arrange
        var auxiliaryOperation = await DataBuilder.CreateAuxiliaryOperationAsync();
        var request = new UpdateAuxiliaryOperationRequest
        {
            Name = "Updated Auxiliary Operation",
            DurationInSeconds = 3600
        };

        // Act
        var result = await GetService<IAuxiliaryOperationsService>()
            .UpdateAuxiliaryOperationAsync(auxiliaryOperation.Id, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Updated Auxiliary Operation");
        result.Value.Duration.Should().Be(TimeSpan.FromSeconds(3600));

        var updated = await DbContext.AuxiliaryOperations.FirstOrDefaultAsync(a => a.Id == auxiliaryOperation.Id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Auxiliary Operation");
        updated.DurationInSeconds.Should().Be(3600);
    }

    [Test]
    public async Task UpdateAuxiliaryOperationAsync_WithNonExistentAuxiliaryOperation_ShouldReturnNotFound()
    {
        // Arrange
        var request = new UpdateAuxiliaryOperationRequest
        {
            Name = "Updated Auxiliary Operation",
            DurationInSeconds = 3600
        };

        // Act
        var result = await GetService<IAuxiliaryOperationsService>().UpdateAuxiliaryOperationAsync(99999, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("AuxiliaryOperation");
        result.Error.Message.Should().Contain("not found");
    }

    [Test]
    public async Task DeleteAuxiliaryOperationAsync_WithExistingAuxiliaryOperation_ShouldDeleteAuxiliaryOperation()
    {
        // Arrange
        var auxiliaryOperation = await DataBuilder.CreateAuxiliaryOperationAsync();

        // Act
        var result = await GetService<IAuxiliaryOperationsService>()
            .DeleteAuxiliaryOperationAsync(auxiliaryOperation.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deleted = await DbContext.AuxiliaryOperations.FirstOrDefaultAsync(a => a.Id == auxiliaryOperation.Id);
        deleted.Should().BeNull();
    }

    [Test]
    public async Task DeleteAuxiliaryOperationAsync_WithNonExistentAuxiliaryOperation_ShouldReturnNotFound()
    {
        // Act
        var result = await GetService<IAuxiliaryOperationsService>().DeleteAuxiliaryOperationAsync(99999);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("AuxiliaryOperation");
        result.Error.Message.Should().Contain("not found");
    }
}
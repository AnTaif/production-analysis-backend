using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ProductionAnalysis.Application.Implementation.Dictionaries;
using ProductionAnalysis.Application.Tests.Infrastructure;
using ProductionAnalysis.Client.Models.Dictionaries;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Application.Tests.Dictionaries;

public class PositionsServiceTests : BaseIntegrationTest
{
    [Test]
    public async Task CreatePositionAsync_WithValidRequest_ShouldCreatePosition()
    {
        // Arrange
        var request = new CreatePositionRequest
        {
            Name = "Test Position",
            Role = "TestRole"
        };

        // Act
        var result = await GetService<IPositionsService>().CreatePositionAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Test Position");
        result.Value.Role.Should().Be("TestRole");

        var created = await DbContext.Positions.FirstOrDefaultAsync(p => p.Id == result.Value.Id);
        created.Should().NotBeNull();
        created!.Name.Should().Be("Test Position");
        created.Role.Should().Be("TestRole");
    }

    [Test]
    public async Task CreatePositionAsync_WithNullRole_ShouldCreatePosition()
    {
        // Arrange
        var request = new CreatePositionRequest
        {
            Name = "Test Position",
            Role = null
        };

        // Act
        var result = await GetService<IPositionsService>().CreatePositionAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Test Position");
        result.Value.Role.Should().BeNull();
    }

    [Test]
    public async Task UpdatePositionAsync_WithValidRequest_ShouldUpdatePosition()
    {
        // Arrange
        var position = await CreatePositionAsync();
        var request = new UpdatePositionRequest
        {
            Name = "Updated Position",
            Role = "UpdatedRole"
        };

        // Act
        var result = await GetService<IPositionsService>().UpdatePositionAsync(position.Id, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Updated Position");
        result.Value.Role.Should().Be("UpdatedRole");

        var updated = await DbContext.Positions.FirstOrDefaultAsync(p => p.Id == position.Id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Position");
        updated.Role.Should().Be("UpdatedRole");
    }

    [Test]
    public async Task UpdatePositionAsync_WithNonExistentPosition_ShouldReturnNotFound()
    {
        // Arrange
        var request = new UpdatePositionRequest
        {
            Name = "Updated Position",
            Role = "UpdatedRole"
        };

        // Act
        var result = await GetService<IPositionsService>().UpdatePositionAsync(99999, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Position");
        result.Error.Message.Should().Contain("not found");
    }

    [Test]
    public async Task DeletePositionAsync_WithExistingPosition_ShouldDeletePosition()
    {
        // Arrange
        var position = await CreatePositionAsync();

        // Act
        var result = await GetService<IPositionsService>().DeletePositionAsync(position.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deleted = await DbContext.Positions.FirstOrDefaultAsync(p => p.Id == position.Id);
        deleted.Should().BeNull();
    }

    [Test]
    public async Task DeletePositionAsync_WithNonExistentPosition_ShouldReturnNotFound()
    {
        // Act
        var result = await GetService<IPositionsService>().DeletePositionAsync(99999);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Position");
        result.Error.Message.Should().Contain("not found");
    }

    private async Task<PositionDbo> CreatePositionAsync()
    {
        var position = new PositionDbo
        {
            Name = "Test Position",
            Role = "TestRole"
        };

        DbContext.Positions.Add(position);
        await DbContext.SaveChangesAsync();
        return position;
    }
}
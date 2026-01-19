using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ProductionAnalysis.Application.Implementation.Dictionaries;
using ProductionAnalysis.Application.Tests.Infrastructure;
using ProductionAnalysis.Client.Models.Dictionaries;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Application.Tests.Dictionaries;

public class DowntimeReasonGroupsServiceTests : BaseIntegrationTest
{
    [Test]
    public async Task CreateDowntimeReasonGroupAsync_WithValidRequest_ShouldCreateDowntimeReasonGroup()
    {
        // Arrange
        var request = new CreateDowntimeReasonGroupRequest
        {
            Name = "Test Group",
            Description = "Test Description"
        };

        // Act
        var result = await GetService<IDowntimeReasonGroupsService>().CreateDowntimeReasonGroupAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Test Group");
        result.Value.Description.Should().Be("Test Description");

        var created = await DbContext.DowntimeReasonGroups.FirstOrDefaultAsync(d => d.Id == result.Value.Id);
        created.Should().NotBeNull();
        created!.Name.Should().Be("Test Group");
        created.Description.Should().Be("Test Description");
    }

    [Test]
    public async Task UpdateDowntimeReasonGroupAsync_WithValidRequest_ShouldUpdateDowntimeReasonGroup()
    {
        // Arrange
        var group = await CreateDowntimeReasonGroupAsync();
        var request = new UpdateDowntimeReasonGroupRequest
        {
            Name = "Updated Group",
            Description = "Updated Description"
        };

        // Act
        var result = await GetService<IDowntimeReasonGroupsService>().UpdateDowntimeReasonGroupAsync(group.Id, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Updated Group");
        result.Value.Description.Should().Be("Updated Description");

        var updated = await DbContext.DowntimeReasonGroups.FirstOrDefaultAsync(d => d.Id == group.Id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Group");
        updated.Description.Should().Be("Updated Description");
    }

    [Test]
    public async Task UpdateDowntimeReasonGroupAsync_WithNonExistentGroup_ShouldReturnNotFound()
    {
        // Arrange
        var request = new UpdateDowntimeReasonGroupRequest
        {
            Name = "Updated Group",
            Description = "Updated Description"
        };

        // Act
        var result = await GetService<IDowntimeReasonGroupsService>().UpdateDowntimeReasonGroupAsync(99999, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("DowntimeReasonGroup");
        result.Error.Message.Should().Contain("not found");
    }

    [Test]
    public async Task DeleteDowntimeReasonGroupAsync_WithExistingGroup_ShouldDeleteDowntimeReasonGroup()
    {
        // Arrange
        var group = await CreateDowntimeReasonGroupAsync();

        // Act
        var result = await GetService<IDowntimeReasonGroupsService>().DeleteDowntimeReasonGroupAsync(group.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deleted = await DbContext.DowntimeReasonGroups.FirstOrDefaultAsync(d => d.Id == group.Id);
        deleted.Should().BeNull();
    }

    [Test]
    public async Task DeleteDowntimeReasonGroupAsync_WithNonExistentGroup_ShouldReturnNotFound()
    {
        // Act
        var result = await GetService<IDowntimeReasonGroupsService>().DeleteDowntimeReasonGroupAsync(99999);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("DowntimeReasonGroup");
        result.Error.Message.Should().Contain("not found");
    }

    private async Task<DowntimeReasonGroupDbo> CreateDowntimeReasonGroupAsync()
    {
        var group = new DowntimeReasonGroupDbo
        {
            Name = "Test Group",
            Description = "Test Description"
        };

        DbContext.DowntimeReasonGroups.Add(group);
        await DbContext.SaveChangesAsync();
        return group;
    }
}
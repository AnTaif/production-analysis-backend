using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ProductionAnalysis.Application.Implementation.Dictionaries;
using ProductionAnalysis.Application.Tests.Infrastructure;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Tests.Dictionaries;

public class ShiftsServiceTests : BaseIntegrationTest
{
    [Test]
    public async Task CreateShiftAsync_WithValidRequest_ShouldCreateShift()
    {
        // Arrange
        var request = new CreateShiftRequest
        {
            Name = "Test Shift",
            StartTime = new TimeOnly(8, 0)
        };

        // Act
        var result = await GetService<IShiftsService>().CreateShiftAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Test Shift");
        result.Value.StartTime.Should().Be(new TimeOnly(8, 0));

        var created = await DbContext.Shifts.FirstOrDefaultAsync(s => s.Id == result.Value.Id);
        created.Should().NotBeNull();
        created!.Name.Should().Be("Test Shift");
        created.StartTime.Should().Be(new TimeOnly(8, 0));
    }

    [Test]
    public async Task UpdateShiftAsync_WithValidRequest_ShouldUpdateShift()
    {
        // Arrange
        var shift = await DataBuilder.CreateShiftAsync();
        var request = new UpdateShiftRequest
        {
            Name = "Updated Shift",
            StartTime = new TimeOnly(9, 0)
        };

        // Act
        var result = await GetService<IShiftsService>().UpdateShiftAsync(shift.Id, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Updated Shift");
        result.Value.StartTime.Should().Be(new TimeOnly(9, 0));

        var updated = await DbContext.Shifts.FirstOrDefaultAsync(s => s.Id == shift.Id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Shift");
        updated.StartTime.Should().Be(new TimeOnly(9, 0));
    }

    [Test]
    public async Task UpdateShiftAsync_WithNonExistentShift_ShouldReturnNotFound()
    {
        // Arrange
        var request = new UpdateShiftRequest
        {
            Name = "Updated Shift",
            StartTime = new TimeOnly(9, 0)
        };

        // Act
        var result = await GetService<IShiftsService>().UpdateShiftAsync(99999, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Shift");
        result.Error.Message.Should().Contain("not found");
    }

    [Test]
    public async Task DeleteShiftAsync_WithExistingShift_ShouldDeleteShift()
    {
        // Arrange
        var shift = await DataBuilder.CreateShiftAsync();

        // Act
        var result = await GetService<IShiftsService>().DeleteShiftAsync(shift.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deleted = await DbContext.Shifts.FirstOrDefaultAsync(s => s.Id == shift.Id);
        deleted.Should().BeNull();
    }

    [Test]
    public async Task DeleteShiftAsync_WithNonExistentShift_ShouldReturnNotFound()
    {
        // Act
        var result = await GetService<IShiftsService>().DeleteShiftAsync(99999);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Shift");
        result.Error.Message.Should().Contain("not found");
    }
}
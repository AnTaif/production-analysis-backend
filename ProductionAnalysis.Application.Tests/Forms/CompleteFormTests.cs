using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using FormStatus = ProductionAnalysis.Application.Domain.Forms.FormStatus;

namespace ProductionAnalysis.Application.Tests.Forms;

public class CompleteFormTests : FormsTestBase
{
    [Test]
    public async Task CompleteFormAsync_ShouldChangeStatusToCompleted()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);
        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var createRequest = CreateSingleProductFormRequest(shift.Id, assigneeId);
        var createResult = await FormsService.CreateAsync(createRequest, user.Id);
        createResult.IsSuccess.Should().BeTrue();

        var form = await UnitOfWork.Forms.FindAsync(createResult.Value.Id);
        form!.Status.Should().Be(FormStatus.InProgress);

        // Act
        var result = await FormsService.CompleteFormAsync(createResult.Value.Id, user.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var completedForm = await UnitOfWork.Forms.FindAsync(createResult.Value.Id);
        completedForm.Should().NotBeNull();
        completedForm.Status.Should().Be(FormStatus.Completed);
    }

    [Test]
    public async Task CompleteFormAsync_WithNonExistentForm_ShouldReturnNotFound()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();

        // Act
        var result = await FormsService.CompleteFormAsync(99999, user.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("not found");
    }

    [Test]
    public async Task CompleteFormAsync_WhenFormAlreadyCompleted_ShouldReturnConflict()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);
        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var createRequest = CreateSingleProductFormRequest(shift.Id, assigneeId);
        var createResult = await FormsService.CreateAsync(createRequest, user.Id);
        createResult.IsSuccess.Should().BeTrue();

        var firstComplete = await FormsService.CompleteFormAsync(createResult.Value.Id, user.Id);
        firstComplete.IsSuccess.Should().BeTrue();

        // Act
        var result = await FormsService.CompleteFormAsync(createResult.Value.Id, user.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("completed");
    }
}
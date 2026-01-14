using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Tests.Forms;

public class GetFormTests : FormsTestBase
{
    [Test]
    public async Task GetByIdAsync_WithExistingForm_ShouldReturnForm()
    {
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);
        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var createRequest = CreateSingleProductFormRequest(shift.Id, assigneeId);
        var createResult = await FormsService.CreateAsync(createRequest, user.Id);
        createResult.IsSuccess.Should().BeTrue();

        var result = await FormsService.GetByIdAsync(createResult.Value.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(createResult.Value.Id);
        result.Value.PaType.Should().Be(PaTypeDto.SingleProductWithCycleTime);
    }

    [Test]
    public async Task GetByIdAsync_WithNonExistentForm_ShouldReturnNotFound()
    {
        var result = await FormsService.GetByIdAsync(99999);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("not found");
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnFormWithShiftAndDepartment()
    {
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);
        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var createRequest = CreateSingleProductFormRequest(shift.Id, assigneeId);
        var createResult = await FormsService.CreateAsync(createRequest, user.Id);
        createResult.IsSuccess.Should().BeTrue();

        var result = await FormsService.GetByIdAsync(createResult.Value.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Shift.Should().NotBeNull();
        result.Value.Shift.Id.Should().Be(shift.Id);
        result.Value.Department.Should().NotBeNull();
        result.Value.Department.Id.Should().Be(1);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnFormWithProductNamesInContext()
    {
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);
        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var createRequest = CreateSingleProductFormRequest(shift.Id, assigneeId);
        var createResult = await FormsService.CreateAsync(createRequest, user.Id);
        createResult.IsSuccess.Should().BeTrue();

        var result = await FormsService.GetByIdAsync(createResult.Value.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Context.Product.Should().NotBeNull();
        result.Value.Context.Product.ProductName.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task GetFormRowsAsync_WithExistingForm_ShouldReturnRows()
    {
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);
        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var createRequest = CreateSingleProductFormRequest(shift.Id, assigneeId);
        var createResult = await FormsService.CreateAsync(createRequest, user.Id);
        createResult.IsSuccess.Should().BeTrue();

        var result = await FormsService.GetFormRowsAsync(createResult.Value.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().NotBeEmpty();
    }

    [Test]
    public async Task GetFormRowsAsync_WithNonExistentForm_ShouldReturnNotFound()
    {
        var result = await FormsService.GetFormRowsAsync(99999);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("not found");
    }
}
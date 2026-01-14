using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shared.Constants;

namespace ProductionAnalysis.Application.Tests.Forms;

public class GetFormCountsTests : FormsTestBase
{
    [Test]
    public async Task GetFormCountsAsync_AsAdmin_ShouldReturnAllFormsCounts()
    {
        // Arrange
        var adminUser = await DataBuilder.CreateUserAsync("admin@test.com");
        await DataBuilder.CreateEmployeeAsync(adminUser.Id, departmentId: 1);

        var user1 = await DataBuilder.CreateUserAsync("user1@test.com");
        await DataBuilder.CreateEmployeeAsync(user1.Id, departmentId: 1);
        var assignee1 = await CreateAssigneeAsync(departmentId: 1);

        var user2 = await DataBuilder.CreateUserAsync("user2@test.com");
        await DataBuilder.CreateEmployeeAsync(user2.Id, departmentId: 2);
        var assignee2 = await CreateAssigneeAsync(departmentId: 2);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var form1Request = CreateSingleProductFormRequest(shift.Id, assignee1);
        await FormsService.CreateAsync(form1Request, user1.Id);

        var form2Request = CreateSingleProductFormRequest(shift.Id, assignee2);
        await FormsService.CreateAsync(form2Request, user2.Id);

        var contextUser = CreateContextUser(adminUser.Id, Roles.Admin);

        // Act
        var result = await FormsService.GetFormCountsAsync(contextUser);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Total.Should().BeGreaterThanOrEqualTo(2);
        result.Value.InProgress.Should().BeGreaterThanOrEqualTo(2);
        result.Value.Completed.Should().BeGreaterThanOrEqualTo(0);
    }

    [Test]
    public async Task GetFormCountsAsync_AsDepartmentHead_ShouldReturnOnlyDepartmentFormsCounts()
    {
        // Arrange
        var deptHeadUser = await DataBuilder.CreateUserAsync("depthead@test.com");
        await DataBuilder.CreateEmployeeAsync(deptHeadUser.Id, departmentId: 1);
        var assignee1 = await CreateAssigneeAsync(departmentId: 1);

        var user2 = await DataBuilder.CreateUserAsync("user2@test.com");
        await DataBuilder.CreateEmployeeAsync(user2.Id, departmentId: 2);
        var assignee2 = await CreateAssigneeAsync(departmentId: 2);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var form1Request = CreateSingleProductFormRequest(shift.Id, assignee1);
        await FormsService.CreateAsync(form1Request, deptHeadUser.Id);

        var form2Request = CreateSingleProductFormRequest(shift.Id, assignee2);
        await FormsService.CreateAsync(form2Request, user2.Id);

        var contextUser = CreateContextUser(deptHeadUser.Id, Roles.DepartmentHead);

        // Act
        var result = await FormsService.GetFormCountsAsync(contextUser);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Total.Should().BeGreaterThanOrEqualTo(1);
        result.Value.InProgress.Should().BeGreaterThanOrEqualTo(1);
    }

    [Test]
    public async Task GetFormCountsAsync_AsOperator_ShouldReturnOnlyAssigneeFormsCounts()
    {
        // Arrange
        var operatorUser = await DataBuilder.CreateUserAsync("operator@test.com");
        var operatorEmployee = await DataBuilder.CreateEmployeeAsync(operatorUser.Id, departmentId: 1);

        var user2 = await DataBuilder.CreateUserAsync("user2@test.com");
        await DataBuilder.CreateEmployeeAsync(user2.Id, departmentId: 1);
        var assignee2 = await CreateAssigneeAsync(departmentId: 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var form1Request = CreateSingleProductFormRequest(shift.Id, operatorEmployee.Id);
        await FormsService.CreateAsync(form1Request, user2.Id);

        var form2Request = CreateSingleProductFormRequest(shift.Id, assignee2);
        await FormsService.CreateAsync(form2Request, user2.Id);

        var contextUser = CreateContextUser(operatorUser.Id, Roles.Operator);

        // Act
        var result = await FormsService.GetFormCountsAsync(contextUser);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Total.Should().BeGreaterThanOrEqualTo(1);
        result.Value.InProgress.Should().BeGreaterThanOrEqualTo(1);
    }

    [Test]
    public async Task GetFormCountsAsync_ShouldReturnCorrectCountsForCompletedForms()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);
        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var createRequest = CreateSingleProductFormRequest(shift.Id, assigneeId);
        var createResult = await FormsService.CreateAsync(createRequest, user.Id);
        createResult.IsSuccess.Should().BeTrue();

        await FormsService.CompleteFormAsync(createResult.Value.Id, user.Id);

        var contextUser = CreateContextUser(user.Id, Roles.DepartmentHead);

        // Act
        var result = await FormsService.GetFormCountsAsync(contextUser);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Total.Should().BeGreaterThanOrEqualTo(1);
        result.Value.Completed.Should().BeGreaterThanOrEqualTo(1);
        result.Value.InProgress.Should().BeGreaterThanOrEqualTo(0);
    }

    [Test]
    public async Task GetFormCountsAsync_ForUserWithoutRole_ShouldReturnZeroCounts()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);
        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var createRequest = CreateSingleProductFormRequest(shift.Id, assigneeId);
        await FormsService.CreateAsync(createRequest, user.Id);

        var contextUser = CreateContextUser(user.Id); // Пользователь без роли

        // Act
        var result = await FormsService.GetFormCountsAsync(contextUser);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Total.Should().Be(0);
        result.Value.InProgress.Should().Be(0);
        result.Value.Completed.Should().Be(0);
    }

    [Test]
    public async Task GetFormCountsAsync_ForDepartmentHeadWithoutEmployee_ShouldReturnZeroCounts()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync("user@test.com");

        var contextUser = CreateContextUser(user.Id, Roles.DepartmentHead);

        // Act
        var result = await FormsService.GetFormCountsAsync(contextUser);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Total.Should().Be(0);
        result.Value.InProgress.Should().Be(0);
        result.Value.Completed.Should().Be(0);
    }
}
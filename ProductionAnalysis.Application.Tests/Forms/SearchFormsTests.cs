using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ProductionAnalysis.Client.Models.Forms;
using Shared.Constants;

namespace ProductionAnalysis.Application.Tests.Forms;

public class SearchFormsTests : FormsTestBase
{
    [Test]
    public async Task SearchFormsAsync_ShouldReturnPaginatedResults()
    {
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);
        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var createRequest = CreateSingleProductFormRequest(shift.Id, assigneeId);
        await FormsService.CreateAsync(createRequest, user.Id);

        var searchFilter = new SearchFormsFilterDto
        {
            PageNumber = 1,
            PageSize = 10
        };

        var contextUser = CreateContextUser(user.Id, Roles.DepartmentHead);
        var result = await FormsService.SearchFormsAsync(searchFilter, contextUser);

        result.Should().NotBeNull();
        result.Value.Items.Should().NotBeEmpty();
        result.Value.TotalCount.Should().BeGreaterThan(0);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
    }

    [Test]
    public async Task SearchFormsAsync_WithDepartmentFilter_ShouldReturnFilteredResults()
    {
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);
        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var createRequest = CreateSingleProductFormRequest(shift.Id, assigneeId);
        await FormsService.CreateAsync(createRequest, user.Id);

        var searchFilter = new SearchFormsFilterDto
        {
            DepartmentId = 1,
            PageNumber = 1,
            PageSize = 10
        };

        var contextUser = CreateContextUser(user.Id, Roles.DepartmentHead);
        var result = await FormsService.SearchFormsAsync(searchFilter, contextUser);

        result.Should().NotBeNull();
        result.Value.Items.Should().NotBeEmpty();
        result.Value.Items.Should().OnlyContain(f => f.DepartmentId == 1);
    }

    [Test]
    public async Task SearchFormsAsync_ForAdmin_ShouldReturnAllForms()
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

        // Создаем формы в разных департаментах
        var form1Request = CreateSingleProductFormRequest(shift.Id, assignee1);
        await FormsService.CreateAsync(form1Request, user1.Id);

        var form2Request = CreateSingleProductFormRequest(shift.Id, assignee2);
        await FormsService.CreateAsync(form2Request, user2.Id);

        var searchFilter = new SearchFormsFilterDto
        {
            PageNumber = 1,
            PageSize = 10
        };

        var adminContextUser = CreateContextUser(adminUser.Id, Roles.Admin);

        // Act
        var result = await FormsService.SearchFormsAsync(searchFilter, adminContextUser);

        // Assert
        result.Should().NotBeNull();
        result.Value.Items.Should().NotBeEmpty();
        result.Value.TotalCount.Should().BeGreaterThanOrEqualTo(2);
        result.Value.Items.Should().Contain(f => f.DepartmentId == 1);
        result.Value.Items.Should().Contain(f => f.DepartmentId == 2);
    }

    [Test]
    public async Task SearchFormsAsync_ForDepartmentHead_ShouldReturnOnlyFormsFromHisDepartment()
    {
        // Arrange
        var deptHeadUser = await DataBuilder.CreateUserAsync("depthead@test.com");
        await DataBuilder.CreateEmployeeAsync(deptHeadUser.Id, departmentId: 1);
        var assignee1 = await CreateAssigneeAsync(departmentId: 1);

        var user2 = await DataBuilder.CreateUserAsync("user2@test.com");
        await DataBuilder.CreateEmployeeAsync(user2.Id, departmentId: 2);
        var assignee2 = await CreateAssigneeAsync(departmentId: 2);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        // Создаем форму в департаменте 1
        var form1Request = CreateSingleProductFormRequest(shift.Id, assignee1);
        await FormsService.CreateAsync(form1Request, deptHeadUser.Id);

        // Создаем форму в департаменте 2
        var form2Request = CreateSingleProductFormRequest(shift.Id, assignee2);
        await FormsService.CreateAsync(form2Request, user2.Id);

        var searchFilter = new SearchFormsFilterDto
        {
            PageNumber = 1,
            PageSize = 10
        };

        var deptHeadContextUser = CreateContextUser(deptHeadUser.Id, Roles.DepartmentHead);

        // Act
        var result = await FormsService.SearchFormsAsync(searchFilter, deptHeadContextUser);

        // Assert
        result.Should().NotBeNull();
        result.Value.Items.Should().NotBeEmpty();
        result.Value.Items.Should().OnlyContain(f => f.DepartmentId == 1);
        result.Value.Items.Should().NotContain(f => f.DepartmentId == 2);
    }

    [Test]
    public async Task SearchFormsAsync_ForOperator_ShouldReturnOnlyFormsWhereHeIsAssignee()
    {
        // Arrange
        var operatorUser = await DataBuilder.CreateUserAsync("operator@test.com");
        var operatorEmployee = await DataBuilder.CreateEmployeeAsync(operatorUser.Id, departmentId: 1);

        var user2 = await DataBuilder.CreateUserAsync("user2@test.com");
        await DataBuilder.CreateEmployeeAsync(user2.Id, departmentId: 1);
        var assignee2 = await CreateAssigneeAsync(departmentId: 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        // Создаем форму, где оператор является исполнителем
        var form1Request = CreateSingleProductFormRequest(shift.Id, operatorEmployee.Id);
        var form1Result = await FormsService.CreateAsync(form1Request, user2.Id);
        form1Result.IsSuccess.Should().BeTrue();
        var form1Id = form1Result.Value.Id;

        // Создаем форму, где оператор НЕ является исполнителем
        var form2Request = CreateSingleProductFormRequest(shift.Id, assignee2);
        var form2Result = await FormsService.CreateAsync(form2Request, user2.Id);
        form2Result.IsSuccess.Should().BeTrue();
        var form2Id = form2Result.Value.Id;

        var searchFilter = new SearchFormsFilterDto
        {
            PageNumber = 1,
            PageSize = 10
        };

        var operatorContextUser = CreateContextUser(operatorUser.Id, Roles.Operator);

        // Act
        var result = await FormsService.SearchFormsAsync(searchFilter, operatorContextUser);

        // Assert
        result.Should().NotBeNull();
        result.Value.Items.Should().NotBeEmpty();
        result.Value.Items.Should().Contain(f => f.Id == form1Id);
        result.Value.Items.Should().NotContain(f => f.Id == form2Id);
    }

    [Test]
    public async Task SearchFormsAsync_ForUserWithoutRole_ShouldReturnEmptyResult()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);
        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var createRequest = CreateSingleProductFormRequest(shift.Id, assigneeId);
        await FormsService.CreateAsync(createRequest, user.Id);

        var searchFilter = new SearchFormsFilterDto
        {
            PageNumber = 1,
            PageSize = 10
        };

        var contextUser = CreateContextUser(user.Id); // Пользователь без роли

        // Act
        var result = await FormsService.SearchFormsAsync(searchFilter, contextUser);

        // Assert
        result.Should().NotBeNull();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Test]
    public async Task SearchFormsAsync_WithStatusFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);
        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var createRequest = CreateSingleProductFormRequest(shift.Id, assigneeId);
        var createResult = await FormsService.CreateAsync(createRequest, user.Id);
        createResult.IsSuccess.Should().BeTrue();

        // Завершаем форму
        await FormsService.CompleteFormAsync(createResult.Value.Id, user.Id);

        var searchFilter = new SearchFormsFilterDto
        {
            Status = FormStatus.Completed,
            PageNumber = 1,
            PageSize = 10
        };

        var contextUser = CreateContextUser(user.Id, Roles.DepartmentHead);

        // Act
        var result = await FormsService.SearchFormsAsync(searchFilter, contextUser);

        // Assert
        result.Should().NotBeNull();
        result.Value.Items.Should().Contain(f => f.Id == createResult.Value.Id);
        result.Value.Items.Should().OnlyContain(f => f.Status == FormStatus.Completed);
    }
}
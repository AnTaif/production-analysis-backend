using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shared.Constants;

namespace ProductionAnalysis.Application.Tests.Forms;

public class DeleteFormTests : FormsTestBase
{
    [Test]
    public async Task DeleteFormAsync_AsAdmin_ShouldDeleteForm()
    {
        // Arrange
        var adminUser = await DataBuilder.CreateUserAsync("admin@test.com");
        await DataBuilder.CreateEmployeeAsync(adminUser.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);
        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var createRequest = CreateSingleProductFormRequest(shift.Id, assigneeId);
        var createResult = await FormsService.CreateAsync(createRequest, adminUser.Id);
        createResult.IsSuccess.Should().BeTrue();

        var formId = createResult.Value.Id;

        var contextUser = CreateContextUser(adminUser.Id, Roles.Admin);

        // Act
        var result = await FormsService.DeleteFormAsync(formId, contextUser);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deletedForm = await UnitOfWork.Forms.FindAsync(formId);
        deletedForm.Should().BeNull();
    }

    [Test]
    public async Task DeleteFormAsync_AsDepartmentHead_ShouldDeleteOwnForm()
    {
        // Arrange
        var deptHeadUser = await DataBuilder.CreateUserAsync("depthead@test.com");
        await DataBuilder.CreateEmployeeAsync(deptHeadUser.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);
        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var createRequest = CreateSingleProductFormRequest(shift.Id, assigneeId);
        var createResult = await FormsService.CreateAsync(createRequest, deptHeadUser.Id);
        createResult.IsSuccess.Should().BeTrue();

        var formId = createResult.Value.Id;

        var contextUser = CreateContextUser(deptHeadUser.Id, Roles.DepartmentHead);

        // Act
        var result = await FormsService.DeleteFormAsync(formId, contextUser);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deletedForm = await UnitOfWork.Forms.FindAsync(formId);
        deletedForm.Should().BeNull();
    }

    [Test]
    public async Task DeleteFormAsync_AsDepartmentHead_ShouldNotDeleteFormCreatedByOtherUser()
    {
        // Arrange
        var deptHeadUser = await DataBuilder.CreateUserAsync("depthead@test.com");
        await DataBuilder.CreateEmployeeAsync(deptHeadUser.Id, departmentId: 1);

        var otherUser = await DataBuilder.CreateUserAsync("other@test.com");
        await DataBuilder.CreateEmployeeAsync(otherUser.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);
        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var createRequest = CreateSingleProductFormRequest(shift.Id, assigneeId);
        var createResult = await FormsService.CreateAsync(createRequest, otherUser.Id);
        createResult.IsSuccess.Should().BeTrue();

        var formId = createResult.Value.Id;

        var contextUser = CreateContextUser(deptHeadUser.Id, Roles.DepartmentHead);

        // Act
        var result = await FormsService.DeleteFormAsync(formId, contextUser);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("can only delete forms created by themselves");

        var form = await UnitOfWork.Forms.FindAsync(formId);
        form.Should().NotBeNull();
    }

    [Test]
    public async Task DeleteFormAsync_AsOperator_ShouldReturnForbidden()
    {
        // Arrange
        var operatorUser = await DataBuilder.CreateUserAsync("operator@test.com");
        await DataBuilder.CreateEmployeeAsync(operatorUser.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);
        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var createRequest = CreateSingleProductFormRequest(shift.Id, assigneeId);
        var createResult = await FormsService.CreateAsync(createRequest, operatorUser.Id);
        createResult.IsSuccess.Should().BeTrue();

        var formId = createResult.Value.Id;

        var contextUser = CreateContextUser(operatorUser.Id, Roles.Operator);

        // Act
        var result = await FormsService.DeleteFormAsync(formId, contextUser);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Only Admin or DepartmentHead can delete forms");

        var form = await UnitOfWork.Forms.FindAsync(formId);
        form.Should().NotBeNull();
    }

    [Test]
    public async Task DeleteFormAsync_WithNonExistentForm_ShouldReturnNotFound()
    {
        // Arrange
        var adminUser = await DataBuilder.CreateUserAsync("admin@test.com");
        var contextUser = CreateContextUser(adminUser.Id, Roles.Admin);

        // Act
        var result = await FormsService.DeleteFormAsync(99999, contextUser);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("not found");
    }

    [Test]
    public async Task DeleteFormAsync_ShouldDeleteAllRelatedRows()
    {
        // Arrange
        var adminUser = await DataBuilder.CreateUserAsync("admin@test.com");
        await DataBuilder.CreateEmployeeAsync(adminUser.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);
        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var createRequest = CreateSingleProductFormRequest(shift.Id, assigneeId);
        var createResult = await FormsService.CreateAsync(createRequest, adminUser.Id);
        createResult.IsSuccess.Should().BeTrue();

        var formId = createResult.Value.Id;
        var form = await UnitOfWork.Forms.FindAsync(formId);
        form!.Rows.Should().NotBeEmpty();

        var contextUser = CreateContextUser(adminUser.Id, Roles.Admin);

        // Act
        var result = await FormsService.DeleteFormAsync(formId, contextUser);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var rows = await DbContext.FormRows.Where(r => r.FormId == formId).ToListAsync();
        rows.Should().BeEmpty();
    }
}
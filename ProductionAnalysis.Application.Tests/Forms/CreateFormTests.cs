using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Client.Models.Forms;
using FormStatus = ProductionAnalysis.Application.Domain.Forms.FormStatus;
using ShiftConstants = ProductionAnalysis.Application.Implementation.Forms.ShiftConstants;

namespace ProductionAnalysis.Application.Tests.Forms;

public class CreateFormTests : FormsTestBase
{
    [Test]
    public async Task CreateAsync_ShouldCreateFormWithCumulativeValues()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);

        var template = await DbContext.Templates
            .Include(t => t.Indicators)
            .FirstAsync(t => t.Id == 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        // Act
        var request = new CreateFormRequest
        {
            PaType = (PaTypeDto)template.PaTypeId,
            ShiftId = shift.Id,
            AssigneeId = assigneeId,
            FormDate = DateTime.UtcNow.Date,
            Product = new ProductContextRequest
            {
                ProductId = 1,
                DailyRate = 400,
                CycleTime = 72
            }
        };

        var result = await FormsService.CreateAsync(request, user.Id);
        result.IsSuccess.Should().BeTrue();

        // Assert
        var form = await UnitOfWork.Forms.FindAsync(result.Value.Id);
        form.Should().NotBeNull();

        var workRows = form.Rows
            .Where(r => !r.IsAuxiliaryOperation)
            .OrderBy(r => r.Order)
            .ToList();

        workRows.Should().HaveCount(ShiftConstants.ShiftDurationHours);

        const int planIndicatorId = 2;
        const int planCumulativeIndicatorId = 19;
        var firstValue = GetValue(workRows[0], planIndicatorId);

        var secondCumulative = GetCumulativeValue(workRows[1], planCumulativeIndicatorId);
        var secondValue = GetValue(workRows[1], planIndicatorId);

        var thirdCumulative = GetCumulativeValue(workRows[2], planCumulativeIndicatorId);
        var thirdValue = GetValue(workRows[2], planIndicatorId);

        secondCumulative.Should().Be(firstValue + secondValue);
        thirdCumulative.Should().Be(firstValue + secondValue + thirdValue);
    }

    [Test]
    public async Task CreateAsync_ForLessThanOnePerHour_ShouldCreateFormWithOperationCycles()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);

        var template = await DbContext.Templates
            .Include(t => t.Indicators)
            .FirstAsync(t => t.PaTypeId == 4);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        // Act
        var request = CreateLessThanOnePerHourFormRequest(shift.Id, 1, 4);
        var result = await FormsService.CreateAsync(request, user.Id);
        result.IsSuccess.Should().BeTrue();

        // Assert
        var form = await UnitOfWork.Forms.FindAsync(result.Value.Id);
        form.Should().NotBeNull();
        form.PaType.Should().Be(PaType.LessThanOnePerHour);

        var workRows = form.Rows
            .Where(r => !r.IsAuxiliaryOperation)
            .OrderBy(r => r.Order)
            .ToList();

        workRows.Should().NotBeEmpty();

        // Проверяем, что строки сгруппированы по GroupKey
        var groupedRows = workRows
            .Where(r => r.GroupKey.HasValue)
            .GroupBy(r => r.GroupKey)
            .ToList();

        groupedRows.Should().NotBeEmpty();

        // Проверяем, что в каждой группе есть строки для всех операций
        foreach (var group in groupedRows)
        {
            var groupRows = group.OrderBy(r => r.Order).ToList();
            groupRows.Should().HaveCountGreaterThanOrEqualTo(1);

            // Проверяем, что у всех строк группы одинаковый GroupKey
            groupRows.Should().OnlyContain(r => r.GroupKey == group.Key);

            // Проверяем, что время работы одинаковое для всех строк группы
            var workTimeValues = groupRows
                .Select(r => r.Values.TryGetValue("16", out var v) ? v.Value : null)
                .Distinct()
                .ToList();
            workTimeValues.Should().HaveCount(1, "время работы должно быть одинаковым для всех строк группы");

            // Проверяем, что план одинаковый для всех строк группы
            var planValues = groupRows
                .Select(r => r.Values.TryGetValue("1", out var v) ? v.Value : null)
                .Distinct()
                .ToList();
            planValues.Should().HaveCount(1, "план должен быть одинаковым для всех строк группы");
        }
    }

    [Test]
    public async Task CreateAsync_ForLessThanOnePerShift_ShouldCreateFormWithSeparateRowsForEachOperation()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);

        var template = await DbContext.Templates
            .Include(t => t.Indicators)
            .FirstAsync(t => t.PaTypeId == 5);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        // Act
        var request = CreateLessThanOnePerShiftFormRequest(shift.Id, 1, 4);
        var result = await FormsService.CreateAsync(request, user.Id);
        result.IsSuccess.Should().BeTrue();

        // Assert
        var form = await UnitOfWork.Forms.FindAsync(result.Value.Id);
        form.Should().NotBeNull();
        form.PaType.Should().Be(PaType.LessThanOnePerShift);

        var workRows = form.Rows
            .Where(r => !r.IsAuxiliaryOperation)
            .OrderBy(r => r.Order)
            .ToList();

        workRows.Should().NotBeEmpty();

        // Проверяем, что каждая операция - отдельная строка (нет GroupKey)
        workRows.Should().OnlyContain(r => r.GroupKey == null,
            "для типа LessThanOnePerShift каждая операция должна быть отдельной строкой без GroupKey");

        // Проверяем, что есть строки для всех связанных операций
        workRows.Should().HaveCountGreaterThanOrEqualTo(3,
            "должны быть строки для всех связанных операций");
    }

    [Test]
    public async Task CreateAsync_ForSingleProductWithCycleTime_ShouldNotSetGroupKey()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var request = CreateSingleProductFormRequest(shift.Id, assigneeId);

        // Act
        var result = await FormsService.CreateAsync(request, user.Id);
        result.IsSuccess.Should().BeTrue();

        // Assert
        var form = await UnitOfWork.Forms.FindAsync(result.Value.Id);
        form.Should().NotBeNull();

        var workRows = form.Rows
            .Where(r => !r.IsAuxiliaryOperation)
            .ToList();

        workRows.Should().NotBeEmpty();

        // Проверяем, что для обычных форм GroupKey не устанавливается
        workRows.Should().OnlyContain(r => r.GroupKey == null,
            "для обычных форм GroupKey не должен устанавливаться");
    }

    [Test]
    public async Task CreateAsync_WithNonExistentTemplate_ShouldReturnNotFound()
    {
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var request = new CreateFormRequest
        {
            PaType = (PaTypeDto)9999,
            ShiftId = shift.Id,
            AssigneeId = assigneeId,
            FormDate = DateTime.UtcNow.Date,
            Product = new ProductContextRequest
            {
                ProductId = 1,
                DailyRate = 400,
                CycleTime = 72
            }
        };

        var result = await FormsService.CreateAsync(request, user.Id);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("PaType:");
        result.Error.Message.Should().Contain("must be a valid value");
    }

    [Test]
    public async Task CreateAsync_WithNonExistentEmployee_ShouldReturnNotFound()
    {
        var user = await DataBuilder.CreateUserAsync();
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var request = new CreateFormRequest
        {
            PaType = PaTypeDto.SingleProductWithCycleTime,
            ShiftId = shift.Id,
            AssigneeId = assigneeId,
            FormDate = DateTime.UtcNow.Date,
            Product = new ProductContextRequest
            {
                ProductId = 1,
                DailyRate = 400,
                CycleTime = 72
            }
        };

        var result = await FormsService.CreateAsync(request, user.Id);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Employee");
        result.Error.Message.Should().Contain("not found");
    }

    [Test]
    public async Task CreateAsync_WithNonExistentShift_ShouldReturnNotFound()
    {
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);

        var request = new CreateFormRequest
        {
            PaType = PaTypeDto.SingleProductWithCycleTime,
            ShiftId = 99999,
            AssigneeId = assigneeId,
            FormDate = DateTime.UtcNow.Date,
            Product = new ProductContextRequest
            {
                ProductId = 1,
                DailyRate = 400,
                CycleTime = 72
            }
        };

        var result = await FormsService.CreateAsync(request, user.Id);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Shift");
        result.Error.Message.Should().Contain("not found");
    }

    [Test]
    public async Task CreateAsync_WithAssigneeFromDifferentDepartment_ShouldReturnError()
    {
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 2);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var request = new CreateFormRequest
        {
            PaType = PaTypeDto.SingleProductWithCycleTime,
            ShiftId = shift.Id,
            AssigneeId = assigneeId,
            FormDate = DateTime.UtcNow.Date,
            Product = new ProductContextRequest
            {
                ProductId = 1,
                DailyRate = 400,
                CycleTime = 72
            }
        };

        var result = await FormsService.CreateAsync(request, user.Id);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("same department");
    }

    [Test]
    public async Task CreateAsync_ForLessThanOnePerHour_WithoutOperation_ShouldReturnError()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var request = new CreateFormRequest
        {
            PaType = PaTypeDto.LessThanOnePerHour,
            ShiftId = shift.Id,
            AssigneeId = assigneeId,
            FormDate = DateTime.UtcNow.Date,
            OperationOrProduct = null // Операция не указана
        };

        // Act
        var result = await FormsService.CreateAsync(request, user.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("OperationOrProduct");
    }

    [Test]
    public async Task CreateAsync_ForLessThanOnePerShift_WithoutOperation_ShouldReturnError()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var request = new CreateFormRequest
        {
            PaType = PaTypeDto.LessThanOnePerShift,
            ShiftId = shift.Id,
            AssigneeId = assigneeId,
            FormDate = DateTime.UtcNow.Date,
            OperationOrProduct = null // Операция не указана
        };

        // Act
        var result = await FormsService.CreateAsync(request, user.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Operation");
    }

    [Test]
    public async Task CreateAsync_ShouldSetFormDate()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);
        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);
        var formDate = new DateTime(2025, 9, 17).ToUniversalTime();

        var request = new CreateFormRequest
        {
            PaType = PaTypeDto.SingleProductWithCycleTime,
            ShiftId = shift.Id,
            AssigneeId = assigneeId,
            FormDate = formDate,
            Product = new ProductContextRequest
            {
                ProductId = 1,
                DailyRate = 400,
                CycleTime = 72
            }
        };

        // Act
        var result = await FormsService.CreateAsync(request, user.Id);
        result.IsSuccess.Should().BeTrue();

        // Assert
        var form = await UnitOfWork.Forms.FindAsync(result.Value.Id);
        form.Should().NotBeNull();
        form.FormDate.Should().Be(formDate);
    }

    [Test]
    public async Task CreateAsync_ShouldSetInitialStatusToInProgress()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);
        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var request = CreateSingleProductFormRequest(shift.Id, assigneeId);

        // Act
        var result = await FormsService.CreateAsync(request, user.Id);
        result.IsSuccess.Should().BeTrue();

        // Assert
        var form = await UnitOfWork.Forms.FindAsync(result.Value.Id);
        form.Should().NotBeNull();
        form.Status.Should().Be(FormStatus.InProgress);
    }
}
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Implementation.Forms;
using ProductionAnalysis.Application.Tests.Infrastructure;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Tests.Forms;

public class FormsServiceIntegrationTests : BaseIntegrationTest
{
    [Test]
    public async Task CreateAsync_ShouldCreateFormWithCumulativeValues()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);

        var template = await DbContext.Templates
            .Include(t => t.Indicators)
            .FirstAsync(t => t.Id == 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        // Act
        var request = new CreateFormRequest
        {
            PaType = (PaTypeDto)template.PaTypeId,
            ShiftId = shift.Id,
            Product = new ProductContextDto
            {
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

        const int planIndicatorId = 1;
        var firstValue = GetValue(workRows[0], planIndicatorId);

        var secondCumulative = GetCumulativeValue(workRows[1], planIndicatorId);
        var secondValue = GetValue(workRows[1], planIndicatorId);

        var thirdCumulative = GetCumulativeValue(workRows[2], planIndicatorId);
        var thirdValue = GetValue(workRows[2], planIndicatorId);

        secondCumulative.Should().Be(firstValue + secondValue);
        thirdCumulative.Should().Be(firstValue + secondValue + thirdValue);
    }

    [Test]
    public async Task UpdateFormRowAsync_ShouldRecalculateCumulativeValues()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var request = new CreateFormRequest
        {
            PaType = PaTypeDto.SingleProductWithCycleTime,
            ShiftId = shift.Id,
            Product = new ProductContextDto
            {
                DailyRate = 400,
                CycleTime = 72
            }
        };

        var result = await FormsService.CreateAsync(request, user.Id);
        result.IsSuccess.Should().BeTrue();

        var form = await UnitOfWork.Forms.FindAsync(result.Value.Id);
        form.Should().NotBeNull();
        const int factIndicatorId = 2;
        const int factValue = 50;

        // Act
        await UpdateRowAsync(form.Id, 1, factIndicatorId, factValue, user.Id);
        await UpdateRowAsync(form.Id, 2, factIndicatorId, factValue, user.Id);

        // Assert
        var updatedForm = await UnitOfWork.Forms.FindAsync(form.Id);
        var secondRow = updatedForm!.Rows
            .Where(r => !r.IsAuxiliaryOperation)
            .OrderBy(r => r.Order)
            .Skip(1)
            .First();

        var secondCumulative = GetCumulativeValue(secondRow, factIndicatorId);
        secondCumulative.Should().Be(100);
    }

    [Test]
    public async Task UpdateFormRowAsync_ShouldRecalculateFormulas()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);
        var request = new CreateFormRequest
        {
            PaType = PaTypeDto.SingleProductWithCycleTime,
            ShiftId = shift.Id,
            Product = new ProductContextDto
            {
                DailyRate = 400,
                CycleTime = 72
            }
        };

        var result = await FormsService.CreateAsync(request, user.Id);
        result.IsSuccess.Should().BeTrue();

        var form = await UnitOfWork.Forms.FindAsync(result.Value.Id);
        form.Should().NotBeNull();
        var firstRow = form.Rows
            .Where(r => !r.IsAuxiliaryOperation)
            .OrderBy(r => r.Order)
            .First();

        const int planIndicatorId = 1;
        const int factIndicatorId = 2;
        const int deviationIndicatorId = 3;
        const int planValue = 50;
        const int factValue = 80;

        // Act
        await UpdateRowAsync(form.Id, firstRow.Order, new Dictionary<int, object>
        {
            { factIndicatorId, factValue }
        }, user.Id);

        // Assert
        var updatedForm = await UnitOfWork.Forms.FindAsync(form.Id);
        var updatedRow = updatedForm!.Rows.Single(r => r.Order == firstRow.Order);

        GetValue(updatedRow, planIndicatorId).Should().Be(planValue);
        GetValue(updatedRow, factIndicatorId).Should().Be(factValue);

        var deviationValue = GetValue(updatedRow, deviationIndicatorId);
        const int expectedDeviation = factValue - planValue;
        deviationValue.Should().Be(expectedDeviation);
    }

    private async Task UpdateRowAsync(int formId, short rowOrder, int indicatorId, int value, Guid userId)
    {
        var updateRequest = new UpdateFormRowRequest
        {
            Values = new Dictionary<int, object> { { indicatorId, value } }
        };

        var result = await FormsService.UpdateFormRowAsync(formId, rowOrder, updateRequest, userId);
        result.IsSuccess.Should().BeTrue();
    }

    private async Task UpdateRowAsync(int formId, short rowOrder, Dictionary<int, object> values, Guid userId)
    {
        var updateRequest = new UpdateFormRowRequest { Values = values };
        var result = await FormsService.UpdateFormRowAsync(formId, rowOrder, updateRequest, userId);
        result.IsSuccess.Should().BeTrue();
    }

    private static int GetValue(FormRow row, int indicatorId)
    {
        var key = indicatorId.ToString();
        if (!row.Values.TryGetValue(key, out var rowValue))
        {
            return 0;
        }

        return Convert.ToInt32(rowValue.Value);
    }

    private static int GetCumulativeValue(FormRow row, int indicatorId)
    {
        var key = indicatorId.ToString();
        if (!row.Values.TryGetValue(key, out var rowValue))
        {
            return 0;
        }

        if (rowValue.CumulativeValue == null)
        {
            return 0;
        }

        return Convert.ToInt32(rowValue.CumulativeValue);
    }

    [Test]
    public async Task SearchFormsAsync_ShouldReturnPaginatedResults()
    {
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var createRequest = new CreateFormRequest
        {
            PaType = PaTypeDto.SingleProductWithCycleTime,
            ShiftId = shift.Id,
            Product = new ProductContextDto
            {
                DailyRate = 400,
                CycleTime = 72
            }
        };

        await FormsService.CreateAsync(createRequest, user.Id);

        var searchFilter = new SearchFormsFilterDto
        {
            PageNumber = 1,
            PageSize = 10
        };

        var result = await FormsService.SearchFormsAsync(searchFilter);

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

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var createRequest = new CreateFormRequest
        {
            PaType = PaTypeDto.SingleProductWithCycleTime,
            ShiftId = shift.Id,
            Product = new ProductContextDto
            {
                DailyRate = 400,
                CycleTime = 72
            }
        };

        await FormsService.CreateAsync(createRequest, user.Id);

        var searchFilter = new SearchFormsFilterDto
        {
            DepartmentId = 1,
            PageNumber = 1,
            PageSize = 10
        };

        var result = await FormsService.SearchFormsAsync(searchFilter);

        result.Should().NotBeNull();
        result.Value.Items.Should().NotBeEmpty();
        result.Value.Items.Should().OnlyContain(f => f.DepartmentId == 1);
    }

    [Test]
    public async Task GetByIdAsync_WithExistingForm_ShouldReturnForm()
    {
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var createRequest = new CreateFormRequest
        {
            PaType = PaTypeDto.SingleProductWithCycleTime,
            ShiftId = shift.Id,
            Product = new ProductContextDto
            {
                DailyRate = 400,
                CycleTime = 72
            }
        };

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
    public async Task GetFormRowsAsync_WithExistingForm_ShouldReturnRows()
    {
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var createRequest = new CreateFormRequest
        {
            PaType = PaTypeDto.SingleProductWithCycleTime,
            ShiftId = shift.Id,
            Product = new ProductContextDto
            {
                DailyRate = 400,
                CycleTime = 72
            }
        };

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

    [Test]
    public async Task CreateAsync_WithNonExistentTemplate_ShouldReturnNotFound()
    {
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var request = new CreateFormRequest
        {
            PaType = (PaTypeDto)99999,
            ShiftId = shift.Id,
            Product = new ProductContextDto
            {
                DailyRate = 400,
                CycleTime = 72
            }
        };

        var result = await FormsService.CreateAsync(request, user.Id);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Template");
        result.Error.Message.Should().Contain("not found");
    }

    [Test]
    public async Task CreateAsync_WithNonExistentEmployee_ShouldReturnNotFound()
    {
        var user = await DataBuilder.CreateUserAsync();

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var request = new CreateFormRequest
        {
            PaType = PaTypeDto.SingleProductWithCycleTime,
            ShiftId = shift.Id,
            Product = new ProductContextDto
            {
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

        var request = new CreateFormRequest
        {
            PaType = PaTypeDto.SingleProductWithCycleTime,
            ShiftId = 99999,
            Product = new ProductContextDto
            {
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
}
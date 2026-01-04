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
    public async Task CreateAsync_ShouldCreateFormWithCorrectRowsAndCumulativeValues()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);

        var template = await DbContext.Templates
            .Include(t => t.Indicators)
            .FirstAsync(t => t.Id == 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var request = new CreateFormRequest
        {
            PaTypeId = template.PaTypeId,
            ShiftId = shift.Id,
            Product = new ProductContextDto
            {
                DailyRate = 400,
                CycleTime = 72
            }
        };

        // Act
        var result = await FormsService.CreateAsync(request, user.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().BeGreaterThan(0);

        var form = await UnitOfWork.Forms.FindAsync(result.Value.Id);
        form.Should().NotBeNull();
        form.Rows.Should().NotBeEmpty();

        var workRows = form.Rows.Where(r => !r.IsAdditionalOperation).OrderBy(r => r.Order).ToList();
        workRows.Should().HaveCount(ShiftConstants.ShiftDurationHours);

        foreach (var row in workRows)
        {
            row.Values.Should().ContainKey(ShiftConstants.WorktimeIndicatorId.ToString());
        }

        const int planIndicatorId = 1;
        var rowsWithPlan = workRows
            .Where(r => r.Values.ContainsKey(planIndicatorId.ToString()))
            .ToList();

        rowsWithPlan.Should().HaveCount(ShiftConstants.ShiftDurationHours);

        var firstPlanCumulativeValue = GetCumulativeValue(rowsWithPlan[0], planIndicatorId);
        var secondPlanCumulativeValue = GetCumulativeValue(rowsWithPlan[1], planIndicatorId);
        var thirdPlanCumulativeValue = GetCumulativeValue(rowsWithPlan[2], planIndicatorId);

        firstPlanCumulativeValue.Should().Be(50);
        secondPlanCumulativeValue.Should().Be(100);
        thirdPlanCumulativeValue.Should().Be(150);
    }

    [Test]
    public async Task UpdateFormRowAsync_ShouldUpdateValuesAndRecalculateCumulativeValues()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);

        var template = await DbContext.Templates
            .Include(t => t.Indicators)
            .FirstAsync(t => t.Id == 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var createRequest = new CreateFormRequest
        {
            PaTypeId = template.PaTypeId,
            ShiftId = shift.Id,
            Product = new ProductContextDto
            {
                DailyRate = 100,
                CycleTime = 5
            }
        };

        var createResult = await FormsService.CreateAsync(createRequest, user.Id);
        createResult.IsSuccess.Should().BeTrue();

        var form = await UnitOfWork.Forms.FindAsync(createResult.Value.Id);
        var firstRow = form!.Rows.OrderBy(r => r.Order).First(r => !r.IsAdditionalOperation);

        // Act
        const int factIndicatorId = 2;
        const int factValue = 50;

        var updateRequest = new UpdateFormRowRequest
        {
            Values = new Dictionary<int, object>
            {
                { factIndicatorId, factValue }
            }
        };

        var updateResult = await FormsService.UpdateFormRowAsync(
            form.Id,
            1,
            updateRequest,
            user.Id);

        var updateRequest2 = new UpdateFormRowRequest
        {
            Values = new Dictionary<int, object>
            {
                { factIndicatorId, factValue }
            }
        };

        await FormsService.UpdateFormRowAsync(
            form.Id,
            2,
            updateRequest2,
            user.Id);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        updateResult.Value.Should().NotBeNull();

        var updatedForm = await UnitOfWork.Forms.FindAsync(form.Id);
        var updatedRow = updatedForm!.Rows.Single(r => r.Order == firstRow.Order);

        updatedRow.Values.Should().ContainKey(factIndicatorId.ToString());
        var updatedValue = GetValue(updatedRow, factIndicatorId);
        updatedValue.Should().Be(factValue);

        var subsequentRows = updatedForm.Rows
            .Where(r => !r.IsAdditionalOperation && r.Order > firstRow.Order)
            .OrderBy(r => r.Order)
            .ToList();

        var secondRow = subsequentRows.First();
        var secondCumulative = GetCumulativeValue(secondRow, factIndicatorId);
        secondCumulative.Should().Be(100);
    }

    [Test]
    public async Task UpdateFormRowAsync_ShouldRecalculateFormulasWhenDependentValuesChange()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);

        var template = await DbContext.Templates
            .Include(t => t.Indicators)
            .FirstAsync(t => t.Id == 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var createRequest = new CreateFormRequest
        {
            PaTypeId = template.PaTypeId,
            ShiftId = shift.Id,
            Product = new ProductContextDto
            {
                DailyRate = 400,
                CycleTime = 72
            }
        };

        var createResult = await FormsService.CreateAsync(createRequest, user.Id);
        createResult.IsSuccess.Should().BeTrue();

        var form = await UnitOfWork.Forms.FindAsync(createResult.Value.Id);
        var firstRow = form!.Rows.OrderBy(r => r.Order).First(r => !r.IsAdditionalOperation);

        // Act
        const int planIndicatorId = 1;
        const int factIndicatorId = 2;
        const int deviationIndicatorId = 3;

        const int planValue = 50;
        const int factValue = 80;

        var updateRequest = new UpdateFormRowRequest
        {
            Values = new Dictionary<int, object>
            {
                { factIndicatorId, factValue }
            }
        };

        var updateResult = await FormsService.UpdateFormRowAsync(
            form.Id,
            firstRow.Order,
            updateRequest,
            user.Id);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();

        var updatedForm = await UnitOfWork.Forms.FindAsync(form.Id);
        var updatedRow = updatedForm!.Rows.Single(r => r.Order == firstRow.Order);

        updatedRow.Values.Should().ContainKey(planIndicatorId.ToString());
        updatedRow.Values.Should().ContainKey(factIndicatorId.ToString());

        GetValue(updatedRow, planIndicatorId).Should().Be(planValue);
        GetValue(updatedRow, factIndicatorId).Should().Be(factValue);

        var deviationValue = GetValue(updatedRow, deviationIndicatorId);
        const int expectedDeviation = factValue - planValue;
        deviationValue.Should().Be(expectedDeviation);
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
}
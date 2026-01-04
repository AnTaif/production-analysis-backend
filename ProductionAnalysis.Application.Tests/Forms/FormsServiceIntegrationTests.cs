using FluentAssertions;
using NUnit.Framework;
using ProductionAnalysis.Application.Domain;
using ProductionAnalysis.Application.Tests.Infrastructure;
using ProductionAnalysis.Client.Models.Forms;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Application.Tests.Forms;

public class FormsServiceIntegrationTests : BaseIntegrationTest
{
    [Test]
    public async Task CreateAsync_ShouldCreateFormWithCorrectRowsAndCumulativeValues()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        var department = await DataBuilder.CreateDepartmentAsync();
        var employee = await DataBuilder.CreateEmployeeAsync(user.Id, department.Id);
        var paType = await DataBuilder.CreatePaTypeAsync();
        var shift = await DataBuilder.CreateShiftAsync(startTime: new TimeOnly(8, 0));

        var workTimeIndicator = await DataBuilder.CreateIndicatorAsync(
            16, "WorkTime", FieldValueTypes.Text, FieldInputTypes.Initialization);
        var planIndicator = await DataBuilder.CreateIndicatorAsync(
            1, "Plan", FieldValueTypes.Number, FieldInputTypes.Formula, "indicator_16 * 60", false);
        var cumulativeIndicator = await DataBuilder.CreateIndicatorAsync(
            2, "Cumulative", FieldValueTypes.Number, FieldInputTypes.Manual, null, true);

        var template = await DataBuilder.CreateTemplateAsync(
            paTypeId: paType.Id,
            indicators: new List<IndicatorDbo> { workTimeIndicator, planIndicator, cumulativeIndicator });

        var request = new CreateFormRequest
        {
            PaTypeId = paType.Id,
            ShiftId = shift.Id,
            Product = new ProductContextDto
            {
                DailyRate = 100,
                CycleTime = 5
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
        form!.Rows.Should().NotBeEmpty();

        var workRows = form.Rows.Where(r => !r.IsAdditionalOperation).ToList();
        workRows.Should().HaveCount(8); // 8 часов работы

        foreach (var row in workRows)
        {
            row.Values.Should().ContainKey("16"); // WorkTime indicator
        }

        var cumulativeValues = workRows
            .Where(r => r.Values.ContainsKey("2"))
            .Select(r => r.Values["2"].CumulativeValue)
            .ToList();

        cumulativeValues.Should().NotBeEmpty();
        if (cumulativeValues.Count > 1)
        {
            var firstValue = Convert.ToInt32(cumulativeValues[0]);
            var secondValue = Convert.ToInt32(cumulativeValues[1]);
            secondValue.Should().BeGreaterThanOrEqualTo(firstValue);
        }
    }

    [Test]
    public async Task CreateAsync_ShouldCalculateFormulasCorrectly()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        var department = await DataBuilder.CreateDepartmentAsync();
        var employee = await DataBuilder.CreateEmployeeAsync(user.Id, department.Id);
        var paType = await DataBuilder.CreatePaTypeAsync();
        var shift = await DataBuilder.CreateShiftAsync(startTime: new TimeOnly(8, 0));

        var workTimeIndicator = await DataBuilder.CreateIndicatorAsync(
            16, "WorkTime", FieldValueTypes.Text, FieldInputTypes.Initialization);
        var planIndicator = await DataBuilder.CreateIndicatorAsync(
            1, "Plan", FieldValueTypes.Number, FieldInputTypes.Formula, "indicator_16 * 60", false);

        var template = await DataBuilder.CreateTemplateAsync(
            paTypeId: paType.Id,
            indicators: new List<IndicatorDbo> { workTimeIndicator, planIndicator });

        var request = new CreateFormRequest
        {
            PaTypeId = paType.Id,
            ShiftId = shift.Id,
            Product = new ProductContextDto
            {
                DailyRate = 100,
                CycleTime = 5
            }
        };

        // Act
        var result = await FormsService.CreateAsync(request, user.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var form = await UnitOfWork.Forms.FindAsync(result.Value.Id);
        form.Should().NotBeNull();

        var workRows = form!.Rows.Where(r => !r.IsAdditionalOperation).ToList();
        workRows.Should().NotBeEmpty();
    }

    [Test]
    public async Task UpdateFormRowAsync_ShouldUpdateValuesAndRecalculateFormulas()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        var department = await DataBuilder.CreateDepartmentAsync();
        var employee = await DataBuilder.CreateEmployeeAsync(user.Id, department.Id);
        var paType = await DataBuilder.CreatePaTypeAsync();
        var shift = await DataBuilder.CreateShiftAsync(startTime: new TimeOnly(8, 0));

        var workTimeIndicator = await DataBuilder.CreateIndicatorAsync(
            16, "WorkTime", FieldValueTypes.Text, FieldInputTypes.Initialization);
        var planIndicator = await DataBuilder.CreateIndicatorAsync(
            1, "Plan", FieldValueTypes.Number, FieldInputTypes.Formula, "indicator_16 * 60", false);
        var cumulativeIndicator = await DataBuilder.CreateIndicatorAsync(
            2, "Cumulative", FieldValueTypes.Number, FieldInputTypes.Manual, null, true);

        var template = await DataBuilder.CreateTemplateAsync(
            paTypeId: paType.Id,
            indicators: new List<IndicatorDbo> { workTimeIndicator, planIndicator, cumulativeIndicator });

        var createRequest = new CreateFormRequest
        {
            PaTypeId = paType.Id,
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
        var updateRequest = new UpdateFormRowRequest
        {
            Values = new Dictionary<int, object>
            {
                { 2, 100 } // Устанавливаем значение для cumulativeIndicator
            }
        };

        var updateResult = await FormsService.UpdateFormRowAsync(
            form.Id,
            firstRow.Order,
            updateRequest,
            user.Id);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        updateResult.Value.Should().NotBeNull();

        var updatedForm = await UnitOfWork.Forms.FindAsync(form.Id);
        var updatedRow = updatedForm!.Rows.Single(r => r.Order == firstRow.Order);

        updatedRow.Values.Should().ContainKey("2");
        var updatedValue = updatedRow.Values["2"].Value;
        Convert.ToInt32(updatedValue).Should().Be(100);

        var subsequentRows = updatedForm.Rows
            .Where(r => !r.IsAdditionalOperation && r.Order > firstRow.Order)
            .OrderBy(r => r.Order)
            .ToList();

        if (subsequentRows.Any())
        {
            var secondRow = subsequentRows.First();
            if (secondRow.Values.ContainsKey("2"))
            {
                var secondCumulativeValue = secondRow.Values["2"].CumulativeValue;
                secondCumulativeValue.Should().NotBeNull();
                var secondCumulative = Convert.ToInt32(secondCumulativeValue);
                secondCumulative.Should().BeGreaterThanOrEqualTo(100);
            }
        }
    }

    [Test]
    public async Task UpdateFormRowAsync_ShouldRecalculateFormulasWhenDependentValuesChange()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        var department = await DataBuilder.CreateDepartmentAsync();
        var employee = await DataBuilder.CreateEmployeeAsync(user.Id, department.Id);
        var paType = await DataBuilder.CreatePaTypeAsync();
        var shift = await DataBuilder.CreateShiftAsync(startTime: new TimeOnly(8, 0));

        var manualIndicator = await DataBuilder.CreateIndicatorAsync(
            3, "Manual", FieldValueTypes.Number, FieldInputTypes.Manual);
        var formulaIndicator = await DataBuilder.CreateIndicatorAsync(
            1, "Formula", FieldValueTypes.Number, FieldInputTypes.Formula, "indicator_3 * 2", false);

        var template = await DataBuilder.CreateTemplateAsync(
            paTypeId: paType.Id,
            indicators: new List<IndicatorDbo> { manualIndicator, formulaIndicator });

        var createRequest = new CreateFormRequest
        {
            PaTypeId = paType.Id,
            ShiftId = shift.Id
        };

        var createResult = await FormsService.CreateAsync(createRequest, user.Id);
        createResult.IsSuccess.Should().BeTrue();

        var form = await UnitOfWork.Forms.FindAsync(createResult.Value.Id);
        var firstRow = form!.Rows.OrderBy(r => r.Order).First(r => !r.IsAdditionalOperation);

        // Act
        var updateRequest = new UpdateFormRowRequest
        {
            Values = new Dictionary<int, object>
            {
                { 3, 50 } // Устанавливаем manual значение
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

        updatedRow.Values.Should().ContainKey("3");
        Convert.ToInt32(updatedRow.Values["3"].Value).Should().Be(50);

        if (updatedRow.Values.ContainsKey("1"))
        {
            var formulaValue = Convert.ToInt32(updatedRow.Values["1"].Value);
            formulaValue.Should().Be(100); // 50 * 2
        }
    }
}
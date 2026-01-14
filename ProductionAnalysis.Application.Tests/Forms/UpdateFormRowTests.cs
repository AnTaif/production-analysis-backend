using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Tests.Forms;

public class UpdateFormRowTests : FormsTestBase
{
    [Test]
    public async Task UpdateFormRowAsync_ShouldRecalculateCumulativeValues()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var request = CreateSingleProductFormRequest(shift.Id, assigneeId);

        var result = await FormsService.CreateAsync(request, user.Id);
        result.IsSuccess.Should().BeTrue();

        var form = await UnitOfWork.Forms.FindAsync(result.Value.Id);
        form.Should().NotBeNull();
        const int factIndicatorId = 3;
        const int factCumulativeIndicatorId = 20; // Накопительный индикатор для факта
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

        var secondCumulative = GetCumulativeValue(secondRow, factCumulativeIndicatorId);
        secondCumulative.Should().Be(100);
    }

    [Test]
    public async Task UpdateFormRowAsync_ShouldRecalculateFormulas()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);
        var request = CreateSingleProductFormRequest(shift.Id, assigneeId);

        var result = await FormsService.CreateAsync(request, user.Id);
        result.IsSuccess.Should().BeTrue();

        var form = await UnitOfWork.Forms.FindAsync(result.Value.Id);
        form.Should().NotBeNull();
        var firstRow = form.Rows
            .Where(r => !r.IsAuxiliaryOperation)
            .OrderBy(r => r.Order)
            .First();

        const int planIndicatorId = 2;
        const int factIndicatorId = 3;
        const int deviationIndicatorId = 4;
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

    [Test]
    public async Task UpdateFormRowAsync_ShouldRecalculateCumulativeValuesAfterFormulaRecalculation()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);
        var request = CreateSingleProductFormRequest(shift.Id, assigneeId);

        var result = await FormsService.CreateAsync(request, user.Id);
        result.IsSuccess.Should().BeTrue();

        var form = await UnitOfWork.Forms.FindAsync(result.Value.Id);
        form.Should().NotBeNull();

        var workRows = form.Rows
            .Where(r => !r.IsAuxiliaryOperation)
            .OrderBy(r => r.Order)
            .ToList();

        const int planIndicatorId = 2;
        const int factIndicatorId = 3;
        const int deviationIndicatorId = 4; // Формула: факт - план
        const int deviationCumulativeIndicatorId = 21; // Накопительное отклонение

        // Получаем начальные значения плана для первой и второй строк
        var firstRowPlan = GetValue(workRows[0], planIndicatorId);
        var secondRowPlan = GetValue(workRows[1], planIndicatorId);

        const int firstRowFact = 80;
        const int secondRowFact = 70;

        // Act - обновляем факт в первой строке
        await UpdateRowAsync(form.Id, workRows[0].Order, factIndicatorId, firstRowFact, user.Id);

        // Assert - проверяем первую строку
        var updatedForm = await UnitOfWork.Forms.FindAsync(form.Id);
        var updatedFirstRow = updatedForm!.Rows
            .Where(r => !r.IsAuxiliaryOperation)
            .OrderBy(r => r.Order)
            .First();

        // Проверяем, что отклонение пересчиталось
        var firstRowDeviation = GetValue(updatedFirstRow, deviationIndicatorId);
        var expectedFirstRowDeviation = firstRowFact - firstRowPlan;
        firstRowDeviation.Should().Be(expectedFirstRowDeviation,
            "отклонение должно пересчитаться после обновления факта");

        // Проверяем, что накопительное отклонение пересчиталось на основе актуального отклонения
        var firstRowDeviationCumulative = GetCumulativeValue(updatedFirstRow, deviationCumulativeIndicatorId);
        firstRowDeviationCumulative.Should().Be(expectedFirstRowDeviation,
            "накопительное отклонение должно быть равно отклонению в первой строке");

        // Act - обновляем факт во второй строке
        await UpdateRowAsync(form.Id, workRows[1].Order, factIndicatorId, secondRowFact, user.Id);

        // Assert - проверяем вторую строку
        updatedForm = await UnitOfWork.Forms.FindAsync(form.Id);
        var updatedSecondRow = updatedForm!.Rows
            .Where(r => !r.IsAuxiliaryOperation)
            .OrderBy(r => r.Order)
            .Skip(1)
            .First();

        // Проверяем, что отклонение пересчиталось
        var secondRowDeviation = GetValue(updatedSecondRow, deviationIndicatorId);
        var expectedSecondRowDeviation = secondRowFact - secondRowPlan;
        secondRowDeviation.Should().Be(expectedSecondRowDeviation,
            "отклонение должно пересчитаться после обновления факта во второй строке");

        // Проверяем, что накопительное отклонение пересчиталось с учетом отклонения из первой строки
        var secondRowDeviationCumulative = GetCumulativeValue(updatedSecondRow, deviationCumulativeIndicatorId);
        var expectedSecondRowDeviationCumulative = expectedFirstRowDeviation + expectedSecondRowDeviation;
        secondRowDeviationCumulative.Should().Be(expectedSecondRowDeviationCumulative,
            "накопительное отклонение во второй строке должно быть суммой отклонений из первой и второй строк");
    }

    [Test]
    public async Task UpdateFormRowAsync_ShouldReturnAllRowsAfterUpdate()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);
        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);
        var request = CreateSingleProductFormRequest(shift.Id, assigneeId);

        var createResult = await FormsService.CreateAsync(request, user.Id);
        createResult.IsSuccess.Should().BeTrue();

        var form = await UnitOfWork.Forms.FindAsync(createResult.Value.Id);
        var firstRow = form!.Rows
            .Where(r => !r.IsAuxiliaryOperation)
            .OrderBy(r => r.Order)
            .First();

        const int factIndicatorId = 3;
        const int factValue = 50;

        // Act
        var updateRequest = new UpdateFormRowRequest
        {
            Values = new Dictionary<int, object> { { factIndicatorId, factValue } }
        };

        var result = await FormsService.UpdateFormRowAsync(form.Id, firstRow.Order, updateRequest, user.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Rows.Should().NotBeEmpty();
        result.Value.Rows.Should().HaveCount(form.Rows.Count);
    }

    [Test]
    public async Task UpdateFormRowAsync_ShouldReturnTotalsAfterUpdate()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);
        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);
        var request = CreateSingleProductFormRequest(shift.Id, assigneeId);

        var createResult = await FormsService.CreateAsync(request, user.Id);
        createResult.IsSuccess.Should().BeTrue();

        var form = await UnitOfWork.Forms.FindAsync(createResult.Value.Id);
        var firstRow = form!.Rows
            .Where(r => !r.IsAuxiliaryOperation)
            .OrderBy(r => r.Order)
            .First();

        const int factIndicatorId = 3;
        const int factValue = 50;

        // Act
        var updateRequest = new UpdateFormRowRequest
        {
            Values = new Dictionary<int, object> { { factIndicatorId, factValue } }
        };

        var result = await FormsService.UpdateFormRowAsync(form.Id, firstRow.Order, updateRequest, user.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Totals.Should().NotBeNull();
    }

    [Test]
    public async Task UpdateFormRowAsync_WithNonExistentForm_ShouldReturnNotFound()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        var updateRequest = new UpdateFormRowRequest
        {
            Values = new Dictionary<int, object> { { 3, 50 } }
        };

        // Act
        var result = await FormsService.UpdateFormRowAsync(99999, 1, updateRequest, user.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("not found");
    }

    [Test]
    public async Task UpdateFormRowAsync_WithNonExistentRow_ShouldReturnNotFound()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);
        var assigneeId = await CreateAssigneeAsync(departmentId: 1);
        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);
        var request = CreateSingleProductFormRequest(shift.Id, assigneeId);

        var createResult = await FormsService.CreateAsync(request, user.Id);
        createResult.IsSuccess.Should().BeTrue();

        var updateRequest = new UpdateFormRowRequest
        {
            Values = new Dictionary<int, object> { { 3, 50 } }
        };

        // Act
        var result = await FormsService.UpdateFormRowAsync(createResult.Value.Id, 999, updateRequest, user.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("not found");
    }
}
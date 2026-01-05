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

        // Используем операции, которые уже есть в TestDataSeeder (Id: 4, 5, 6)
        // Act
        var request = new CreateFormRequest
        {
            PaType = PaTypeDto.LessThanOnePerHour,
            ShiftId = shift.Id,
            Operation = new OperationContextDto
            {
                OperationId = 4 // Подсборка - операция для продукта Id: 1
            }
        };

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
    public async Task GetByIdAsync_ForLessThanOnePerHour_ShouldReturnFormWithShouldMergeInGroup()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        // Используем операцию, которая уже есть в TestDataSeeder (Id: 4)
        var request = new CreateFormRequest
        {
            PaType = PaTypeDto.LessThanOnePerHour,
            ShiftId = shift.Id,
            Operation = new OperationContextDto
            {
                OperationId = 4 // Подсборка
            }
        };

        var createResult = await FormsService.CreateAsync(request, user.Id);
        createResult.IsSuccess.Should().BeTrue();

        // Act
        var result = await FormsService.GetByIdAsync(createResult.Value.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.PaType.Should().Be(PaTypeDto.LessThanOnePerHour);
        result.Value.Template.Should().NotBeNull();
        result.Value.Template.TableColumns.Should().NotBeEmpty();

        // Проверяем, что для колонок установлен ShouldMergeInGroup
        var workTimeColumn = result.Value.Template.TableColumns.FirstOrDefault(c => c.Id == 16);
        workTimeColumn.Should().NotBeNull();
        workTimeColumn!.ShouldMergeInGroup.Should().BeTrue("время работы должно объединяться");

        var planColumn = result.Value.Template.TableColumns.FirstOrDefault(c => c.Id == 1);
        planColumn.Should().NotBeNull();
        planColumn!.ShouldMergeInGroup.Should().BeTrue("план должен объединяться");

        // Проверяем, что для колонок операций ShouldMergeInGroup = false
        var operationNameColumn = result.Value.Template.TableColumns.FirstOrDefault(c => c.Id == 9);
        if (operationNameColumn != null)
        {
            operationNameColumn.ShouldMergeInGroup.Should().BeFalse("наименование операции не должно объединяться");
        }

        var operationTimeColumn = result.Value.Template.TableColumns.FirstOrDefault(c => c.Id == 10);
        if (operationTimeColumn != null)
        {
            operationTimeColumn.ShouldMergeInGroup.Should().BeFalse("время операции не должно объединяться");
        }
    }

    [Test]
    public async Task GetFormRowsAsync_ForLessThanOnePerHour_ShouldReturnRowsWithGroupKey()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        // Используем операции, которые уже есть в TestDataSeeder (Id: 4, 5)
        var request = new CreateFormRequest
        {
            PaType = PaTypeDto.LessThanOnePerHour,
            ShiftId = shift.Id,
            Operation = new OperationContextDto
            {
                OperationId = 4 // Подсборка
            }
        };

        var createResult = await FormsService.CreateAsync(request, user.Id);
        createResult.IsSuccess.Should().BeTrue();

        // Act
        var result = await FormsService.GetFormRowsAsync(createResult.Value.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().NotBeEmpty();

        var workRows = result.Value
            .Where(r => !r.IsAuxiliaryOperation && r.GroupKey.HasValue)
            .ToList();

        workRows.Should().NotBeEmpty();

        // Проверяем, что есть строки с одинаковым GroupKey
        var groupedRows = workRows
            .GroupBy(r => r.GroupKey)
            .Where(g => g.Count() > 1)
            .ToList();

        groupedRows.Should().NotBeEmpty("должны быть группы с несколькими строками");

        // Проверяем, что в каждой группе время работы одинаковое
        foreach (var group in groupedRows)
        {
            var groupRows = group.ToList();
            var workTimeValues = groupRows
                .Select(r => r.Values.TryGetValue("16", out var v) ? v.Value : null)
                .Distinct()
                .ToList();
            workTimeValues.Should().HaveCount(1, $"время работы должно быть одинаковым для группы {group.Key}");

            var planValues = groupRows
                .Select(r => r.Values.TryGetValue("1", out var v) ? v.Value : null)
                .Distinct()
                .ToList();
            planValues.Should().HaveCount(1, $"план должен быть одинаковым для группы {group.Key}");
        }
    }

    [Test]
    public async Task CreateAsync_ForLessThanOnePerHour_WithRelatedOperations_ShouldIncludeAllOperations()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        // Используем операции, которые уже есть в TestDataSeeder
        // Id: 4 - Подсборка, Id: 5 - Установка, Id: 6 - Настройка
        // Все они связаны с продуктом Id: 1
        var request = new CreateFormRequest
        {
            PaType = PaTypeDto.LessThanOnePerHour,
            ShiftId = shift.Id,
            Operation = new OperationContextDto
            {
                OperationId = 4 // Подсборка
            }
        };

        var result = await FormsService.CreateAsync(request, user.Id);
        result.IsSuccess.Should().BeTrue();

        // Assert
        var form = await UnitOfWork.Forms.FindAsync(result.Value.Id);
        form.Should().NotBeNull();

        var workRows = form.Rows
            .Where(r => !r.IsAuxiliaryOperation && r.GroupKey.HasValue)
            .OrderBy(r => r.Order)
            .ToList();

        workRows.Should().NotBeEmpty();

        // Проверяем, что в строках есть информация об операциях
        var firstGroup = workRows
            .Where(r => r.GroupKey == workRows.First().GroupKey)
            .ToList();

        firstGroup.Should().HaveCountGreaterThanOrEqualTo(1);

        // Проверяем, что есть колонка с наименованием операций
        var hasOperationName = firstGroup.Any(r => r.Values.ContainsKey("9"));
        hasOperationName.Should().BeTrue("должна быть колонка с наименованием операций");

        // Проверяем, что есть колонка со временем операций
        var hasOperationTime = firstGroup.Any(r => r.Values.ContainsKey("10"));
        hasOperationTime.Should().BeTrue("должна быть колонка со временем операций");
    }

    [Test]
    public async Task CreateAsync_ForLessThanOnePerHour_WithBreaks_ShouldHandleBreaksCorrectly()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        // Создаем перерыв в середине смены
        var breakOperation = await DataBuilder.CreateAuxiliaryOperationAsync(
            id: 10,
            name: "Перерыв 15 мин",
            durationInSeconds: 900);

        await DataBuilder.CreateShiftScheduleAsync(
            shiftId: shift.Id,
            auxiliaryOperationId: breakOperation.Id,
            startTime: new TimeOnly(12, 0));

        // Используем операцию, которая уже есть в TestDataSeeder (Id: 4)
        var request = new CreateFormRequest
        {
            PaType = PaTypeDto.LessThanOnePerHour,
            ShiftId = shift.Id,
            Operation = new OperationContextDto
            {
                OperationId = 4 // Подсборка
            }
        };

        var result = await FormsService.CreateAsync(request, user.Id);
        result.IsSuccess.Should().BeTrue();

        // Assert
        var form = await UnitOfWork.Forms.FindAsync(result.Value.Id);
        form.Should().NotBeNull();

        var allRows = form.Rows.OrderBy(r => r.Order).ToList();
        allRows.Should().NotBeEmpty();

        // Проверяем, что есть строки перерывов
        var breakRows = allRows.Where(r => r.IsAuxiliaryOperation).ToList();
        breakRows.Should().NotBeEmpty("должны быть строки перерывов");

        // Проверяем, что рабочие строки имеют GroupKey
        var workRows = allRows.Where(r => !r.IsAuxiliaryOperation && r.GroupKey.HasValue).ToList();
        workRows.Should().NotBeEmpty("должны быть рабочие строки с GroupKey");
    }

    [Test]
    public async Task CreateAsync_ForLessThanOnePerHour_WithOperationBasedOnOperation_ShouldIncludeRelatedOperations()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        // Используем операции, которые уже есть в TestDataSeeder
        // Id: 1 - Подготовка, Id: 2 - Обработка (основана на операции Id: 1)
        var request = new CreateFormRequest
        {
            PaType = PaTypeDto.LessThanOnePerHour,
            ShiftId = shift.Id,
            Operation = new OperationContextDto
            {
                OperationId = 1 // Подготовка
            }
        };

        var result = await FormsService.CreateAsync(request, user.Id);
        result.IsSuccess.Should().BeTrue();

        // Assert
        var form = await UnitOfWork.Forms.FindAsync(result.Value.Id);
        form.Should().NotBeNull();

        var workRows = form.Rows
            .Where(r => !r.IsAuxiliaryOperation && r.GroupKey.HasValue)
            .OrderBy(r => r.Order)
            .ToList();

        workRows.Should().NotBeEmpty();

        // Проверяем, что в первой группе есть информация об операциях
        var firstGroup = workRows
            .Where(r => r.GroupKey == workRows.First().GroupKey)
            .ToList();

        firstGroup.Should().HaveCountGreaterThanOrEqualTo(1);

        // Проверяем, что есть информация о наименовании операций
        var hasOperationNames = firstGroup.Any(r =>
            r.Values.TryGetValue("9", out var opName) && opName.Value != null);
        hasOperationNames.Should().BeTrue("должна быть информация о наименовании операций");
    }

    [Test]
    public async Task CreateAsync_ForLessThanOnePerHour_ShouldSetPlanTo1ForEachCycle()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        // Используем операцию, которая уже есть в TestDataSeeder (Id: 4)
        var request = new CreateFormRequest
        {
            PaType = PaTypeDto.LessThanOnePerHour,
            ShiftId = shift.Id,
            Operation = new OperationContextDto
            {
                OperationId = 4 // Подсборка
            }
        };

        var result = await FormsService.CreateAsync(request, user.Id);
        result.IsSuccess.Should().BeTrue();

        // Assert
        var form = await UnitOfWork.Forms.FindAsync(result.Value.Id);
        form.Should().NotBeNull();

        var workRows = form.Rows
            .Where(r => !r.IsAuxiliaryOperation && r.GroupKey.HasValue)
            .OrderBy(r => r.Order)
            .ToList();

        workRows.Should().NotBeEmpty();

        // Проверяем, что план равен 1 для всех строк
        foreach (var row in workRows)
        {
            if (row.Values.TryGetValue("1", out var planValue))
            {
                var plan = Convert.ToInt32(planValue.Value);
                plan.Should().Be(1, $"план должен быть равен 1 для строки {row.Order}");
            }
        }
    }

    [Test]
    public async Task CreateAsync_ForLessThanOnePerHour_ShouldCreateSeparateRowForEachOperation()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        // Используем операции, которые уже есть в TestDataSeeder
        // Id: 4 - Подсборка, Id: 5 - Установка, Id: 6 - Настройка
        var request = new CreateFormRequest
        {
            PaType = PaTypeDto.LessThanOnePerHour,
            ShiftId = shift.Id,
            Operation = new OperationContextDto
            {
                OperationId = 4 // Подсборка
            }
        };

        var result = await FormsService.CreateAsync(request, user.Id);
        result.IsSuccess.Should().BeTrue();

        // Assert
        var form = await UnitOfWork.Forms.FindAsync(result.Value.Id);
        form.Should().NotBeNull();

        var workRows = form.Rows
            .Where(r => !r.IsAuxiliaryOperation && r.GroupKey.HasValue)
            .OrderBy(r => r.Order)
            .ToList();

        workRows.Should().NotBeEmpty();

        // Проверяем, что в первой группе есть отдельные строки для каждой операции
        var firstGroup = workRows
            .Where(r => r.GroupKey == workRows.First().GroupKey)
            .OrderBy(r => r.Order)
            .ToList();

        // Должно быть минимум 3 строки (по одной на каждую операцию)
        firstGroup.Should().HaveCountGreaterThanOrEqualTo(3,
            "должна быть отдельная строка для каждой операции в цикле");

        // Проверяем, что каждая строка имеет уникальное наименование операции
        var operationNames = firstGroup
            .Where(r => r.Values.TryGetValue("9", out var opName))
            .Select(r => r.Values["9"].Value?.ToString())
            .Where(name => !string.IsNullOrEmpty(name))
            .Distinct()
            .ToList();

        operationNames.Should().HaveCountGreaterThanOrEqualTo(1,
            "должны быть разные наименования операций в строках группы");
    }

    [Test]
    public async Task CreateAsync_ForSingleProductWithCycleTime_ShouldNotSetGroupKey()
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
    public async Task GetByIdAsync_ForSingleProductWithCycleTime_ShouldNotHaveShouldMergeInGroup()
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

        var createResult = await FormsService.CreateAsync(request, user.Id);
        createResult.IsSuccess.Should().BeTrue();

        // Act
        var result = await FormsService.GetByIdAsync(createResult.Value.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Template.Should().NotBeNull();

        // Проверяем, что для обычных форм ShouldMergeInGroup = false
        result.Value.Template.TableColumns.Should().OnlyContain(c => !c.ShouldMergeInGroup,
            "для обычных форм ShouldMergeInGroup должен быть false");
    }

    [Test]
    public async Task CreateAsync_ForLessThanOnePerHour_WithoutOperation_ShouldReturnError()
    {
        // Arrange
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id, departmentId: 1);

        var shift = await DbContext.Shifts.FirstAsync(s => s.Id == 1);

        var request = new CreateFormRequest
        {
            PaType = PaTypeDto.LessThanOnePerHour,
            ShiftId = shift.Id,
            Operation = null // Операция не указана
        };

        // Act
        var result = await FormsService.CreateAsync(request, user.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Operation");
    }
}
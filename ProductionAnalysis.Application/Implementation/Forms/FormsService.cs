using System.Globalization;
using Core.Results;
using ProductionAnalysis.Application.Converters;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Forms;
using FormStatus = ProductionAnalysis.Application.Domain.Forms.FormStatus;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IFormsService
{
    Task<PaginatedResult<FormShortDto>> SearchFormsAsync(SearchFormsFilterDto searchFilter);
    Task<Result<FormShortDto>> CreateAsync(CreateFormRequest request, Guid creatorId);
    Task<Result<FormDto>> GetByIdAsync(int formId);
    Task<Result<ICollection<FormRowDto>>> GetFormRowsAsync(int formId);
    Task<Result<FormRowDto>> UpdateFormRowAsync(int formId, short rowOrder, UpdateFormRowRequest request, Guid userId);
}

[RegisterScoped]
public class FormsService(
    IPaUnitOfWork unitOfWork,
    IFormRowInitializer formRowInitializer,
    IFormRowValueFilter formRowValueFilter,
    IFormRowFormulaCalculator formRowFormulaCalculator,
    ICumulativeValueCalculator cumulativeValueCalculator,
    ITotalValueCalculator totalValueCalculator
)
    : IFormsService
{
    public async Task<PaginatedResult<FormShortDto>> SearchFormsAsync(SearchFormsFilterDto searchFilter)
    {
        var domainFilter = searchFilter.ToDomain();
        var (forms, totalCount) = await unitOfWork.Forms.SearchFormsAsync(domainFilter);

        var dtos = forms.Select(f => f.ToShortDto()).ToList();

        var response = new PaginatedResponse<FormShortDto>(
            dtos,
            totalCount,
            domainFilter.PageNumber,
            domainFilter.PageSize
        );

        return response;
    }

    public async Task<Result<FormShortDto>> CreateAsync(CreateFormRequest request, Guid creatorId)
    {
        var template = await unitOfWork.Templates.FindLatestVerAsync(request.PaTypeId);
        if (template is null)
        {
            return ServiceError.NotFound($"Template for PaType {request.PaTypeId} not found");
        }

        var employee = await unitOfWork.Dictionaries.FindEmployeeByUserIdAsync(creatorId);
        if (employee is null)
        {
            return ServiceError.NotFound($"Employee for user {creatorId} not found");
        }

        var shift = await unitOfWork.Dictionaries.SelectShiftByIdAsync(request.ShiftId);
        if (shift == null)
        {
            return ServiceError.NotFound($"Shift not found by id {request.ShiftId}");
        }

        var context = request.ExtractDomainContext();

        var newForm = new Form(
            0,
            request.PaTypeId,
            FormStatus.InProgress,
            DateTime.UtcNow,
            DateTime.UtcNow,
            context,
            template,
            new List<FormRow>(),
            creatorId,
            shift.Id,
            employee.DepartmentId
        );

        var form = await unitOfWork.Forms.CreateAsync(newForm);

        var schedules = await unitOfWork.Dictionaries.SelectShiftSchedulesByShiftIdAsync(shift.Id);
        var rows = await formRowInitializer.InitializeRowsForShiftAsync(
            shift.StartTime,
            schedules,
            template,
            form.Context);

        unitOfWork.FormRows.AddRows(form.Id, rows);
        await unitOfWork.SaveChangesAsync();

        return form.ToShortDto();
    }

    public async Task<Result<FormDto>> GetByIdAsync(int formId)
    {
        var form = await unitOfWork.Forms.FindAsync(formId);

        if (form == null)
        {
            return ServiceError.NotFound($"Form with id {formId} not found");
        }

        // Вычисляем и обновляем итоговые значения, если они еще не вычислены или устарели
        var totals = totalValueCalculator.CalculateTotals(form);
        var hasTotals = form.TotalValues != null && AreTotalsEqual(form.TotalValues, totals);

        if (!hasTotals && totals.Count > 0)
        {
            await unitOfWork.Forms.UpdateTotalValuesAsync(formId, totals, form.CreatorId);
            // Перезагружаем форму с обновленными totals
            form = await unitOfWork.Forms.FindAsync(formId);
            if (form == null)
            {
                return ServiceError.NotFound($"Form with id {formId} not found after totals update");
            }
        }

        return form.ToDto();
    }

    private static bool AreTotalsEqual(Dictionary<int, object> totals1, Dictionary<int, object> totals2)
    {
        if (totals1.Count != totals2.Count)
        {
            return false;
        }

        foreach (var (key, value1) in totals1)
        {
            if (!totals2.TryGetValue(key, out var value2))
            {
                return false;
            }

            if (!AreValuesEqual(value1, value2))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AreValuesEqual(object? value1, object? value2)
    {
        if (value1 == null && value2 == null)
        {
            return true;
        }

        if (value1 == null || value2 == null)
        {
            return false;
        }

        // Пытаемся сравнить как числа
        if (TryConvertToDouble(value1, out var num1) && TryConvertToDouble(value2, out var num2))
        {
            return Math.Abs(num1 - num2) < 0.0001;
        }

        return value1.Equals(value2);
    }

    private static bool TryConvertToDouble(object value, out double result)
    {
        result = 0;
        return value switch
        {
            int i => (result = i, true).Item2,
            long l => (result = l, true).Item2,
            double d => (result = d, true).Item2,
            decimal dec => (result = (double)dec, true).Item2,
            float f => (result = f, true).Item2,
            string s => double.TryParse(s, NumberStyles.Any,
                CultureInfo.InvariantCulture, out result),
            _ => false
        };
    }

    public async Task<Result<ICollection<FormRowDto>>> GetFormRowsAsync(int formId)
    {
        var form = await unitOfWork.Forms.FindAsync(formId);

        if (form == null)
        {
            return ServiceError.NotFound($"Form with id {formId} not found");
        }

        return form.Rows.ToRowDtos();
    }

    public async Task<Result<FormRowDto>> UpdateFormRowAsync(int formId, short rowOrder, UpdateFormRowRequest request,
        Guid userId)
    {
        var form = await unitOfWork.Forms.FindAsync(formId);
        if (form is null)
        {
            return ServiceError.NotFound($"Form {formId} not found");
        }

        var row = form.Rows.SingleOrDefault(r => r.Order == rowOrder);
        if (row is null)
        {
            return ServiceError.NotFound($"Row {rowOrder} not found in form {formId}");
        }

        var filteredValues = formRowValueFilter.FilterUpdatableValues(
            request.Values,
            form.TemplateSnapshot);

        if (filteredValues.Count == 0)
        {
            return row.ToRowDto();
        }

        var template = form.TemplateSnapshot;

        await unitOfWork.FormRows.UpdateRowValuesAsync(
            formId,
            rowOrder,
            filteredValues,
            userId);

        await unitOfWork.SaveChangesAsync();

        // Перезагружаем форму с обновленными значениями для расчета формул
        var formForFormulas = await unitOfWork.Forms.FindAsync(formId);
        if (formForFormulas == null)
        {
            return ServiceError.NotFound($"Form {formId} not found after update");
        }

        var updatedRow = formForFormulas.Rows.SingleOrDefault(r => r.Order == rowOrder);
        if (updatedRow == null)
        {
            return ServiceError.NotFound($"Row {rowOrder} not found in form {formId} after update");
        }

        var updatedIndicatorIds = filteredValues.Select(v => v.IndicatorId).ToList();
        var formulaValuesToUpdate = await formRowFormulaCalculator.CalculateFormulaValuesAsync(
            updatedRow,
            template,
            updatedIndicatorIds,
            formForFormulas.Context);

        if (formulaValuesToUpdate.Count != 0)
        {
            await unitOfWork.FormRows.UpdateRowValuesAsync(
                formId,
                rowOrder,
                formulaValuesToUpdate,
                userId);
            await unitOfWork.SaveChangesAsync();
        }

        var formForCumulative = await unitOfWork.Forms.FindAsync(formId);
        if (formForCumulative != null)
        {
            var cumulativeValuesToUpdate = cumulativeValueCalculator.CalculateCumulativeValues(
                formForCumulative,
                rowOrder);

            if (cumulativeValuesToUpdate.Count > 0)
            {
                await unitOfWork.FormRows.UpdateMultipleRowsValuesAsync(
                    formId,
                    cumulativeValuesToUpdate,
                    userId);
            }
        }

        await unitOfWork.SaveChangesAsync();

        // Вычисляем и обновляем итоговые значения
        var formForTotals = await unitOfWork.Forms.FindAsync(formId);
        if (formForTotals != null)
        {
            var totals = totalValueCalculator.CalculateTotals(formForTotals);
            if (totals.Count > 0)
            {
                await unitOfWork.Forms.UpdateTotalValuesAsync(formId, totals, userId);
            }
        }

        var finalForm = await unitOfWork.Forms.FindAsync(formId);
        var finalRow = finalForm?.Rows.SingleOrDefault(r => r.Order == rowOrder);

        if (finalRow == null)
        {
            return ServiceError.NotFound($"Form row with Order={rowOrder} not found after update");
        }

        return finalRow.ToRowDto();
    }
}
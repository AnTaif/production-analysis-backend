using Core.Results;
using ProductionAnalysis.Application.Converters;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IFormRowUpdateOrchestrator
{
    Task<Result<UpdateFormRowResponse>> UpdateRowAsync(
        int formId,
        short rowOrder,
        UpdateFormRowRequest request,
        Guid userId);
}

/// <summary>
/// Оркестратор обновления строки формы
/// </summary>
[RegisterScoped]
public class FormRowUpdateOrchestrator(
    IPaUnitOfWork unitOfWork,
    IFormRowValueFilter formRowValueFilter,
    IFormRowFormulaCalculator formRowFormulaCalculator,
    ICumulativeValueCalculator cumulativeValueCalculator,
    IFormTotalsUpdater formTotalsUpdater
) : IFormRowUpdateOrchestrator
{
    public async Task<Result<UpdateFormRowResponse>> UpdateRowAsync(
        int formId,
        short rowOrder,
        UpdateFormRowRequest request,
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
            return new UpdateFormRowResponse
            {
                Rows = form.Rows.ToRowDtos(),
                Totals = form.TotalValues ?? new Dictionary<int, object>()
            };
        }

        await unitOfWork.FormRows.UpdateRowValuesAsync(formId, rowOrder, filteredValues, userId);
        await unitOfWork.SaveChangesAsync();

        form = (await unitOfWork.Forms.FindAsync(formId))!;

        var template = form.TemplateSnapshot;
        var updatedRow = form.Rows.SingleOrDefault(r => r.Order == rowOrder)!;

        var updatedIndicatorIds = filteredValues.Select(v => v.IndicatorId).ToList();
        var formulaValuesToUpdate = formRowFormulaCalculator.CalculateFormulaValues(
            updatedRow,
            template,
            updatedIndicatorIds,
            form.Context);

        if (formulaValuesToUpdate.Count > 0)
        {
            await unitOfWork.FormRows.UpdateRowValuesAsync(formId, rowOrder, formulaValuesToUpdate, userId);
            await unitOfWork.SaveChangesAsync();

            form = (await unitOfWork.Forms.FindAsync(formId))!;
        }

        var cumulativeValuesToUpdate = cumulativeValueCalculator.CalculateCumulativeValues(form, rowOrder);
        if (cumulativeValuesToUpdate.Count > 0)
        {
            await unitOfWork.FormRows.UpdateMultipleRowsValuesAsync(formId, cumulativeValuesToUpdate, userId);
        }

        await formTotalsUpdater.UpdateTotalsIfNeededAsync(form, userId);
        await unitOfWork.SaveChangesAsync();

        form = (await unitOfWork.Forms.FindAsync(formId))!;

        return new UpdateFormRowResponse
        {
            Rows = form.Rows.ToRowDtos(),
            Totals = form.TotalValues ?? new Dictionary<int, object>()
        };
    }
}
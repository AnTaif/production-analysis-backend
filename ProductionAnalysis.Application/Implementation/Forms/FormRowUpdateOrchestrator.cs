using Core.Results;
using ProductionAnalysis.Application.Converters;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Templates;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IFormRowUpdateOrchestrator
{
    Task<Result<ICollection<FormRowDto>>> UpdateRowAsync(
        int formId,
        short rowOrder,
        UpdateFormRowRequest request,
        Guid userId);
}

/// <summary>
///     Оркестратор для координации обновления строки формы
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
    public async Task<Result<ICollection<FormRowDto>>> UpdateRowAsync(
        int formId,
        short rowOrder,
        UpdateFormRowRequest request,
        Guid userId)
    {
        var form = await unitOfWork.Forms.FindAsync(formId);
        if (form is null) return ServiceError.NotFound($"Form {formId} not found");

        var row = form.Rows.SingleOrDefault(r => r.Order == rowOrder);
        if (row is null) return ServiceError.NotFound($"Row {rowOrder} not found in form {formId}");

        var filteredValues = formRowValueFilter.FilterUpdatableValues(
            request.Values,
            form.TemplateSnapshot);

        if (filteredValues.Count == 0)
        {
            // Если нет значений для обновления, возвращаем все строки в текущем состоянии
            return form.Rows.ToRowDtos();
        }

        await unitOfWork.FormRows.UpdateRowValuesAsync(formId, rowOrder, filteredValues, userId);
        await unitOfWork.SaveChangesAsync();

        // Перезагружаем форму, чтобы получить обновленные значения
        form = await unitOfWork.Forms.FindAsync(formId);
        if (form == null) return ServiceError.NotFound($"Form {formId} not found after update");

        await RecalculateFormulasAsync(formId, rowOrder, filteredValues, form.TemplateSnapshot, userId);
        await RecalculateCumulativeValuesAsync(formId, rowOrder, userId);

        await UpdateTotalsAsync(formId, userId);
        return await GetAllRowsAsync(formId);
    }

    private async Task RecalculateFormulasAsync(
        int formId,
        short rowOrder,
        ICollection<FormRowValueData> filteredValues,
        Template template,
        Guid userId)
    {
        // Перезагружаем форму, чтобы получить обновленные значения после SaveChangesAsync
        var form = await unitOfWork.Forms.FindAsync(formId);
        if (form == null) return;

        var updatedRow = form.Rows.SingleOrDefault(r => r.Order == rowOrder);
        if (updatedRow == null) return;

        var updatedIndicatorIds = filteredValues.Select(v => v.IndicatorId).ToList();
        var formulaValuesToUpdate = await formRowFormulaCalculator.CalculateFormulaValuesAsync(
            updatedRow,
            template,
            updatedIndicatorIds,
            form.Context);

        if (formulaValuesToUpdate.Count != 0)
        {
            await unitOfWork.FormRows.UpdateRowValuesAsync(
                formId,
                rowOrder,
                formulaValuesToUpdate,
                userId);
            await unitOfWork.SaveChangesAsync();
        }
    }

    private async Task RecalculateCumulativeValuesAsync(int formId, short rowOrder, Guid userId)
    {
        var form = await unitOfWork.Forms.FindAsync(formId);
        if (form == null) return;

        var cumulativeValuesToUpdate = cumulativeValueCalculator.CalculateCumulativeValues(
            form,
            rowOrder);

        if (cumulativeValuesToUpdate.Count > 0)
        {
            await unitOfWork.FormRows.UpdateMultipleRowsValuesAsync(
                formId,
                cumulativeValuesToUpdate,
                userId);
            await unitOfWork.SaveChangesAsync();
        }
    }

    private async Task UpdateTotalsAsync(int formId, Guid userId)
    {
        var form = await unitOfWork.Forms.FindAsync(formId);
        if (form != null) await formTotalsUpdater.UpdateTotalsIfNeededAsync(form, userId);
    }

    private async Task<Result<ICollection<FormRowDto>>> GetAllRowsAsync(int formId)
    {
        var form = await unitOfWork.Forms.FindAsync(formId);
        if (form == null) return ServiceError.NotFound($"Form {formId} not found after update");

        return form.Rows.ToRowDtos();
    }
}
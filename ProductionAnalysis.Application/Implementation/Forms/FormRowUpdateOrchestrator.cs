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
    public async Task<Result<UpdateFormRowResponse>> UpdateRowAsync(
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
            return new UpdateFormRowResponse
            {
                Rows = form.Rows.ToRowDtos(),
                Totals = form.TotalValues ?? new Dictionary<int, object>()
            };
        }

        // Обновляем значения строки
        await unitOfWork.FormRows.UpdateRowValuesAsync(formId, rowOrder, filteredValues, userId);
        await unitOfWork.SaveChangesAsync();

        // Перезагружаем форму один раз для всех последующих операций
        form = await unitOfWork.Forms.FindAsync(formId);
        if (form == null) return ServiceError.NotFound($"Form {formId} not found after update");

        var template = form.TemplateSnapshot;
        var updatedRow = form.Rows.SingleOrDefault(r => r.Order == rowOrder);
        if (updatedRow == null) return ServiceError.NotFound($"Row {rowOrder} not found after update");

        // Пересчитываем формулы
        var updatedIndicatorIds = filteredValues.Select(v => v.IndicatorId).ToList();
        var formulaValuesToUpdate = await formRowFormulaCalculator.CalculateFormulaValuesAsync(
            updatedRow,
            template,
            updatedIndicatorIds,
            form.Context);

        // Применяем обновления формул
        if (formulaValuesToUpdate.Count > 0)
        {
            await unitOfWork.FormRows.UpdateRowValuesAsync(formId, rowOrder, formulaValuesToUpdate, userId);
            await unitOfWork.SaveChangesAsync();

            // Перезагружаем форму после обновления формул, чтобы накопительные значения рассчитывались на актуальных данных
            form = await unitOfWork.Forms.FindAsync(formId);
            if (form == null) return ServiceError.NotFound($"Form {formId} not found after formula update");
        }

        // Пересчитываем накопительные значения (могут затрагивать несколько строк)
        // Важно: это делается после пересчета формул, чтобы накопительные значения рассчитывались на основе актуальных значений формул
        var cumulativeValuesToUpdate = cumulativeValueCalculator.CalculateCumulativeValues(form, rowOrder);
        if (cumulativeValuesToUpdate.Count > 0)
        {
            await unitOfWork.FormRows.UpdateMultipleRowsValuesAsync(formId, cumulativeValuesToUpdate, userId);
        }

        // Обновляем итоги
        await formTotalsUpdater.UpdateTotalsIfNeededAsync(form, userId);

        // Сохраняем все изменения одним запросом
        await unitOfWork.SaveChangesAsync();

        // Загружаем финальную версию формы для возврата
        form = await unitOfWork.Forms.FindAsync(formId);
        if (form == null) return ServiceError.NotFound($"Form {formId} not found after update");

        return new UpdateFormRowResponse
        {
            Rows = form.Rows.ToRowDtos(),
            Totals = form.TotalValues ?? new Dictionary<int, object>()
        };
    }
}
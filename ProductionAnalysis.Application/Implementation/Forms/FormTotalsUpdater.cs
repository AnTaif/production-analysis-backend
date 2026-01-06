using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Repositories;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IFormTotalsUpdater
{
    Task UpdateTotalsIfNeededAsync(Form form, Guid userId);
}

/// <summary>
///     Сервис для обновления итоговых значений формы
/// </summary>
[RegisterScoped]
public class FormTotalsUpdater(
    IPaUnitOfWork unitOfWork,
    ITotalValueCalculator totalValueCalculator
) : IFormTotalsUpdater
{
    public async Task UpdateTotalsIfNeededAsync(Form form, Guid userId)
    {
        var calculatedTotals = totalValueCalculator.CalculateTotals(form);

        var needsUpdate = form.TotalValues == null
                          || !ValueComparer.AreDictionariesEqual(form.TotalValues, calculatedTotals);

        if (needsUpdate && calculatedTotals.Count > 0)
            await unitOfWork.Forms.UpdateTotalValuesAsync(form.Id, calculatedTotals, userId);
    }
}
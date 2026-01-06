using ProductionAnalysis.Application.Domain.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;

/// <summary>
///     Результат обработки перерыва
/// </summary>
public class BreakProcessingResult
{
    public ICollection<FormRowData> Rows { get; init; } = new List<FormRowData>();
    public int NextBreakIndex { get; init; }
}
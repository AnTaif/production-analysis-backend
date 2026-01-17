using ProductionAnalysis.Application.Domain.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies.Common;

public interface IRowInitializationStrategy
{
    bool CanHandle(PaType paType);
    ICollection<FormRowData> Initialize(RowInitializationContext context);
}
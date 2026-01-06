using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies.Common;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization;

public interface IRowInitializationStrategyFactory
{
    IRowInitializationStrategy GetStrategy(PaType paType);
}

[RegisterScoped]
public class RowInitializationStrategyFactory(IEnumerable<IRowInitializationStrategy> strategies)
    : IRowInitializationStrategyFactory
{
    public IRowInitializationStrategy GetStrategy(PaType paType)
    {
        var strategy = strategies.FirstOrDefault(s => s.CanHandle(paType));

        if (strategy == null)
            throw new InvalidOperationException(
                $"No initialization strategy found for PaType {paType}");

        return strategy;
    }
}
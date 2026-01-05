using ExhaustiveMatching;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Converters;

public static class PaTypeConverter
{
    public static PaTypeDto ToDto(this PaType paType)
    {
        return paType switch
        {
            PaType.SingleProductWithCycleTime => PaTypeDto.SingleProductWithCycleTime,
            PaType.SingleProductWithWorkstationCapacity => PaTypeDto.SingleProductWithWorkstationCapacity,
            PaType.MultipleProductsWithCycleTime => PaTypeDto.MultipleProductsWithCycleTime,
            PaType.LessThanOnePerHour => PaTypeDto.LessThanOnePerHour,
            _ => throw ExhaustiveMatch.Failed(typeof(PaType))
        };
    }

    public static PaType ToDomain(this PaTypeDto paType)
    {
        return paType switch
        {
            PaTypeDto.SingleProductWithCycleTime => PaType.SingleProductWithCycleTime,
            PaTypeDto.SingleProductWithWorkstationCapacity => PaType.SingleProductWithWorkstationCapacity,
            PaTypeDto.MultipleProductsWithCycleTime => PaType.MultipleProductsWithCycleTime,
            PaTypeDto.LessThanOnePerHour => PaType.LessThanOnePerHour,
            _ => throw ExhaustiveMatch.Failed(typeof(PaTypeDto))
        };
    }
}
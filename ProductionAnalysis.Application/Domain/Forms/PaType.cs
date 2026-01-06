namespace ProductionAnalysis.Application.Domain.Forms;

public enum PaType
{
    SingleProductWithCycleTime = 1,
    SingleProductWithWorkstationCapacity = 2,
    MultipleProductsWithCycleTime = 3,
    LessThanOnePerHour = 4,
    LessThanOnePerShift = 5
}
using System.ComponentModel;

namespace ProductionAnalysis.Client.Models.Forms;

public enum PaTypeDto
{
    [Description("Почасовой по времени такта")]
    SingleProductWithCycleTime = 1,

    [Description("Почасовой по мощности рабочего места")]
    SingleProductWithWorkstationCapacity = 2,

    [Description("Почасовой несколько номенклатур")]
    MultipleProductsWithCycleTime = 3,

    [Description("Менее 1 шт. в час")]
    LessThanOnePerHour = 4,

    [Description("Менее 1 шт. в смену")]
    LessThanOnePerShift = 5,
}
using ProductionAnalysis.Client.Models.Forms;
using ProductionAnalysis.Client.Models.Forms.FormsCreation;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Forms;

/// <summary>
/// Пример запроса на создание формы с контекстом продукта
/// </summary>
public class CreateFormRequestWithProductContextExample : IExamplesProvider<CreateFormRequest>
{
    public CreateFormRequest GetExamples()
    {
        return new CreateFormRequest
        {
            PaTypeId = 1,
            ShiftId = 1,
            Context = new Dictionary<string, CreateFormRequestContextBase>
            {
                {
                    "product",
                    new CreateFormRequestProductContext
                    {
                        ProductId = 1,
                        CycleTime = 120, // 2 минуты в секундах
                        WorkstationCapacity = 2,
                        DailyRate = 100
                    }
                }
            }
        };
    }
}

/// <summary>
/// Пример запроса на создание формы с контекстом списка продуктов
/// </summary>
public class CreateFormRequestWithProductListContextExample : IExamplesProvider<CreateFormRequest>
{
    public CreateFormRequest GetExamples()
    {
        return new CreateFormRequest
        {
            PaTypeId = 3,
            ShiftId = 1,
            Context = new Dictionary<string, CreateFormRequestContextBase>
            {
                {
                    "productList",
                    new CreateFormRequestProductListContext
                    {
                        Products = new List<CreateFormRequestProductContext>
                        {
                            new()
                            {
                                ProductId = 1,
                                CycleTime = 120,
                                WorkstationCapacity = 2,
                                DailyRate = 100
                            },
                            new()
                            {
                                ProductId = 2,
                                CycleTime = 180,
                                WorkstationCapacity = 1,
                                DailyRate = 80
                            }
                        }
                    }
                }
            }
        };
    }
}

/// <summary>
/// Пример запроса на создание формы с контекстом операции
/// </summary>
public class CreateFormRequestWithOperationContextExample : IExamplesProvider<CreateFormRequest>
{
    public CreateFormRequest GetExamples()
    {
        return new CreateFormRequest
        {
            PaTypeId = 2,
            ShiftId = 1,
            Context = new Dictionary<string, CreateFormRequestContextBase>
            {
                {
                    "operation",
                    new CreateFormRequestOperationContext()
                }
            }
        };
    }
}
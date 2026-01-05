using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class AuxiliaryOperationDtoExample : IExamplesProvider<AuxiliaryOperationDto>
{
    public AuxiliaryOperationDto GetExamples()
    {
        return new AuxiliaryOperationDto
        {
            Id = 1,
            Name = "Обед 30 мин",
            Duration = TimeSpan.FromMinutes(30)
        };
    }
}

public class EnumerableAuxiliaryOperationDtoExample : IExamplesProvider<IEnumerable<AuxiliaryOperationDto>>
{
    public IEnumerable<AuxiliaryOperationDto> GetExamples()
    {
        return new List<AuxiliaryOperationDto>
        {
            new() { Id = 1, Name = "Обед 30 мин", Duration = TimeSpan.FromMinutes(30) },
            new() { Id = 2, Name = "Перерыв 15 мин", Duration = TimeSpan.FromMinutes(15) },
            new() { Id = 3, Name = "Уборка 15 мин", Duration = TimeSpan.FromMinutes(15) },
            new() { Id = 4, Name = "Переналадка 15 мин", Duration = TimeSpan.FromMinutes(15) }
        };
    }
}
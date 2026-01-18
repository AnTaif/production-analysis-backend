using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class PositionDtoExample : IExamplesProvider<PositionDto>
{
    public PositionDto GetExamples()
    {
        return new PositionDto
        {
            Id = 1,
            Name = "Бригадир",
            Role = "Operator"
        };
    }
}

public class EnumerablePositionDtoExample : IExamplesProvider<IEnumerable<PositionDto>>
{
    public IEnumerable<PositionDto> GetExamples()
    {
        return new List<PositionDto>
        {
            new()
            {
                Id = 1,
                Name = "Бригадир",
                Role = "Operator"
            },
            new()
            {
                Id = 5,
                Name = "Оператор",
                Role = "JustEmployee"
            },
            new()
            {
                Id = 7,
                Name = "Технолог",
                Role = "JustEmployee"
            }
        };
    }
}
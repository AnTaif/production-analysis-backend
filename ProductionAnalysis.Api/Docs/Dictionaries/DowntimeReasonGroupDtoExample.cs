using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class DowntimeReasonGroupDtoExample : IExamplesProvider<DowntimeReasonGroupDto>
{
    public DowntimeReasonGroupDto GetExamples()
    {
        return new DowntimeReasonGroupDto
        {
            Id = 1,
            Name = "Тех.",
            Description = "Технические причины (поломка оборудования / инструмента, нет энергоносителей и тд.)"
        };
    }
}

public class EnumerableDowntimeReasonGroupDtoExample : IExamplesProvider<IEnumerable<DowntimeReasonGroupDto>>
{
    public IEnumerable<DowntimeReasonGroupDto> GetExamples()
    {
        return new List<DowntimeReasonGroupDto>
        {
            new()
            {
                Id = 1, Name = "Орг.",
                Description = "Организационные причины (отсутствие или неопытность работника, опоздание и тд.)"
            },
            new()
            {
                Id = 2, Name = "Тех.",
                Description = "Технические причины (поломка оборудования / инструмента, нет энергоносителей и тд.)"
            },
            new()
            {
                Id = 3, Name = "Лог.",
                Description = "Логистика, нет поставок (заготовок, инструмента, расходных материалов)"
            }
        };
    }
}
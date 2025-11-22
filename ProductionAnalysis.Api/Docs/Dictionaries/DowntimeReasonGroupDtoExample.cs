using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class DowntimeReasonGroupDtoExample : IExamplesProvider<DowntimeReasonGroupDto>
{
    public DowntimeReasonGroupDto GetExamples()
    {
        return new DowntimeReasonGroupDto(
            1,
            "Тех.",
            "Технические причины (поломка оборудования / инструмента, нет энергоносителей и тд.)"
        );
    }
}

public class EnumerableDowntimeReasonGroupDtoExample : IExamplesProvider<IEnumerable<DowntimeReasonGroupDto>>
{
    public IEnumerable<DowntimeReasonGroupDto> GetExamples()
    {
        return new List<DowntimeReasonGroupDto>
        {
            new(1, "Орг.", "Организационные причины (отсутствие или неопытность работника, опоздание и тд.)"),
            new(2, "Тех.", "Технические причины (поломка оборудования / инструмента, нет энергоносителей и тд.)"),
            new(3, "Лог.", "Логистика, нет поставок (заготовок, инструмента, расходных материалов)")
        };
    }
}
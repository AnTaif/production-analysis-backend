using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class ShiftDtoExample : IExamplesProvider<ShiftDto>
{
    public ShiftDto GetExamples()
    {
        return new ShiftDto(
            1,
            "1",
            new TimeOnly(07, 00)
        );
    }
}

public class EnumerableShiftDtoExample : IExamplesProvider<IEnumerable<ShiftDto>>
{
    public IEnumerable<ShiftDto> GetExamples()
    {
        return new List<ShiftDto>
        {
            new(1, "1", new TimeOnly(8, 0)),
            new(2, "2", new TimeOnly(16, 0)),
            new(3, "3 (ночная)", new TimeOnly(0, 0))
        };
    }
}
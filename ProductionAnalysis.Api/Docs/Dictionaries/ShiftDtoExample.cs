using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class ShiftDtoExample : IExamplesProvider<ShiftDto>
{
    public ShiftDto GetExamples()
    {
        return new ShiftDto
        {
            Id = 1,
            Name = "1",
            StartTime = new TimeOnly(07, 00)
        };
    }
}

public class EnumerableShiftDtoExample : IExamplesProvider<IEnumerable<ShiftDto>>
{
    public IEnumerable<ShiftDto> GetExamples()
    {
        return new List<ShiftDto>
        {
            new() { Id = 1, Name = "1", StartTime = new TimeOnly(8, 0) },
            new() { Id = 2, Name = "2", StartTime = new TimeOnly(16, 0) },
            new() { Id = 3, Name = "3 (ночная)", StartTime = new TimeOnly(0, 0) }
        };
    }
}
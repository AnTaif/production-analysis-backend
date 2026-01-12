using Core.Results;
using ProductionAnalysis.Client.Models.Dictionaries;
using ProductionAnalysis.Client.Models.Forms;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Forms;

public class FormShortDtoExample : IExamplesProvider<FormShortDto>
{
    public FormShortDto GetExamples()
    {
        return new FormShortDto
        {
            Id = 4,
            PaType = PaTypeDto.SingleProductWithCycleTime,
            Status = FormStatus.InProgress,
            CreationDate = DateTime.UtcNow.AddDays(-1),
            UpdateDate = DateTime.UtcNow,
            DepartmentId = 2,
            Creator = new EmployeeDto
            {
                Id = 15,
                FullName = "Иванов Иван Иванович",
                Position = "Бригадир",
                DepartmentId = 2,
                UserId = Guid.Parse("12345678-1234-1234-1234-123456789012")
            },
            Assignee = new EmployeeDto
            {
                Id = 20,
                FullName = "Петров Пётр Петрович",
                Position = "Оператор",
                DepartmentId = 2,
                UserId = null
            },
            ProductNames = "Корпус редуктора",
            Shift = new ShiftDto
            {
                Id = 1,
                Name = "1",
                StartTime = new TimeOnly(8, 0)
            }
        };
    }
}

public class PaginatedFormShortDtoExample : IExamplesProvider<PaginatedResponse<FormShortDto>>
{
    public PaginatedResponse<FormShortDto> GetExamples()
    {
        return new PaginatedResponse<FormShortDto>(
            [
                new FormShortDto
                {
                    Id = 4,
                    PaType = PaTypeDto.SingleProductWithCycleTime,
                    Status = FormStatus.InProgress,
                    CreationDate = DateTime.UtcNow.AddDays(-1),
                    UpdateDate = DateTime.UtcNow,
                    DepartmentId = 2,
                    Creator = new EmployeeDto
                    {
                        Id = 15,
                        FullName = "Иванов Иван Иванович",
                        Position = "Бригадир",
                        DepartmentId = 2,
                        UserId = Guid.Parse("12345678-1234-1234-1234-123456789012")
                    },
                    Assignee = new EmployeeDto
                    {
                        Id = 20,
                        FullName = "Петров Пётр Петрович",
                        Position = "Оператор",
                        DepartmentId = 2,
                        UserId = null
                    },
                    ProductNames = "Корпус редуктора",
                    Shift = new ShiftDto
                    {
                        Id = 1,
                        Name = "1",
                        StartTime = new TimeOnly(8, 0)
                    }
                },

                new FormShortDto
                {
                    Id = 5,
                    PaType = PaTypeDto.LessThanOnePerShift,
                    Status = FormStatus.Completed,
                    CreationDate = DateTime.UtcNow.AddDays(-2),
                    UpdateDate = DateTime.UtcNow.AddDays(-1),
                    DepartmentId = 2,
                    Creator = new EmployeeDto
                    {
                        Id = 15,
                        FullName = "Иванов Иван Иванович",
                        Position = "Бригадир",
                        DepartmentId = 2,
                        UserId = Guid.Parse("12345678-1234-1234-1234-123456789012")
                    },
                    Assignee = new EmployeeDto
                    {
                        Id = 21,
                        FullName = "Сидоров Алексей Алексеевич",
                        Position = "Старший оператор",
                        DepartmentId = 2,
                        UserId = Guid.Parse("87654321-4321-4321-4321-210987654321")
                    },
                    ProductNames = "Установка прибора",
                    Shift = new ShiftDto
                    {
                        Id = 2,
                        Name = "2",
                        StartTime = new TimeOnly(16, 0)
                    }
                }
            ],
            2,
            1,
            10
        );
    }
}
using ProductionAnalysis.Client.Models.Dictionaries;
using ProductionAnalysis.Client.Models.Forms;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Forms;

public class FormDtoExample : IExamplesProvider<FormDto>
{
    public FormDto GetExamples()
    {
        return new FormDto
        {
            Id = 4,
            PaType = PaTypeDto.SingleProductWithCycleTime,
            Status = FormStatus.InProgress,
            CreationDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow,
            FormDate = DateTime.UtcNow.Date,
            Context = new FormContextDto
            {
                Product = new ProductContextDto
                {
                    ProductId = 1,
                    CycleTime = 60,
                    WorkstationCapacity = null,
                    DailyRate = 400,
                    ProductName = "Корпус редуктора"
                },
                OperationOrProduct = null
            },
            Rows = new List<FormRowDto>
            {
                new()
                {
                    Order = 1,
                    IsAuxiliaryOperation = false,
                    Values = new Dictionary<string, FormRowValueDto>
                    {
                        {
                            "1", new FormRowValueDto
                            {
                                Value = "60"
                            }
                        },
                        {
                            "16", new FormRowValueDto
                            {
                                Value = "07:00-08:00"
                            }
                        }
                    }
                },
                new()
                {
                    Order = 2,
                    IsAuxiliaryOperation = false,
                    Values = new Dictionary<string, FormRowValueDto>
                    {
                        {
                            "1", new FormRowValueDto
                            {
                                Value = "60"
                            }
                        },
                        {
                            "16", new FormRowValueDto
                            {
                                Value = "08:00-09:00"
                            }
                        }
                    }
                },
                new()
                {
                    Order = 3,
                    IsAuxiliaryOperation = true,
                    Values = new Dictionary<string, FormRowValueDto>
                    {
                        {
                            "1", new FormRowValueDto
                            {
                                Value = "60"
                            }
                        },
                        {
                            "16", new FormRowValueDto
                            {
                                Value = "09:00-09:15 Перерыв 15 мин"
                            }
                        }
                    }
                },
                new()
                {
                    Order = 4,
                    IsAuxiliaryOperation = false,
                    Values = new Dictionary<string, FormRowValueDto>
                    {
                        {
                            "1", new FormRowValueDto
                            {
                                Value = "45"
                            }
                        },
                        {
                            "16", new FormRowValueDto
                            {
                                Value = "09:15-10:00"
                            }
                        }
                    }
                }
            },
            Template = new FormTemplateDto
            {
                TableColumns = new List<FormFieldDto>
                {
                    new()
                    {
                        Id = 1,
                        Name = "План, шт.",
                        InputType = "initialization",
                        InputSelector = "",
                        ValueType = "number"
                    },
                    new()
                    {
                        Id = 2,
                        Name = "Факт, шт.",
                        InputType = "manual",
                        InputSelector = "",
                        ValueType = "number"
                    },
                    new()
                    {
                        Id = 3,
                        Name = "Отклонен, шт.",
                        InputType = "formula",
                        InputSelector = "",
                        ValueType = "number"
                    },
                    new()
                    {
                        Id = 4,
                        Name = "Простой, мин.",
                        InputType = "manual",
                        InputSelector = null,
                        ValueType = "number"
                    },
                    new()
                    {
                        Id = 5,
                        Name = "Ответственный за простой",
                        InputType = "dictionary",
                        InputSelector = "employees",
                        ValueType = "text"
                    },
                    new()
                    {
                        Id = 6,
                        Name = "Причина отклонения/комментарий",
                        InputType = "manual",
                        InputSelector = null,
                        ValueType = "text"
                    },
                    new()
                    {
                        Id = 7,
                        Name = "Группы причин",
                        InputType = "dictionary",
                        InputSelector = "downtime-reason-groups",
                        ValueType = "text"
                    },
                    new()
                    {
                        Id = 8,
                        Name = "Принятые меры",
                        InputType = "manual",
                        InputSelector = null,
                        ValueType = "text"
                    },
                    new()
                    {
                        Id = 16,
                        Name = "Время работы, час.",
                        InputType = "initialization",
                        InputSelector = null,
                        ValueType = "text"
                    }
                }
            },
            Shift = new ShiftDto
            {
                Id = 1,
                Name = "1",
                StartTime = new TimeOnly(8, 0)
            },
            Department = new DepartmentDto
            {
                Id = 1,
                Name = "Цех №1",
                EnterpriseId = 1
            }
        };
    }
}
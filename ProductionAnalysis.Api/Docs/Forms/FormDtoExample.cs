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
            PaTypeId = 1,
            Status = FormStatus.InProgress,
            CreationDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow,
            Context = new FormContextDto
            {
                Product = new ProductContextDto
                {
                    ProductId = 1,
                    CycleTime = 60,
                    WorkstationCapacity = null,
                    DailyRate = 400
                },
                Operation = null
            },
            Rows = new List<FormRowDto>
            {
                new()
                {
                    Order = 1,
                    IsAdditionalOperation = false,
                    Values = new Dictionary<string, FormRowValueDto>
                    {
                        {
                            "1", new FormRowValueDto
                            {
                                Value = "60",
                                CumulativeValue = "60"
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
                    IsAdditionalOperation = false,
                    Values = new Dictionary<string, FormRowValueDto>
                    {
                        {
                            "1", new FormRowValueDto
                            {
                                Value = "60",
                                CumulativeValue = "120"
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
                    IsAdditionalOperation = true,
                    Values = new Dictionary<string, FormRowValueDto>
                    {
                        {
                            "1", new FormRowValueDto
                            {
                                Value = "60",
                                CumulativeValue = "180"
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
                    IsAdditionalOperation = false,
                    Values = new Dictionary<string, FormRowValueDto>
                    {
                        {
                            "1", new FormRowValueDto
                            {
                                Value = "45",
                                CumulativeValue = "225"
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
                        Name = "Отклонение, шт.",
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
            }
        };
    }
}
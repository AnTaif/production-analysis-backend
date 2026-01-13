using ProductionAnalysis.Client.Models.Forms;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Forms;

public class UpdateFormRowResponseExample : IExamplesProvider<UpdateFormRowResponse>
{
    public UpdateFormRowResponse GetExamples()
    {
        return new UpdateFormRowResponse
        {
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
                            "2", new FormRowValueDto
                            {
                                Value = "55"
                            }
                        },
                        {
                            "3", new FormRowValueDto
                            {
                                Value = "5"
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
                            "2", new FormRowValueDto
                            {
                                Value = "62"
                            }
                        },
                        {
                            "3", new FormRowValueDto
                            {
                                Value = "-2"
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
                            "2", new FormRowValueDto
                            {
                                Value = "48"
                            }
                        },
                        {
                            "3", new FormRowValueDto
                            {
                                Value = "-3"
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
            Totals = new Dictionary<int, object>
            {
                { 1, 225 },
                { 2, 165 },
                { 3, 0 }
            }
        };
    }
}
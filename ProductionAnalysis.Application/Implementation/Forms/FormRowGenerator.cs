using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Templates;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms;

public class FormRowGenerator
{
    private const int ShiftDurationHours = 8;
    private const int ShiftDurationMinutes = 40; // 8 часов 40 минут для смены 1

    public static async Task<ICollection<FormRowData>> GenerateRowsForShiftAsync(
        TimeOnly shiftStartTime,
        ICollection<ShiftScheduleDto> schedules,
        Template template,
        IPaUnitOfWork unitOfWork)
    {
        var rows = new List<FormRowData>();
        short order = 1;

        // Получаем индикаторы из шаблона
        // Ищем индикатор для времени работы - может быть "Время работы, час" или подобный
        var workTimeIndicator = template.Indicators.FirstOrDefault(i =>
            i.Name.Contains("Время работы", StringComparison.OrdinalIgnoreCase) ||
            i.Name.Contains("workTime", StringComparison.OrdinalIgnoreCase) ||
            i.Name.Contains("Время", StringComparison.OrdinalIgnoreCase));

        // Ищем индикатор для наименования операции
        var operationNameIndicator = template.Indicators.FirstOrDefault(i =>
            i.Name.Contains("Наименование", StringComparison.OrdinalIgnoreCase) ||
            i.Name.Contains("операции", StringComparison.OrdinalIgnoreCase));

        if (workTimeIndicator == null)
        {
            // Если не нашли специальный индикатор, используем первый доступный текстовый индикатор
            workTimeIndicator = template.Indicators.FirstOrDefault(i =>
                                    i.ValueType.Equals("Text", StringComparison.OrdinalIgnoreCase))
                                ?? template.Indicators.First();

            if (workTimeIndicator == null)
            {
                throw new InvalidOperationException("Template must contain at least one indicator");
            }
        }

        // Получаем все дополнительные операции для быстрого доступа
        var additionalOperations = await unitOfWork.Dictionaries.SelectAdditionalOperationsAsync();
        var additionalOpsDict = additionalOperations.ToDictionary(ao => ao.Id);

        // Сортируем расписание по времени начала
        var sortedSchedules = schedules.OrderBy(s => s.StartTime).ToList();

        // Генерируем строки времени работы
        var shiftEndTime = shiftStartTime.AddHours(ShiftDurationHours).AddMinutes(ShiftDurationMinutes);
        var currentTime = shiftStartTime;
        var scheduleIndex = 0;

        while (currentTime < shiftEndTime)
        {
            // Определяем конец текущего часа
            var hourEnd = currentTime.AddHours(1);
            if (hourEnd > shiftEndTime)
            {
                hourEnd = shiftEndTime;
            }

            // Проверяем, есть ли обед/перерыв в текущем интервале
            var nextSchedule = scheduleIndex < sortedSchedules.Count
                ? sortedSchedules[scheduleIndex]
                : null;

            if (nextSchedule != null && currentTime <= nextSchedule.StartTime && nextSchedule.StartTime < hourEnd)
            {
                // Есть обед/перерыв в этом часе
                // Создаем строку работы до перерыва
                if (currentTime < nextSchedule.StartTime)
                {
                    var workRowBeforeBreak = new FormRowData
                    {
                        Order = order++,
                        IsAdditionalOperation = false,
                        Values = new List<FormRowValueData>
                        {
                            new FormRowValueData
                            {
                                IndicatorId = workTimeIndicator.Id,
                                Value = $"{currentTime:HH:mm}-{nextSchedule.StartTime:HH:mm}"
                            }
                        }
                    };
                    rows.Add(workRowBeforeBreak);
                }

                // Создаем строку для обед/перерыв
                var additionalOp = additionalOpsDict.GetValueOrDefault(nextSchedule.AdditionalOperationId);
                if (additionalOp != null)
                {
                    var breakEndTime = nextSchedule.StartTime.Add(additionalOp.Duration);
                    var breakRowValues = new List<FormRowValueData>
                    {
                        new FormRowValueData
                        {
                            IndicatorId = workTimeIndicator.Id,
                            Value = $"{nextSchedule.StartTime:HH:mm}-{breakEndTime:HH:mm}"
                        }
                    };

                    if (operationNameIndicator != null)
                    {
                        breakRowValues.Add(new FormRowValueData
                        {
                            IndicatorId = operationNameIndicator.Id,
                            Value = additionalOp.Name
                        });
                    }

                    var breakRow = new FormRowData
                    {
                        Order = order++,
                        IsAdditionalOperation = true,
                        AdditionalOperationId = nextSchedule.AdditionalOperationId,
                        Values = breakRowValues
                    };
                    rows.Add(breakRow);

                    // Продолжаем с времени окончания перерыва
                    currentTime = breakEndTime;
                }
                else
                {
                    currentTime = nextSchedule.StartTime;
                }

                scheduleIndex++;

                // Если после перерыва осталось время в этом часе, создаем строку работы
                if (currentTime < hourEnd)
                {
                    var workRowAfterBreak = new FormRowData
                    {
                        Order = order++,
                        IsAdditionalOperation = false,
                        Values = new List<FormRowValueData>
                        {
                            new FormRowValueData
                            {
                                IndicatorId = workTimeIndicator.Id,
                                Value = $"{currentTime:HH:mm}-{hourEnd:HH:mm}"
                            }
                        }
                    };
                    rows.Add(workRowAfterBreak);
                    currentTime = hourEnd;
                }
            }
            else
            {
                // Обычный час работы без перерывов
                var workRow = new FormRowData
                {
                    Order = order++,
                    IsAdditionalOperation = false,
                    Values = new List<FormRowValueData>
                    {
                        new FormRowValueData
                        {
                            IndicatorId = workTimeIndicator.Id,
                            Value = $"{currentTime:HH:mm}-{hourEnd:HH:mm}"
                        }
                    }
                };
                rows.Add(workRow);
                currentTime = hourEnd;
            }
        }

        return rows;
    }
}
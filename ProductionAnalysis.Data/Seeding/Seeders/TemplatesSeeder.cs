using Microsoft.EntityFrameworkCore;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Seeding.Seeders;

public class TemplatesSeeder(PaDbContext dbContext)
{
    public async Task SeedAsync()
    {
        if (await dbContext.Templates.AnyAsync())
            return;

        var indicators = await dbContext.Indicators.ToDictionaryAsync(i => i.Id);

        // Базовые индикаторы
        var worktime = indicators[1];
        var plan = indicators[2];
        var fact = indicators[3];
        var deviation = indicators[4];
        var downtime = indicators[5];
        var downtimeResponsible = indicators[6];
        var downTimeReasonsGroup = indicators[7];
        var downtimeReasonAndActionsTaken = indicators[8];
        var operationName = indicators[10];
        var operationTime = indicators[11];
        var startTimePlan = indicators[12];
        var startTimeFact = indicators[13];
        var endTimePlan = indicators[14];
        var endTimeFact = indicators[15];
        var planMinutes = indicators[16];
        var factMinutes = indicators[17];
        var deviationMinutes = indicators[18];

        // Накопительные индикаторы
        var planCumulative = indicators[19];
        var factCumulative = indicators[20];
        var deviationCumulative = indicators[21];
        var deviationMinutesCumulative = indicators[22];

        var template1 = CreateTemplate(
            1,
            "По времени такта",
            PaType.SingleProductWithCycleTime
        );

        AddIndicatorsToTemplate(template1,
            [
                worktime, plan, planCumulative, fact, factCumulative, deviation, deviationCumulative,
                downtime, downtimeResponsible, downTimeReasonsGroup, downtimeReasonAndActionsTaken
            ]
        );

        var template2 = CreateTemplate(
            2,
            "По мощности рабочего места",
            PaType.SingleProductWithWorkstationCapacity
        );

        AddIndicatorsToTemplate(template2,
            [
                worktime, plan, planCumulative, fact, factCumulative, deviation, deviationCumulative,
                downtime, downtimeResponsible, downTimeReasonsGroup, downtimeReasonAndActionsTaken
            ]
        );

        var template3 = CreateTemplate(
            3,
            "Несколько номенклатур",
            PaType.MultipleProductsWithCycleTime
        );

        AddIndicatorsToTemplate(template3,
            [
                worktime, plan, planCumulative, fact, factCumulative, deviation, deviationCumulative,
                downtime, downtimeResponsible, downTimeReasonsGroup, downtimeReasonAndActionsTaken
            ]
        );

        var template4 = CreateTemplate(
            4,
            "Менее 1 изделия в час",
            PaType.LessThanOnePerHour
        );

        AddIndicatorsToTemplate(template4,
            [
                worktime, operationName, operationTime, plan, planCumulative, fact, factCumulative,
                deviation, deviationCumulative, downtime, downtimeResponsible,
                downTimeReasonsGroup, downtimeReasonAndActionsTaken
            ]
        );

        var template5 = CreateTemplate(
            5,
            "Менее 1 изделия в смену",
            PaType.LessThanOnePerShift
        );

        AddIndicatorsToTemplate(template5,
            [
                operationName, startTimePlan, startTimeFact, endTimePlan, endTimeFact,
                deviationMinutes, deviationMinutesCumulative, downtimeResponsible,
                downTimeReasonsGroup, downtimeReasonAndActionsTaken
            ]
        );

        dbContext.Templates.AddRange(template1, template2, template3, template4, template5);
        await dbContext.SaveChangesAsync();
    }

    private static TemplateDbo CreateTemplate(int id, string name, PaType paType, int version = 0)
    {
        return new TemplateDbo
        {
            Id = id,
            Name = name,
            PaTypeId = (int)paType,
            Version = version
        };
    }

    private static void AddIndicatorsToTemplate(
        TemplateDbo template,
        IndicatorDbo[] indicators)
    {
        for (short order = 0; order < indicators.Length; order++)
        {
            var indicator = indicators[order];
            template.TemplateIndicators.Add(new TemplateIndicatorDbo
            {
                TemplateId = template.Id,
                IndicatorId = indicator.Id,
                Indicator = indicator,
                Order = order
            });
        }
    }
}
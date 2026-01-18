using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductionAnalysis.Application.Implementation.Forms;
using ProductionAnalysis.Client.Models.Forms;
using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Models;

namespace ProductionAnalysis.Data.Seeding.Seeders;

public class FormsSeeder(
    PaDbContext dbContext,
    UserManager<UserDbo> userManager,
    IFormsService formsService,
    ILogger logger)
{
    public async Task SeedAsync()
    {
        if (await dbContext.Forms.AnyAsync())
            return;

        var departmentHeadUser = await userManager.FindByEmailAsync("departmentHead@mail.ru");
        if (departmentHeadUser == null)
        {
            logger.LogWarning("DepartmentHead user not found, skipping forms seeding");
            return;
        }

        // Находим сотрудника-оператора по email
        var operatorEmployee = await dbContext.Employees
            .FirstOrDefaultAsync(e => e.Email == "operator@mail.ru");

        if (operatorEmployee == null)
        {
            logger.LogWarning("Operator employee not found, skipping forms seeding");
            return;
        }

        var operatorUserId = operatorEmployee.Id;

        var today = DateTime.UtcNow.Date;

        // PA Type 1: SingleProductWithCycleTime
        var form1Request = new CreateFormRequest
        {
            PaType = PaTypeDto.SingleProductWithCycleTime,
            ShiftId = 1,
            AssigneeId = operatorUserId,
            FormDate = today,
            Product = new ProductContextRequest
            {
                ProductId = 1, // Втулка
                CycleTime = 60,
                DailyRate = 400
            }
        };

        await formsService.CreateAsync(form1Request, departmentHeadUser.Id);

        // PA Type 2: SingleProductWithWorkstationCapacity
        var form2Request = new CreateFormRequest
        {
            PaType = PaTypeDto.SingleProductWithWorkstationCapacity,
            ShiftId = 1,
            AssigneeId = operatorUserId,
            FormDate = today.AddDays(-1),
            Product = new ProductContextRequest
            {
                ProductId = 2, // Шайба
                WorkstationCapacity = 120,
                DailyRate = 960
            }
        };

        await formsService.CreateAsync(form2Request, departmentHeadUser.Id);

        // PA Type 3: MultipleProductsWithCycleTime
        var form3Request = new CreateFormRequest
        {
            PaType = PaTypeDto.MultipleProductsWithCycleTime,
            ShiftId = 1,
            AssigneeId = operatorUserId,
            FormDate = today.AddDays(-2),
            Products = new List<ProductContextRequest>
            {
                new()
                {
                    ProductId = 1, // Втулка
                    CycleTime = 72,
                    DailyRate = 200
                },
                new()
                {
                    ProductId = 3, // Подшипник
                    CycleTime = 60,
                    DailyRate = 500
                }
            }
        };

        await formsService.CreateAsync(form3Request, departmentHeadUser.Id);

        // PA Type 4: LessThanOnePerHour (с операцией)
        var form4Request = new CreateFormRequest
        {
            PaType = PaTypeDto.LessThanOnePerHour,
            ShiftId = 1,
            AssigneeId = operatorUserId,
            FormDate = today.AddDays(-3),
            OperationOrProduct = new OperationOrProductContextRequest
            {
                OperationId = 7 // Установка прибора
            }
        };

        await formsService.CreateAsync(form4Request, departmentHeadUser.Id);

        // PA Type 4: LessThanOnePerHour (с продуктом)
        var form4ProductRequest = new CreateFormRequest
        {
            PaType = PaTypeDto.LessThanOnePerHour,
            ShiftId = 1,
            AssigneeId = operatorUserId,
            FormDate = today.AddDays(-4),
            OperationOrProduct = new OperationOrProductContextRequest
            {
                OperationId = 7 // Втулка
            }
        };

        await formsService.CreateAsync(form4ProductRequest, departmentHeadUser.Id);

        // PA Type 5: LessThanOnePerShift (с операцией)
        var form5Request = new CreateFormRequest
        {
            PaType = PaTypeDto.LessThanOnePerShift,
            ShiftId = 1,
            AssigneeId = operatorUserId,
            FormDate = today.AddDays(-5),
            OperationOrProduct = new OperationOrProductContextRequest
            {
                OperationId = 21
            }
        };

        await formsService.CreateAsync(form5Request, departmentHeadUser.Id);

        // PA Type 5: LessThanOnePerShift (с продуктом)
        var form5ProductRequest = new CreateFormRequest
        {
            PaType = PaTypeDto.LessThanOnePerShift,
            ShiftId = 1,
            AssigneeId = operatorUserId,
            FormDate = today.AddDays(-6),
            OperationOrProduct = new OperationOrProductContextRequest
            {
                ProductId = 1 // Втулка
            }
        };

        await formsService.CreateAsync(form5ProductRequest, departmentHeadUser.Id);

        // Создаем дополнительные формы в завершенном статусе
        await SeedCompletedFormsAsync(departmentHeadUser.Id, operatorUserId, today);

        logger.LogInformation("Seeded test forms for all PA types");
    }

    private async Task SeedCompletedFormsAsync(Guid creatorId, int assigneeId, DateTime today)
    {
        // PA Type 1: SingleProductWithCycleTime (завершенная)
        var completedForm1Request = new CreateFormRequest
        {
            PaType = PaTypeDto.SingleProductWithCycleTime,
            ShiftId = 1,
            AssigneeId = assigneeId,
            FormDate = today.AddDays(-7),
            Product = new ProductContextRequest
            {
                ProductId = 3, // Подшипник
                CycleTime = 45,
                DailyRate = 600
            }
        };

        var completedForm1Result = await formsService.CreateAsync(completedForm1Request, creatorId);
        if (completedForm1Result.IsSuccess)
        {
            await formsService.CompleteFormAsync(completedForm1Result.Value.Id, creatorId);
        }

        // PA Type 2: SingleProductWithWorkstationCapacity (завершенная)
        var completedForm2Request = new CreateFormRequest
        {
            PaType = PaTypeDto.SingleProductWithWorkstationCapacity,
            ShiftId = 1,
            AssigneeId = assigneeId,
            FormDate = today.AddDays(-8),
            Product = new ProductContextRequest
            {
                ProductId = 4, // Фланец
                WorkstationCapacity = 100,
                DailyRate = 800
            }
        };

        var completedForm2Result = await formsService.CreateAsync(completedForm2Request, creatorId);
        if (completedForm2Result.IsSuccess)
        {
            await formsService.CompleteFormAsync(completedForm2Result.Value.Id, creatorId);
        }

        // PA Type 3: MultipleProductsWithCycleTime (завершенная)
        var completedForm3Request = new CreateFormRequest
        {
            PaType = PaTypeDto.MultipleProductsWithCycleTime,
            ShiftId = 1,
            AssigneeId = assigneeId,
            FormDate = today.AddDays(-9),
            Products = new List<ProductContextRequest>
            {
                new()
                {
                    ProductId = 2, // Шайба
                    CycleTime = 30,
                    DailyRate = 800
                },
                new()
                {
                    ProductId = 4, // Фланец
                    CycleTime = 50,
                    DailyRate = 500
                }
            }
        };

        var completedForm3Result = await formsService.CreateAsync(completedForm3Request, creatorId);
        if (completedForm3Result.IsSuccess)
        {
            await formsService.CompleteFormAsync(completedForm3Result.Value.Id, creatorId);
        }

        // PA Type 4: LessThanOnePerHour (завершенная, с операцией)
        var completedForm4Request = new CreateFormRequest
        {
            PaType = PaTypeDto.LessThanOnePerHour,
            ShiftId = 1,
            AssigneeId = assigneeId,
            FormDate = today.AddDays(-10),
            OperationOrProduct = new OperationOrProductContextRequest
            {
                OperationId = 7
            }
        };

        var completedForm4Result = await formsService.CreateAsync(completedForm4Request, creatorId);
        if (completedForm4Result.IsSuccess)
        {
            await formsService.CompleteFormAsync(completedForm4Result.Value.Id, creatorId);
        }

        // PA Type 5: LessThanOnePerShift (завершенная, с операцией)
        var completedForm5Request = new CreateFormRequest
        {
            PaType = PaTypeDto.LessThanOnePerShift,
            ShiftId = 1,
            AssigneeId = assigneeId,
            FormDate = today.AddDays(-11),
            OperationOrProduct = new OperationOrProductContextRequest
            {
                OperationId = 21
            }
        };

        var completedForm5Result = await formsService.CreateAsync(completedForm5Request, creatorId);
        if (completedForm5Result.IsSuccess)
        {
            await formsService.CompleteFormAsync(completedForm5Result.Value.Id, creatorId);
        }

        logger.LogInformation("Seeded completed test forms");
    }
}
using System.Text.Json;
using FluentAssertions;
using Moq;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Templates;
using ProductionAnalysis.Application.Implementation.Forms;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Dictionaries;
using Xunit;

namespace ProductionAnalysis.Application.Tests.Forms;

public class FormRowInitializerTests
{
    private readonly Mock<IDictionariesRepository> dictionariesRepositoryMock;
    private readonly Mock<IProductContextExtractor> productContextExtractorMock;
    private readonly Mock<IFormRowDataFactory> formRowDataFactoryMock;
    private readonly FormRowInitializer initializer;

    public FormRowInitializerTests()
    {
        var unitOfWorkMock = new Mock<IPaUnitOfWork>();
        dictionariesRepositoryMock = new Mock<IDictionariesRepository>();
        productContextExtractorMock = new Mock<IProductContextExtractor>();
        formRowDataFactoryMock = new Mock<IFormRowDataFactory>();

        unitOfWorkMock.Setup(u => u.Dictionaries).Returns(dictionariesRepositoryMock.Object);

        initializer = new FormRowInitializer(
            unitOfWorkMock.Object,
            productContextExtractorMock.Object,
            formRowDataFactoryMock.Object);
    }

    [Fact]
    public async Task InitializeRowsForShiftAsync_WithoutBreaks_ShouldCreateWorkRowsForEachHour()
    {
        // Arrange
        var shiftStartTime = new TimeOnly(7, 0);
        var schedules = new List<ShiftScheduleDto>();
        var template = CreateTemplate();
        var productContext = new ProductContext { DailyRate = 100, CycleTime = 60 };

        dictionariesRepositoryMock
            .Setup(r => r.SelectAdditionalOperationsAsync())
            .ReturnsAsync(new List<AdditionalOperationDto>());

        productContextExtractorMock
            .Setup(e => e.Extract(It.IsAny<Dictionary<string, object>?>()))
            .Returns(productContext);

        formRowDataFactoryMock
            .Setup(f => f.CreateWorkRow(
                It.IsAny<short>(),
                It.IsAny<Indicator>(),
                It.IsAny<Indicator?>(),
                It.IsAny<TimeOnly>(),
                It.IsAny<TimeOnly>(),
                It.IsAny<ProductContext?>()))
            .Returns<short, Indicator, Indicator?, TimeOnly, TimeOnly, ProductContext?>((order, _, _, start, end, _) =>
                CreateWorkRow(order, start, end));

        // Act
        var result = await initializer.InitializeRowsForShiftAsync(
            shiftStartTime,
            schedules,
            template);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().BeGreaterThan(0);
        result.All(r => !r.IsAdditionalOperation).Should().BeTrue();
        result.Should().OnlyContain(r => r.Order > 0);

        result.Count.Should().Be(9);
    }

    [Fact]
    public async Task InitializeRowsForShiftAsync_WithBreak_ShouldCreateWorkRowBreakRowAndWorkRow()
    {
        // Arrange
        var shiftStartTime = new TimeOnly(7, 0);
        var breakStartTime = new TimeOnly(9, 0);
        var breakDuration = TimeSpan.FromMinutes(15);

        var schedules = new List<ShiftScheduleDto>
        {
            new()
            {
                Id = 1,
                ShiftId = 1,
                AdditionalOperationId = 1,
                StartTime = breakStartTime
            }
        };

        var additionalOperations = new List<AdditionalOperationDto>
        {
            new()
            {
                Id = 1,
                Name = "Перерыв 15 мин",
                Duration = breakDuration
            }
        };

        var template = CreateTemplate();
        var productContext = new ProductContext { DailyRate = 100, CycleTime = 60 };

        dictionariesRepositoryMock
            .Setup(r => r.SelectAdditionalOperationsAsync())
            .ReturnsAsync(additionalOperations);

        productContextExtractorMock
            .Setup(e => e.Extract(It.IsAny<Dictionary<string, object>?>()))
            .Returns(productContext);

        var workRowCallCount = 0;
        var breakRowCallCount = 0;

        formRowDataFactoryMock
            .Setup(f => f.CreateWorkRow(
                It.IsAny<short>(),
                It.IsAny<Indicator>(),
                It.IsAny<Indicator?>(),
                It.IsAny<TimeOnly>(),
                It.IsAny<TimeOnly>(),
                It.IsAny<ProductContext?>()))
            .Returns<short, Indicator, Indicator?, TimeOnly, TimeOnly, ProductContext?>((order, _, _, start, end, _) =>
            {
                workRowCallCount++;
                return CreateWorkRow(order, start, end);
            });

        formRowDataFactoryMock
            .Setup(f => f.CreateBreakRow(
                It.IsAny<short>(),
                It.IsAny<Indicator>(),
                It.IsAny<TimeOnly>(),
                It.IsAny<TimeOnly>(),
                It.IsAny<string>(),
                It.IsAny<int>()))
            .Returns<short, Indicator, TimeOnly, TimeOnly, string, int>((order, _, start, end, name, _) =>
            {
                breakRowCallCount++;
                return CreateBreakRow(order, start, end, name);
            });

        // Act
        var result = await initializer.InitializeRowsForShiftAsync(
            shiftStartTime,
            schedules,
            template);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().BeGreaterThan(0);

        // Должна быть создана хотя бы одна строка перерыва
        result.Any(r => r.IsAdditionalOperation).Should().BeTrue();
        breakRowCallCount.Should().BeGreaterThan(0);

        // Должны быть созданы рабочие строки до и после перерыва
        workRowCallCount.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task InitializeRowsForShiftAsync_WithProductContext_ShouldCalculatePlan()
    {
        // Arrange
        var shiftStartTime = new TimeOnly(7, 0);
        var schedules = new List<ShiftScheduleDto>();
        var template = CreateTemplate();
        var productContext = new ProductContext { DailyRate = 100, CycleTime = 60 };

        dictionariesRepositoryMock
            .Setup(r => r.SelectAdditionalOperationsAsync())
            .ReturnsAsync(new List<AdditionalOperationDto>());

        productContextExtractorMock
            .Setup(e => e.Extract(It.IsAny<Dictionary<string, object>?>()))
            .Returns(productContext);

        formRowDataFactoryMock
            .Setup(f => f.CreateWorkRow(
                It.IsAny<short>(),
                It.IsAny<Indicator>(),
                It.IsAny<Indicator?>(),
                It.IsAny<TimeOnly>(),
                It.IsAny<TimeOnly>(),
                It.IsAny<ProductContext?>()))
            .Returns<short, Indicator, Indicator?, TimeOnly, TimeOnly, ProductContext?>((order, _, planIndicator, start,
                end, ctx) =>
            {
                var row = CreateWorkRow(order, start, end);

                // Проверяем, что план был рассчитан
                if (planIndicator != null && ctx != null)
                {
                    var planValue = row.Values.FirstOrDefault(v => v.IndicatorId == planIndicator.Id);
                    planValue.Should().NotBeNull();
                }

                return row;
            });

        // Act
        var result = await initializer.InitializeRowsForShiftAsync(
            shiftStartTime,
            schedules,
            template,
            CreateFormContext(productContext));

        // Assert
        result.Should().NotBeNull();
        formRowDataFactoryMock.Verify(
            f => f.CreateWorkRow(
                It.IsAny<short>(),
                It.IsAny<Indicator>(),
                It.IsAny<Indicator?>(),
                It.IsAny<TimeOnly>(),
                It.IsAny<TimeOnly>(),
                productContext),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task InitializeRowsForShiftAsync_WithoutProductContext_ShouldNotCalculatePlan()
    {
        // Arrange
        var shiftStartTime = new TimeOnly(7, 0);
        var schedules = new List<ShiftScheduleDto>();
        var template = CreateTemplate();

        dictionariesRepositoryMock
            .Setup(r => r.SelectAdditionalOperationsAsync())
            .ReturnsAsync(new List<AdditionalOperationDto>());

        productContextExtractorMock
            .Setup(e => e.Extract(It.IsAny<Dictionary<string, object>?>()))
            .Returns((ProductContext?)null);

        formRowDataFactoryMock
            .Setup(f => f.CreateWorkRow(
                It.IsAny<short>(),
                It.IsAny<Indicator>(),
                It.IsAny<Indicator?>(),
                It.IsAny<TimeOnly>(),
                It.IsAny<TimeOnly>(),
                It.IsAny<ProductContext?>()))
            .Returns<short, Indicator, Indicator?, TimeOnly, TimeOnly, ProductContext?>((order, _, _, start, end, _) =>
                CreateWorkRow(order, start, end));

        // Act
        var result = await initializer.InitializeRowsForShiftAsync(
            shiftStartTime,
            schedules,
            template);

        // Assert
        result.Should().NotBeNull();
        formRowDataFactoryMock.Verify(
            f => f.CreateWorkRow(
                It.IsAny<short>(),
                It.IsAny<Indicator>(),
                It.IsAny<Indicator?>(),
                It.IsAny<TimeOnly>(),
                It.IsAny<TimeOnly>(),
                null),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task InitializeRowsForShiftAsync_WithMultipleBreaks_ShouldProcessAllBreaks()
    {
        // Arrange
        var shiftStartTime = new TimeOnly(7, 0);
        var schedules = new List<ShiftScheduleDto>
        {
            new() { Id = 1, ShiftId = 1, AdditionalOperationId = 1, StartTime = new TimeOnly(9, 0) },
            new() { Id = 2, ShiftId = 1, AdditionalOperationId = 2, StartTime = new TimeOnly(11, 0) },
            new() { Id = 3, ShiftId = 1, AdditionalOperationId = 3, StartTime = new TimeOnly(13, 50) }
        };

        var additionalOperations = new List<AdditionalOperationDto>
        {
            new() { Id = 1, Name = "Перерыв 15 мин", Duration = TimeSpan.FromMinutes(15) },
            new() { Id = 2, Name = "Обед 30 мин", Duration = TimeSpan.FromMinutes(30) },
            new() { Id = 3, Name = "Перерыв 15 мин", Duration = TimeSpan.FromMinutes(15) }
        };

        var template = CreateTemplate();
        var productContext = new ProductContext { DailyRate = 100, CycleTime = 60 };

        dictionariesRepositoryMock
            .Setup(r => r.SelectAdditionalOperationsAsync())
            .ReturnsAsync(additionalOperations);

        productContextExtractorMock
            .Setup(e => e.Extract(It.IsAny<Dictionary<string, object>?>()))
            .Returns(productContext);

        var breakRowCallCount = 0;

        formRowDataFactoryMock
            .Setup(f => f.CreateWorkRow(
                It.IsAny<short>(),
                It.IsAny<Indicator>(),
                It.IsAny<Indicator?>(),
                It.IsAny<TimeOnly>(),
                It.IsAny<TimeOnly>(),
                It.IsAny<ProductContext?>()))
            .Returns<short, Indicator, Indicator?, TimeOnly, TimeOnly, ProductContext?>((order, _, _, start, end, _) =>
                CreateWorkRow(order, start, end));

        formRowDataFactoryMock
            .Setup(f => f.CreateBreakRow(
                It.IsAny<short>(),
                It.IsAny<Indicator>(),
                It.IsAny<TimeOnly>(),
                It.IsAny<TimeOnly>(),
                It.IsAny<string>(),
                It.IsAny<int>()))
            .Returns<short, Indicator, TimeOnly, TimeOnly, string, int>((order, _, start, end, name, _) =>
            {
                breakRowCallCount++;
                return CreateBreakRow(order, start, end, name);
            });

        // Act
        var result = await initializer.InitializeRowsForShiftAsync(
            shiftStartTime,
            schedules,
            template);

        // Assert
        result.Should().NotBeNull();
        breakRowCallCount.Should().Be(3); // Должно быть создано 3 строки перерывов
        result.Count(r => r.IsAdditionalOperation).Should().Be(3);
    }

    [Fact]
    public async Task InitializeRowsForShiftAsync_ShouldOrderRowsCorrectly()
    {
        // Arrange
        var shiftStartTime = new TimeOnly(7, 0);
        var schedules = new List<ShiftScheduleDto>();
        var template = CreateTemplate();
        var productContext = new ProductContext { DailyRate = 100, CycleTime = 60 };

        dictionariesRepositoryMock
            .Setup(r => r.SelectAdditionalOperationsAsync())
            .ReturnsAsync(new List<AdditionalOperationDto>());

        productContextExtractorMock
            .Setup(e => e.Extract(It.IsAny<Dictionary<string, object>?>()))
            .Returns(productContext);

        var orderSequence = new List<short>();

        formRowDataFactoryMock
            .Setup(f => f.CreateWorkRow(
                It.IsAny<short>(),
                It.IsAny<Indicator>(),
                It.IsAny<Indicator?>(),
                It.IsAny<TimeOnly>(),
                It.IsAny<TimeOnly>(),
                It.IsAny<ProductContext?>()))
            .Returns<short, Indicator, Indicator?, TimeOnly, TimeOnly, ProductContext?>((order, _, _, start, end, _) =>
            {
                orderSequence.Add(order);
                return CreateWorkRow(order, start, end);
            });

        // Act
        var result = await initializer.InitializeRowsForShiftAsync(
            shiftStartTime,
            schedules,
            template);

        // Assert
        result.Should().NotBeNull();
        orderSequence.Should().BeInAscendingOrder();
        result.Select(r => r.Order).Should().BeInAscendingOrder();
        result.Select(r => r.Order).Should().BeEquivalentTo(orderSequence);
    }

    private static Template CreateTemplate()
    {
        return new Template
        {
            Id = 1,
            Name = "Test Template",
            PaTypeId = 1,
            Version = 1,
            Indicators = new List<Indicator>
            {
                new() { Id = 16, Name = "Время работы, час.", ValueType = "Text", InputType = "Initialization" },
                new() { Id = 1, Name = "План, шт.", ValueType = "Number", InputType = "Initialization" }
            }
        };
    }

    private static FormRowData CreateWorkRow(short order, TimeOnly startTime, TimeOnly endTime)
    {
        return new FormRowData
        {
            Order = order,
            IsAdditionalOperation = false,
            Values = new List<FormRowValueData>
            {
                new() { IndicatorId = 16, Value = $"{startTime:HH:mm}-{endTime:HH:mm}" },
                new() { IndicatorId = 1, Value = "10" } // Примерное значение плана
            }
        };
    }

    private static FormRowData CreateBreakRow(short order, TimeOnly startTime, TimeOnly endTime, string operationName)
    {
        return new FormRowData
        {
            Order = order,
            IsAdditionalOperation = true,
            AdditionalOperationId = 1,
            Values = new List<FormRowValueData>
            {
                new() { IndicatorId = 16, Value = $"{startTime:HH:mm}-{endTime:HH:mm} {operationName}" }
            }
        };
    }

    private static Dictionary<string, object> CreateFormContext(ProductContext productContext)
    {
        var json = JsonSerializer.Serialize(new
        {
            dailyRate = productContext.DailyRate,
            cycleTime = productContext.CycleTime
        });

        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        return new Dictionary<string, object>
        {
            { "product", jsonElement }
        };
    }
}
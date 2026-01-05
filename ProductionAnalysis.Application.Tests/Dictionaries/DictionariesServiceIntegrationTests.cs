using FluentAssertions;
using NUnit.Framework;
using ProductionAnalysis.Application.Tests.Infrastructure;

namespace ProductionAnalysis.Application.Tests.Dictionaries;

public class DictionariesServiceIntegrationTests : BaseIntegrationTest
{
    [Test]
    public async Task GetDepartmentsAsync_ShouldReturnDepartments()
    {
        var result = await DictionariesService.GetDepartmentsAsync();

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(d => d.Id > 0 && !string.IsNullOrEmpty(d.Name));
    }

    [Test]
    public async Task GetDowntimeReasonGroupsAsync_ShouldReturnGroups()
    {
        var result = await DictionariesService.GetDowntimeReasonGroupsAsync();

        result.Should().NotBeNull();
    }

    [Test]
    public async Task GetEmployeesAsync_ShouldReturnEmployees()
    {
        var user = await DataBuilder.CreateUserAsync();
        await DataBuilder.CreateEmployeeAsync(user.Id);

        var result = await DictionariesService.GetEmployeesAsync();

        result.Should().NotBeNull();
        result.Should().Contain(e => e.UserId == user.Id);
    }

    [Test]
    public async Task GetEnterprisesAsync_ShouldReturnEnterprises()
    {
        var result = await DictionariesService.GetEnterprisesAsync();

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(e => e.Id > 0 && !string.IsNullOrEmpty(e.Name));
    }

    [Test]
    public async Task GetAdditionalOperationsAsync_ShouldReturnOperations()
    {
        var result = await DictionariesService.GetAdditionalOperationsAsync();

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(o => o.Id > 0 && !string.IsNullOrEmpty(o.Name));
    }

    [Test]
    public async Task GetOperationsAsync_ShouldReturnOperations()
    {
        var result = await DictionariesService.GetOperationsAsync();

        result.Should().NotBeNull();
    }

    [Test]
    public async Task GetProductsAsync_ShouldReturnProducts()
    {
        var result = await DictionariesService.GetProductsAsync();

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(p => p.Id > 0 && !string.IsNullOrEmpty(p.Name));
    }

    [Test]
    public async Task GetShiftsAsync_ShouldReturnShifts()
    {
        var result = await DictionariesService.GetShiftsAsync();

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(s => s.Id > 0 && !string.IsNullOrEmpty(s.Name));
    }
}
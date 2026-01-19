using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ProductionAnalysis.Application.Implementation.Dictionaries;
using ProductionAnalysis.Application.Tests.Infrastructure;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Tests.Dictionaries;

public class DepartmentsServiceTests : BaseIntegrationTest
{
    [Test]
    public async Task CreateDepartmentAsync_WithValidRequest_ShouldCreateDepartment()
    {
        // Arrange
        var enterprise = await DataBuilder.CreateEnterpriseAsync();
        var request = new CreateDepartmentRequest
        {
            Name = "Test Department",
            EnterpriseId = enterprise.Id
        };

        // Act
        var result = await GetService<IDepartmentsService>().CreateDepartmentAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Test Department");
        result.Value.EnterpriseId.Should().Be(enterprise.Id);

        var created = await DbContext.Departments.FirstOrDefaultAsync(d => d.Id == result.Value.Id);
        created.Should().NotBeNull();
        created!.Name.Should().Be("Test Department");
    }

    [Test]
    public async Task CreateDepartmentAsync_WithNonExistentEnterprise_ShouldReturnNotFound()
    {
        // Arrange
        var request = new CreateDepartmentRequest
        {
            Name = "Test Department",
            EnterpriseId = 99999
        };

        // Act
        var result = await GetService<IDepartmentsService>().CreateDepartmentAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Enterprise");
        result.Error.Message.Should().Contain("not found");
    }

    [Test]
    public async Task UpdateDepartmentAsync_WithValidRequest_ShouldUpdateDepartment()
    {
        // Arrange
        var enterprise = await DataBuilder.CreateEnterpriseAsync();
        var department = await DataBuilder.CreateDepartmentAsync(enterpriseId: enterprise.Id);
        var newEnterprise = await DataBuilder.CreateEnterpriseAsync(id: 2, name: "New Enterprise");

        var request = new UpdateDepartmentRequest
        {
            Name = "Updated Department",
            EnterpriseId = newEnterprise.Id
        };

        // Act
        var result = await GetService<IDepartmentsService>().UpdateDepartmentAsync(department.Id, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Updated Department");
        result.Value.EnterpriseId.Should().Be(newEnterprise.Id);

        var updated = await DbContext.Departments.FirstOrDefaultAsync(d => d.Id == department.Id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Department");
        updated.EnterpriseId.Should().Be(newEnterprise.Id);
    }

    [Test]
    public async Task UpdateDepartmentAsync_WithNonExistentDepartment_ShouldReturnNotFound()
    {
        // Arrange
        var enterprise = await DataBuilder.CreateEnterpriseAsync();
        var request = new UpdateDepartmentRequest
        {
            Name = "Updated Department",
            EnterpriseId = enterprise.Id
        };

        // Act
        var result = await GetService<IDepartmentsService>().UpdateDepartmentAsync(99999, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Department");
        result.Error.Message.Should().Contain("not found");
    }

    [Test]
    public async Task UpdateDepartmentAsync_WithNonExistentEnterprise_ShouldReturnNotFound()
    {
        // Arrange
        var enterprise = await DataBuilder.CreateEnterpriseAsync();
        var department = await DataBuilder.CreateDepartmentAsync(enterpriseId: enterprise.Id);

        var request = new UpdateDepartmentRequest
        {
            Name = "Updated Department",
            EnterpriseId = 99999
        };

        // Act
        var result = await GetService<IDepartmentsService>().UpdateDepartmentAsync(department.Id, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Enterprise");
        result.Error.Message.Should().Contain("not found");
    }

    [Test]
    public async Task DeleteDepartmentAsync_WithExistingDepartment_ShouldDeleteDepartment()
    {
        // Arrange
        var enterprise = await DataBuilder.CreateEnterpriseAsync();
        var department = await DataBuilder.CreateDepartmentAsync(enterpriseId: enterprise.Id);

        // Act
        var result = await GetService<IDepartmentsService>().DeleteDepartmentAsync(department.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deleted = await DbContext.Departments.FirstOrDefaultAsync(d => d.Id == department.Id);
        deleted.Should().BeNull();
    }

    [Test]
    public async Task DeleteDepartmentAsync_WithNonExistentDepartment_ShouldReturnNotFound()
    {
        // Act
        var result = await GetService<IDepartmentsService>().DeleteDepartmentAsync(99999);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Department");
        result.Error.Message.Should().Contain("not found");
    }
}
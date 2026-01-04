using FluentAssertions;
using NUnit.Framework;
using ProductionAnalysis.Application.Tests.Infrastructure;
using ProductionAnalysis.Client.Models.Auth;

namespace ProductionAnalysis.Application.Tests.Auth;

public class AuthServiceIntegrationTests : BaseIntegrationTest
{
    [Test]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
    {
        const string email = "test@example.com";
        const string password = "TestPassword123";
        await DataBuilder.CreateUserAsync(email, password);

        var request = new LoginRequest
        {
            Email = email,
            Password = password
        };

        var result = await AuthService.LoginAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Email.Should().Be(email);
        result.Value.Token.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task LoginAsync_WithNonExistentUser_ShouldReturnNotFound()
    {
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "AnyPassword"
        };

        var result = await AuthService.LoginAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("not found");
    }

    [Test]
    public async Task LoginAsync_WithInvalidPassword_ShouldReturnBadRequest()
    {
        const string email = "test@example.com";
        const string password = "CorrectPassword123";
        await DataBuilder.CreateUserAsync(email, password);

        var request = new LoginRequest
        {
            Email = email,
            Password = "WrongPassword"
        };

        var result = await AuthService.LoginAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Bad credentials");
    }
}
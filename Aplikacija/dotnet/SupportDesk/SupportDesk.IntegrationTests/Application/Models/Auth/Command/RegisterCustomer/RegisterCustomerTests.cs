using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Models.Auth.Command.RegisterCustomer;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.User.Enums;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.IntegrationTests.Application.Models.Auth.Command.RegisterCustomer;

[TestFixture]
internal sealed class RegisterCustomerTests : IntegrationTestsBase
{
    [Test]
    public async Task Handle_WithValidData_ShouldRegisterCustomer()
    {
        RegisterCustomerCommandResult? result = null;
        
        Assert.DoesNotThrowAsync(async () =>
        {
            result = await CommandDispatcher.DispatchAsync(new RegisterCustomerCommand(
                DefaultCustomerUserName,
                DefaultCustomerEmail,
                DefaultCustomerPassword));
        });
        
        Assert.That(result, Is.Not.Null);

        var customerId = new UserId(result.RefreshToken.UserId);

        var persistedCustomer = await DbContext.DomainUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == customerId);
        
        Assert.That(persistedCustomer, Is.Not.Null);
        Assert.That(persistedCustomer.Email.EmailValue, Is.EqualTo(DefaultCustomerEmail));
        Assert.That(persistedCustomer.UserName.UserNameValue, Is.EqualTo(DefaultCustomerUserName)); 
        Assert.That(persistedCustomer.Role, Is.EqualTo(UserRole.Customer));
        Assert.That(persistedCustomer.OrganizationId, Is.Null);
        
        var persistedRefreshToken = await DbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Token == result.RefreshToken.Value);
        
        Assert.That(persistedRefreshToken, Is.Not.Null);
        Assert.That(persistedRefreshToken.Token, Is.EqualTo(result.RefreshToken.Value));
        Assert.That(persistedRefreshToken.UserId, Is.EqualTo(customerId.IdValue));
    }

    [TestCase(DefaultCustomerEmail, DefaultCustomerPassword, "")]
    [TestCase("", DefaultCustomerPassword, DefaultCustomerUserName)]
    [TestCase(DefaultCustomerEmail, "", DefaultCustomerUserName)]
    [TestCase("", "", "")]
    public async Task Handle_WithInvalidData_ShouldThrowValidationException(
        string email,
        string password,
        string username)
    {
        Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new RegisterCustomerCommand(username, email, password));
        });
    }
}
using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Models.Auth.Command.RegisterOrganization;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Organization.Enums;
using SupportDesk.Domain.Models.User.Enums;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.IntegrationTests.Application.Models.Auth.Command.RegisterOrganization;

[TestFixture]
internal sealed class RegisterOrganizationTests : IntegrationTestsBase
{
    [Test]
    public async Task Handle_WithValidData_ShouldRegisterOrganization()
    {
        RegisterOrganizationCommandResult? result = null;
        
        Assert.DoesNotThrowAsync(async () =>
        {
            result = await CommandDispatcher.DispatchAsync(new RegisterOrganizationCommand(
                DefaultOrganizationAdminUserName,
                DefaultOrganizationName,
                DefaultOrganizationAdminEmail,
                DefaultOrganizationAdminPassword));
        });
        
        Assert.That(result, Is.Not.Null);
        
        var organizationAdminId = new UserId(result.RefreshToken.UserId);
        
        var persistedOrganizationAdmin = await DbContext.DomainUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == organizationAdminId);
        
        Assert.That(persistedOrganizationAdmin, Is.Not.Null);
        Assert.That(persistedOrganizationAdmin.Email.EmailValue, Is.EqualTo(DefaultOrganizationAdminEmail));
        Assert.That(persistedOrganizationAdmin.UserName.UserNameValue, Is.EqualTo(DefaultOrganizationAdminUserName));
        Assert.That(persistedOrganizationAdmin.Role, Is.EqualTo(UserRole.OrganizationAdmin));
        Assert.That(persistedOrganizationAdmin.OrganizationId, Is.Not.Null);

        var persistedOrganization = await DbContext.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == persistedOrganizationAdmin.OrganizationId);
        
        Assert.That(persistedOrganization, Is.Not.Null);
        Assert.That(persistedOrganization.Name.NameValue, Is.EqualTo(DefaultOrganizationName));
        Assert.That(persistedOrganization.Status, Is.EqualTo(OrganizationStatus.Active));
        
        var persistedRefreshToken = await DbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Token == result.RefreshToken.Value);
        
        Assert.That(persistedRefreshToken, Is.Not.Null);
        Assert.That(persistedRefreshToken.Token, Is.EqualTo(result.RefreshToken.Value));
        Assert.That(persistedRefreshToken.UserId, Is.EqualTo(organizationAdminId.IdValue));
    }

    [TestCase("", DefaultOrganizationName, DefaultOrganizationAdminEmail, DefaultOrganizationAdminPassword)]
    [TestCase(DefaultOrganizationAdminUserName, "", DefaultOrganizationAdminEmail, DefaultOrganizationAdminPassword)]
    [TestCase(DefaultOrganizationAdminUserName, DefaultOrganizationName, "", DefaultOrganizationAdminPassword)]
    [TestCase(DefaultOrganizationAdminUserName, DefaultOrganizationName, DefaultOrganizationAdminEmail, "")]
    [TestCase("", "", "", "")]   
    public async Task Handle_WithInvalidData_ShouldThrowValidationException(
        string userName,
        string email,
        string password,
        string organizationName)
    {
        Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new RegisterOrganizationCommand(userName, organizationName, email, password));
        });
    }
}
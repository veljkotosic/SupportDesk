using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.User.Enums;
using SupportDesk.Domain.Models.User.Events;
using SupportDesk.Domain.Models.User.Validation.Rules;
using SupportDesk.TestsUtility;
using UserModel = SupportDesk.Domain.Models.User.User;

namespace SupportDesk.UnitTests.Domain.Models.User;

[TestFixture]
internal sealed class UserTests
{
    [TestCase("customer@example.com", "customer_user", null, UserRole.Customer)]
    [TestCase("agent@example.com", "support_agent", "11111111-1111-1111-1111-111111111111", UserRole.SupportAgent)]
    [TestCase("admin@example.com", "org_admin", "22222222-2222-2222-2222-222222222222", UserRole.OrganizationAdmin)]
    public void Create_WithValidData_ShouldCreateUser(
        string validEmail, 
        string validUserName, 
        string? organizationIdString, 
        UserRole role)
    {
        var organizationId = organizationIdString is not null ? Guid.Parse(organizationIdString) : (Guid?)null;
        var timeProvider = TimeProvider.System;

        var user = UserModel.Create(
            validEmail,
            validUserName,
            organizationId,
            role,
            timeProvider);

        Assert.Multiple(() =>
        {
            Assert.That(user.Id.IdValue, Is.Not.EqualTo(Guid.Empty));
            Assert.That(user.Email.EmailValue, Is.EqualTo(validEmail));
            Assert.That(user.UserName.UserNameValue, Is.EqualTo(validUserName));
            Assert.That(user.OrganizationId?.IdValue, Is.EqualTo(organizationId));
            Assert.That(user.Role, Is.EqualTo(role));
            Assert.That(user.CreatedAt.CreatedAtValue, Is.Not.EqualTo(default(DateTime)));
            Assert.That(user.GetDomainEvents(), Has.Some.TypeOf<UserCreatedDomainEvent>());
        });
    }

    [Test]
    public void Create_WhenCustomerHasOrganizationId_ShouldBreakCustomerCannotBePartOfOrganizationRule()
    {
        var organizationId = Guid.NewGuid();
        var timeProvider = TimeProvider.System;

        var exception = Assert.Throws<ValidationException>(() => UserModel.Create(
            "customer@example.com",
            "customer_user",
            organizationId,
            UserRole.Customer,
            timeProvider));

        AssertUtility.AssertHasBrokenExactRule<CustomerCannotBePartOfOrganizationRule>(exception);
    }
}
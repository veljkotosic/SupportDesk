using Microsoft.Extensions.Time.Testing;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.SupportAgentInvite.Enums;
using SupportDesk.Domain.Models.SupportAgentInvite.Events;
using SupportDesk.Domain.Models.SupportAgentInvite.Options;
using SupportDesk.Domain.Models.SupportAgentInvite.Validation;
using SupportDesk.Domain.Models.SupportAgentInvite.ValueObjects;
using SupportDesk.TestsUtility;
using SupportAgentInviteModel = SupportDesk.Domain.Models.SupportAgentInvite.SupportAgentInvite;

namespace SupportDesk.UnitTests.Domain.Models.SupportAgentInvite;

[TestFixture]
internal sealed class SupportAgentInviteTests
{
    [TestCase("agent@example.com")]
    [TestCase("support.agent@company.org")]
    public void Create_WithValidData_ShouldCreateSupportAgentInvite(string validEmail)
    {
        var timeProvider = TimeProvider.System;
        
        var organizationId = Guid.NewGuid();

        var invite = SupportAgentInviteModel.Create(validEmail, organizationId, timeProvider);

        Assert.Multiple(() =>
        {
            Assert.That(invite.Id.IdValue, Is.Not.EqualTo(Guid.Empty));
            Assert.That(invite.Code.CodeValue, Is.Not.EqualTo(Guid.Empty));
            Assert.That(invite.Email.EmailValue, Is.EqualTo(validEmail));
            Assert.That(invite.OrganizationId.IdValue, Is.EqualTo(organizationId));
            Assert.That(invite.Status, Is.EqualTo(SupportAgentInviteStatus.Active));
            Assert.That(invite.CreatedAt.CreatedAtValue, Is.Not.EqualTo(default(DateTime)));
            Assert.That(invite.ExpiresAt.ExpiresAtValue, Is.GreaterThan(DateTime.UtcNow));
            Assert.That(invite.UsedAt, Is.Null);
            Assert.That(invite.RevokedAt, Is.Null);
            Assert.That(invite.GetDomainEvents(), Has.Some.TypeOf<SupportAgentInviteCreatedDomainEvent>());
        });
    }

    [Test]
    public void Use_WhenActiveAndNotExpired_ShouldMarkAsUsedAndRaiseDomainEvent()
    {
        var timeProvider = TimeProvider.System;
        
        var invite = SupportAgentInviteModel.Create("agent@example.com", Guid.NewGuid(), timeProvider);
        invite.ClearDomainEvents();

        invite.Use(timeProvider);

        Assert.Multiple(() =>
        {
            Assert.That(invite.Status, Is.EqualTo(SupportAgentInviteStatus.Used));
            Assert.That(invite.UsedAt, Is.Not.Null);
            Assert.That(invite.GetDomainEvents(), Has.Some.TypeOf<SupportAgentInviteUsedDomainEvent>());
        });
    }

    [Test]
    public void Use_WhenAlreadyUsed_ShouldThrowValidationException()
    {
        var timeProvider = TimeProvider.System;
        
        var invite = SupportAgentInviteModel.Create("agent@example.com", Guid.NewGuid(), timeProvider);
        invite.Use(timeProvider);

        var exception = Assert.Throws<ValidationException>(() => invite.Use(timeProvider));

        AssertUtility.AssertHasProducedExactError(exception, SupportAgentInviteErrors.InvalidInviteCode(invite.Code));
    }

    [Test]
    public void Use_WhenExpired_ShouldThrowValidationException()
    {
        var fakeTimeProvider = new FakeTimeProvider(DateTime.UtcNow);
        
        var invite = SupportAgentInviteModel.Create("agent@example.com", Guid.NewGuid(), fakeTimeProvider);
        
        fakeTimeProvider.Advance(TimeSpan.FromDays(SupportAgentInviteOptionsDefaults.ExpirationDays + 1));
        
        var exception = Assert.Throws<ValidationException>(() => invite.Use(fakeTimeProvider));
        
        AssertUtility.AssertHasProducedExactError(exception, SupportAgentInviteErrors.ExpiredInviteCode(invite.Code));       
    }
}
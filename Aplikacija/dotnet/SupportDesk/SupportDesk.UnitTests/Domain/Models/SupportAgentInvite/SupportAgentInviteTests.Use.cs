using Microsoft.Extensions.Time.Testing;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.SupportAgentInvite.Enums;
using SupportDesk.Domain.Models.SupportAgentInvite.Events;
using SupportDesk.Domain.Models.SupportAgentInvite.Options;
using SupportDesk.Domain.Models.SupportAgentInvite.Validation;
using SupportDesk.TestsUtility;
using SupportAgentInviteModel = SupportDesk.Domain.Models.SupportAgentInvite.SupportAgentInvite;

namespace SupportDesk.UnitTests.Domain.Models.SupportAgentInvite;

[TestFixture]
internal sealed partial class SupportAgentInviteTests
{
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
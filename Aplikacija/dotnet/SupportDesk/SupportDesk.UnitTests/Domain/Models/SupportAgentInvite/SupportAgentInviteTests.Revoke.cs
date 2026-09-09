using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.SupportAgentInvite.Enums;
using SupportDesk.Domain.Models.SupportAgentInvite.Events;
using SupportDesk.Domain.Models.SupportAgentInvite.Validation;
using SupportDesk.TestsUtility;
using SupportAgentInviteModel = SupportDesk.Domain.Models.SupportAgentInvite.SupportAgentInvite;

namespace SupportDesk.UnitTests.Domain.Models.SupportAgentInvite;

[TestFixture]
internal sealed partial class SupportAgentInviteTests
{
    [Test]
    public void Revoke_WithActiveInvite_ShouldRevokeInviteAndRaiseDomainEvent()
    {
        var timeProvider = TimeProvider.System;
        
        var invite = SupportAgentInviteModel.Create("agent@example.com", Guid.NewGuid(), timeProvider);
        
        invite.Revoke(timeProvider);

        Assert.Multiple(() =>
        {
            Assert.That(invite.Status, Is.EqualTo(SupportAgentInviteStatus.Revoked));
            Assert.That(invite.RevokedAt, Is.Not.Null);
            Assert.That(invite.GetDomainEvents(), Has.Some.TypeOf<SupportAgentInviteRevokedDomainEvent>());
        });
    }
    
    [Test]
    public void Revoke_WithUsedInvite_ShouldProduceCannotRevokeError()
    {
        var timeProvider = TimeProvider.System;
        
        var invite = SupportAgentInviteModel.Create("agent@example.com", Guid.NewGuid(), timeProvider);

        invite.Use(timeProvider);
        
        var exception = Assert.Throws<ValidationException>(() => invite.Revoke(timeProvider));
        
        AssertUtility.AssertHasProducedExactError(exception, SupportAgentInviteErrors.CannotRevoke());
    }
    
    [Test]
    public void Revoke_WithRevokedInvite_ShouldProduceCannotRevokeError()
    {
        var timeProvider = TimeProvider.System;
        
        var invite = SupportAgentInviteModel.Create("agent@example.com", Guid.NewGuid(), timeProvider);

        invite.Revoke(timeProvider);
        
        var exception = Assert.Throws<ValidationException>(() => invite.Revoke(timeProvider));
        
        AssertUtility.AssertHasProducedExactError(exception, SupportAgentInviteErrors.CannotRevoke());
    }
}
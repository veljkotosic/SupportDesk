using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.TicketNotification.Enums;
using SupportDesk.Domain.Models.TicketNotification.Validation;
using SupportDesk.TestsUtility;
using TicketNotificationModel = SupportDesk.Domain.Models.TicketNotification.TicketNotification;

namespace SupportDesk.UnitTests.Domain.Models.TicketNotification;

[TestFixture]
internal sealed partial class TicketNotificationTests
{
    [Test]
    public void Read_WithUnreadNotification_ShouldReadNotification()
    {
        var organizationId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var timeProvider = TimeProvider.System;

        var notification = TicketNotificationModel.Create(
            organizationId,
            ticketId,
            "Text",
            customerId,
            timeProvider);
        
        Assert.DoesNotThrow(() => notification.Read());
        
        Assert.That(notification.Status, Is.EqualTo(TicketNotificationStatus.Read));
    }
    
    [Test]
    public void Read_WithReadNotification_ShouldProduceAlreadyReadError()
    {
        var organizationId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var timeProvider = TimeProvider.System;
        
        var notification = TicketNotificationModel.Create(
            organizationId,
            ticketId,
            "Text",
            customerId,
            timeProvider);
        
        notification.Read();
        
        var exception = Assert.Throws<ValidationException>(() => notification.Read());
        
        AssertUtility.AssertHasProducedExactError(exception, TicketNotificationErrors.AlreadyRead(notification.Id));
    }
}
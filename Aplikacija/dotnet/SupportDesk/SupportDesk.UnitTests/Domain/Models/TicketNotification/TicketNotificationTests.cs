using SupportDesk.Domain.Models.TicketNotification.Enums;
using SupportDesk.Domain.Models.TicketNotification.Events;
using TicketNotificationModel = SupportDesk.Domain.Models.TicketNotification.TicketNotification;

namespace SupportDesk.UnitTests.Domain.Models.TicketNotification;

[TestFixture]
internal sealed class TicketNotificationTests
{
    [TestCase("Test Notification")]
    public void Create_WithValidData_ShouldCreateTicketNotification(string validText)
    {
        var organizationId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var timeProvider = TimeProvider.System;

        var notification = TicketNotificationModel.Create(
            organizationId,
            ticketId,
            validText,
            timeProvider);

        Assert.Multiple(() =>
        {
            Assert.That(notification.Id.IdValue, Is.Not.EqualTo(Guid.Empty));
            Assert.That(notification.OrganizationId.IdValue, Is.EqualTo(organizationId));
            Assert.That(notification.TicketId.IdValue, Is.EqualTo(ticketId));
            Assert.That(notification.Text.TextValue, Is.EqualTo(validText));
            Assert.That(notification.Status, Is.EqualTo(TicketNotificationStatus.Unread));
            Assert.That(notification.CreatedAt.CreatedAtValue, Is.Not.EqualTo(default(DateTime)));
            Assert.That(notification.GetDomainEvents(), Has.Some.TypeOf<TicketNotificationCreatedDomainEvent>());
        });
    }
}
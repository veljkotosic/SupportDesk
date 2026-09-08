using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.TicketNotification.Enums;
using SupportDesk.Domain.Models.TicketNotification.Events;
using SupportDesk.Domain.Models.TicketNotification.Options;
using TicketNotificationModel = SupportDesk.Domain.Models.TicketNotification.TicketNotification;

namespace SupportDesk.UnitTests.Domain.Models.TicketNotification;

[TestFixture]
internal sealed partial class TicketNotificationTests
{
    [TestCase("Test Notification")]
    public void Create_WithValidData_ShouldCreateTicketNotification(string validText)
    {
        var organizationId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var timeProvider = TimeProvider.System;

        var notification = TicketNotificationModel.Create(
            organizationId,
            ticketId,
            validText,
            customerId,
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
    
    [Test]
    public void Create_WithInvalidData_ShouldThrowValidationException()
    {
        var invalidText = string.Join("", Enumerable.Repeat("a", TicketNotificationOptionsDefaults.TextMaximumLength + 1));
        
        var organizationId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var timeProvider = TimeProvider.System;

        Assert.Throws<ValidationException>(() =>
        {
            _ = TicketNotificationModel.Create(
                organizationId,
                ticketId,
                invalidText,
                customerId,
                timeProvider);
        });
    }
}
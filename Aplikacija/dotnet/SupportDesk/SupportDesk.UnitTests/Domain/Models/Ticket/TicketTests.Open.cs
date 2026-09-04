using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.Ticket.Events;
using TicketModel = SupportDesk.Domain.Models.Ticket.Ticket;

namespace SupportDesk.UnitTests.Domain.Models.Ticket;

[TestFixture]
internal sealed partial class TicketTests
{
    [TestCase(TicketPriority.Low, "Test Ticket")]
    [TestCase(TicketPriority.High, "Test Ticket")]
    [TestCase(TicketPriority.Medium, "Test Ticket")]
    public void Open_WithValidData_ShouldCreateTicket(TicketPriority priority, string validSubject)
    {
        var organizationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var timeProvider = TimeProvider.System;

        var ticket = TicketModel.Open(
            organizationId,
            customerId,
            categoryId,
            priority,
            validSubject,
            timeProvider);

        Assert.Multiple(() =>
        {
            Assert.That(ticket.Id.IdValue, Is.Not.EqualTo(Guid.Empty));
            Assert.That(ticket.OrganizationId.IdValue, Is.EqualTo(organizationId));
            Assert.That(ticket.CustomerId.IdValue, Is.EqualTo(customerId));
            Assert.That(ticket.SupportAgentId, Is.Null);
            Assert.That(ticket.CategoryId.IdValue, Is.EqualTo(categoryId));
            Assert.That(ticket.Status, Is.EqualTo(TicketStatus.Open));
            Assert.That(ticket.Priority, Is.EqualTo(priority));
            Assert.That(ticket.Feedback, Is.EqualTo(TicketFeedback.None));
            Assert.That(ticket.Subject.SubjectValue, Is.EqualTo(validSubject));
            Assert.That(ticket.OpenedAt.OpenedAtValue, Is.Not.EqualTo(default(DateTime)));
            Assert.That(ticket.AssignedAt, Is.Null);
            Assert.That(ticket.ClosedAt, Is.Null);
            Assert.That(ticket.LastMessageAt, Is.Null);
            Assert.That(ticket.GetDomainEvents(), Has.Some.TypeOf<TicketOpenedDomainEvent>());
        });
    }
    
    [TestCase(TicketPriority.Low, "")]
    public void Open_WithInvalidData_ShouldThrowValidationException(TicketPriority priority, string invalidSubject)
    {
        var organizationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var timeProvider = TimeProvider.System;

        Assert.Throws<ValidationException>(() =>
        {
            _ = TicketModel.Open(
                organizationId,
                customerId,
                categoryId,
                priority,
                invalidSubject,
                timeProvider);
        });
    }
}
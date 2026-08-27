using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.Ticket.Events;
using TicketModel = SupportDesk.Domain.Models.Ticket.Ticket;

namespace SupportDesk.UnitTests.Domain.Models.Ticket;

[TestFixture]
internal sealed class TicketTests
{
    [TestCase(TicketPriority.Low, "Test Ticket")]
    [TestCase(TicketPriority.High, "Test Ticket")]
    [TestCase(TicketPriority.Medium, "Test Ticket")]
    public void Create_WithValidData_ShouldCreateTicket(TicketPriority priority, string validSubject)
    {
        var organizationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var timeProvider = TimeProvider.System;

        var ticket = TicketModel.Create(
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
            Assert.That(ticket.GetDomainEvents(), Has.Some.TypeOf<TicketCreatedDomainEvent>());
        });
    }
}
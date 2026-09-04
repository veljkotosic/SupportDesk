using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.Ticket.Events;
using SupportDesk.Domain.Models.Ticket.Validation;
using SupportDesk.TestsUtility;
using TicketModel = SupportDesk.Domain.Models.Ticket.Ticket;

namespace SupportDesk.UnitTests.Domain.Models.Ticket;

[TestFixture]
internal sealed partial class TicketTests
{
    [Test]
    public void Assign_WithTicketOpened_ShouldAssignTicket()
    {
        var organizationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var timeProvider = TimeProvider.System;

        var ticket = TicketModel.Open(
            organizationId,
            customerId,
            categoryId,
            TicketPriority.Medium,
            "Test Ticket",
            timeProvider);
        
        var supportAgentId = Guid.NewGuid();
        
        Assert.DoesNotThrow(() => ticket.Assign(supportAgentId, timeProvider));
        
        Assert.That(ticket.Status, Is.EqualTo(TicketStatus.Assigned));
        Assert.That(ticket.SupportAgentId, Is.Not.Null);
        Assert.That(ticket.SupportAgentId.IdValue, Is.EqualTo(supportAgentId));
        Assert.That(ticket.AssignedAt, Is.Not.Null);
        Assert.That(ticket.AssignedAt.AssignedAtValue, Is.Not.EqualTo(default(DateTime)));
        Assert.That(ticket.GetDomainEvents(), Has.Some.TypeOf<TicketAssignedDomainEvent>());
    }

    [Test]
    public void Assign_WithTicketAssigned_ShouldProduceCannotAssignError()
    {
        var organizationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var timeProvider = TimeProvider.System;

        var ticket = TicketModel.Open(
            organizationId,
            customerId,
            categoryId,
            TicketPriority.Medium,
            "Test Ticket",
            timeProvider);
        
        var supportAgentId = Guid.NewGuid();
        
        ticket.Assign(supportAgentId, timeProvider);
        
        var exception = Assert.Throws<ValidationException>(() => ticket.Assign(supportAgentId, timeProvider));
        
        AssertUtility.AssertHasProducedExactError(exception, TicketErrors.CannotAssign());
    }
    
    [Test]
    public void Assign_WithTicketClosed_ShouldProduceCannotAssignError()
    {
        var organizationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var timeProvider = TimeProvider.System;

        var ticket = TicketModel.Open(
            organizationId,
            customerId,
            categoryId,
            TicketPriority.Medium,
            "Test Ticket",
            timeProvider);
        
        var supportAgentId = Guid.NewGuid();
        
        ticket.Assign(supportAgentId, timeProvider);
        ticket.Close(timeProvider);
        
        var exception = Assert.Throws<ValidationException>(() => ticket.Assign(supportAgentId, timeProvider));
        
        AssertUtility.AssertHasProducedExactError(exception, TicketErrors.CannotAssign());     
    }
}
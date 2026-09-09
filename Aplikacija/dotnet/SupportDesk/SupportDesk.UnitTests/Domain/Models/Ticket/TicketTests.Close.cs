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
    public void Close_WithTicketAssigned_ShouldCloseTicket()
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
        
        Assert.DoesNotThrow(() => ticket.Close(timeProvider));
        
        Assert.That(ticket.Status, Is.EqualTo(TicketStatus.Closed));
        Assert.That(ticket.ClosedAt, Is.Not.Null);
        Assert.That(ticket.ClosedAt.ClosedAtValue, Is.Not.EqualTo(default(DateTime)));
        Assert.That(ticket.GetDomainEvents(), Has.Some.TypeOf<TicketClosedDomainEvent>());
    }
    
    [Test]
    public void Close_WithTicketOpened_ShouldProduceCannotCloseError()
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
        
        var exception = Assert.Throws<ValidationException>(() => ticket.Close(timeProvider));
        
        AssertUtility.AssertHasProducedExactError(exception, TicketErrors.CannotClose());    
    }
    
    [Test]
    public void Close_WithTicketClosed_ShouldProduceCannotCloseError()
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
        
        var exception = Assert.Throws<ValidationException>(() => ticket.Close(timeProvider));
        
        AssertUtility.AssertHasProducedExactError(exception, TicketErrors.CannotClose());     
    }
}
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.Ticket.Validation;
using SupportDesk.TestsUtility;
using TicketModel = SupportDesk.Domain.Models.Ticket.Ticket;

namespace SupportDesk.UnitTests.Domain.Models.Ticket;

[TestFixture]
internal sealed partial class TicketTests 
{
    [TestCase(TicketFeedback.Helpful)]
    [TestCase(TicketFeedback.Unhelpful)]
    public void GiveFeedback_WithValidFeedback_WithTicketClosed_ShouldGiveFeedback(TicketFeedback feedback)
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
        
        ticket.GiveFeedback(feedback);
        
        Assert.That(ticket.Feedback, Is.EqualTo(feedback));
    }

    [Test]
    public void GiveFeedback_WithTicketOpened_ShouldProduceCannotGiveFeedbackError()
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
        
        var exception = Assert.Throws<ValidationException>(() => ticket.GiveFeedback(TicketFeedback.Helpful));   
        
        AssertUtility.AssertHasProducedExactError(exception, TicketErrors.CannotGiveFeedbackIfTicketIsNotClosed());
    }
    
    [Test]
    public void GiveFeedback_WithTicketAssigned_ShouldProduceCannotGiveFeedbackError()
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
        
        var exception = Assert.Throws<ValidationException>(() => ticket.GiveFeedback(TicketFeedback.Helpful));   
        
        AssertUtility.AssertHasProducedExactError(exception, TicketErrors.CannotGiveFeedbackIfTicketIsNotClosed());
    }
    
    [Test]
    public void GiveFeedback_WithTicketAlreadyGaveFeedback_ShouldProduceFeedbackAlreadyGivenError()
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
        ticket.GiveFeedback(TicketFeedback.Helpful);
        
        var exception = Assert.Throws<ValidationException>(() => ticket.GiveFeedback(TicketFeedback.Helpful));   
        
        AssertUtility.AssertHasProducedExactError(exception, TicketErrors.FeedbackAlreadyGiven());
    }
    
    [TestCase(TicketFeedback.None)]
    [TestCase((TicketFeedback)100)]
    public void GiveFeedback_WithInvalidFeedback_ShouldProduceInvalidFeedbackError(TicketFeedback feedback)
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
        
        var exception = Assert.Throws<ValidationException>(() => ticket.GiveFeedback(feedback));   
        
        AssertUtility.AssertHasProducedExactError(exception, TicketErrors.InvalidFeedback());
    }
}
using SupportDesk.Domain.Abstract.Validation;
using MessageModel = SupportDesk.Domain.Models.Message.Message;

namespace SupportDesk.UnitTests.Domain.Models.Message;

[TestFixture]
internal sealed partial class MessageTests
{
    [TestCase("Test Message")]
    public void Create_WithValidData_ShouldCreateMessage(string validText)
    {
        var timeProvider = TimeProvider.System;
        
        var organizationId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var senderId = Guid.NewGuid();

        var message = MessageModel.Create(organizationId, ticketId, senderId, validText, timeProvider);

        Assert.Multiple(() =>
        {
            Assert.That(message.Id.IdValue, Is.Not.EqualTo(Guid.Empty));
            Assert.That(message.OrganizationId.IdValue, Is.EqualTo(organizationId));
            Assert.That(message.TicketId.IdValue, Is.EqualTo(ticketId));
            Assert.That(message.SenderId.IdValue, Is.EqualTo(senderId));
            Assert.That(message.Text.TextValue, Is.EqualTo(validText));
            Assert.That(message.CreatedAt.CreatedAtValue, Is.Not.EqualTo(default(DateTime)));
        });
    }

    [TestCase("")]
    public void Create_WithInvalidData_ShouldThrowValidationException(string invalidText)
    {
        var timeProvider = TimeProvider.System;
        
        var organizationId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var senderId = Guid.NewGuid();

        Assert.Throws<ValidationException>(() =>
        {
            _ = MessageModel.Create(organizationId, ticketId, senderId, invalidText, timeProvider);
        });
    }
}
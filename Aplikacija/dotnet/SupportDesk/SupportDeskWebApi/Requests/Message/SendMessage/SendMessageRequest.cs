using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Message.SendMessage;

public record SendMessageRequest(
    Guid TicketId,
    string Text)
    : IRequest<SendMessageResult>;
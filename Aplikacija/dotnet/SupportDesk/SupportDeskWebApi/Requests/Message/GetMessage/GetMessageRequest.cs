using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Message.GetMessage;

public record GetMessageRequest(Guid MessageId) : IRequest<GetMessageResult>;
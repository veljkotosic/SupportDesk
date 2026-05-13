using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Message.SendMessage;

public record SendMessageResult(Guid MessageId) : IRequestResult;
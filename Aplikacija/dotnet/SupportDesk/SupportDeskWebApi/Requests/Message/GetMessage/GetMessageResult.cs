using SupportDeskWebApi.Requests.Abstract;
using SupportDeskWebApi.Requests.Message.Common;

namespace SupportDeskWebApi.Requests.Message.GetMessage;

public record GetMessageResult(MessageDetailsDto Message) : IRequestResult;
namespace SupportDesk.WebApi.Controllers.v1.Message.Requests;

public sealed record SendMessageRequest(Guid TicketId, string Text);
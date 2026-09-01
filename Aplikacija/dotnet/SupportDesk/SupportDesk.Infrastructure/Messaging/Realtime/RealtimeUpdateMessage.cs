using SupportDesk.Application.Abstract.Messaging;

namespace SupportDesk.Infrastructure.Messaging.Realtime;

public sealed record RealtimeUpdateMessage(
    RealtimeHubType HubType,
    string TargetGroup,
    string Action,
    string PayloadJson
);
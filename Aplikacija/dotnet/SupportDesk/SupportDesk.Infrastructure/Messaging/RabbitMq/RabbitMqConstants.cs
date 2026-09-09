namespace SupportDesk.Infrastructure.Messaging.RabbitMq;

public static class RabbitMqConstants
{
    public const string DomainEventsExchange = "supportdesk.domain-events";
    
    public const string RealtimeUpdatesExchange = "supportdesk.realtime-updates";
    
    public const string WorkerDomainEventsQueue = "supportdesk.worker.domain-events";
}
using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.Note.Events;

namespace SupportDesk.Application.Models.Notes.Event.NoteCreated;

internal sealed class LogNoteCreatedDomainEventHandler
    : IDomainEventHandler<NoteCreatedDomainEvent>
{
    private readonly ILogger<LogNoteCreatedDomainEventHandler> _logger;

    public LogNoteCreatedDomainEventHandler(ILogger<LogNoteCreatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(NoteCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Note with id '{NoteId}' created.", domainEvent.NoteId.IdValue);
        
        return Task.CompletedTask;       
    }
}
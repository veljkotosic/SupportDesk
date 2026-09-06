using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Application.Abstract.Messaging;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Note.Events;
using SupportDesk.Domain.Models.Note.Validation;
using SupportDesk.Domain.Models.Note.ValueObjects;

namespace SupportDesk.Application.Models.Notes.Event.NoteCreated;

internal sealed class PublishNoteCreatedRealtimeUpdateDomainEventHandler
    : IDomainEventHandler<NoteCreatedDomainEvent>
{
    private readonly IApplicationDbContext _applicationDbContext;
    private readonly IRealtimePublisher _realtimePublisher;

    public PublishNoteCreatedRealtimeUpdateDomainEventHandler(
        IApplicationDbContext applicationDbContext,
        IRealtimePublisher realtimePublisher)
    {
        _applicationDbContext = applicationDbContext;
        _realtimePublisher = realtimePublisher;
    }

    public async Task HandleAsync(NoteCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var noteDto = await GetQuery(domainEvent.NoteId).FirstOrDefaultAsync(cancellationToken);

        if (noteDto is null)
        {
            throw new ValidationException(NoteErrors.NotFound(domainEvent.NoteId));
        }
        
        await _realtimePublisher.PublishAsync(
            RealtimeHubType.Ticket,
            $"{domainEvent.NoteId.IdValue}:organization",
            "NewNote",
            noteDto,
            cancellationToken);
    }

    private IQueryable<TicketViewNoteDto> GetQuery(NoteId noteId)
    {
        return from note in _applicationDbContext.Notes.IgnoreQueryFilters().AsNoTracking()
            where note.Id == noteId

            select new TicketViewNoteDto(
                note.Id.IdValue,
                note.OrganizationId.IdValue,
                note.TicketId.IdValue,
                note.AuthorId.IdValue,
                note.Text.TextValue,
                note.CreatedAt.CreatedAtValue);
    }
}

internal sealed record TicketViewNoteDto(
    Guid Id,
    Guid OrganizationId,
    Guid TicketId,
    Guid AuthorId,
    string Text, 
    DateTime CreatedAt);
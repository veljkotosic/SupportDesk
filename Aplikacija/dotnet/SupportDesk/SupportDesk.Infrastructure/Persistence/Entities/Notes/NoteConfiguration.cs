using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.Note;
using SupportDesk.Domain.Models.Note.Options;
using SupportDesk.Domain.Models.Note.ValueObjects;
using SupportDesk.Domain.Models.Organization;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.Domain.Models.User;
using SupportDesk.Domain.Models.User.ValueObjects;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.Persistence.Entities.Notes;

public class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    private readonly SupportDeskDbContext _context;

    public NoteConfiguration(SupportDeskDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("Notes");
        
        builder.HasKey(note => note.Id);
        
        builder.Property(note => note.Id)
            .HasConversion(
                noteId => noteId.IdValue,
                value => new NoteId(value))
            .ValueGeneratedNever()
            .IsRequired();
        
        builder.Property(note => note.OrganizationId)
            .HasConversion(
                organizationId => organizationId.IdValue,
                value => new OrganizationId(value))
            .IsRequired();
        
        builder.Property(note => note.TicketId)
            .HasConversion(
                ticketId => ticketId.IdValue,
                value => new TicketId(value))
            .IsRequired();
        
        builder.Property(note => note.AuthorId)
            .HasConversion(
                authorId => authorId.IdValue,
                value => new UserId(value))
            .IsRequired();
        
        builder.Property(note => note.Text)
            .HasConversion(
                text => text.TextValue,
                value => new NoteText(value))
            .HasMaxLength(NoteOptionsDefaults.TextMaximumLength)
            .IsRequired();
        
        builder.Property(note => note.CreatedAt)
            .HasConversion(
                createdAt => createdAt.CreatedAtValue,
                value => new CreatedAt(value))
            .IsRequired();
        
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(note => note.OrganizationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne<Ticket>()
            .WithMany()
            .HasForeignKey(note => note.TicketId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(note => note.AuthorId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasQueryFilter(note => 
            note.OrganizationId == (_context.OrganizationId != null ? new OrganizationId(_context.OrganizationId.Value) : null));
    }
}
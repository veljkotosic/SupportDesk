using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.Message;
using SupportDesk.Domain.Models.Note;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.User;
using SupportDesk.Domain.Models.User.ValueObjects;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.Persistence.Entities.Users;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    private readonly SupportDeskDbContext _context;

    public UserConfiguration(SupportDeskDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .HasConversion(
                userId => userId.IdValue,
                value => new UserId(value))
            .ValueGeneratedNever()
            .IsRequired();
        
        builder.Property(user => user.Email)
            .HasConversion(
                email => email.EmailValue,
                value => new Email(value))
            .IsRequired();
        
        builder.Property(user => user.UserName)
            .HasConversion(
                userName => userName.UserNameValue,
                value => new UserName(value))
            .IsRequired();
        
        builder.Property(user => user.OrganizationId)
            .HasConversion<Guid?>(
                organizationId => organizationId != null ? organizationId.IdValue : null,
                value => value != null ? new OrganizationId((Guid)value) : null)
            .IsRequired(false);

        builder.Property(user => user.Role)
            .IsRequired();
        
        builder.Property(user => user.CreatedAt)
            .HasConversion(
                createdAt => createdAt.CreatedAtValue,
                value => new CreatedAt(value))
            .IsRequired();
        
        builder.HasMany<Ticket>()
            .WithOne()
            .HasForeignKey(ticket => ticket.CustomerId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany<Ticket>()
            .WithOne()
            .HasForeignKey(ticket => ticket.SupportAgentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany<Message>()
            .WithOne()
            .HasForeignKey(message => message.SenderId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany<Note>()
            .WithOne()
            .HasForeignKey(note => note.AuthorId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(user => 
            _context.OrganizationId == null || 
            user.OrganizationId == (_context.OrganizationId != null ? new OrganizationId(_context.OrganizationId.Value) : null));
    }
}
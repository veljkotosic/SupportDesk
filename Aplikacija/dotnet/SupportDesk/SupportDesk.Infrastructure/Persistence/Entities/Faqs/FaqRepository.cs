using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.Faq;
using SupportDesk.Domain.Models.Faq.Repository;
using SupportDesk.Domain.Models.Faq.ValueObjects;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.Persistence.Entities.Faqs;

public sealed class FaqRepository
    : AbstractRepository<Faq, FaqId>, IFaqRepository
{
    public FaqRepository(
        SupportDeskDbContext context,
        IDomainEventCollector domainEventCollector) 
        : base(context, domainEventCollector)
    {
        
    }
}
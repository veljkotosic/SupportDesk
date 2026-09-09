using SupportDesk.Domain.Abstract.Repository;
using SupportDesk.Domain.Models.Faq.ValueObjects;

namespace SupportDesk.Domain.Models.Faq.Repository;

public interface IFaqRepository : IAbstractRepository<Faq, FaqId>
{
    
}
using SupportDeskWebApi.Data.Entities.Common.Repository;

namespace SupportDeskWebApi.Data.Entities.Faq.Repository;

public interface IFaqRepository : IRepository<Faq>, IDeleteRepository<Faq>
{
    
}
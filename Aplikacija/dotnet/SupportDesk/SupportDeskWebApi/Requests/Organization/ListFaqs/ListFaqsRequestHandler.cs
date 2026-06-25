using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Organization.ListFaqs;

public class ListFaqsRequestHandler 
    : IRequestHandler<ListFaqsRequest, ListFaqsResult>
{
    private readonly SupportDeskDbContext _context;

    public ListFaqsRequestHandler(SupportDeskDbContext context)
    {
        _context = context;
    }

    public async Task<ListFaqsResult> HandleAsync(ListFaqsRequest request, CancellationToken cancellationToken = default)
    {
        var faqs = await _context.Faqs
            .AsNoTracking()
            .Where(faq => faq.OrganizationId == request.OrganizationId)
            .OrderBy(faq => faq.Question)
            .Select(faq => new FaqListingDto(faq.Id, faq.Question, faq.Answer))
            .ToListAsync(cancellationToken);

        return new ListFaqsResult(faqs);
    }
}

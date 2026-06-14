using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.OrganizationAdmin.GetSettings;

public class GetSettingsRequestHandler : IRequestHandler<GetSettingsRequest, GetSettingsResult>
{
    private readonly SupportDeskDbContext _context;

    public GetSettingsRequestHandler(SupportDeskDbContext context)
    {
        _context = context;
    }

    public async Task<GetSettingsResult> HandleAsync(GetSettingsRequest request, CancellationToken cancellationToken = default)
    {
        var faqs = await _context.Faqs
            .AsNoTracking()
            .OrderBy(faq => faq.Question)
            .Select(faq => new FaqSettingsDto(faq.Id, faq.Question, faq.Answer))
            .ToListAsync(cancellationToken);

        var templateAnswers = await _context.TemplateAnswers
            .AsNoTracking()
            .OrderBy(template => template.Title)
            .Select(template => new TemplateAnswerSettingsDto(template.Id, template.Title, template.Text))
            .ToListAsync(cancellationToken);

        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new CategorySettingsDto(
                category.Id,
                category.Name,
                category.Description,
                category.Tickets.Count))
            .ToListAsync(cancellationToken);

        return new GetSettingsResult(faqs, templateAnswers, categories);
    }
}

using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.TemplateAnswer.GetTemplateAnswers;

public class GetTemplateAnswersRequestHandler
    : IRequestHandler<GetTemplateAnswersRequest, GetTemplateAnswersResult>
{
    private readonly SupportDeskDbContext _context;

    public GetTemplateAnswersRequestHandler(SupportDeskDbContext context)
    {
        _context = context;
    }

    public async Task<GetTemplateAnswersResult> HandleAsync(GetTemplateAnswersRequest request, CancellationToken cancellationToken = default)
    {
        var templateAnswers = await _context.TemplateAnswers
            .AsNoTracking()
            .OrderBy(template => template.Title)
            .Select(template => new TemplateAnswerDto(template.Id, template.Title, template.Text))
            .ToListAsync(cancellationToken);

        return new GetTemplateAnswersResult(templateAnswers);
    }
}

using SupportDeskWebApi.Data.Entities.TemplateAnswer.Repository;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.TemplateAnswer.RemoveTemplateAnswer;

public class RemoveTemplateAnswerRequestHandler
    : IRequestHandler<RemoveTemplateAnswerRequest>
{
    private readonly ITemplateAnswerRepository _templateAnswerRepository;

    public RemoveTemplateAnswerRequestHandler(ITemplateAnswerRepository templateAnswerRepository)
    {
        _templateAnswerRepository = templateAnswerRepository;
    }

    public async Task HandleAsync(RemoveTemplateAnswerRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _templateAnswerRepository.GetByIdAsync(request.TemplateAnswerId, cancellationToken);

        if (category is null)
        {
            throw new Exception("Template answer not found");
        }
        
        await _templateAnswerRepository.DeleteAsync(category, cancellationToken);
    }
}
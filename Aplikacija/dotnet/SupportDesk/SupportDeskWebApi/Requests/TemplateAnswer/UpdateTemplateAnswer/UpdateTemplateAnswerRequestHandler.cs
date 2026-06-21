using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.TemplateAnswer.Repository;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.TemplateAnswer.UpdateTemplateAnswer;

public class UpdateTemplateAnswerRequestHandler 
    : IRequestHandler<UpdateTemplateAnswerRequest>
{
    private readonly ITemplateAnswerRepository _templateAnswerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTemplateAnswerRequestHandler(ITemplateAnswerRepository templateAnswerRepository, IUnitOfWork unitOfWork)
    {
        _templateAnswerRepository = templateAnswerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(UpdateTemplateAnswerRequest request, CancellationToken cancellationToken = default)
    {
        var templateAnswer = await _templateAnswerRepository.GetByIdAsync(request.TemplateAnswerId, cancellationToken);

        if (templateAnswer is null)
        {
            throw new InvalidOperationException("Template answer not found.");
        }

        var templateAnswerWithSameTitle = await _templateAnswerRepository.GetByTitleAsync(request.Title, cancellationToken);
        
        if (templateAnswerWithSameTitle is not null && templateAnswerWithSameTitle.Id != templateAnswer.Id)
        {
            throw new InvalidOperationException("Template answer with the same title already exists.");
        }

        templateAnswer.Title = request.Title;
        templateAnswer.Text = request.Text;
        
        await _templateAnswerRepository.SaveAsync(templateAnswer, cancellationToken);      
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

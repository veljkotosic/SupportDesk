using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.TemplateAnswer.Repository;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.TemplateAnswer.AddTemplateAnswer;

public class AddTemplateAnswerRequestHandler
    : IRequestHandler<AddTemplateAnswerRequest, AddTemplateAnswerResult>
{
    private readonly IUserContext _userContext;
    private readonly ITemplateAnswerRepository _templateAnswerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddTemplateAnswerRequestHandler(
        IUserContext userContext,
        ITemplateAnswerRepository templateAnswerRepository,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _templateAnswerRepository = templateAnswerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AddTemplateAnswerResult> HandleAsync(AddTemplateAnswerRequest request, CancellationToken cancellationToken = default)
    {
        var organizationId = _userContext.GetCurrentUsersOrganizationId();

        var templateAnswer = await _templateAnswerRepository.GetByTitleAsync(request.Title, cancellationToken);
            
        if (templateAnswer is not null)        
        {
            throw new Exception("Template answer with the same title already exists.");
        }

        templateAnswer = new Data.Entities.TemplateAnswer.TemplateAnswer
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Title = request.Title,
            Text = request.Text
        };
            
        await _templateAnswerRepository.SaveAsync(templateAnswer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
            
        return new AddTemplateAnswerResult(templateAnswer.Id);
    }
}
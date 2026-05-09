using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.Faq.Repository;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Faq.AddFaq;

public class AddFaqRequestHandler
    : IRequestHandler<AddFaqRequest, AddFaqResult>
{
    private readonly IUserContext _userContext;
    private readonly IFaqRepository _faqRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddFaqRequestHandler(
        IUserContext userContext,
        IFaqRepository faqRepository,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _faqRepository = faqRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AddFaqResult> HandleAsync(AddFaqRequest request, CancellationToken cancellationToken = default)
    {
        var organizationId = _userContext.GetCurrentUsersOrganizationId();

        var faq = new Data.Entities.Faq.Faq
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Question = request.Question,
            Answer = request.Answer
        };
            
        await _faqRepository.SaveAsync(faq, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
            
        return new AddFaqResult(faq.Id);
    }
}
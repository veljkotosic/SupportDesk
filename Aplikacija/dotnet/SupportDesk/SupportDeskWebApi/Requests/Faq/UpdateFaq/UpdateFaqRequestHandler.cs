using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.Faq.Repository;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Faq.UpdateFaq;

public class UpdateFaqRequestHandler 
    : IRequestHandler<UpdateFaqRequest>
{
    private readonly IFaqRepository _faqRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFaqRequestHandler(IFaqRepository faqRepository, IUnitOfWork unitOfWork)
    {
        _faqRepository = faqRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(UpdateFaqRequest request, CancellationToken cancellationToken = default)
    {
        var faq = await _faqRepository.GetByIdAsync(request.FaqId, cancellationToken);

        if (faq is null)
        {
            throw new InvalidOperationException("FAQ not found.");
        }

        faq.Question = request.Question;
        faq.Answer = request.Answer;
        
        await _faqRepository.SaveAsync(faq, cancellationToken);       
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

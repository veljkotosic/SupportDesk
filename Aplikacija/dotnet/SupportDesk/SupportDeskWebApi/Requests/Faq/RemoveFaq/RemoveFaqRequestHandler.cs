using SupportDeskWebApi.Data.Entities.Faq.Repository;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Faq.RemoveFaq;

public class RemoveFaqRequestHandler 
    : IRequestHandler<RemoveFaqRequest>
{
    private readonly IFaqRepository _faqRepository;

    public RemoveFaqRequestHandler(IFaqRepository faqRepository)
    {
        _faqRepository = faqRepository;
    }

    public async Task HandleAsync(RemoveFaqRequest request, CancellationToken cancellationToken = default)
    {
        var faq = await _faqRepository.GetByIdAsync(request.FaqId, cancellationToken);

        if (faq is null)
        {
            throw new Exception("Faq not found");
        }
        
        await _faqRepository.DeleteAsync(faq, cancellationToken);
    }
}
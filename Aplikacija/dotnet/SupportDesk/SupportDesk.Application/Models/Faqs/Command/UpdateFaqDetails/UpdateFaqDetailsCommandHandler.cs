using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Faq;
using SupportDesk.Domain.Models.Faq.Repository;
using SupportDesk.Domain.Models.Faq.ValueObjects;

namespace SupportDesk.Application.Models.Faqs.Command.UpdateFaqDetails;

internal sealed class UpdateFaqDetailsCommandHandler
    : AbstractCommandHandler<UpdateFaqDetailsCommand, UpdateFaqDetailsCommandHandlerContext>
{
    private readonly IFaqRepository _faqRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public UpdateFaqDetailsCommandHandler(
        PermissionChecker permissionChecker,
        IFaqRepository faqRepository,
        IUnitOfWork unitOfWork) 
        : base(permissionChecker)
    {
        _faqRepository = faqRepository;
        _unitOfWork = unitOfWork;
    }

    protected override async Task<UpdateFaqDetailsCommandHandlerContext> PrepareAsync(UpdateFaqDetailsCommand command, CancellationToken cancellationToken)
    {
        var faqId = new FaqId(command.FaqId);
        
        var faq = await _faqRepository.GetByIdAsync(faqId, cancellationToken);
        
        return new UpdateFaqDetailsCommandHandlerContext(faq, faqId);       
    }

    protected override async Task ExecuteAsync(UpdateFaqDetailsCommand command, UpdateFaqDetailsCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var faq = context.Faq!;
        faq.UpdateDetails(command.Question, command.Answer);
        
        await _faqRepository.SaveAsync(faq, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);       
    }

    public override IReadOnlyList<ICollection<IRule>> GetValidationStages(UpdateFaqDetailsCommandHandlerContext context)
    {
        return 
        [
            [
                new DomainModelExistsRule<Faq, FaqId>(context.Faq, context.FaqId)
            ]
        ];
    }
}
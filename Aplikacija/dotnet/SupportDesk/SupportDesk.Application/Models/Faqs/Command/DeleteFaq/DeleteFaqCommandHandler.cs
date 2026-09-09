using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Faq;
using SupportDesk.Domain.Models.Faq.Repository;
using SupportDesk.Domain.Models.Faq.ValueObjects;

namespace SupportDesk.Application.Models.Faqs.Command.DeleteFaq;

internal sealed class DeleteFaqCommandHandler
    : AbstractCommandHandler<DeleteFaqCommand, DeleteFaqCommandHandlerContext>
{
    private readonly TimeProvider _timeProvider;
    private readonly IFaqRepository _faqRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public DeleteFaqCommandHandler(
        PermissionChecker permissionChecker,
        TimeProvider timeProvider,
        IFaqRepository faqRepository,
        IUnitOfWork unitOfWork) 
        : base(permissionChecker)
    {
        _timeProvider = timeProvider;
        _faqRepository = faqRepository;
        _unitOfWork = unitOfWork;
    }

    protected override async Task<DeleteFaqCommandHandlerContext> PrepareAsync(DeleteFaqCommand command, CancellationToken cancellationToken)
    {
        var faqId = new FaqId(command.FaqId);
        
        var faq = await _faqRepository.GetByIdAsync(faqId, cancellationToken);
        
        return new DeleteFaqCommandHandlerContext(faq, faqId);      
    }

    protected override async Task ExecuteAsync(DeleteFaqCommand command, DeleteFaqCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var faq = context.Faq!;
        
        faq.Delete(_timeProvider);
        
        await _faqRepository.SaveAsync(faq, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public override IReadOnlyList<ICollection<IRule>> GetValidationStages(DeleteFaqCommandHandlerContext context)
    {
        return 
        [
            [
                new DomainModelExistsRule<Faq, FaqId>(context.Faq, context.FaqId)    
            ]
        ];
    }
}
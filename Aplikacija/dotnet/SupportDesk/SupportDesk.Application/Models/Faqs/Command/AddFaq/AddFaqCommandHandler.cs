using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Command.Context;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Models.Faq;
using SupportDesk.Domain.Models.Faq.Repository;

namespace SupportDesk.Application.Models.Faqs.Command.AddFaq;

internal sealed class AddFaqCommandHandler
    : AbstractCommandHandler<AddFaqCommand, AddFaqCommandResult, EmptyCommandHandlerContext>
{
    private readonly TimeProvider _timeProvider;
    private readonly ITenantContext _tenantContext;
    private readonly IFaqRepository _faqRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public AddFaqCommandHandler(
        PermissionChecker permissionChecker,
        TimeProvider timeProvider,
        ITenantContext tenantContext,
        IFaqRepository faqRepository,
        IUnitOfWork unitOfWork)
        : base(permissionChecker)
    {
        _timeProvider = timeProvider;
        _tenantContext = tenantContext;
        _faqRepository = faqRepository;
        _unitOfWork = unitOfWork;
    }

    protected override Task<EmptyCommandHandlerContext> PrepareAsync(AddFaqCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(new EmptyCommandHandlerContext());       
    }

    protected override async Task<AddFaqCommandResult> ExecuteAsync(AddFaqCommand command, EmptyCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var organizationId = (Guid)_tenantContext.GetCurrentOrganizationId()!;

        var faq = Faq.Create(organizationId, command.Question, command.Answer, _timeProvider);
        
        await _faqRepository.SaveAsync(faq, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new AddFaqCommandResult(faq.Id.IdValue);       
    }
}
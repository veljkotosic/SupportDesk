using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Command.Context;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Models.TemplateAnswer;
using SupportDesk.Domain.Models.TemplateAnswer.Repository;

namespace SupportDesk.Application.Models.TemplateAnswers.Command.AddTemplateAnswer;

internal sealed class AddTemplateAnswerCommandHandler
    : AbstractCommandHandler<AddTemplateAnswerCommand, AddTemplateAnswerCommandResult, EmptyCommandHandlerContext>
{
    private readonly TimeProvider _timeProvider;
    private readonly ITenantContext _tenantContext;
    private readonly ITemplateAnswerRepository _templateAnswerRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public AddTemplateAnswerCommandHandler(
        PermissionChecker permissionChecker,
        TimeProvider timeProvider,
        ITenantContext tenantContext,
        ITemplateAnswerRepository templateAnswerRepository,
        IUnitOfWork unitOfWork) 
        : base(permissionChecker)
    {
        _timeProvider = timeProvider;
        _tenantContext = tenantContext;
        _templateAnswerRepository = templateAnswerRepository;
        _unitOfWork = unitOfWork;
    }

    protected override Task<EmptyCommandHandlerContext> PrepareAsync(AddTemplateAnswerCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(new EmptyCommandHandlerContext());       
    }

    protected override async Task<AddTemplateAnswerCommandResult> ExecuteAsync(AddTemplateAnswerCommand command, EmptyCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var organizationId = (Guid)_tenantContext.GetCurrentOrganizationId()!;

        var templateAnswer = TemplateAnswer.Create(
            organizationId,
            command.Title,
            command.Text,
            _timeProvider);
        
        await _templateAnswerRepository.SaveAsync(templateAnswer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new AddTemplateAnswerCommandResult(templateAnswer.Id.IdValue);      
    }
}
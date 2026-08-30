using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.TemplateAnswer;
using SupportDesk.Domain.Models.TemplateAnswer.Repository;
using SupportDesk.Domain.Models.TemplateAnswer.ValueObjects;

namespace SupportDesk.Application.Models.TemplateAnswers.Command.DeleteTemplateAnswer;

internal sealed class DeleteTemplateAnswerCommandHandler
    : AbstractCommandHandler<DeleteTemplateAnswerCommand, DeleteTemplateAnswerCommandHandlerContext>
{
    private readonly TimeProvider _timeProvider;
    private readonly ITemplateAnswerRepository _templateAnswerRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public DeleteTemplateAnswerCommandHandler(
        PermissionChecker permissionChecker,
        TimeProvider timeProvider,
        ITemplateAnswerRepository templateAnswerRepository,
        IUnitOfWork unitOfWork) 
        : base(permissionChecker)
    {
        _timeProvider = timeProvider;
        _templateAnswerRepository = templateAnswerRepository;
        _unitOfWork = unitOfWork;
    }

    protected override async Task<DeleteTemplateAnswerCommandHandlerContext> PrepareAsync(DeleteTemplateAnswerCommand command, CancellationToken cancellationToken)
    {
        var templateAnswerId = new TemplateAnswerId(command.TemplateAnswerId);
        
        var templateAnswer = await _templateAnswerRepository.GetByIdAsync(templateAnswerId, cancellationToken);
        
        return new DeleteTemplateAnswerCommandHandlerContext(templateAnswer, templateAnswerId);      
    }

    protected override async Task ExecuteAsync(DeleteTemplateAnswerCommand command, DeleteTemplateAnswerCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var templateAnswer = context.TemplateAnswer!;
        
        templateAnswer.Delete(_timeProvider);
        
        await _templateAnswerRepository.SaveAsync(templateAnswer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);      
    }

    public override IReadOnlyList<ICollection<IRule>> GetValidationStages(DeleteTemplateAnswerCommandHandlerContext context)
    {
        return
        [
            [
                new DomainModelExistsRule<TemplateAnswer, TemplateAnswerId>(context.TemplateAnswer, context.TemplateAnswerId)
            ]    
        ];
    }
}
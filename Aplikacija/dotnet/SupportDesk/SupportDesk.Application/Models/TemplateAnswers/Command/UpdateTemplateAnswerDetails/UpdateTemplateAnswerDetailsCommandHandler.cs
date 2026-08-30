using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.TemplateAnswer;
using SupportDesk.Domain.Models.TemplateAnswer.Repository;
using SupportDesk.Domain.Models.TemplateAnswer.ValueObjects;

namespace SupportDesk.Application.Models.TemplateAnswers.Command.UpdateTemplateAnswerDetails;

internal sealed class UpdateTemplateAnswerDetailsCommandHandler
    : AbstractCommandHandler<UpdateTemplateAnswerDetailsCommand, UpdateTemplateAnswerDetailsCommandHandlerContext>
{
    private readonly ITemplateAnswerRepository _templateAnswerRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public UpdateTemplateAnswerDetailsCommandHandler(
        PermissionChecker permissionChecker,
        ITemplateAnswerRepository templateAnswerRepository,
        IUnitOfWork unitOfWork) 
        : base(permissionChecker)
    {
        _templateAnswerRepository = templateAnswerRepository;
        _unitOfWork = unitOfWork;
    }

    protected override async Task<UpdateTemplateAnswerDetailsCommandHandlerContext> PrepareAsync(UpdateTemplateAnswerDetailsCommand command, CancellationToken cancellationToken)
    {
        var templateAnswerId = new TemplateAnswerId(command.TemplateAnswerId);
        
        var templateAnswer = await _templateAnswerRepository.GetByIdAsync(templateAnswerId, cancellationToken);
        
        return new UpdateTemplateAnswerDetailsCommandHandlerContext(templateAnswer, templateAnswerId); 
    }

    protected override async Task ExecuteAsync(UpdateTemplateAnswerDetailsCommand command, UpdateTemplateAnswerDetailsCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var templateAnswer = context.TemplateAnswer!;
        
        templateAnswer.UpdateDetails(command.Title, command.Text);
        
        await _templateAnswerRepository.SaveAsync(templateAnswer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);      
    }

    public override IReadOnlyList<ICollection<IRule>> GetValidationStages(UpdateTemplateAnswerDetailsCommandHandlerContext context)
    {
        return
        [
            [
                new DomainModelExistsRule<TemplateAnswer, TemplateAnswerId>(context.TemplateAnswer, context.TemplateAnswerId)
            ]
        ];
    }
}
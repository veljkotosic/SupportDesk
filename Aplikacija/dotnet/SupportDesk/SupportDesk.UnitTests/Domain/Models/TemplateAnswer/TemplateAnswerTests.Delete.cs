using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.TemplateAnswer.Events;
using SupportDesk.Domain.Models.TemplateAnswer.Validation;
using SupportDesk.TestsUtility;
using TemplateAnswerModel = SupportDesk.Domain.Models.TemplateAnswer.TemplateAnswer;

namespace SupportDesk.UnitTests.Domain.Models.TemplateAnswer;

[TestFixture]
internal sealed partial class TemplateAnswerTests
{
    [Test]
    public void Delete_WithValidData_ShouldMarkFaqAsDeleted_AndRaiseDomainEvent()
    {
        var timeProvider = TimeProvider.System;
        
        var templateAnswer = TemplateAnswerModel.Create(Guid.NewGuid(), ValidTitle, ValidText, timeProvider);
        
        templateAnswer.Delete(timeProvider);
        
        Assert.That(templateAnswer.DeletedAt, Is.Not.Null);
        Assert.That(templateAnswer.GetDomainEvents(), Has.Some.TypeOf<TemplateAnswerDeletedDomainEvent>());
    }
    
    [Test]
    public void Delete_WithFaqAlreadyDeleted_ShouldProduceAlreadyDeletedError()
    {
        var timeProvider = TimeProvider.System;
        
        var templateAnswer = TemplateAnswerModel.Create(Guid.NewGuid(), ValidTitle, ValidText, timeProvider);
        
        templateAnswer.Delete(timeProvider);

        var exception = Assert.Throws<ValidationException>(() =>
        {
            templateAnswer.Delete(timeProvider);
        });
        
        AssertUtility.AssertHasProducedExactError(exception, TemplateAnswerErrors.AlreadyDeleted(templateAnswer.Id));
    }
}
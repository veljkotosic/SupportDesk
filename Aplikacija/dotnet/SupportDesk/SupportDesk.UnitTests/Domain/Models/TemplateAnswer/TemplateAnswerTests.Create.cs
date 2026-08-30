using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.TemplateAnswer.Events;
using TemplateAnswerModel = SupportDesk.Domain.Models.TemplateAnswer.TemplateAnswer;

namespace SupportDesk.UnitTests.Domain.Models.TemplateAnswer;

[TestFixture]
internal sealed partial class TemplateAnswerTests
{
    private const string ValidTitle = "Test Title";
    private const string ValidText = "Test Text";
    
    [TestCase(ValidTitle, ValidText)]
    public void Create_WithValidData_ShouldCreateTemplateAnswer(string validTitle, string validText)
    {
        var timeProvider = TimeProvider.System;
        
        var organizationId = Guid.NewGuid();

        var templateAnswer = TemplateAnswerModel.Create(organizationId, validTitle, validText, timeProvider);

        Assert.Multiple(() =>
        {
            Assert.That(templateAnswer.Id.IdValue, Is.Not.EqualTo(Guid.Empty));
            Assert.That(templateAnswer.OrganizationId.IdValue, Is.EqualTo(organizationId));
            Assert.That(templateAnswer.Title.TitleValue, Is.EqualTo(validTitle));
            Assert.That(templateAnswer.Text.TextValue, Is.EqualTo(validText));
            Assert.That(templateAnswer.CreatedAt.CreatedAtValue, Is.Not.EqualTo(default(DateTime)));
            Assert.That(templateAnswer.DeletedAt, Is.Null);
            Assert.That(templateAnswer.GetDomainEvents(), Has.Some.TypeOf<TemplateAnswerCreatedDomainEvent>());
        });
    }
    
    [TestCase("", "")]
    public void Create_WithInvalidData_ShouldThrowValidationException(string invalidQuestion, string invalidAnswer)
    {
        var timeProvider = TimeProvider.System;

        Assert.Throws<ValidationException>(() =>
        {
            _ = TemplateAnswerModel.Create(Guid.NewGuid(), invalidQuestion, invalidAnswer, timeProvider);
        });
    }
}
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Faq.Events;
using FaqModel = SupportDesk.Domain.Models.Faq.Faq;

namespace SupportDesk.UnitTests.Domain.Models.Faq;

[TestFixture]
internal sealed partial class FaqTests
{
    private const string ValidQuestion = "Test Faq Question";
    private const string ValidAnswer = "Test Faq Answer";
    
    [TestCase(ValidQuestion, ValidAnswer)]
    public void Create_WithValidData_ShouldCreateFaq(string validQuestion, string validAnswer)
    {
        var organizationId = Guid.NewGuid();

        var faq = FaqModel.Create(organizationId, validQuestion, validAnswer, TimeProvider.System);

        Assert.Multiple(() =>
        {
            Assert.That(faq.Id.IdValue, Is.Not.EqualTo(Guid.Empty));
            Assert.That(faq.OrganizationId.IdValue, Is.EqualTo(organizationId));
            Assert.That(faq.Question.QuestionValue, Is.EqualTo(validQuestion));
            Assert.That(faq.Answer.AnswerValue, Is.EqualTo(validAnswer));
            Assert.That(faq.CreatedAt.CreatedAtValue, Is.Not.EqualTo(default(DateTime)));
            Assert.That(faq.DeletedAt, Is.Null);
            Assert.That(faq.GetDomainEvents(), Has.Some.TypeOf<FaqCreatedDomainEvent>());
        });
    }
    
    [TestCase("", "")]
    public void Create_WithInvalidData_ShouldThrowValidationException(string invalidQuestion, string invalidAnswer)
    {
        var timeProvider = TimeProvider.System;

        Assert.Throws<ValidationException>(() =>
        {
            _ = FaqModel.Create(Guid.NewGuid(), invalidQuestion, invalidAnswer, timeProvider);
        });
    }
}
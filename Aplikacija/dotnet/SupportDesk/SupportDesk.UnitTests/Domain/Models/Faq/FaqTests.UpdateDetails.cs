using SupportDesk.Domain.Models.Faq.Events;
using FaqModel = SupportDesk.Domain.Models.Faq.Faq;

namespace SupportDesk.UnitTests.Domain.Models.Faq;

[TestFixture]
internal sealed partial class FaqTests
{
    [Test]
    public void UpdateDetails_WithDifferentQuestion_ShouldUpdateDetails_AndRaiseDomainEvent()
    {
        var timeProvider = TimeProvider.System;
        
        var faq = FaqModel.Create(Guid.NewGuid(), ValidQuestion, ValidAnswer, timeProvider);
        
        var newQuestion = "Test Faq Question 2";
        
        faq.UpdateDetails(newQuestion, null);
        
        Assert.Multiple(() =>
        {
            Assert.That(faq.Question.QuestionValue, Is.EqualTo(newQuestion));
            Assert.That(faq.Answer.AnswerValue, Is.EqualTo(ValidAnswer));
            Assert.That(faq.GetDomainEvents(), Has.Some.TypeOf<FaqDetailsUpdatedDomainEvent>());
        });
    }
    
    [Test]
    public void UpdateDetails_WithDifferentAnswer_ShouldUpdateDetails_AndRaiseDomainEvent()
    {
        var timeProvider = TimeProvider.System;
        
        var faq = FaqModel.Create(Guid.NewGuid(), ValidQuestion, ValidAnswer, timeProvider);
        
        var newAnswer = "Test Faq Answer 2";
        
        faq.UpdateDetails(null, newAnswer);
        
        Assert.Multiple(() =>
        {
            Assert.That(faq.Question.QuestionValue, Is.EqualTo(ValidQuestion));
            Assert.That(faq.Answer.AnswerValue, Is.EqualTo(newAnswer));
            Assert.That(faq.GetDomainEvents(), Has.Some.TypeOf<FaqDetailsUpdatedDomainEvent>());
        });
    }
    
    [Test]
    public void UpdateDetails_WithDifferentQuestionAndAnswer_ShouldUpdateDetails_AndRaiseDomainEvent()
    {
        var timeProvider = TimeProvider.System;
        
        var faq = FaqModel.Create(Guid.NewGuid(), ValidQuestion, ValidAnswer, timeProvider);
        
        var newQuestion = "Test Faq Question 2";
        var newAnswer = "Test Faq Answer 2";
        
        faq.UpdateDetails(newQuestion, newAnswer);
        
        Assert.Multiple(() =>
        {
            Assert.That(faq.Question.QuestionValue, Is.EqualTo(newQuestion));
            Assert.That(faq.Answer.AnswerValue, Is.EqualTo(newAnswer));
            Assert.That(faq.GetDomainEvents(), Has.Some.TypeOf<FaqDetailsUpdatedDomainEvent>());
        });
    }
    
    [TestCase(ValidQuestion, ValidAnswer)]
    [TestCase(ValidQuestion, null)]
    [TestCase(null, ValidAnswer)]
    [TestCase(null, null)]   
    public void UpdateDetails_WithSameData_ShouldUpdateDetails_AndNotRaiseDomainEvent(string? name, string? description)
    {
        var timeProvider = TimeProvider.System;
        
        var faq = FaqModel.Create(Guid.NewGuid(), ValidQuestion, ValidAnswer, timeProvider);
        
        faq.UpdateDetails(name, description);
        
        Assert.Multiple(() =>
        {
            Assert.That(faq.Question.QuestionValue, Is.EqualTo(ValidQuestion));
            Assert.That(faq.Answer.AnswerValue, Is.EqualTo(ValidAnswer));
            Assert.That(faq.GetDomainEvents(), Has.None.TypeOf<FaqDetailsUpdatedDomainEvent>());
        });
    }
}
using SupportDesk.Domain.Models.TemplateAnswer.Events;
using TemplateAnswerModel = SupportDesk.Domain.Models.TemplateAnswer.TemplateAnswer;

namespace SupportDesk.UnitTests.Domain.Models.TemplateAnswer;

[TestFixture]
internal sealed partial class TemplateAnswerTests
{
    [Test]
    public void UpdateDetails_WithDifferentTitle_ShouldUpdateDetails_AndRaiseDomainEvent()
    {
        var timeProvider = TimeProvider.System;
        
        var templateAnswer = TemplateAnswerModel.Create(Guid.NewGuid(), ValidTitle, ValidText, timeProvider);
        
        var newTitle = "Test Title 2";
        
        templateAnswer.UpdateDetails(newTitle, null);
        
        Assert.Multiple(() =>
        {
            Assert.That(templateAnswer.Title.TitleValue, Is.EqualTo(newTitle));
            Assert.That(templateAnswer.Text.TextValue, Is.EqualTo(ValidText));
            Assert.That(templateAnswer.GetDomainEvents(), Has.Some.TypeOf<TemplateAnswerDetailsUpdatedDomainEvent>());
        });
    }
    
    [Test]
    public void UpdateDetails_WithDifferentText_ShouldUpdateDetails_AndRaiseDomainEvent()
    {
        var timeProvider = TimeProvider.System;
        
        var templateAnswer = TemplateAnswerModel.Create(Guid.NewGuid(), ValidTitle, ValidText, timeProvider);
        
        var newText = "Test Text 2";
        
        templateAnswer.UpdateDetails(null, newText);
        
        Assert.Multiple(() =>
        {
            Assert.That(templateAnswer.Title.TitleValue, Is.EqualTo(ValidTitle));
            Assert.That(templateAnswer.Text.TextValue, Is.EqualTo(newText));
            Assert.That(templateAnswer.GetDomainEvents(), Has.Some.TypeOf<TemplateAnswerDetailsUpdatedDomainEvent>());
        });
    }
    
    [Test]
    public void UpdateDetails_WithDifferentTitleAndText_ShouldUpdateDetails_AndRaiseDomainEvent()
    {
        var timeProvider = TimeProvider.System;
        
        var templateAnswer = TemplateAnswerModel.Create(Guid.NewGuid(), ValidTitle, ValidText, timeProvider);
        
        var newTitle = "Test Title 2";
        var newText = "Test Text 2";
        
        templateAnswer.UpdateDetails(newTitle, newText);
        
        Assert.Multiple(() =>
        {
            Assert.That(templateAnswer.Title.TitleValue, Is.EqualTo(newTitle));
            Assert.That(templateAnswer.Text.TextValue, Is.EqualTo(newText));
            Assert.That(templateAnswer.GetDomainEvents(), Has.Some.TypeOf<TemplateAnswerDetailsUpdatedDomainEvent>());
        });
    }
    
    [TestCase(ValidTitle, ValidText)]
    [TestCase(ValidTitle, null)]
    [TestCase(null, ValidText)]
    [TestCase(null, null)]   
    public void UpdateDetails_WithSameData_ShouldUpdateDetails_AndNotRaiseDomainEvent(string? name, string? description)
    {
        var timeProvider = TimeProvider.System;
        
        var templateAnswer = TemplateAnswerModel.Create(Guid.NewGuid(), ValidTitle, ValidText, timeProvider);
        
        templateAnswer.UpdateDetails(name, description);
        
        Assert.Multiple(() =>
        {
            Assert.That(templateAnswer.Title.TitleValue, Is.EqualTo(ValidTitle));
            Assert.That(templateAnswer.Text.TextValue, Is.EqualTo(ValidText));
            Assert.That(templateAnswer.GetDomainEvents(), Has.None.TypeOf<TemplateAnswerDeletedDomainEvent>());
        });
    }
}
using SupportDesk.Domain.Models.TemplateAnswer.Events;
using TemplateAnswerModel = SupportDesk.Domain.Models.TemplateAnswer.TemplateAnswer;

namespace SupportDesk.UnitTests.Domain.Models.TemplateAnswer;

[TestFixture]
internal sealed class TemplateAnswerTests
{
    [TestCase("Test title", "Test text")]
    public void Create_WithValidData_ShouldCreateTemplateAnswer(string validTitle, string validText)
    {
        var organizationId = Guid.NewGuid();

        var templateAnswer = TemplateAnswerModel.Create(organizationId, validTitle, validText);

        Assert.Multiple(() =>
        {
            Assert.That(templateAnswer.Id.IdValue, Is.Not.EqualTo(Guid.Empty));
            Assert.That(templateAnswer.OrganizationId.IdValue, Is.EqualTo(organizationId));
            Assert.That(templateAnswer.Title.TitleValue, Is.EqualTo(validTitle));
            Assert.That(templateAnswer.Text.TextValue, Is.EqualTo(validText));
            Assert.That(templateAnswer.GetDomainEvents(), Has.Some.TypeOf<TemplateAnswerCreatedDomainEvent>());
        });
    }
}
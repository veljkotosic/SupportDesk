using FaqModel = SupportDesk.Domain.Models.Faq.Faq;

namespace SupportDesk.UnitTests.Domain.Models.Faq;

[TestFixture]
internal sealed class FaqTests
{
    [TestCase("Test Faq Question", "Test Faq Answer")]
    public void Create_WithValidData_ShouldCreateFaq(string validQuestion, string validAnswer)
    {
        var organizationId = Guid.NewGuid();

        var faq = FaqModel.Create(organizationId, validQuestion, validAnswer);

        Assert.Multiple(() =>
        {
            Assert.That(faq.Id.IdValue, Is.Not.EqualTo(Guid.Empty));
            Assert.That(faq.OrganizationId.IdValue, Is.EqualTo(organizationId));
            Assert.That(faq.Question.QuestionValue, Is.EqualTo(validQuestion));
            Assert.That(faq.Answer.AnswerValue, Is.EqualTo(validAnswer));
        });
    }
}
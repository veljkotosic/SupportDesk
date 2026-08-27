using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.TemplateAnswer.Options;
using SupportDesk.Domain.Models.TemplateAnswer.ValueObjects;
using SupportDesk.TestsUtility;

namespace SupportDesk.UnitTests.Domain.Models.TemplateAnswer.ValueObjects;

[TestFixture]
internal sealed class TemplateAnswerTextTests
{
    [TestCase("Test text")]
    public void Constructor_WithValidText_ShouldCreateText(string validText)
    {
        Assert.DoesNotThrow(() => _ = new TemplateAnswerText(validText));
    }

    [Test]
    public void Constructor_WithTextTooLong_ShouldBreakTextMaxLengthRule()
    {
        var textTooLong = string.Join("", Enumerable.Repeat("a", TemplateAnswerOptionsDefaults.TextMaximumLength + 1));

        var exception = Assert.Throws<ValidationException>(() => _ = new TemplateAnswerText(textTooLong));

        AssertUtility.AssertHasBrokenExactRule<MaxLengthRule>(exception);
    }

    [TestCase("")]
    public void Constructor_WithTextTooShort_ShouldBreakTextMinLengthRule(string textTooShort)
    {
        var exception = Assert.Throws<ValidationException>(() => _ = new TemplateAnswerText(textTooShort));

        AssertUtility.AssertHasBrokenExactRule<MinLengthRule>(exception);
    }
}
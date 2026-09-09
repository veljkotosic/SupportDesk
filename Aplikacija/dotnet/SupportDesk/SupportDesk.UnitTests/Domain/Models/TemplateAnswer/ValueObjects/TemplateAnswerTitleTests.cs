using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.TemplateAnswer.Options;
using SupportDesk.Domain.Models.TemplateAnswer.ValueObjects;
using SupportDesk.TestsUtility;

namespace SupportDesk.UnitTests.Domain.Models.TemplateAnswer.ValueObjects;

[TestFixture]
internal sealed class TemplateAnswerTitleTests
{
    [TestCase("Test title")]
    public void Constructor_WithValidTitle_ShouldCreateTitle(string validTitle)
    {
        Assert.DoesNotThrow(() => _ = new TemplateAnswerTitle(validTitle));
    }

    [Test]
    public void Constructor_WithTitleTooLong_ShouldBreakTitleMaxLengthRule()
    {
        var titleTooLong = string.Join("", Enumerable.Repeat("a", TemplateAnswerOptionsDefaults.TitleMaximumLength + 1));

        var exception = Assert.Throws<ValidationException>(() => _ = new TemplateAnswerTitle(titleTooLong));

        AssertUtility.AssertHasBrokenExactRule<MaxLengthRule>(exception);
    }

    [TestCase("")]
    public void Constructor_WithTitleTooShort_ShouldBreakTitleMinLengthRule(string titleTooShort)
    {
        var exception = Assert.Throws<ValidationException>(() => _ = new TemplateAnswerTitle(titleTooShort));

        AssertUtility.AssertHasBrokenExactRule<MinLengthRule>(exception);
    }
}
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Faq.Options;
using SupportDesk.Domain.Models.Faq.ValueObjects;
using SupportDesk.TestsUtility;

namespace SupportDesk.UnitTests.Domain.Models.Faq.ValueObjects;

[TestFixture]
internal sealed class FaqQuestionTests
{
    [TestCase("Test question")]
    public void Constructor_WithValidQuestion_ShouldCreateQuestion(string validQuestion)
    {
        Assert.DoesNotThrow(() => _ = new FaqQuestion(validQuestion));
    }

    [Test]
    public void Constructor_WithQuestionTooLong_ShouldBreakQuestionMaxLengthRule()
    {
        var questionTooLong = string.Join("", Enumerable.Repeat("a", FaqOptionsDefaults.QuestionMaximumLength + 1));

        var exception = Assert.Throws<ValidationException>(() => _ = new FaqQuestion(questionTooLong));

        AssertUtility.AssertHasBrokenExactRule<MaxLengthRule>(exception);
    }

    [TestCase("")]
    public void Constructor_WithQuestionTooShort_ShouldBreakQuestionMinLengthRule(string questionTooShort)
    {
        var exception = Assert.Throws<ValidationException>(() => _ = new FaqQuestion(questionTooShort));

        AssertUtility.AssertHasBrokenExactRule<MinLengthRule>(exception);
    }
}
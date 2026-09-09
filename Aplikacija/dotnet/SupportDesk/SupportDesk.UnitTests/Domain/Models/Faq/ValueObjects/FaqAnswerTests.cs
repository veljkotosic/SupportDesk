using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Faq.Options;
using SupportDesk.Domain.Models.Faq.ValueObjects;
using SupportDesk.TestsUtility;

namespace SupportDesk.UnitTests.Domain.Models.Faq.ValueObjects;

[TestFixture]
internal sealed class FaqAnswerTests
{
    [TestCase("Test answer")]
    public void Constructor_WithValidAnswer_ShouldCreateAnswer(string validAnswer)
    {
        Assert.DoesNotThrow(() => _ = new FaqAnswer(validAnswer));
    }

    [Test]
    public void Constructor_WithAnswerTooLong_ShouldBreakAnswerMaxLengthRule()
    {
        var answerTooLong = string.Join("", Enumerable.Repeat("a", FaqOptionsDefaults.AnswerMaximumLength + 1));

        var exception = Assert.Throws<ValidationException>(() => _ = new FaqAnswer(answerTooLong));

        AssertUtility.AssertHasBrokenExactRule<MaxLengthRule>(exception);
    }

    [TestCase("")]
    public void Constructor_WithAnswerTooShort_ShouldBreakAnswerMinLengthRule(string answerTooShort)
    {
        var exception = Assert.Throws<ValidationException>(() => _ = new FaqAnswer(answerTooShort));

        AssertUtility.AssertHasBrokenExactRule<MinLengthRule>(exception);
    }
}
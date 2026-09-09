using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Message.Options;
using SupportDesk.Domain.Models.Message.ValueObjects;
using SupportDesk.TestsUtility;

namespace SupportDesk.UnitTests.Domain.Models.Message.ValueObjects;

[TestFixture]
internal sealed class MessageTextTests
{
    [TestCase("Test support message.")]
    public void Constructor_WithValidText_ShouldCreateText(string validText)
    {
        Assert.DoesNotThrow(() => _ = new MessageText(validText));
    }

    [Test]
    public void Constructor_WithTextTooLong_ShouldBreakTextMaxLengthRule()
    {
        var textTooLong = string.Join("", Enumerable.Repeat("a", MessageOptionsDefaults.TextMaximumLength + 1));

        var exception = Assert.Throws<ValidationException>(() => _ = new MessageText(textTooLong));

        AssertUtility.AssertHasBrokenExactRule<MaxLengthRule>(exception);
    }

    [TestCase("")]
    public void Constructor_WithTextTooShort_ShouldBreakTextMinLengthRule(string textTooShort)
    {
        var exception = Assert.Throws<ValidationException>(() => _ = new MessageText(textTooShort));

        AssertUtility.AssertHasBrokenExactRule<MinLengthRule>(exception);
    }
}
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Note.Options;
using SupportDesk.Domain.Models.Note.ValueObjects;
using SupportDesk.TestsUtility;

namespace SupportDesk.UnitTests.Domain.Models.Note.ValueObjects;

[TestFixture]
internal sealed class NoteTextTests
{
    [TestCase("Test note.")]
    public void Constructor_WithValidText_ShouldCreateText(string validText)
    {
        Assert.DoesNotThrow(() => _ = new NoteText(validText));
    }

    [Test]
    public void Constructor_WithTextTooLong_ShouldBreakTextMaxLengthRule()
    {
        var textTooLong = string.Join("", Enumerable.Repeat("a", NoteOptionsDefaults.TextMaximumLength + 1));

        var exception = Assert.Throws<ValidationException>(() => _ = new NoteText(textTooLong));

        AssertUtility.AssertHasBrokenExactRule<MaxLengthRule>(exception);
    }

    [TestCase("")]
    public void Constructor_WithTextTooShort_ShouldBreakTextMinLengthRule(string textTooShort)
    {
        var exception = Assert.Throws<ValidationException>(() => _ = new NoteText(textTooShort));

        AssertUtility.AssertHasBrokenExactRule<MinLengthRule>(exception);
    }
}
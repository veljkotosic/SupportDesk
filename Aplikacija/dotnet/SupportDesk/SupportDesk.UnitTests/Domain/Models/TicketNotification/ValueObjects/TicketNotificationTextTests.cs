using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.TicketNotification.Options;
using SupportDesk.Domain.Models.TicketNotification.ValueObjects;
using SupportDesk.TestsUtility;

namespace SupportDesk.UnitTests.Domain.Models.TicketNotification.ValueObjects;

[TestFixture]
internal sealed class TicketNotificationTextTests
{
    [TestCase("Test text")]
    public void Constructor_WithValidText_ShouldCreateText(string validText)
    {
        Assert.DoesNotThrow(() => _ = new TicketNotificationText(validText));
    }

    [Test]
    public void Constructor_WithTextTooLong_ShouldBreakTextMaxLengthRule()
    {
        var textTooLong = string.Join("", Enumerable.Repeat("a", TicketNotificationOptionsDefaults.TextMaximumLength + 1));

        var exception = Assert.Throws<ValidationException>(() => _ = new TicketNotificationText(textTooLong));

        AssertUtility.AssertHasBrokenExactRule<MaxLengthRule>(exception);
    }
}
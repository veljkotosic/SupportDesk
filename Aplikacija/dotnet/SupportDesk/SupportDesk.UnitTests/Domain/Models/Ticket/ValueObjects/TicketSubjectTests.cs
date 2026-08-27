using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Ticket.Options;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.TestsUtility;

namespace SupportDesk.UnitTests.Domain.Models.Ticket.ValueObjects;

[TestFixture]
internal sealed class TicketSubjectTests
{
    [TestCase("Test Subject")]
    public void Constructor_WithValidSubject_ShouldCreateSubject(string validSubject)
    {
        Assert.DoesNotThrow(() => _ = new TicketSubject(validSubject));
    }

    [Test]
    public void Constructor_WithSubjectTooLong_ShouldBreakSubjectMaxLengthRule()
    {
        var subjectTooLong = string.Join("", Enumerable.Repeat("a", TicketOptionsDefaults.SubjectMaximumLength + 1));

        var exception = Assert.Throws<ValidationException>(() => _ = new TicketSubject(subjectTooLong));

        AssertUtility.AssertHasBrokenExactRule<MaxLengthRule>(exception);
    }

    [TestCase("")]
    public void Constructor_WithSubjectTooShort_ShouldBreakSubjectMinLengthRule(string subjectTooShort)
    {
        var exception = Assert.Throws<ValidationException>(() => _ = new TicketSubject(subjectTooShort));

        AssertUtility.AssertHasBrokenExactRule<MinLengthRule>(exception);
    }
}
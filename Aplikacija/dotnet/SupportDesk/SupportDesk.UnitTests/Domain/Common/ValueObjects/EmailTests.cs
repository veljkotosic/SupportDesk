using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.TestsUtility;

namespace SupportDesk.UnitTests.Domain.Common.ValueObjects;

[TestFixture]
internal sealed class EmailTests
{   
    [TestCase("test@test.test")]
    public void Constructor_WithValidEmail_ShouldCreateEmail(string validEmail)
    {
        Assert.DoesNotThrow(() => _ = new Email(validEmail));
    }

    [TestCase("")]
    [TestCase("invalid-email")]
    [TestCase("test@")]
    [TestCase("@example.com")]
    public void Constructor_WithInvalidEmail_ShouldBreakEmailRule(string invalidEmail)
    {
        var exception = Assert.Throws<ValidationException>(() => _ = new Email(invalidEmail));
        
        AssertUtility.AssertHasBrokenExactRule<EmailFormatRule>(exception);
    }
}
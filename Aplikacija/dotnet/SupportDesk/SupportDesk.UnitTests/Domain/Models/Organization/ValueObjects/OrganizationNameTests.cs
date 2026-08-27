using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Organization.Options;
using SupportDesk.Domain.Models.Organization.Validation.Rules;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.TestsUtility;

namespace SupportDesk.UnitTests.Domain.Models.Organization.ValueObjects;

[TestFixture]
internal sealed class OrganizationNameTests
{
    [TestCase("A.T.R. Doo")]
    public void Constructor_WithValidName_ShouldCreateName(string validName)
    {
        Assert.DoesNotThrow(() => _ = new OrganizationName(validName));
    }

    [Test]
    public void Constructor_WithNameTooLong_ShouldBreakOrganizationNameMaxLengthRule()
    {
        var nameTooLong = string.Join("", Enumerable.Repeat("a", OrganizationOptionsDefaults.NameMaximumLength + 1));

        var exception = Assert.Throws<ValidationException>(() => _ = new OrganizationName(nameTooLong));

        AssertUtility.AssertHasBrokenExactRule<MaxLengthRule>(exception);
    }

    [TestCase("")]
    public void Constructor_WithNameTooShort_ShouldBreakOrganizationNameMinLengthRule(string nameTooShort)
    {
        var exception = Assert.Throws<ValidationException>(() => _ = new OrganizationName(nameTooShort));

        AssertUtility.AssertHasBrokenExactRule<MinLengthRule>(exception);
    }

    [TestCase("A.T.R. Doo\t")]
    [TestCase("A.T.R. Doo\n")]
    [TestCase("A.T.R. Doo😊")]
    [TestCase("A.T.R. Doo€")]
    [TestCase("A.T.R. Doočćž")]
    [TestCase("A.T.R. Doo©")]
    public void Constructor_WithNameWithInvalidCharacters_ShouldBreakOrganizationNameCharsetRule(string invalidName)
    {
        var exception = Assert.Throws<ValidationException>(() => _ = new OrganizationName(invalidName));

        AssertUtility.AssertHasBrokenExactRule<OrganizationNameCharsetRule>(exception);
    }
}
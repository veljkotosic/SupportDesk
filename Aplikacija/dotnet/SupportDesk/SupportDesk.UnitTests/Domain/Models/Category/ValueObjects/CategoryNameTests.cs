using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Category.Options;
using SupportDesk.Domain.Models.Category.ValueObjects;
using SupportDesk.TestsUtility;

namespace SupportDesk.UnitTests.Domain.Models.Category.ValueObjects;

[TestFixture]
internal sealed class CategoryNameTests
{
    [TestCase("Test-123_456")]
    public void Constructor_WithValidName_ShouldCreateName(string validName)
    {
        Assert.DoesNotThrow(() => _ = new CategoryName(validName));
    }

    [Test]  
    public void Constructor_WithNameTooLong_ShouldBreakCategoryNameMaxLengthRule()
    {
        var nameTooLong = string.Join("", Enumerable.Repeat("a", CategoryOptionsDefaults.NameMaximumLength + 1));

        var exception = Assert.Throws<ValidationException>(() => _ = new CategoryName(nameTooLong));
        
        AssertUtility.AssertHasBrokenExactRule<MaxLengthRule>(exception);
    }
    
    [TestCase("")]  
    public void Constructor_WithNameTooShort_ShouldBreakCategoryNameMinLengthRule(string nameTooShort)
    {
        var exception = Assert.Throws<ValidationException>(() => _ = new CategoryName(nameTooShort));
        
        AssertUtility.AssertHasBrokenExactRule<MinLengthRule>(exception);
    }
}
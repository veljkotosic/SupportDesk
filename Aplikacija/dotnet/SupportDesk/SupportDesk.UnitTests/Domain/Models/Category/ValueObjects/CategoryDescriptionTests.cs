using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Category.Options;
using SupportDesk.Domain.Models.Category.Validation.Rules;
using SupportDesk.Domain.Models.Category.ValueObjects;
using SupportDesk.TestsUtility;

namespace SupportDesk.UnitTests.Domain.Models.Category.ValueObjects;

[TestFixture]
internal sealed class CategoryDescriptionTests
{
    [TestCase("Test Description")]
    [TestCase("aA _1")]
    public void Constructor_WithValidDescription_ShouldCreateDescription(string validDescription)
    {
        Assert.DoesNotThrow(() => _ = new CategoryDescription(validDescription));
    }

    [Test]
    public void Constructor_WithDescriptionTooLong_ShouldBreakDescriptionMaxLengthRule()
    {
        var descriptionTooLong = string.Join("", Enumerable.Repeat("a", CategoryOptionsDefaults.DescriptionMaximumLength + 1));

        var exception = Assert.Throws<ValidationException>(() => _ = new CategoryDescription(descriptionTooLong));
        
        AssertUtility.AssertHasBrokenExactRule<MaxLengthRule>(exception);
    }
    
    [TestCase("Test Description \t")]           
    [TestCase("Test Description \n")]           
    [TestCase("Test Description 😊")]            
    [TestCase("Test Description €")]            
    [TestCase("Test Description čćž")]          
    [TestCase("Test Description ©")]
    public void Constructor_WithDescriptionWithInvalidCharacters_ShouldBreakDescriptionCharsetRule(string invalidDescription)
    {
        var exception = Assert.Throws<ValidationException>(() => _ = new CategoryDescription(invalidDescription));
        
        AssertUtility.AssertHasBrokenExactRule<CategoryDescriptionCharsetRule>(exception);
    }
}
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Category.Validation;
using SupportDesk.TestsUtility;
using CategoryModel = SupportDesk.Domain.Models.Category.Category;

namespace SupportDesk.UnitTests.Domain.Models.Category;

[TestFixture]
internal sealed class CategoryTests
{
    [TestCase("Test Category", "Test Description")]
    public void Create_WithValidData_ShouldCreateCategory(string validName, string validDescription)
    {
        var timeProvider = TimeProvider.System;
        
        var category = CategoryModel.Create(Guid.NewGuid(), validName, validDescription, timeProvider);
        
        Assert.Multiple(() =>
        {
            Assert.That(category.Id.IdValue, Is.Not.EqualTo(Guid.Empty));
            Assert.That(category.Name.NameValue, Is.EqualTo(validName));
            Assert.That(category.Description.DescriptionValue, Is.EqualTo(validDescription)); 
            Assert.That(category.CreatedAt.CreatedAtValue, Is.Not.EqualTo(default(DateTime)));
            Assert.That(category.DeletedAt, Is.Null);
        });
    }

    [Test]
    public void Delete_WithValidData_ShouldMarkCategoryAsDeleted()
    {
        var timeProvider = TimeProvider.System;
        
        var category = CategoryModel.Create(Guid.NewGuid(), "Test Category", "Test Description", timeProvider);
        
        category.Delete(timeProvider);
        
        Assert.That(category.DeletedAt, Is.Not.Null);
    }

    [Test]
    public void Delete_WithCategoryAlreadyDeleted_ShouldThrowValidationException()
    {
        var timeProvider = TimeProvider.System;
        
        var category = CategoryModel.Create(
            Guid.NewGuid(),
            "Test Category",
            "Test Description",
            timeProvider);
        
        category.Delete(timeProvider);

        var exception = Assert.Throws<ValidationException>(() => category.Delete(timeProvider));
        
        AssertUtility.AssertHasProducedExactError(exception, CategoryErrors.AlreadyDeleted(category.Name));
    }
}
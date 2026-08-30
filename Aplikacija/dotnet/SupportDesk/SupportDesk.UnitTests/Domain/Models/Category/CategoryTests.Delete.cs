using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Category.Validation;
using SupportDesk.TestsUtility;
using CategoryModel = SupportDesk.Domain.Models.Category.Category;

namespace SupportDesk.UnitTests.Domain.Models.Category;

[TestFixture]
internal sealed partial class CategoryTests
{
    [Test]
    public void Delete_WithValidData_ShouldMarkCategoryAsDeleted()
    {
        var timeProvider = TimeProvider.System;
        
        var category = CategoryModel.Create(Guid.NewGuid(), ValidName, ValidDescription, timeProvider);
        
        category.Delete(timeProvider);
        
        Assert.That(category.DeletedAt, Is.Not.Null);
    }

    [Test]
    public void Delete_WithCategoryAlreadyDeleted_ShouldThrowValidationException()
    {
        var timeProvider = TimeProvider.System;
        
        var category = CategoryModel.Create(Guid.NewGuid(), ValidName, ValidDescription, timeProvider);
        
        category.Delete(timeProvider);

        var exception = Assert.Throws<ValidationException>(() => category.Delete(timeProvider));
        
        AssertUtility.AssertHasProducedExactError(exception, CategoryErrors.AlreadyDeleted(category.Name));
    }
}
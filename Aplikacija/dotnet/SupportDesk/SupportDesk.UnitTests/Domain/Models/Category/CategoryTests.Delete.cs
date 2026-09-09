using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Category.Events;
using SupportDesk.Domain.Models.Category.Validation;
using SupportDesk.TestsUtility;
using CategoryModel = SupportDesk.Domain.Models.Category.Category;

namespace SupportDesk.UnitTests.Domain.Models.Category;

[TestFixture]
internal sealed partial class CategoryTests
{
    [Test]
    public void Delete_WithValidData_ShouldMarkCategoryAsDeleted_AndRaiseDomainEvent()
    {
        var timeProvider = TimeProvider.System;
        
        var category = CategoryModel.Create(Guid.NewGuid(), ValidName, ValidDescription, timeProvider);
        
        category.Delete(timeProvider);
        
        Assert.That(category.DeletedAt, Is.Not.Null);
        Assert.That(category.GetDomainEvents(), Has.Some.TypeOf<CategoryDeletedDomainEvent>());
    }

    [Test]
    public void Delete_WithCategoryAlreadyDeleted_ShouldProduceAlreadyDeletedError()
    {
        var timeProvider = TimeProvider.System;
        
        var category = CategoryModel.Create(Guid.NewGuid(), ValidName, ValidDescription, timeProvider);
        
        category.Delete(timeProvider);

        var exception = Assert.Throws<ValidationException>(() => category.Delete(timeProvider));
        
        AssertUtility.AssertHasProducedExactError(exception, CategoryErrors.AlreadyDeleted(category.Name));
    }
}
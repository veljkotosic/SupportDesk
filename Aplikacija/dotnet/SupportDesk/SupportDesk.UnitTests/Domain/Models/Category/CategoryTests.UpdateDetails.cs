using SupportDesk.Domain.Models.Category.Events;
using CategoryModel = SupportDesk.Domain.Models.Category.Category;

namespace SupportDesk.UnitTests.Domain.Models.Category;

[TestFixture]
internal sealed partial class CategoryTests
{
    [Test]
    public void UpdateDetails_WithDifferentName_ShouldUpdateDetails_AndRaiseDomainEvent()
    {
        var timeProvider = TimeProvider.System;
        
        var category = CategoryModel.Create(Guid.NewGuid(), ValidName, ValidDescription, timeProvider);
        
        var newName = "Test Category 2";
        
        category.UpdateDetails(newName, null);
        
        Assert.Multiple(() =>
        {
            Assert.That(category.Name.NameValue, Is.EqualTo(newName));
            Assert.That(category.Description.DescriptionValue, Is.EqualTo(ValidDescription));
            Assert.That(category.GetDomainEvents(), Has.Some.TypeOf<CategoryDetailsUpdatedDomainEvent>());
        });
    }
    
    [Test]
    public void UpdateDetails_WithDifferentDescription_ShouldUpdateDetails_AndRaiseDomainEvent()
    {
        var timeProvider = TimeProvider.System;
        
        var category = CategoryModel.Create(Guid.NewGuid(), ValidName, ValidDescription, timeProvider);
        
        var newDescription = "Test Description 2";
        
        category.UpdateDetails(null, newDescription);
        
        Assert.Multiple(() =>
        {
            Assert.That(category.Name.NameValue, Is.EqualTo(ValidName));
            Assert.That(category.Description.DescriptionValue, Is.EqualTo(newDescription));
            Assert.That(category.GetDomainEvents(), Has.Some.TypeOf<CategoryDetailsUpdatedDomainEvent>());
        });
    }
    
    [Test]
    public void UpdateDetails_WithDifferentNameAndDescription_ShouldUpdateDetails_AndRaiseDomainEvent()
    {
        var timeProvider = TimeProvider.System;
        
        var category = CategoryModel.Create(Guid.NewGuid(), ValidName, ValidDescription, timeProvider);
        
        var newName = "Test Category 2";
        var newDescription = "Test Description 2";
        
        category.UpdateDetails(newName, newDescription);
        
        Assert.Multiple(() =>
        {
            Assert.That(category.Name.NameValue, Is.EqualTo(newName));
            Assert.That(category.Description.DescriptionValue, Is.EqualTo(newDescription));
            Assert.That(category.GetDomainEvents(), Has.Some.TypeOf<CategoryDetailsUpdatedDomainEvent>());
        });
    }
    
    [TestCase(ValidName, ValidDescription)]
    [TestCase(ValidName, null)]
    [TestCase(null, ValidDescription)]
    [TestCase(null, null)]   
    public void UpdateDetails_WithSameData_ShouldUpdateDetails_AndNotRaiseDomainEvent(string? name, string? description)
    {
        var timeProvider = TimeProvider.System;
        
        var category = CategoryModel.Create(Guid.NewGuid(), ValidName, ValidDescription, timeProvider);
        
        category.UpdateDetails(name, description);
        
        Assert.Multiple(() =>
        {
            Assert.That(category.Name.NameValue, Is.EqualTo(ValidName));
            Assert.That(category.Description.DescriptionValue, Is.EqualTo(ValidDescription));
            Assert.That(category.GetDomainEvents(), Has.None.TypeOf<CategoryDetailsUpdatedDomainEvent>());
        });
    }
}
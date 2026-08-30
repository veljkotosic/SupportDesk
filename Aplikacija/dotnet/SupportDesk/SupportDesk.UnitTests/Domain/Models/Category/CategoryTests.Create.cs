using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Category.Events;
using CategoryModel = SupportDesk.Domain.Models.Category.Category;

namespace SupportDesk.UnitTests.Domain.Models.Category;

[TestFixture]
internal sealed partial class CategoryTests
{
    private const string ValidName = "Test Category";
    private const string ValidDescription = "Test Description";
    
    [TestCase(ValidName, ValidDescription)]
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
            Assert.That(category.GetDomainEvents(), Has.Some.TypeOf<CategoryCreatedDomainEvent>());
        });
    }

    [TestCase("", "")]
    public void Create_WithInvalidData_ShouldThrowValidationException(string invalidName, string invalidDescription)
    {
        var timeProvider = TimeProvider.System;

        Assert.Throws<ValidationException>(() =>
        {
            _ = CategoryModel.Create(Guid.NewGuid(), invalidName, invalidDescription, timeProvider);
        });
    }
}
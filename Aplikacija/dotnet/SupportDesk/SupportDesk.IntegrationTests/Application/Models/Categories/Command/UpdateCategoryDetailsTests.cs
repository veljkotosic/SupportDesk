using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Application.Models.Categories.Command.UpdateCategoryDetails;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Category.Validation;
using SupportDesk.Domain.Models.Category.Validation.Rules;
using SupportDesk.Domain.Models.Category.ValueObjects;
using SupportDesk.Domain.Models.User;
using SupportDesk.TestsUtility;

namespace SupportDesk.IntegrationTests.Application.Models.Categories.Command;

[TestFixture]
internal sealed class UpdateCategoryDetailsTests : IntegrationTestsBase
{
    private User _user;
    
    [SetUp]
    public async Task Setup()
    {
        _user = await RegisterOrganizationAdmin();
        
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(_user.Id.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(_user.OrganizationId!.IdValue);
    }

    [Test]
    public async Task Handle_WithPermissions_WithValidData_ShouldUpdateCategoryDetails()
    {
        var category = await CreateCategory(_user.OrganizationId!.IdValue, "Test Category", "Test Description", TimeProvider.System);
            
        var newName = "Test Category 2";
        var newDescription = "Test Description 2";
        
        Assert.DoesNotThrowAsync(async () =>
        {
            await CommandDispatcher.DispatchAsync(new UpdateCategoryDetailsCommand(
                category.Id.IdValue,
                newName, 
                newDescription));
        });
        
        var persistedCategory = await DbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == category.Id);
        
        Assert.That(persistedCategory, Is.Not.Null);
        Assert.That(persistedCategory.Name.NameValue, Is.EqualTo(newName));
        Assert.That(persistedCategory.Description.DescriptionValue, Is.EqualTo(newDescription)); 
    }
    
    [Test]
    public async Task Handle_WithPermissions_WithSameData_ShouldNotChange()
    {
        var name = "Test Category";
        var description = "Test Description";
        
        var category = await CreateCategory(_user.OrganizationId!.IdValue, name, description, TimeProvider.System);
            
        var newName = "Test Category";
        var newDescription = "Test Description";
        
        Assert.DoesNotThrowAsync(async () =>
        {
            await CommandDispatcher.DispatchAsync(new UpdateCategoryDetailsCommand(
                category.Id.IdValue,
                newName, 
                newDescription));
        });
        
        var persistedCategory = await DbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == category.Id);
        
        Assert.That(persistedCategory, Is.Not.Null);
        Assert.That(persistedCategory.Name.NameValue, Is.EqualTo(name));
        Assert.That(persistedCategory.Description.DescriptionValue, Is.EqualTo(description));       
    }

    [Test]
    public async Task Handle_WithPermissions_WithExistingName_ShouldBreakCannotUpdateCategoryNameWithExistingOneRule()
    {
        var name = "Test Category";
        var description = "Test Description";
        
        var category = await CreateCategory(_user.OrganizationId!.IdValue, name, description, TimeProvider.System);
            
        var otherName = "Test Category 2";
        var otherDescription = "Test Description 2";
        
        _ = await CreateCategory(_user.OrganizationId!.IdValue, otherName, otherDescription, TimeProvider.System);

        var exception = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new UpdateCategoryDetailsCommand(
                category.Id.IdValue,
                otherName,
                otherDescription));
        });
        
        AssertUtility.AssertHasBrokenExactRule<CannotUpdateCategoryNameWithExistingOneRule>(exception);
    }
    
    [Test]
    public async Task Handle_WithInvalidCategoryId_ShouldThrowProduceCategoryNotFoundError()
    {
        var categoryId = Guid.NewGuid();
        
        var exception = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new UpdateCategoryDetailsCommand(categoryId, "Test", "Test"));
        });
        
        AssertUtility.AssertHasProducedExactError(exception, CategoryErrors.NotFound(new CategoryId(categoryId)));
    }
    
    [Test]
    public async Task Handle_WithoutPermissions_ShouldThrowPermissionException()
    {
        await PermissionService.RevokePermissionAsync(_user.Id.IdValue, Permissions.Categories.Update);
        
        Assert.ThrowsAsync<PermissionException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new UpdateCategoryDetailsCommand(Guid.NewGuid(), "Test", "Test"));
        });
    }
}
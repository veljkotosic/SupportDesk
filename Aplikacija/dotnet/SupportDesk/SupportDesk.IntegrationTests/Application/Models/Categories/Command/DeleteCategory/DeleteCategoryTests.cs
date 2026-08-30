using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Application.Models.Categories.Command.DeleteCategory;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Category.Validation;
using SupportDesk.Domain.Models.Category.Validation.Rules;
using SupportDesk.Domain.Models.Category.ValueObjects;
using SupportDesk.Domain.Models.User;
using SupportDesk.TestsUtility;

namespace SupportDesk.IntegrationTests.Application.Models.Categories.Command.DeleteCategory;

[TestFixture]
internal sealed class DeleteCategoryTests : IntegrationTestsBase
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
    public async Task Handle_WithPermissions_WithValidCategory_ShouldDeleteCategory()
    {
        var addedCategory = await CreateCategory(_user.OrganizationId!.IdValue, "Test Category", "Test Description", TimeProvider.System);
        
        Assert.DoesNotThrowAsync(async () => {
            await CommandDispatcher.DispatchAsync(new DeleteCategoryCommand(addedCategory.Id.IdValue));
        });

        var persistedCategory = await DbContext.Categories
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == addedCategory.Id);
        
        Assert.That(persistedCategory, Is.Not.Null);
        Assert.That(persistedCategory.DeletedAt, Is.Not.Null);
    }

    [Test]
    public async Task Handle_WithPermissions_WithUnexistingCategory_ShouldBreakDomainModelExistsRule()
    {
        var categoryId = new CategoryId(Guid.NewGuid());
        
        var exception = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new DeleteCategoryCommand(categoryId.IdValue));
        });
        
        AssertUtility.AssertHasProducedExactError(exception, CategoryErrors.NotFound(categoryId));
    }

    [Test]
    public async Task Handle_WithPermissions_WithDeletedCategory_ShouldBreakDomainModelExistsRule()
    {
        var addedCategory = await CreateCategory(_user.OrganizationId!.IdValue, "Test Category", "Test Description", TimeProvider.System);
        addedCategory.Delete(TimeProvider.System);
        await DbContext.SaveChangesAsync(); 
        DbContext.ChangeTracker.Clear();
        
        var exception = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new DeleteCategoryCommand(addedCategory.Id.IdValue));
        });
        
        AssertUtility.AssertHasProducedExactError(exception, CategoryErrors.NotFound(addedCategory.Id));
    }

    [Test]
    public async Task Handle_WithoutPermissions_ShouldThrowPermissionException()
    {
        await PermissionService.RevokePermissionAsync(_user.Id.IdValue, Permissions.Categories.Delete);

        Assert.ThrowsAsync<PermissionException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new DeleteCategoryCommand(Guid.NewGuid()));
        });
    }
}
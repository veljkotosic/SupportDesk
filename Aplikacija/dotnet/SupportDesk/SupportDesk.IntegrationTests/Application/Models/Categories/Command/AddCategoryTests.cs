using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Application.Models.Categories.Command.AddCategory;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Category.Validation.Rules;
using SupportDesk.Domain.Models.Category.ValueObjects;
using SupportDesk.Domain.Models.User;
using SupportDesk.TestsUtility;

namespace SupportDesk.IntegrationTests.Application.Models.Categories.Command;

[TestFixture]
internal sealed class AddCategoryTests : IntegrationTestsBase
{
    private const string ValidName = "Test Category";
    private const string ValidDescription = "Test Description";
    
    private const string InvalidName = "";
    private const string InvalidDescription = "";

    private User _user;
    private TimeProvider _timeProvider = null!;

    [SetUp]
    public async Task Setup()
    {
        _user = await RegisterOrganizationAdmin();
        
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(_user.Id.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(_user.OrganizationId!.IdValue);
   }

    [Test]
    public async Task Handle_WithPermissions_WithValidInput_ShouldAddCategory()
    {
        AddCategoryCommandResult? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await CommandDispatcher.DispatchAsync(new AddCategoryCommand(ValidName, ValidDescription));
        });

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.CategoryId, Is.Not.EqualTo(Guid.Empty));
        
        var categoryIdVo = new CategoryId(result.CategoryId);

        var persistedCategory = await DbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == categoryIdVo);

        Assert.That(persistedCategory, Is.Not.Null);
        Assert.That(persistedCategory!.Name.NameValue, Is.EqualTo(ValidName));
        Assert.That(persistedCategory.Description.DescriptionValue, Is.EqualTo(ValidDescription));
        Assert.That(persistedCategory.OrganizationId, Is.EqualTo(_user.OrganizationId));
    }

    [Test]
    public async Task Handle_WithPermissions_WithExistingName_ShouldBreakCannotCreateCategoryWithExistingNameRule()
    {
        _timeProvider = TimeProvider.System;
        
        _ = await CreateCategory(_user.OrganizationId!.IdValue, ValidName, ValidDescription, _timeProvider);
        
        var exception = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new AddCategoryCommand(ValidName, ValidDescription));
        });       
        
        AssertUtility.AssertHasBrokenExactRule<CannotCreateCategoryWithExistingNameRule>(exception);
    }

    [Test]
    public async Task Handle_WithPermissions_WithInvalidInput_ShouldThrowValidationException()
    {
        Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new AddCategoryCommand(InvalidName, InvalidDescription));
        });
    }

    [Test]
    public async Task Handle_WithoutPermissions_ShouldThrowPermissionException()
    {
        await PermissionService.RevokePermissionAsync(_user.Id.IdValue, Permissions.Categories.Add);
        
        Assert.ThrowsAsync<PermissionException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new AddCategoryCommand(ValidName, ValidDescription));
        });
    }
}
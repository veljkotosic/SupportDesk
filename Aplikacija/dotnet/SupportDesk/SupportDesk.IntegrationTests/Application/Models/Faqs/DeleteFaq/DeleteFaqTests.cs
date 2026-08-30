using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Application.Models.Faqs.Command.DeleteFaq;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Faq.Validation;
using SupportDesk.Domain.Models.Faq.ValueObjects;
using SupportDesk.Domain.Models.User;
using SupportDesk.TestsUtility;

namespace SupportDesk.IntegrationTests.Application.Models.Faqs.DeleteFaq;

internal sealed class DeleteFaqTests : IntegrationTestsBase
{
    private User _user;
    private readonly TimeProvider _timeProvider = TimeProvider.System;
    
    [SetUp]
    public async Task Setup()
    {
        _user = await RegisterOrganizationAdmin();
        
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(_user.Id.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(_user.OrganizationId!.IdValue);
    }
    
    [Test]
    public async Task Handle_WithPermissions_WithValidFaq_ShouldDeleteFaq()
    {
        var addedFaq = await CreateFaq(_user.OrganizationId!.IdValue, "Test Question", "Test Answer", _timeProvider);
        
        Assert.DoesNotThrowAsync(async () =>
        {
            await CommandDispatcher.DispatchAsync(new DeleteFaqCommand(addedFaq.Id.IdValue));
        });
        
        var persistedFaq = await DbContext.Faqs
            .AsNoTracking()
            .IgnoreQueryFilters(["SoftDeleteFilter"])
            .FirstOrDefaultAsync(f => f.Id == addedFaq.Id);
        
        Assert.That(persistedFaq, Is.Not.Null);
        Assert.That(persistedFaq.DeletedAt, Is.Not.Null);
    }
    
    [Test]
    public async Task Handle_WithPermissions_WithUnexistingFaq_ShouldBreakDomainModelExistsRule()
    {
        var faqId = new FaqId(Guid.NewGuid());
        
        var exception = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new DeleteFaqCommand(faqId.IdValue));
        });
        
        AssertUtility.AssertHasProducedExactError(exception, FaqErrors.NotFound(faqId));
    }
    
    [Test]
    public async Task Handle_WithPermissions_WithDeletedFaq_ShouldBreakDomainModelExistsRule()
    {
        var addedFaq = await CreateFaq(_user.OrganizationId!.IdValue, "Test Question", "Test Answer", _timeProvider);
        addedFaq.Delete(_timeProvider);
        await DbContext.SaveChangesAsync();   
        DbContext.ChangeTracker.Clear();
        
        var exception = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new DeleteFaqCommand(addedFaq.Id.IdValue));
        });
        
        AssertUtility.AssertHasProducedExactError(exception, FaqErrors.NotFound(addedFaq.Id));
    }
    
    [Test]
    public async Task Handle_WithoutPermissions_ShouldThrowPermissionException()
    {
        await PermissionService.RevokePermissionAsync(_user.Id.IdValue, Permissions.Faqs.Delete);

        Assert.ThrowsAsync<PermissionException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new DeleteFaqCommand(Guid.NewGuid()));
        });
    }
}
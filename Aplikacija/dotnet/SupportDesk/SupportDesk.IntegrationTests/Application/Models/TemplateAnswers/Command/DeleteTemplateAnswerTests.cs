using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Application.Models.TemplateAnswers.Command.DeleteTemplateAnswer;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.TemplateAnswer.Validation;
using SupportDesk.Domain.Models.TemplateAnswer.ValueObjects;
using SupportDesk.Domain.Models.User;
using SupportDesk.Infrastructure.Persistence.Database;
using SupportDesk.TestsUtility;

namespace SupportDesk.IntegrationTests.Application.Models.TemplateAnswers.Command;

[TestFixture]
internal sealed class DeleteTemplateAnswerTests : IntegrationTestsBase
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
    public async Task Handle_WithPermissions_WithValidTemplateAnswer_ShouldDeleteTemplateAnswer()
    {
        var addedTemplateAnswer = await CreateTemplateAnswer(_user.OrganizationId!.IdValue, "Test Title", "Test Text", _timeProvider);
        
        Assert.DoesNotThrowAsync(async () =>
        {
            await CommandDispatcher.DispatchAsync(new DeleteTemplateAnswerCommand(addedTemplateAnswer.Id.IdValue));
        });
        
        var persistedTemplateAnswer = await DbContext.TemplateAnswers
            .AsNoTracking()
            .IgnoreQueryFilters([QueryFilterKeys.SoftDeleteFilter])
            .FirstOrDefaultAsync(t => t.Id == addedTemplateAnswer.Id);
        
        Assert.That(persistedTemplateAnswer, Is.Not.Null);
        Assert.That(persistedTemplateAnswer.DeletedAt, Is.Not.Null);
    }
    
    [Test]
    public async Task Handle_WithPermissions_WithUnexistingTemplateAnswer_ShouldBreakDomainModelExistsRule()
    {
        var templateAnswerId = new TemplateAnswerId(Guid.NewGuid());
        
        var exception = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new DeleteTemplateAnswerCommand(templateAnswerId.IdValue));
        });
        
        AssertUtility.AssertHasProducedExactError(exception, TemplateAnswerErrors.NotFound(templateAnswerId));
    }
    
    [Test]
    public async Task Handle_WithPermissions_WithDeletedTemplateAnswer_ShouldBreakDomainModelExistsRule()
    {
        var addedTemplateAnswer = await CreateTemplateAnswer(_user.OrganizationId!.IdValue, "Test Title", "Test Text", _timeProvider);
        addedTemplateAnswer.Delete(_timeProvider);
        await DbContext.SaveChangesAsync();   
        DbContext.ChangeTracker.Clear();
        
        var exception = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new DeleteTemplateAnswerCommand(addedTemplateAnswer.Id.IdValue));
        });
        
        AssertUtility.AssertHasProducedExactError(exception, TemplateAnswerErrors.NotFound(addedTemplateAnswer.Id));
    }
    
    [Test]
    public async Task Handle_WithoutPermissions_ShouldThrowPermissionException()
    {
        await PermissionService.RevokePermissionAsync(_user.Id.IdValue, Permissions.TemplateAnswers.Delete);

        Assert.ThrowsAsync<PermissionException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new DeleteTemplateAnswerCommand(Guid.NewGuid()));
        });
    }
}
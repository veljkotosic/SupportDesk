using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Application.Models.TemplateAnswers.Command.UpdateTemplateAnswerDetails;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.TemplateAnswer.Validation;
using SupportDesk.Domain.Models.TemplateAnswer.ValueObjects;
using SupportDesk.Domain.Models.User;
using SupportDesk.TestsUtility;

namespace SupportDesk.IntegrationTests.Application.Models.TemplateAnswers.UpdateTemplateAnswerDetails;

[TestFixture]
internal sealed class UpdateTemplateAnswerDetailsTests : IntegrationTestsBase
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
    public async Task Handle_WithPermissions_WithValidData_ShouldUpdateFaqDetails()
    {
        var templateAnswer = await CreateTemplateAnswer(_user.OrganizationId!.IdValue, "Test Title", "Test Text", _timeProvider);
            
        var newTitle = "Test Title 2";
        var newText = "Test Text 2";
        
        Assert.DoesNotThrowAsync(async () =>
        {
            await CommandDispatcher.DispatchAsync(new UpdateTemplateAnswerDetailsCommand(
                templateAnswer.Id.IdValue,
                newTitle, 
                newText));
        });
        
        var persistedTemplateAnswer = await DbContext.TemplateAnswers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == templateAnswer.Id);
        
        Assert.That(persistedTemplateAnswer, Is.Not.Null);
        Assert.That(persistedTemplateAnswer.Title.TitleValue, Is.EqualTo(newTitle));
        Assert.That(persistedTemplateAnswer.Text.TextValue, Is.EqualTo(newText)); 
    }
    
    [Test]
    public async Task Handle_WithPermissions_WithSameData_ShouldNotChange()
    {
        var title = "Test Title";
        var text = "Test Text";
        
        var templateAnswer = await CreateTemplateAnswer(_user.OrganizationId!.IdValue, title, text, _timeProvider);
            
        var newTitle = "Test Title";
        var newText = "Test Text";
        
        Assert.DoesNotThrowAsync(async () =>
        {
            await CommandDispatcher.DispatchAsync(new UpdateTemplateAnswerDetailsCommand(
                templateAnswer.Id.IdValue,
                newTitle, 
                newText));
        });
        
        var persistedTemplateAnswer = await DbContext.TemplateAnswers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == templateAnswer.Id);
        
        Assert.That(persistedTemplateAnswer, Is.Not.Null);
        Assert.That(persistedTemplateAnswer.Title.TitleValue, Is.EqualTo(title));
        Assert.That(persistedTemplateAnswer.Text.TextValue, Is.EqualTo(text));       
    }
    
    [Test]
    public async Task Handle_WithInvalidTemplateAnswerId_ShouldThrowProduceFaqNotFoundError()
    {
        var templateAnswerId = Guid.NewGuid();
        
        var exception = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new UpdateTemplateAnswerDetailsCommand(templateAnswerId, "Test", "Test"));
        });
        
        AssertUtility.AssertHasProducedExactError(exception, TemplateAnswerErrors.NotFound(new TemplateAnswerId(templateAnswerId)));
    }
    
    [Test]
    public async Task Handle_WithoutPermissions_ShouldThrowPermissionException()
    {
        await PermissionService.RevokePermissionAsync(_user.Id.IdValue, Permissions.TemplateAnswers.Update);
        
        Assert.ThrowsAsync<PermissionException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new UpdateTemplateAnswerDetailsCommand(Guid.NewGuid(), "Test", "Test"));
        });
    }
}
using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Application.Models.TemplateAnswers.Command.AddTemplateAnswer;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.TemplateAnswer.ValueObjects;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.IntegrationTests.Application.Models.TemplateAnswers.AddTemplateAnswer;

[TestFixture]
internal sealed class AddTemplateAnswerTests : IntegrationTestsBase
{
    private const string ValidTitle = "Test Title";
    private const string ValidText = "Test Text";
    
    private const string InvalidTitle = "";
    private const string InvalidText = "";

    private User _user;

    [SetUp]
    public async Task Setup()
    {
        _user = await RegisterOrganizationAdmin();
        
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(_user.Id.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(_user.OrganizationId!.IdValue);
    }
    
    [Test]
    public async Task Handle_WithPermissions_WithValidData_ShouldAddTemplateAnswer()
    {
        AddTemplateAnswerCommandResult? result = null;   
        
        Assert.DoesNotThrowAsync(async () =>
        {
            result = await CommandDispatcher.DispatchAsync(new AddTemplateAnswerCommand(ValidTitle, ValidText));
        });
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.TemplateAnswerId, Is.Not.EqualTo(Guid.Empty));
        
        var templateAnswerIdVo = new TemplateAnswerId(result.TemplateAnswerId);

        var persistedTemplateAnswer = await DbContext.TemplateAnswers
            .FirstOrDefaultAsync(t => t.Id == templateAnswerIdVo);
        
        Assert.That(persistedTemplateAnswer, Is.Not.Null);
        Assert.That(persistedTemplateAnswer.Title.TitleValue, Is.EqualTo(ValidTitle));
        Assert.That(persistedTemplateAnswer.Text.TextValue, Is.EqualTo(ValidText));
        Assert.That(persistedTemplateAnswer.OrganizationId, Is.EqualTo(_user.OrganizationId));       
    }

    [Test]
    public async Task Handle_WithPermissions_WithInvalidData_ShouldThrowValidationException()
    {
        Assert.ThrowsAsync<ValidationException>(async () =>
        {
            _ = await CommandDispatcher.DispatchAsync(new AddTemplateAnswerCommand(InvalidTitle, InvalidText));
        });
    }

    [Test]
    public async Task Handle_WithoutPermissions_ShouldThrowPermissionException()
    {
        await PermissionService.RevokePermissionAsync(_user.Id.IdValue, Permissions.TemplateAnswers.Add);

        Assert.ThrowsAsync<PermissionException>(async () =>
        {
            _ = await CommandDispatcher.DispatchAsync(new AddTemplateAnswerCommand(ValidTitle, ValidText));
        });
    }
}
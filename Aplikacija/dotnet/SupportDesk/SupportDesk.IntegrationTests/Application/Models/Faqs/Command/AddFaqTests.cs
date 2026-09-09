using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Application.Models.Faqs.Command.AddFaq;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Faq.ValueObjects;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.IntegrationTests.Application.Models.Faqs.Command;

[TestFixture]
internal sealed class AddFaqTests : IntegrationTestsBase
{
    private const string ValidQuestion = "Test Question";
    private const string ValidAnswer = "Test Answer";
    
    private const string InvalidQuestion = "";
    private const string InvalidAnswer = "";

    private User _user;

    [SetUp]
    public async Task Setup()
    {
        _user = await RegisterOrganizationAdmin();
        
        UserContextMock.Setup(context => context.GetCurrentUserId()).Returns(_user.Id.IdValue);
        TenantContextMock.Setup(context => context.GetCurrentOrganizationId()).Returns(_user.OrganizationId!.IdValue);
    }
    
    [Test]
    public async Task Handle_WithPermissions_WithValidData_ShouldAddFaq()
    {
        AddFaqCommandResult? result = null;   
        
        Assert.DoesNotThrowAsync(async () =>
        {
            result = await CommandDispatcher.DispatchAsync(new AddFaqCommand(ValidQuestion, ValidAnswer));
        });
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.FaqId, Is.Not.EqualTo(Guid.Empty));
        
        var faqIdVo = new FaqId(result.FaqId);

        var persistedFaq = await DbContext.Faqs
            .FirstOrDefaultAsync(f => f.Id == faqIdVo);
        
        Assert.That(persistedFaq, Is.Not.Null);
        Assert.That(persistedFaq.Question.QuestionValue, Is.EqualTo(ValidQuestion));
        Assert.That(persistedFaq.Answer.AnswerValue, Is.EqualTo(ValidAnswer));
        Assert.That(persistedFaq.OrganizationId, Is.EqualTo(_user.OrganizationId));       
    }

    [Test]
    public async Task Handle_WithPermissions_WithInvalidData_ShouldThrowValidationException()
    {
        Assert.ThrowsAsync<ValidationException>(async () =>
        {
            _ = await CommandDispatcher.DispatchAsync(new AddFaqCommand(InvalidQuestion, InvalidAnswer));
        });
    }

    [Test]
    public async Task Handle_WithoutPermissions_ShouldThrowPermissionException()
    {
        await PermissionService.RevokePermissionAsync(_user.Id.IdValue, Permissions.Faqs.Add);

        Assert.ThrowsAsync<PermissionException>(async () =>
        {
            _ = await CommandDispatcher.DispatchAsync(new AddFaqCommand(ValidQuestion, ValidAnswer));
        });
    }
}
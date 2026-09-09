using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Application.Models.Faqs.Command.UpdateFaqDetails;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Faq.Validation;
using SupportDesk.Domain.Models.Faq.ValueObjects;
using SupportDesk.Domain.Models.User;
using SupportDesk.TestsUtility;

namespace SupportDesk.IntegrationTests.Application.Models.Faqs.Command;

[TestFixture]
internal sealed class UpdateFaqDetailsTests : IntegrationTestsBase
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
        var faq = await CreateFaq(_user.OrganizationId!.IdValue, "Test Question", "Test Answer", _timeProvider);
            
        var newQuestion = "Test Question 2";
        var newAnswer = "Test Answer 2";
        
        Assert.DoesNotThrowAsync(async () =>
        {
            await CommandDispatcher.DispatchAsync(new UpdateFaqDetailsCommand(
                faq.Id.IdValue,
                newQuestion, 
                newAnswer));
        });
        
        var persistedFaq = await DbContext.Faqs
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == faq.Id);
        
        Assert.That(persistedFaq, Is.Not.Null);
        Assert.That(persistedFaq.Question.QuestionValue, Is.EqualTo(newQuestion));
        Assert.That(persistedFaq.Answer.AnswerValue, Is.EqualTo(newAnswer)); 
    }
    
    [Test]
    public async Task Handle_WithPermissions_WithSameData_ShouldNotChange()
    {
        var question = "Test Question";
        var answer = "Test Answer";
        
        var faq = await CreateFaq(_user.OrganizationId!.IdValue, question, answer, _timeProvider);
            
        var newQuestion = "Test Question";
        var newAnswer = "Test Answer";
        
        Assert.DoesNotThrowAsync(async () =>
        {
            await CommandDispatcher.DispatchAsync(new UpdateFaqDetailsCommand(
                faq.Id.IdValue,
                newQuestion, 
                newAnswer));
        });
        
        var persistedFaq = await DbContext.Faqs
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == faq.Id);
        
        Assert.That(persistedFaq, Is.Not.Null);
        Assert.That(persistedFaq.Question.QuestionValue, Is.EqualTo(question));
        Assert.That(persistedFaq.Answer.AnswerValue, Is.EqualTo(answer));       
    }
    
    [Test]
    public async Task Handle_WithInvalidFaqId_ShouldThrowProduceFaqNotFoundError()
    {
        var faqId = Guid.NewGuid();
        
        var exception = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new UpdateFaqDetailsCommand(faqId, "Test", "Test"));
        });
        
        AssertUtility.AssertHasProducedExactError(exception, FaqErrors.NotFound(new FaqId(faqId)));
    }
    
    [Test]
    public async Task Handle_WithoutPermissions_ShouldThrowPermissionException()
    {
        await PermissionService.RevokePermissionAsync(_user.Id.IdValue, Permissions.Faqs.Update);
        
        Assert.ThrowsAsync<PermissionException>(async () =>
        {
            await CommandDispatcher.DispatchAsync(new UpdateFaqDetailsCommand(Guid.NewGuid(), "Test", "Test"));
        });
    }
}
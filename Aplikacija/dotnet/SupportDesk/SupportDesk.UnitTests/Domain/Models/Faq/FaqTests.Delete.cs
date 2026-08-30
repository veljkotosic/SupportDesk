using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Faq.Events;
using SupportDesk.Domain.Models.Faq.Validation;
using SupportDesk.TestsUtility;
using FaqModel = SupportDesk.Domain.Models.Faq.Faq;

namespace SupportDesk.UnitTests.Domain.Models.Faq;

[TestFixture]
internal sealed partial class FaqTests
{
    [Test]
    public void Delete_WithValidData_ShouldMarkFaqAsDeleted_AndRaiseDomainEvent()
    {
        var timeProvider = TimeProvider.System;
        
        var faq = FaqModel.Create(Guid.NewGuid(), "Test question", "Test answer", timeProvider);
        
        faq.Delete(timeProvider);
        
        Assert.That(faq.DeletedAt, Is.Not.Null);
        Assert.That(faq.GetDomainEvents(), Has.Some.TypeOf<FaqDeletedDomainEvent>());
    }
    
    [Test]
    public void Delete_WithFaqAlreadyDeleted_ShouldProduceAlreadyDeletedError()
    {
        var timeProvider = TimeProvider.System;
        
        var faq = FaqModel.Create(Guid.NewGuid(), "Test question", "Test answer", timeProvider);
        
        faq.Delete(timeProvider);

        var exception = Assert.Throws<ValidationException>(() =>
        {
            faq.Delete(timeProvider);
        });
        
        AssertUtility.AssertHasProducedExactError(exception, FaqErrors.AlreadyDeleted(faq.Id));
    }
}
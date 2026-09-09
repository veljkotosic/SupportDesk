using NetArchTest.Rules;
using SupportDesk.Application.Abstract.Event;

namespace SupportDesk.ArchitectureTests.Application;

[TestFixture]
internal sealed class DomainEventHandlerTests : ApplicationTestsBase
{
    private PredicateList DomainEventHandlers => ApplicationTypes
        .That()
        .ImplementInterface(typeof(IDomainEventHandler<>));
    
    [Test]
    public void DomainEventHandlers_Should_BeInternal()
    {
        var result = DomainEventHandlers
            .Should()
            .BeInternal()
            .GetResult();
        
        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Domain event handlers should be internal");
    }
    
    [Test]
    public void DomainEventHandlers_Should_BeSealed()
    {
        var result = DomainEventHandlers
            .Should()
            .BeSealed()
            .GetResult();
        
        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Domain event handlers should be sealed");
    }
    
    [Test]
    public void DomainEventHandlers_Should_HaveNameEndingWithEventHandler()
    {
        var result = DomainEventHandlers
            .Should()
            .HaveNameEndingWith("EventHandler")
            .GetResult();
        
        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Domain event handler names must end with 'EventHandler'");
    }
}
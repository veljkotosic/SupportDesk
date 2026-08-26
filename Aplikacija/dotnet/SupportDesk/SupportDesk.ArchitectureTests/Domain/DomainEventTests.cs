using NetArchTest.Rules;
using SupportDesk.Domain.Abstract;

namespace SupportDesk.ArchitectureTests.Domain;

[TestFixture]
internal sealed class DomainEventTests : DomainTestsBase
{
    private PredicateList DomainEvents => DomainTypes
        .That()
        .ImplementInterface<IDomainEvent>();
    
    [Test]
    public void DomainEvents_Should_BeSealed()
    {
        var result = DomainEvents
            .Should()
            .BeSealed()
            .GetResult();

        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Domain events should be sealed");
    }
    
    [Test]
    public void DomainEvents_Should_HaveNameEndingWithDomainEvent()
    {
        var result = DomainEvents
            .Should()
            .HaveNameEndingWith("DomainEvent")
            .GetResult();
        
        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Domain event names must end with 'DomainEvent'");   
    }
}
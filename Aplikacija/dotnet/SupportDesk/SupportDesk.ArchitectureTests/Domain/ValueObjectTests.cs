using NetArchTest.Rules;
using SupportDesk.Domain.Abstract.ValueObject;

namespace SupportDesk.ArchitectureTests.Domain;

[TestFixture]
internal sealed class ValueObjectTests : DomainTestsBase
{
    private PredicateList ValueObjects => DomainTypes
        .That()
        .AreNotAbstract()
        .And()
        .Inherit<AbstractValueObject>();
    
    [Test]
    public void ValueObjects_Should_BeSealed()
    {
        var result = ValueObjects
            .Should()
            .BeSealed()
            .GetResult();
        
        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Value objects should be sealed"); 
    }
}
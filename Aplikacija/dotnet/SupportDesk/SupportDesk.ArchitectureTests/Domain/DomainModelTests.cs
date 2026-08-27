using NetArchTest.Rules;
using SupportDesk.ArchitectureTests.Rules;
using SupportDesk.Domain.Abstract;

namespace SupportDesk.ArchitectureTests.Domain;

[TestFixture]
internal sealed class DomainModelTests : DomainTestsBase
{
    private PredicateList DomainModels => DomainTypes
        .That()
        .AreNotAbstract()
        .And()
        .Inherit(typeof(AbstractDomainModel<>));

    [Test]
    public void DomainModels_Should_BeSealed()
    {
        var result = DomainModels
            .Should()
            .BeSealed()
            .GetResult();
        
        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Domain models should be sealed");   
    }

    [Test]
    public void DomainModels_Should_HaveInternalParameterlessConstructor()
    {
        var result = DomainModels
            .Should()
            .MeetCustomRule(new HaveInternalParameterlessConstructorRule())
            .GetResult();
        
        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Domain models should have an internal parameterless constructor");  
    }
}
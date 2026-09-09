using NetArchTest.Rules;
using SupportDesk.Domain.Abstract.Validation;

namespace SupportDesk.ArchitectureTests.Domain;

[TestFixture]
internal sealed class DomainErrorsTests : DomainTestsBase
{
    private PredicateList DomainErrors => DomainTypes
        .That()
        .AreNotAbstract()
        .And()
        .Inherit(typeof(AbstractErrors<,>));
    
    [Test]
    public void DomainErrors_Should_BeSealed()
    {
        var result = DomainErrors
            .Should()
            .BeSealed()
            .GetResult();
        
        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Domain errors should be sealed");
    }
    
    [Test]
    public void DomainErrors_Should_HaveNameEndingWithErrors()
    {
        var result = DomainErrors
            .Should()
            .HaveNameEndingWith("Errors")
            .GetResult();
        
        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Domain errors names must end with 'Errors'");  
    }
}
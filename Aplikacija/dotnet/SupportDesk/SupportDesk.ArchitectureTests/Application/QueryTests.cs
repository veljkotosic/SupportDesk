using NetArchTest.Rules;
using SupportDesk.Application.Abstract.Query;

namespace SupportDesk.ArchitectureTests.Application;

[TestFixture]
internal sealed class QueryTests : ApplicationTestsBase
{
    private PredicateList Queries => ApplicationTypes
        .That()
        .AreNotAbstract()
        .And()
        .ImplementInterface(typeof(IQuery<>));
    
    [Test]
    public void Queries_Should_BeSealed()
    {
        var result = Queries
            .Should()
            .BeSealed()
            .GetResult();
        
        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Queries should be sealed");
    }
    
    [Test]
    public void Queries_Should_HaveNameEndingWithQuery()
    {
        var result = Queries
            .Should()
            .HaveNameEndingWith("Query")
            .GetResult();
        
        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Query names must end with 'Query'");
    }
}
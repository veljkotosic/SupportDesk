using NetArchTest.Rules;
using SupportDesk.Application.Abstract.Query;

namespace SupportDesk.ArchitectureTests.Application;

[TestFixture]
internal sealed class QueryResultTests : ApplicationTestsBase
{
    private PredicateList QueryResults => ApplicationTypes
        .That()
        .ImplementInterface<IQueryResult>();
    
    [Test]
    public void QueryResults_Should_BeSealed()
    {
        var result = QueryResults
            .Should()
            .BeSealed()
            .GetResult();
        
        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Query results should be sealed");
    }
}
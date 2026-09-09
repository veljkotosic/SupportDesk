using NetArchTest.Rules;
using SupportDesk.Application.Abstract.Query;

namespace SupportDesk.ArchitectureTests.Application;

[TestFixture]
internal sealed class QueryHandlerTests : ApplicationTestsBase
{
    private PredicateList QueryHandlers => ApplicationTypes
        .That()
        .AreNotAbstract()
        .And()
        .ImplementInterface(typeof(IQueryHandler<,>));
    
    [Test]
    public void QueryHandlers_Should_BeInternal()
    {
        var result = QueryHandlers
            .Should()
            .BeInternal()
            .GetResult();
        
        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Query handlers should be internal");
    }

    [Test]
    public void QueryHandlers_Should_BeSealed()
    {
        var result = QueryHandlers
            .Should()
            .BeSealed()
            .GetResult();
        
        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Query handlers should be sealed");
    }
    
    [Test]
    public void QueryHandlers_Should_HaveNameEndingWithQueryHandler()
    {
        var result = QueryHandlers
            .Should()
            .HaveNameEndingWith("QueryHandler")
            .GetResult();
        
        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Query handler names must end with 'QueryHandler'");
    }
}
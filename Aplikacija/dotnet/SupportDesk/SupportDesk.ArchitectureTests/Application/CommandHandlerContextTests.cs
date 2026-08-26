using NetArchTest.Rules;
using SupportDesk.Application.Abstract.Command;

namespace SupportDesk.ArchitectureTests.Application;

[TestFixture]
internal sealed class CommandHandlerContextTests : ApplicationTestsBase
{
    private PredicateList CommandHandlerContexts => ApplicationTypes
        .That()
        .ImplementInterface<ICommandHandlerContext>()
        .Or()
        .ImplementInterface(typeof(ICommandHandlerContext<>));

    [Test]
    public void CommandHandlerContext_Should_BeInternal()
    {
        var result = CommandHandlerContexts
            .Should()
            .BeInternal()
            .GetResult();
        
        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Command handler contexts should be internal");
    }
    
    [Test]
    public void CommandHandlerContext_Should_BeSealed()
    {
        var result = CommandHandlerContexts
            .Should()
            .BeSealed()
            .GetResult();
        
        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Command handler contexts should be sealed");
    }
    
    [Test]
    public void CommandHandlerContext_Should_HaveNameEndingWithCommandHandlerContext()
    {
        var result = CommandHandlerContexts
            .Should()
            .HaveNameEndingWith("CommandHandlerContext")
            .GetResult();
        
        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Command handler context names must end with 'CommandHandlerContext'");
    }
}
using NetArchTest.Rules;
using SupportDesk.Application.Abstract.Command;

namespace SupportDesk.ArchitectureTests.Application;

[TestFixture]
internal sealed class CommandHandlerTests : ApplicationTestsBase
{
    private PredicateList CommandHandlers => ApplicationTypes
        .That()
        .AreNotAbstract()
        .And()
        .ImplementInterface(typeof(ICommandHandler<>))
        .Or()
        .AreNotAbstract()
        .And()
        .ImplementInterface(typeof(ICommandHandler<,>));

    [Test]
    public void CommandHandlers_Should_BeInternal()
    {
        var result = CommandHandlers
            .Should()
            .BeInternal()
            .GetResult();
        
        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Command handlers should be internal");
    }
    
    [Test]
    public void CommandHandlers_Should_BeSealed()
    {
        var result = CommandHandlers
            .Should()
            .BeSealed()
            .GetResult();
        
        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Command handlers should be sealed");
    }
    
    [Test]
    public void CommandHandlers_Should_HaveNameEndingWithCommandHandler()
    {
        var result = CommandHandlers
            .Should()
            .HaveNameEndingWith("CommandHandler")
            .GetResult();
        
        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Command handler names must end with 'CommandHandler'");
    }
}
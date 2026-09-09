using NetArchTest.Rules;
using SupportDesk.Application.Abstract.Command;

namespace SupportDesk.ArchitectureTests.Application;

[TestFixture]
internal sealed class CommandTests : ApplicationTestsBase
{
    private PredicateList Commands => ApplicationTypes
        .That()
        .ImplementInterface<ICommand>()
        .Or()
        .ImplementInterface(typeof(ICommand<>));
    
    [Test]
    public void Commands_Should_BeSealed()
    {
        var result = Commands
            .Should()
            .BeSealed()
            .GetResult();
        
        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Commands should be sealed");
    }
    
    [Test]
    public void Commands_Should_HaveNameEndingWithCommand()
    {
        var result = Commands
            .Should()
            .HaveNameEndingWith("Command")
            .GetResult();
        
        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Command names must end with 'Command'"); 
    }
}
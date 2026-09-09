using NetArchTest.Rules;
using SupportDesk.Application.Abstract.Command;

namespace SupportDesk.ArchitectureTests.Application;

[TestFixture]
internal sealed class CommandResultTests : ApplicationTestsBase
{
    private PredicateList CommandResults => ApplicationTypes
        .That()
        .ImplementInterface<ICommandResult>();

    [Test]
    public void CommandResults_Should_BeSealed()
    {
        var result = CommandResults
            .Should()
            .BeSealed()
            .GetResult();
        
        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Command results should be sealed");   
    }
}
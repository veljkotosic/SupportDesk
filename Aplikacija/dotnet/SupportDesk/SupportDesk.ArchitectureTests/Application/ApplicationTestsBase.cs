using NetArchTest.Rules;

namespace SupportDesk.ArchitectureTests.Application;

internal class ApplicationTestsBase : ArchitectureTestsBase
{
    protected Types ApplicationTypes => Types.InAssembly(ApplicationAssembly);
}
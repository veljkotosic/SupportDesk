using NetArchTest.Rules;

namespace SupportDesk.ArchitectureTests.Infrastructure;

internal abstract class InfrastructureTestsBase : ArchitectureTestsBase
{
    protected Types InfrastructureTypes => Types.InAssembly(InfrastructureAssembly);
}
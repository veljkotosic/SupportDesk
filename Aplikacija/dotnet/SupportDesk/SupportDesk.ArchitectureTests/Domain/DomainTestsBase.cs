using NetArchTest.Rules;

namespace SupportDesk.ArchitectureTests.Domain;

internal abstract class DomainTestsBase : ArchitectureTestsBase
{
    protected Types DomainTypes => Types.InAssembly(DomainAssembly);
}
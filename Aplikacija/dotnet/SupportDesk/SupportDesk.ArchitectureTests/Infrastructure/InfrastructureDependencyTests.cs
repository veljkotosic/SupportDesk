namespace SupportDesk.ArchitectureTests.Infrastructure;

[TestFixture]
internal sealed class InfrastructureDependencyTests : InfrastructureTestsBase
{
    [Test]
    public void InfrastructureLayer_ShouldNotDependOn_Presentation()
    {
        var result = InfrastructureTypes
            .ShouldNot()
            .HaveDependencyOnAny(WebApiAssembly.GetName().Name)
            .GetResult();

        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Infrastructure layer should not depend on Presentation");
    }
}
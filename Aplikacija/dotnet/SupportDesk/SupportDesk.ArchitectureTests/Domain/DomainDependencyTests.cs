namespace SupportDesk.ArchitectureTests.Domain;

[TestFixture]
internal sealed class DomainDependencyTests : DomainTestsBase
{
    [Test]
    public void DomainLayer_ShouldNotDependOn_OtherLayers()
    {
        var result = DomainTypes
            .ShouldNot()
            .HaveDependencyOnAny(
                ApplicationAssembly.GetName().Name,
                InfrastructureAssembly.GetName().Name,
                WebApiAssembly.GetName().Name)
            .GetResult();

        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Domain layer should not depend on other layers");
    }
}
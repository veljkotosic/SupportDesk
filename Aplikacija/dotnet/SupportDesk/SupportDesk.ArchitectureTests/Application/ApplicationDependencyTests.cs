using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;

namespace SupportDesk.ArchitectureTests.Application;

[TestFixture]
internal sealed class ApplicationDependencyTests : ApplicationTestsBase
{
    [Test]
    public void ApplicationLayer_ShouldNotDependOn_InfrastructureOrPresentation()
    {
        var result = ApplicationTypes
            .ShouldNot()
            .HaveDependencyOnAny(
                InfrastructureAssembly.GetName().Name,
                WebApiAssembly.GetName().Name)
            .GetResult();

        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Application layer should not depend on Infrastructure or Presentation");
    }
}
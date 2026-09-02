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
    
    [Test]
    public void OnlyQueryHandlers_ShouldDependOn_IApplicationDbContext()
    {
        var nonQueryTypes = ApplicationTypes
            .That()
            .AreNotAbstract()
            .And()
            .DoNotHaveNameEndingWith("QueryHandler");

        var result = nonQueryTypes
            .ShouldNot()
            .HaveDependencyOnAll(typeof(IApplicationDbContext).FullName)
            .GetResult();

        ArchitectureTestsUtility.AssertArchitectureTestResult(result, "Only query handlers are allowed to have a dependency on IApplicationDbContext");
    }
}
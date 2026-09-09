using NetArchTest.Rules;

namespace SupportDesk.ArchitectureTests;

internal static class ArchitectureTestsUtility
{
    public static void AssertArchitectureTestResult(TestResult result, string message)
    {
        Assert.That(result.IsSuccessful, Is.True, $"{message}. Failed types: \n{string.Join(", \n", result.FailingTypes.Select(type => type.FullName))}");
    }
}
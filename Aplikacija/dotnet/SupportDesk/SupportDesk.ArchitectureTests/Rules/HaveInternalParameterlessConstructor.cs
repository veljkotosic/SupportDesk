using Mono.Cecil;
using NetArchTest.Rules;

namespace SupportDesk.ArchitectureTests.Rules;

public sealed class HaveInternalParameterlessConstructorRule : ICustomRule
{
    public bool MeetsRule(TypeDefinition type)
    {
        return type.Methods.Any(m => 
            m.IsConstructor && 
            m is { IsStatic: false, HasParameters: false, IsAssembly: true }); 
    }
}
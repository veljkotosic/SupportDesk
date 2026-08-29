using System.Reflection;
using SupportDesk.Application.Abstract;
using SupportDesk.Domain.Abstract;
using SupportDesk.Infrastructure.Dispatcher;
using SupportDesk.WebApi.Controllers.v1;
using SupportDesk.WebApi.Controllers.v1.Auth;

namespace SupportDesk.ArchitectureTests;

internal abstract class ArchitectureTestsBase
{
    protected static readonly Assembly DomainAssembly = typeof(IDomainEvent).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(IUseCase).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(DomainEventDispatcher).Assembly;
    protected static readonly Assembly WebApiAssembly = typeof(AuthController).Assembly;
}
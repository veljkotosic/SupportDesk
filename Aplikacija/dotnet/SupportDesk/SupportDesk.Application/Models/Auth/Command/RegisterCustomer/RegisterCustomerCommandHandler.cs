using SupportDesk.Application.Abstract.Auth;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Command.Context;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Models.User;
using SupportDesk.Domain.Models.User.Enums;
using SupportDesk.Domain.Models.User.Repository;

namespace SupportDesk.Application.Models.Auth.Command.RegisterCustomer;

internal sealed class RegisterCustomerCommandHandler
    : AbstractCommandHandler<RegisterCustomerCommand, RegisterCustomerCommandResult, EmptyCommandHandlerContext>
{
    private readonly IAuthService _authService;
    private readonly ITokenProvider _tokenProvider;
    private readonly IRefreshTokenManager _refreshTokenManager;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCustomerCommandHandler(
        PermissionChecker permissionChecker,
        IAuthService authService,
        ITokenProvider tokenProvider,
        IRefreshTokenManager refreshTokenManager,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
        : base(permissionChecker)
    {
        _authService = authService;
        _tokenProvider = tokenProvider;
        _refreshTokenManager = refreshTokenManager;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
    }

    protected override Task<EmptyCommandHandlerContext> PrepareAsync(RegisterCustomerCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(new EmptyCommandHandlerContext());
    }

    protected override async Task<RegisterCustomerCommandResult> ExecuteAsync(RegisterCustomerCommand command, EmptyCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var customer = User.Create(command.Email, command.UserName, null, UserRole.Customer);

        await _authService.SignUpWithEmailAndPasswordAsync(customer, command.Password, cancellationToken);
        
        var accessToken = _tokenProvider.GenerateAccessToken(customer);
        var refreshTokenValue = _tokenProvider.GenerateRefreshToken();

        var refreshToken = await _refreshTokenManager.AddAsync(refreshTokenValue, customer.Id.IdValue, customer.Role, cancellationToken);
        await _userRepository.SaveAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new RegisterCustomerCommandResult(accessToken, refreshToken);       
    }
}
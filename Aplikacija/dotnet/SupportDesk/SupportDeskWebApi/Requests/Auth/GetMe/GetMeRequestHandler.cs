using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Auth.AuthService;
using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Auth.GetMe;

public class GetMeRequestHandler : IRequestHandler<GetMeRequest, GetMeResult>
{
    private readonly SupportDeskDbContext _context;
    private readonly IUserContext _userContext;

    public GetMeRequestHandler(SupportDeskDbContext context, IUserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    public async Task<GetMeResult> HandleAsync(GetMeRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.GetCurrentUserId();
        
        var result = await _context.Users
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            throw AuthException.UserNotFound();
        }

        return new GetMeResult(result.Id.ToString(), result.UserName!);
    }
}
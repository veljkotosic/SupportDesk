using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SupportDesk.Application.Abstract.Auth;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Domain.Models.User.Options;
using SupportDesk.Infrastructure.Auth.AuthService;
using SupportDesk.Infrastructure.Auth.Identity;
using SupportDesk.Infrastructure.Auth.Jwt;
using SupportDesk.Infrastructure.Auth.Permission;
using SupportDesk.Infrastructure.DependencyInjection.Configuration;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.DependencyInjection;

public static class AuthRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSupportDeskAuth(IConfiguration configuration)
        {
            var jwtSettings = new JwtSettings
            {
                Issuer = configuration.GetEnvJwtIssuer(),
                Audience = configuration.GetEnvJwtAudience(),
                Key = configuration.GetEnvJwtKey(),
                ExpirationMinutes = configuration.GetEnvJwtExpirationMinutes(),
                CustomerRefreshTokenExpirationDays = configuration.GetEnvJwtCustomerRefreshExpirationDays(),
                OrganizationRefreshTokenExpirationDays = configuration.GetEnvJwtOrganizationRefreshExpirationDays()
            };
            
            services.AddSingleton(jwtSettings);
            
            services.AddIdentityCore<AppIdentityUser>(options =>
                {
                    options.Password.RequiredLength = UserOptionsDefaults.PasswordRequiredLength;
                    options.Password.RequireDigit = UserOptionsDefaults.PasswordRequireDigit;
                    options.Password.RequireLowercase = UserOptionsDefaults.PasswordRequireLowercase;
                    options.Password.RequireUppercase = UserOptionsDefaults.PasswordRequireUppercase;
                    options.Password.RequireNonAlphanumeric = UserOptionsDefaults.PasswordRequireNonAlphanumeric;

                    options.User.RequireUniqueEmail = UserOptionsDefaults.UserRequireUniqueEmail;
                    options.User.AllowedUserNameCharacters = string.Empty;
                })
                .AddEntityFrameworkStores<SupportDeskDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ClockSkew = TimeSpan.FromSeconds(30),

                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                    };

                    options.Events = new JwtBearerEvents()
                    {
                        OnMessageReceived = context =>
                        {
                            if (context.Request.Cookies.TryGetValue("accessToken", out var cookieToken))
                            {
                                context.Token = cookieToken;
                            }
        
                            var accessToken = context.Request.Query["accessToken"];
                            var path = context.HttpContext.Request.Path;

                            if (string.IsNullOrEmpty(context.Token) && !string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization();
        
            services.AddScoped<ITokenProvider, JwtTokenProvider>();
            services.AddScoped<IAuthService, EfAuthService>();

            services.AddScoped<IPermissionService, DbPermissionService>();
            services.AddScoped<PermissionChecker>();
            
            return services;
        }
    }
}
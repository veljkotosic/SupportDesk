using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Auth.AuthService;
using SupportDeskWebApi.Auth.Jwt;
using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Data.Entities.User;
using SupportDeskWebApi.DependencyInjection.Configuration;

namespace SupportDeskWebApi.DependencyInjection;

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
            
            services.AddIdentityCore<User>(options =>
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = true;

                    options.User.RequireUniqueEmail = true;
                    options.User.AllowedUserNameCharacters = string.Empty;
                })
                .AddRoles<IdentityRole<Guid>>()
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
        
            services.AddTransient<ITokenProvider, JwtTokenProvider>();
            services.AddTransient<IAuthService, EfAuthService>();
            
            return services;
        }
    }
}

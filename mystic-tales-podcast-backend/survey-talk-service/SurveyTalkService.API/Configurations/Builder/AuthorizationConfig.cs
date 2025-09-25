using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using SurveyTalkService.API.Authorizations.Handlers;
using SurveyTalkService.API.Authorizations.Requirements;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SystemIO = System.IO;


namespace SurveyTalkService.API.Configurations.Builder
{
    public static class AuthorizationConfig
    {

        public static void AddBuilderAuthorizationConfig(this WebApplicationBuilder builder)
        {
            builder.AddCustomAuthorizationHandlers();
            builder.AddRolePolicy();
            builder.AddEmailPolicy();
            builder.AddLoginRequiredPolicy();

            builder.AddDefaultAuthorization();
        }
        public static void AddCustomAuthorizationHandlers(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IAuthorizationHandler, AccountExistsHandler>();

        }

        public static void AddDefaultAuthorization(this WebApplicationBuilder builder)
        {
            builder.Services.AddAuthorization(options =>
            {
                options.DefaultPolicy = options.GetPolicy(builder.Configuration["AppSettings:DEFAULT_AUTHORIZATION:Policy"])!;
            });
        }

        public static void AddLoginRequiredPolicy(this WebApplicationBuilder builder)
        {
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("LoginRequired", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new AccountExistsRequirement());
                });
            });
        }

        public static void AddRolePolicy(this WebApplicationBuilder builder)
        {
            builder.Services.AddAuthorization(options =>
            {
                // options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                // options.AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"));
                // options.AddPolicy("UserOnly", policy => policy.RequireRole("Customer"));
                options.AddPolicy("AdminRequiredOnly", policy =>
                {
                    policy.RequireRole("Admin");
                    policy.Requirements.Add(new AccountExistsRequirement());
                });
                options.AddPolicy("AdminOrManagerRequired", policy =>
                {
                    policy.RequireRole("Admin", "Manager");
                    policy.Requirements.Add(new AccountExistsRequirement());
                });
                options.AddPolicy("ManagerRequiredOnly", policy =>
                {
                    policy.RequireRole("Manager");
                    policy.Requirements.Add(new AccountExistsRequirement());
                });
                options.AddPolicy("CustomerRequiredOnly", policy =>
                {
                    policy.RequireRole("Customer");
                    policy.Requirements.Add(new AccountExistsRequirement());
                });
                options.AddPolicy("UserTransactionReportAccess", policy =>
                {
                    policy.RequireRole("Customer","Manager");
                    policy.Requirements.Add(new AccountExistsRequirement());
                });


            });

        }

        public static void AddEmailPolicy(this WebApplicationBuilder builder)
        {
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireRootEmail", policy =>
                {
                    policy.RequireClaim(ClaimTypes.Email, "hanguyenhao.20april@gmail.com")
                        .AddAuthenticationSchemes(builder.Configuration["AppSettings:DEFAULT_AUTHENTICATION:Scheme"])
                        .RequireAuthenticatedUser()
                        ;
                });
            });
        }
    }
}

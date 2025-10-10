using Microsoft.AspNetCore.Authorization;
using UserService.API.Authorizations.Handlers;
using UserService.API.Authorizations.Requirements;
using System.Security.Claims;


namespace UserService.API.Configurations.Builder
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
            // builder.Services.AddScoped<IAuthorizationHandler, AccountExistsHandler>();
            builder.Services.AddScoped<IAuthorizationHandler, AccountBasicAccessHandler>();
            builder.Services.AddScoped<IAuthorizationHandler, AccountNoViolationAccessHandler>();
            builder.Services.AddScoped<IAuthorizationHandler, AccountPodcasterAccessHandler>();
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
                options.AddPolicy("BasicAccess", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new AccountBasicAccessRequirement());
                });
            });
        }

        public static void AddRolePolicy(this WebApplicationBuilder builder)
        {
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin.BasicAccess", policy =>
                {
                    policy.RequireRole("Admin");
                    policy.Requirements.Add(new AccountBasicAccessRequirement());
                });
                options.AddPolicy("Staff.BasicAccess", policy =>
                {
                    policy.RequireRole("Staff");
                    policy.Requirements.Add(new AccountBasicAccessRequirement());
                });
                options.AddPolicy("Customer.BasicAccess", policy =>
                {
                    policy.RequireRole("Customer");
                    policy.Requirements.Add(new AccountBasicAccessRequirement());
                });
                options.AddPolicy("AdminOrStaff.BasicAccess", policy =>
                {
                    policy.RequireRole("Admin", "Staff");
                    policy.Requirements.Add(new AccountBasicAccessRequirement());
                });
                options.AddPolicy("Customer.NoViolationAccess", policy =>
                {
                    policy.RequireRole("Customer");
                    policy.Requirements.Add(new AccountNoViolationAccessRequirement());
                });
                options.AddPolicy("Customer.PodcasterAccess", policy =>
                {
                    policy.RequireRole("Staff");
                    policy.Requirements.Add(new AccountPodcasterAccessRequirement());
                });
                options.AddPolicy("Customer.NoViolationAccess.PodcasterAccess", policy =>
                {
                    policy.RequireRole("Customer");
                    policy.Requirements.Add(new AccountNoViolationAccessRequirement());
                    policy.Requirements.Add(new AccountPodcasterAccessRequirement());
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

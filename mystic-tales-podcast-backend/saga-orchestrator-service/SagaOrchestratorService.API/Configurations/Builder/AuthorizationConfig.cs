using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SagaOrchestratorService.API.Authorizations.Handlers;
using SagaOrchestratorService.API.Authorizations.Requirements;


namespace SagaOrchestratorService.API.Configurations.Builder
{
    public static class AuthorizationConfig
    {

        public static void AddBuilderAuthorizationConfig(this WebApplicationBuilder builder)
        {
            builder.AddCustomAuthorizationHandlers();
            builder.AddRolePolicy();
            builder.AddEmailPolicy();
            builder.AddStaticPolicy();

            builder.AddDefaultAuthorization();
        }
        public static void AddCustomAuthorizationHandlers(this WebApplicationBuilder builder)
        {
            // builder.Services.AddScoped<IAuthorizationHandler, AccountBasicAccessHandler>();
            // builder.Services.AddScoped<IAuthorizationHandler, AccountNoViolationAccessHandler>();
            // builder.Services.AddScoped<IAuthorizationHandler, AccountPodcasterAccessHandler>();
            // builder.Services.AddScoped<IAuthorizationHandler, AccountNonPodcasterAccessHandler>();
            // builder.Services.AddScoped<IAuthorizationHandler, AccountOptionalAccessHandler>();
        }

        public static void AddDefaultAuthorization(this WebApplicationBuilder builder)
        {
            builder.Services.AddAuthorization(options =>
            {
                options.DefaultPolicy = options.GetPolicy(builder.Configuration["AppSettings:DEFAULT_AUTHORIZATION:Policy"])!;
            });
        }

        public static void AddStaticPolicy(this WebApplicationBuilder builder)
        {
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("OptionalAccess", policy =>
                {
                    policy.Requirements.Add(new AccountOptionalAccessRequirement());
                });
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
                options.AddPolicy("AdminOrCustomer.BasicAccess", policy =>
                {
                    policy.RequireRole("Admin", "Customer");
                    policy.Requirements.Add(new AccountBasicAccessRequirement());
                });
                options.AddPolicy("AdminOrStaffOrCustomer.BasicAccess", policy =>
                {
                    policy.RequireRole("Admin", "Staff", "Customer");
                    policy.Requirements.Add(new AccountBasicAccessRequirement());
                });
                options.AddPolicy("Customer.NoViolationAccess", policy =>
                {
                    policy.RequireRole("Customer");
                    policy.Requirements.Add(new AccountNoViolationAccessRequirement());
                });
                options.AddPolicy("Customer.PodcasterAccess", policy =>
                {
                    policy.RequireRole("Customer");
                    policy.Requirements.Add(new AccountPodcasterAccessRequirement());
                });
                options.AddPolicy("Customer.NoViolationAccess.NonPodcasterAccess", policy =>
                {
                    policy.RequireRole("Customer");
                    policy.Requirements.Add(new AccountNoViolationAccessRequirement());
                    policy.Requirements.Add(new AccountNonPodcasterAccessRequirement());
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

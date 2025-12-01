using Microsoft.AspNetCore.Authorization;
using BookingManagementService.API.Authorizations.Handlers;
using BookingManagementService.API.Authorizations.Requirements;
using System.Security.Claims;


namespace BookingManagementService.API.Configurations.Builder
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
            // builder.Services.AddScoped<IAuthorizationHandler, AccountExistsHandler>();
            builder.Services.AddScoped<IAuthorizationHandler, AccountBasicAccessHandler>();
            builder.Services.AddScoped<IAuthorizationHandler, AccountNoViolationAccessHandler>();
            builder.Services.AddScoped<IAuthorizationHandler, AccountPodcasterAccessHandler>();
            builder.Services.AddScoped<IAuthorizationHandler, AccountOptionalAccessHandler>();
            builder.Services.AddScoped<IAuthorizationHandler, AdminStaffBasicAccessOrCustomerPodcasterAccessHandler>();
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
                options.AddPolicy("Customer.NoViolationAccess.PodcasterAccess", policy =>
                {
                    policy.RequireRole("Customer");
                    policy.Requirements.Add(new AccountNoViolationAccessRequirement());
                    policy.Requirements.Add(new AccountPodcasterAccessRequirement());
                });
                options.AddPolicy("AdminOrStaffOrCustomer.NoViolationAccess", policy =>
                {
                    policy.RequireRole("Admin", "Staff", "Customer");
                    policy.Requirements.Add(new AccountNoViolationAccessRequirement());
                });
                options.AddPolicy("AdminOrStaff.BasicAccess.Customer.PodcasterAccess", policy =>
                {
                    policy.Requirements.Add(new AdminStaffBasicAccessOrCustomerPodcasterAccessRequirement());
                });
                options.AddPolicy("AdminOrStaffOrCustomer.BasicAccess", policy =>
                {
                    policy.RequireRole("Admin", "Staff", "Customer");
                    policy.Requirements.Add(new AccountBasicAccessRequirement());
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

using System.Security.Claims;
using Architecture_1.BusinessLogic.Services.DbServices.UserServices;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;

namespace Architecture_1.BusinessLogic.Services.IdentityServerServices
{
    public class ResourceOwnerPasswordValidatorService : IResourceOwnerPasswordValidator
    {
        private readonly AuthService _authService;

        public ResourceOwnerPasswordValidatorService(AuthService authService)
        {
            _authService = authService;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            try
            {
                // Tạo request giả
                var loginRequest = new
                {
                    Email = context.UserName,
                    Password = context.Password
                };
                // Console.WriteLine("\n\n\nLogin request: " + loginRequest.Email + " " + loginRequest.Password);

                var loginResult = await _authService.Login(loginRequest);

                var userId = loginResult["User"]["Id"]?.ToString();

                context.Result = new GrantValidationResult(
                    subject: userId+" nguuuuuuuuuuuuuu",
                    authenticationMethod: "custom",
                    claims: new List<Claim>
                    {
                    new Claim("name", loginResult["User"]["FullName"]?.ToString()),
                    new Claim("email", loginResult["User"]["Email"]?.ToString()),
                    new Claim("role", loginResult["User"]["Role"]?.ToString()),
                    new Claim("id", userId)
                    }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("Login failed in validator: " + ex.Message);
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid credentials");
            }
        }
    }
}

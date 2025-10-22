using Microsoft.AspNetCore.Authorization;
using SurveyTalkService.API.Authorizations.Requirements;
using SurveyTalkService.DataAccess.Entities;
using SurveyTalkService.DataAccess.Repositories.interfaces;

namespace SurveyTalkService.API.Authorizations.Handlers
{
    public class AccountExistsHandler : AuthorizationHandler<AccountExistsRequirement>
    {
        private readonly IAccountRepository _accountRepository;
        public AccountExistsHandler(
            IAccountRepository accountRepository
            )
        {
            _accountRepository = accountRepository;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AccountExistsRequirement requirement)
        {
            string userId = context.User.FindFirst("id")?.Value;
            string roleId = context.User.FindFirst("role_id")?.Value;
            // Console.WriteLine($"000000000000000000000000000000000000000000000000000000000UserId: {userId}, RoleId: {roleId}");
            if (roleId == null || !int.TryParse(roleId, out _))
            {
                context.Fail();
                return;
            }
            if (userId == null) { context.Fail(); return; }

            var account = await _accountRepository.FindByIdAsync(int.Parse(userId));
            // var account = await _accountGenericRepository.FindByIdAsync(int.Parse(userId), a => );
            if (account == null)
            {
                Console.WriteLine($"0000000000000000000000000000000000000000000000000000000Account not found: {userId}");
                context.Fail();
            }
            else if (account.RoleId != int.Parse(roleId))
            {
                Console.WriteLine($"0000000000000000000000000000000000000000000000000000000Account role mismatch: {account.RoleId} != {roleId}");
                context.Fail();
            }
            else if (account.IsVerified == false)
            {
                Console.WriteLine($"0000000000000000000000000000000000000000000000000000000Account is not verified: {account.IsVerified}");
                context.Fail(); 
            }
            else if (account.DeactivatedAt != null)
            {
                Console.WriteLine($"0000000000000000000000000000000000000000000000000000000Account is deactivated: {account.DeactivatedAt}");
                context.Fail();
            }
            else
            {
                // Lưu account vào HttpContext.Items
                if (context.Resource is HttpContext httpContext)
                {
                    httpContext.Items["LoggedInAccount"] = account;
                }
                else if (context.Resource is Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext authContext)
                {
                    authContext.HttpContext.Items["LoggedInAccount"] = account;
                }
                // Console.WriteLine($"0000000000000000000000000000000000000000000000000000000Account: {account.SurveyTopicFavorites.Count}");
                context.Succeed(requirement);
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using ApiGatewayService.Entry.Authorizations.Requirements;
// using SurveyTalkService.DataAccess.Entities;
// using SurveyTalkService.DataAccess.Repositories.interfaces;

namespace ApiGatewayService.Entry.Authorizations.Handlers
{
    public class AccountExistsHandler : AuthorizationHandler<AccountExistsRequirement>
    {
        // private readonly IAccountRepository _accountRepository;
        public AccountExistsHandler(
            // IAccountRepository accountRepository
            )
        {
            // _accountRepository = accountRepository;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AccountExistsRequirement requirement)
        {
            // TODO: Implement account validation logic for API Gateway
            // For now, just succeed the requirement
            context.Succeed(requirement);
            await Task.CompletedTask;
        }
    }
}

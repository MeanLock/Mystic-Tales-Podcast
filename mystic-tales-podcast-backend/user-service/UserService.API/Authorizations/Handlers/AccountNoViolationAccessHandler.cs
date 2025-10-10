using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using UserService.API.Authorizations.Requirements;
using UserService.BusinessLogic.Models.CrossService;
using UserService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using UserService.DataAccess.Entities;
using UserService.DataAccess.Repositories.interfaces;

namespace UserService.API.Authorizations.Handlers
{
    public class AccountNoViolationAccessHandler : AuthorizationHandler<AccountNoViolationAccessRequirement>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        public AccountNoViolationAccessHandler(
            IAccountRepository accountRepository,
            HttpServiceQueryClient httpServiceQueryClient
            )
        {
            _accountRepository = accountRepository;
            _httpServiceQueryClient = httpServiceQueryClient;
        }

        public async Task<JObject> GetAccountById(int accountId)
        {
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
                {
                    new BatchQueryItem
                    {
                        Key = "account",
                        QueryType = "findbyid",
                        EntityType = "Account",
                        Parameters = JObject.FromObject(new
                        {
                            id = accountId
                        }),
                        Fields = new[] {
                            "Id",
                            "Email",
                            "Password",
                            "RoleId",
                            "FullName",
                            "Dob",
                            "Gender",
                            "Address",
                            "Phone",
                            "Balance",
                            "MainImageFileKey",
                            "IsVerified",
                            "GoogleId",
                            "VerifyCode",
                            "PodcastListenSlot",
                            "ViolationPoint",
                            "ViolationLevel",
                            "LastViolationPointChanged",
                            "LastViolationLevelChanged",
                            "LastPodcastListenSlotChanged",
                            "DeactivatedAt",
                            "CreatedAt",
                            "UpdatedAt"
                        }
                    }
                }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);

            // Lấy account từ kết quả
            var accountData = result.Results["account"];
            if (accountData is JArray accountArray && accountArray.Count > 0)
            {
                return accountArray.First as JObject;
            }

            return null;
        }


        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AccountNoViolationAccessRequirement requirement)
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

            // var account = await _accountRepository.FindByIdAsync(int.Parse(userId));
            // var account = await _accountGenericRepository.FindByIdAsync(int.Parse(userId), a => );
            var account = await GetAccountById(int.Parse(userId));

            if (account == null)
            {
                Console.WriteLine($"0000000000000000000000000000000000000000000000000000000Account not found: {userId}");
                context.Fail();
            }
            else if (account["RoleId"].Value<int>() != int.Parse(roleId))
            {
                Console.WriteLine($"0000000000000000000000000000000000000000000000000000000Account role mismatch: {account["RoleId"]} != {roleId}");
                context.Fail();
            }
            else if (account["IsVerified"].Value<bool>() == false)
            {
                Console.WriteLine($"0000000000000000000000000000000000000000000000000000000Account is not verified: {account["IsVerified"]}");
                context.Fail();
            }
            else if (account["DeactivatedAt"] != null)
            {
                Console.WriteLine($"0000000000000000000000000000000000000000000000000000000Account is deactivated: {account["DeactivatedAt"]}");
                context.Fail();
            }else if (account["ViolationLevel"].Value<int>() > 0)
            {
                Console.WriteLine($"0000000000000000000000000000000000000000000000000000000Account has violations: {account["ViolationLevel"]}");
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

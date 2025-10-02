using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;
using SurveyTalkService.API.Controllers.UserControllers;
using SurveyTalkService.API.Filters.ExceptionFilters;
using SurveyTalkService.BusinessLogic.DTOs.Account;
using SurveyTalkService.BusinessLogic.Services.DbServices.UserServices;

namespace SurveyTalkService.API.Controllers.UserControllers
{
    [Route("api/User/accounts")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class AccountController : ControllerBase
    {
        private ILogger<AccountController> _logger;
        private readonly AuthService _authService;
        private readonly AccountService _accountService;

        public AccountController(
            ILogger<AccountController> logger,
            AuthService authService,
            AccountService accountService)
        {
            _logger = logger;
            _authService = authService;
            _accountService = accountService;
        }

        // GET /api/User/accounts/customer 
        [HttpGet("customer")]
        [Authorize(Policy = "AdminOrManagerRequired")]
        public async Task<IActionResult> GetCustomerAccounts()
        {
            var accounts = await _accountService.GetCustomerAccounts();

            return Ok(new
            {
                Accounts = accounts
            });
        }

        // GET /api/User/accounts/staff
        [HttpGet("staff")]
        [Authorize(Policy = "AdminRequiredOnly")]
        public async Task<IActionResult> GetStaffAccounts()
        {
            var accounts = await _accountService.GetStaffAccounts();

            return Ok(new
            {
                Accounts = accounts
            });
        }

        // GET /api/User/accounts/{AccountId}
        [HttpGet("{accountId}")]
        [Authorize(Policy = "AdminOrManagerRequired")]
        public async Task<IActionResult> GetAccountById(int accountId)
        {
            var roleId = User.FindFirst("role_id")?.Value;

            AccountDetailDTO account = await _accountService.GetAccountById(accountId);
            

            if (account != null && account.Role.Id == 1 && int.Parse(roleId) != 1)
            {
                return Forbid();
            }

            return Ok(new
            {
                Account = account
            });
        }

        // PUT /api/User/accounts/{AccountId}
        [HttpPut("{accountId}")]
        [Authorize(Policy = "LoginRequired")]
        public async Task<IActionResult> UpdateAccount(int accountId, [FromBody] AccountUpdateDTO data)
        {
            int userId = int.Parse(User.FindFirst("id")?.Value);
            int userRoleId = int.Parse(User.FindFirst("role_id")?.Value);
            var account = await _accountService.GetExistAccountById(accountId);

            if (userRoleId == 1)
            {
                if (account.Role.Id != 1 || account.Id != userId)
                {
                    return Forbid();
                }
            }
            else if (userRoleId == 2)
            {
                if (account.Role.Id != 1 && account.Role.Id != 2 )
                {
                    return Forbid();
                }
            }
            else if (userRoleId == 4)
            {
                if (account.Role.Id != 1 && account.Role.Id != 2 && account.Role.Id != 4)
                {
                    return Forbid();
                }
            }
            else
            {
                return Forbid();
            }

            var updatedAccount = await _accountService.UpdateAccount(accountId, data);
            return Ok(new
            {
                Message = "Cập nhật tài khoản thành công",
            });
        }

        // GET /api/User/accounts/me
        [HttpGet("me")]
        [Authorize(Policy = "LoginRequired")]
        public async Task<IActionResult> GetMe()
        {
            int userId = int.Parse(User.FindFirst("id")?.Value);
            int userRoleId = int.Parse(User.FindFirst("role_id")?.Value);
            var account = await _accountService.GetMe(userId);
            return Ok(new
            {
                Account = account
            });
        }


        // PUT /api/User/accounts/{AccountId}/update-profile
        [HttpPut("{accountId}/update-profile")]
        [Authorize(Policy = "CustomerRequiredOnly")]
        public async Task<IActionResult> UpdateProfile(int accountId, [FromBody] AccountProfileUpdateDTO data)
        {
            int userId = int.Parse(User.FindFirst("id")?.Value);
            int userRoleId = int.Parse(User.FindFirst("role_id")?.Value);
            Console.WriteLine($"UserId: {userId}, AccountId: {accountId}, UserRoleId: {userRoleId}");
            if (userId != accountId)
            {
                return Forbid();
            }

            await _accountService.UpdateAccountProfile(accountId, data);
            return Ok(new
            {
                Message = "Cập nhật hồ sơ thành công",
            });
        }

        // PUT /api/User/accounts/{AccountId}/deactivate/{IsDeactivate}
        [HttpPut("{accountId}/deactivate/{isDeactivate}")]
        [Authorize(Policy = "AdminOrManagerRequired")]
        public async Task<IActionResult> DeactivateAccount(int accountId, bool isDeactivate)
        {
            int userId = int.Parse(User.FindFirst("id")?.Value);
            int userRoleId = int.Parse(User.FindFirst("role_id")?.Value);
            var account = await _accountService.GetExistAccountById(accountId);

            if (account.Id == userId)
            {
                return Forbid("Không thể cập nhật trạng thái tài khoản của chính bạn");
            }

            if (userRoleId == 1)
            {
                if (account.Role.Id == 1 && account.Id != userId)
                {
                    return Forbid();
                }
            }
            else if (userRoleId == 2)
            {
                if (account.Role.Id == 1 || account.Role.Id == 2)
                {
                    return Forbid();
                }
            }
            else
            {
                return Forbid();
            }

            await _accountService.DeactivateAccount(accountId, isDeactivate);
            return Ok(new
            {
                Message = isDeactivate == true ? "Tài khoản đã được vô hiệu hoá thành công" : "Tài khoản đã được kích hoạt lại thành công",
            });
        }



    }
}

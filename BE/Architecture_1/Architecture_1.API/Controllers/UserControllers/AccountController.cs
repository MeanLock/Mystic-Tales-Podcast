using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Architecture_1.API.Controllers.UserControllers;
using Architecture_1.API.Filters.ExceptionFilters;
using Architecture_1.BusinessLogic.Services.DbServices.UserServices;
using Newtonsoft.Json.Linq;

namespace Architecture_1.API.Controllers.AccountControllers
{
    [Route("api/User/accounts")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class AccountController : ControllerBase
    {
        private ILogger<AccountController> _logger;
        private readonly AuthService _authService;
        private readonly AccountService _accountService;

        public AccountController(ILogger<AccountController> logger, AuthService authService, AccountService accountService)
        {
            _logger = logger;
            _authService = authService;
            _accountService = accountService;
        }

        // [GET] /User/accounts/staff
        [HttpGet("staff")]
        public async Task<IActionResult> GetStaffAccounts()
        {
            var staffAccounts = await _accountService.GetStaffAccounts();

            return Ok(new
            {
                Accounts = staffAccounts
            });
        }

        // [POST] /User/accounts/staff
        [HttpPost("staff")]
        public async Task<IActionResult> CreateStaffAccount([FromBody] JToken jsonData)
        {
            dynamic staffAccount = jsonData["Staff"].ToObject<dynamic>();

            await _accountService.CreateStaffAccount(staffAccount);

            return Ok(new
            {
                // message = "Tạo tài khoản nhân viên thành công",
                message = "Create staff account successfully"
            });
        }

        // [GET] /User/accounts/staff/{accountId}
        [HttpGet("staff/{accountId:int}")]
        public async Task<IActionResult> GetStaffAccount(int accountId)
        {
            var staffAccount = await _accountService.GetStaffDetail(accountId);

            return Ok(staffAccount);
        }

        // [PUT] /User/accounts/staff/{accountId}
        [HttpPut("staff/{accountId:int}")]
        public async Task<IActionResult> UpdateStaffAccount(int accountId, [FromBody] JToken jsonData)
        {
            dynamic staffAccount = jsonData["Staff"].ToObject<dynamic>();

            await _accountService.UpdateStaffAccount(accountId, staffAccount);

            return Ok(new
            {
                // message = "Cập nhật tài khoản nhân viên thành công",
                message = "Update staff account successfully"
            });
        }

        // [DELETE] /User/accounts/staff/{accountId}
        [HttpDelete("staff/{accountId:int}")]
        public async Task<IActionResult> DeactivateStaffAccount(int accountId)
        {
            await _accountService.DeactivateAccount(accountId, true, "staff");

            return Ok(new
            {
                // message = "Xóa tài khoản nhân viên thành công",
                message = "Delete staff account successfully"
            });
        }

        // [GET] /User/accounts/member
        [HttpGet("member")]
        public async Task<IActionResult> GetMemberAccounts()
        {
            var memberAccounts = await _accountService.GetMemberAccounts();

            return Ok(new
            {
                Accounts = memberAccounts
            });
        }

        // [POST] /User/accounts/member
        [HttpPost("member")]
        public async Task<IActionResult> CreateMemberAccount([FromBody] JToken jsonData)
        {
            dynamic memberAccount = jsonData["Member"].ToObject<dynamic>();

            await _accountService.CreateMemberAccount(memberAccount);

            return Ok(new
            {
                // message = "Tạo tài khoản thành viên thành công",
                message = "Create member account successfully"
            });
        }

        // [GET] /User/accounts/member/{accountId}
        [HttpGet("member/{accountId:int}")]
        public async Task<IActionResult> GetMemberAccount(int accountId)
        {
            var memberAccount = await _accountService.GetMemberDetail(accountId);

            return Ok(memberAccount);
        }

        // [PUT] /User/accounts/member/{accountId}
        [HttpPut("member/{accountId:int}")]
        public async Task<IActionResult> UpdateMemberAccount(int accountId, [FromBody] JToken jsonData)
        {
            dynamic memberAccount = jsonData["Member"].ToObject<dynamic>();

            await _accountService.UpdateMemberAccount(accountId, memberAccount);

            return Ok(new
            {
                // message = "Cập nhật tài khoản thành viên thành công",
                message = "Update member account successfully"
            });
        }   

        // [DELETE] /User/accounts/member/{accountId}
        [HttpDelete("member/{accountId:int}")]
        public async Task<IActionResult> DeactivateMemberAccount(int accountId)
        {
            await _accountService.DeactivateAccount(accountId, true, "member");

            return Ok(new
            {
                // message = "Xóa tài khoản thành viên thành công",
                message = "Delete member account successfully"
            });
        }
        
        // [GET] /User/accounts/roles
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _accountService.GetRoles();

            return Ok(new
            {
                Roles = roles
            });
        }

        // [GET] /User/accounts/jobTypes
        [HttpGet("jobTypes")]
        public async Task<IActionResult> GetJobTypes(){
            var jobTypes = await _accountService.GetJobTypes();
            
            return Ok(new
            {
                JobTypes = jobTypes
            });
        }

        ///////////////////////////////////////////////////////////////
        [HttpGet("")]
        public async Task<IActionResult> GetAccounts()
        {
            //var accounts = await _accountService.GetAccounts();

            return Ok(new
            {
                Accounts = ""
            });
        }
    }
}

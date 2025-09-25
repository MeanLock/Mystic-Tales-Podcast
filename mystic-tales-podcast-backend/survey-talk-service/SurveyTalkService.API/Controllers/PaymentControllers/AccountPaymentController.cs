using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using SurveyTalkService.API.Filters.ExceptionFilters;
using SurveyTalkService.BusinessLogic.DTOs.Transaction;
using SurveyTalkService.BusinessLogic.Services.DbServices.PaymentServices;
using SurveyTalkService.BusinessLogic.Services.DbServices.UserServices;
using SurveyTalkService.DataAccess.Entities;

namespace SurveyTalkService.API.Controllers.PaymentControllers
{
    [Route("api/Payment/account")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class AccountPaymentController : ControllerBase
    {
        private ILogger<AccountPaymentController> _logger;
        private readonly AuthService _authService;
        private readonly AccountService _accountService;
        private readonly AccountPaymentService _accountPaymentService;

        public AccountPaymentController(
            ILogger<AccountPaymentController> logger,
            AuthService authService,
            AccountService accountService,
            AccountPaymentService accountPaymentService
            )
        {
            _logger = logger;
            _authService = authService;
            _accountService = accountService;
            _accountPaymentService = accountPaymentService;
        }



        // GET /api/Payment/account/balance-deposits/create-payment-link
        [HttpPost("balance-deposits/create-payment-link")]
        [Authorize(Policy = "CustomerRequiredOnly")]
        public async Task<IActionResult> CreateAccountBalanceDepositPaymentLink([FromBody] AccountBalanceDepositDTO accountBalanceDepositDTO)
        {
            int userId = int.Parse(User.FindFirst("id")?.Value);

            string paymentLink = await _accountPaymentService.CreateAccountBalanceDepositPaymentLink(accountBalanceDepositDTO, userId);

            return Ok(new
            {
                Message = "Tạo liên kết thanh toán thành công",
                PaymentLink = paymentLink
            });
        }

        // POST /api/Payment/account/ConfirmPayment
        [HttpPost("confirm-payment")]
        public async Task ConfirmPayment([FromBody] WebhookType? webhookBody)
        {
            await _accountPaymentService.ConfirmPayment(webhookBody);
        }

        // POST /api/Payment/account/balance-withdrawal
        [HttpPost("balance-withdrawal")]
        [Authorize(Policy = "CustomerRequiredOnly")]
        public async Task<IActionResult> WithdrawAccountBalance([FromBody] AccountBalanceWithdrawalDTO accountBalanceWithdrawalDTO)
        {
            Account account = HttpContext.Items["LoggedInAccount"] as Account;


            await _accountPaymentService.WithdrawAccountBalance(account, accountBalanceWithdrawalDTO);

            return Ok(new
            {
                Message = "Yêu cầu rút tiền thành công"
            });
        }

        // GET /api/Payment/account/balance-deposits/history
        [HttpGet("balance-deposits/history")]
        [Authorize(Policy = "UserTransactionReportAccess")]
        public async Task<IActionResult> GetAccountBalanceDepositHistory()
        {
            Account account = HttpContext.Items["LoggedInAccount"] as Account;

            var history = await _accountPaymentService.GetAccountBalanceDepositHistory(account);

            return Ok(new
            {
                Message = "Lấy lịch sử nạp tiền thành công",
                TransactionHistory = history
            });
        }

        // GET /api/Payment/account/balance-withdrawal/history
        [HttpGet("balance-withdrawal/history")]
        [Authorize(Policy = "UserTransactionReportAccess")]
        public async Task<IActionResult> GetAccountBalanceWithdrawalHistory()
        {
            Account account = HttpContext.Items["LoggedInAccount"] as Account;

            var history = await _accountPaymentService.GetAccountBalanceWithdrawalHistory(account);

            return Ok(
                new
                {
                    Message = "Lấy lịch sử rút tiền thành công",
                    TransactionHistory = history
                }
            );
        }

    }
}

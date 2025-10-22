using Microsoft.AspNetCore.Mvc;
using TransactionService.API.Filters.ExceptionFilters;
using TransactionService.BusinessLogic.Models.CrossService;
using TransactionService.BusinessLogic.Services.CrossServiceServices.QueryServices;

namespace TransactionService.API.Controllers.BaseControllers
{
    [Route("api/account-balance-transactions")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class AccountBalanceTransactionController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;

        public AccountBalanceTransactionController(GenericQueryService genericQueryService, HttpServiceQueryClient httpServiceQueryClient)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
        }

    }
}

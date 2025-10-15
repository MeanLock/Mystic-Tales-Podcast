using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionService.DataAccess.Entities.SqlServer;
using TransactionService.DataAccess.Repositories.interfaces;
using TransactionService.DataAccess.UOW;

namespace TransactionService.BusinessLogic.Services.DbServices
{
    public class AccountBalanceTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AccountBalanceTransactionService> _logger;
        private readonly IGenericRepository<AccountBalanceTransaction> _accountBalanceTransactionGenericRepository;
        public AccountBalanceTransactionService(
            IUnitOfWork unitOfWork, 
            ILogger<AccountBalanceTransactionService> logger,
            IGenericRepository<AccountBalanceTransaction> accountBalanceTransactionGenericRepository)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _accountBalanceTransactionGenericRepository = accountBalanceTransactionGenericRepository;
        }
    }
}

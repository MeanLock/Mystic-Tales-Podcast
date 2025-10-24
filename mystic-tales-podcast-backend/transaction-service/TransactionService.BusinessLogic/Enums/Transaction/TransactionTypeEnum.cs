using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionService.BusinessLogic.Enums.Transaction
{
    public enum TransactionTypeEnum
    {
        AccountBalanceDeposits = 1,
        AccountBalanceWithdrawal = 2,
        BookingDeposit = 3,
        BookingDepositRefund = 4,
        BookingDepositCompensation = 5,
        BookingPayTheRest = 6,
        BookingAdditionalStoragePurchase = 7,
        CustomerSubscriptionCyclePayment = 8,
        CustomerSubscriptionCyclePaymentRefund = 9,
        SystemSubscriptionIncome = 10,
        PodcasterSubscriptionIncome = 11,
        SystemBookingIncome = 12,
        PodcasterBookingIncome = 13
    }
}

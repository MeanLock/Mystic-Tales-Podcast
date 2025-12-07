import { appApi } from "@/core/api/appApi";
import type { BalanceChange } from "@/core/types/transaction";

const BASE_WEB_URL =
  import.meta.env.VITE_PUBLIC_WEB_URL ?? "http://localhost:5173";

type AccountBalanceTransaction = {
  Id: string;
  Amount: number;
  TransactionType: {
    Id: number;
    Name: string;
  };
  TransactionStatus: {
    Id: number;
    Name: string;
  };
  CreatedAt: string;
  ChangedAt: string | null;
};

export type WithdrawalRequest = {
  Id: string;
  Amount: number;
  TransferReceiptImageFileKey: string;
  RejectReason: string;
  IsRejected: boolean;
  CompletedAt: string;
  CreatedAt: string;
  UpdatedAt: string;
};

const transactionApi = appApi.injectEndpoints({
  endpoints: (build) => ({
    createPaymentLink: build.mutation<
      {
        PaymentLinkUrl: string;
        AccountBalanceTransactionId: string;
      },
      {
        amount: number;
        description: string;
        returnUrl: string;
        cancelUrl: string;
      }
    >({
      async queryFn({ amount, description, returnUrl, cancelUrl }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: "/api/transaction-service/api/account-balance-transactions/balance-deposits/create-payment-link",
                method: "POST",
                body: {
                  AccountBalanceTransactionCreateInfo: {
                    Amount: amount,
                    Description: description,
                    ReturnUrl: `${BASE_WEB_URL}${returnUrl}`,
                    CancelUrl: `${BASE_WEB_URL}${cancelUrl}`,
                  },
                },
                authMode: "required",
              },
              poll: {
                intervalMs: 1000,
                maxAttempts: 30,
              },
            })
          )
          .unwrap();
        console.log("Create Payment Link Result:", result);
        return { data: result };
      },
    }),
    createWithdrawRequest: build.mutation<
      {
        Message: string;
      },
      {
        Amount: number;
      }
    >({
      async queryFn({ Amount }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: "/api/transaction-service/api/account-balance-transactions/balance-withdrawal",
                method: "POST",
                body: {
                  Amount,
                },
                authMode: "required",
              },
              poll: {
                intervalMs: 1000,
                maxAttempts: 30,
              },
            })
          )
          .unwrap();
        return { data: result };
      },
    }),
    getTransactionHistory: build.query<
      { AccountBalanceTransactionList: AccountBalanceTransaction[] },
      { getEnum: string }
    >({
      query: ({ getEnum }) => ({
        url: `/api/transaction-service/api/account-balance-transactions/balance-change-history/${getEnum}`,
        method: "GET",
        authMode: "required",
      }),
    }),

    getWithdrawalRequestHistory: build.query<
      { AccountBalanceWithdrawalRequestList: WithdrawalRequest[] },
      void
    >({
      query: () => ({
        url: `/api/transaction-service/api/account-balance-transactions/balance-withdrawal-request`,
        method: "GET",
        authMode: "required",
      }),
    }),

    getAccountBalanceChangeHistory: build.query<
      {
        BalanceChangeHistory: BalanceChange[];
      },
      void
    >({
      query: () => ({
        url: `/api/transaction-service/api/account-balance-transactions/balance-change-history`,
        method: "GET",
        authMode: "required",
      }),
    }),
  }),
});

export const {
  useCreatePaymentLinkMutation,
  useCreateWithdrawRequestMutation,
  useGetTransactionHistoryQuery,
  useGetWithdrawalRequestHistoryQuery,
  useGetAccountBalanceChangeHistoryQuery,
} = transactionApi;

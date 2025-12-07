export type BalanceChange = {
  Amount: number;
  TransactionType: TransactionType;
  TransactionStatus: TransactionStatus;
  IsReceived: boolean; // true là cộng vào, false là trừ ra
  CompletedAt: string;
};

export type TransactionType =
  | { Id: 1; Name: "Account Balance Deposit" }
  | { Id: 2; Name: "Account Balance Withdrawal" }
  | { Id: 3; Name: "Booking Deposit" }
  | { Id: 4; Name: "Booking Deposit Refund" }
  | { Id: 6; Name: "Booking Pay The Rest" }
  | { Id: 8; Name: "Customer Subscription Cycle Payment" }
  | { Id: 9; Name: "Customer Subscription Cycle Payment Refund" };

export type TransactionStatus = 
| { Id: 1; Name: "Processing"}
| { Id: 2; Name: "Success" }
| { Id: 3; Name: "Cancelled" }
| { Id: 4; Name: "Error" };

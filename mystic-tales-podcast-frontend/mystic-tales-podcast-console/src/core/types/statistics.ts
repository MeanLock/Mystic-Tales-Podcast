export type SummaryProfit = {
  TotalRevenue: number;
  PercentChange: number;
};

export type PeriodicProfit = {
  StartDate: string; 
  EndDate: string;   
  Revenue: number;
};

export type PeriodicAccountBalanceCount = {
  StartDate: string; 
  EndDate: string;   
  DepositTransactionAmount: number;
  WithdrawalTransactionAmount: number;
};

export type SummaryAccountBalanceCount = {
  DepositTransactionCount: number;
  WithdrawalTransactionCount: number;
  DepositTransactionPercentChange: number;
  WithdrawalTransactionPercentChange: number;
};

export type SummaryAccountRegistrationCount = {
  NewRegistrationCount: number;
  PercentChange: number;
};

export type CommunitySurveySummaryCount = {
  Published: number;
  OnDeadline: number;
  NearDeadline: number;
  LateForDeadline: number;
  Achieved: number;
};
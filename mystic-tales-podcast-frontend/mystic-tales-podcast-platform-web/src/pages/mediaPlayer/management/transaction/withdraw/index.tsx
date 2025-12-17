// @ts-nocheck
import {
  useCreateWithdrawRequestMutation,
  useGetTransactionHistoryQuery,
  useGetWithdrawalRequestHistoryQuery,
} from "@/core/services/transaction/transaction.service";
import type { RootState } from "@/redux/store";
import { useEffect, useState } from "react";
import { useSelector } from "react-redux";
import { TbCoinFilled } from "react-icons/tb";
import MTPCoinOutline from "@/components/coinIcons/CoinIconOutline";

const presetAmounts = [20000, 50000, 100000, 200000, 500000];

const WithDrawPage = () => {
  // REDUX
  const user = useSelector((state: RootState) => state.auth.user);

  // STATES
  const [viewMode, setViewMode] = useState<"withdraw" | "history">("withdraw");
  const [amount, setAmount] = useState<number>(0);
  const [error, setError] = useState<string>("");

  // HOOKS
  // const {
  //   data: withdrawHistoryData,
  //   isLoading: isHistoryLoading,
  //   refetch,
  // } = useGetTransactionHistoryQuery(
  //   { getEnum: "MoneyOut" },
  //   { skip: viewMode !== "history" }
  // );

  const {
    data: withdrawHistoryData,
    isLoading: isHistoryLoading,
    refetch,
  } = useGetWithdrawalRequestHistoryQuery();

  const [createWithdrawRequest, { isLoading: isCreatingWithdrawRequest }] =
    useCreateWithdrawRequestMutation();

  useEffect(() => {
    if (viewMode === "history") {
      refetch();
    }
  }, [viewMode, refetch]);

  // FUNCTIONS
  const handleAmountChange = (value: number) => {
    setAmount(value);
    if (value <= 0) {
      setError("Amount must be greater than 0");
    } else if (user && value > user.Balance) {
      setError("Insufficient balance for this withdrawal");
    } else {
      setError("");
    }
  };

  const handlePresetAmount = (preset: number) => {
    handleAmountChange(preset);
  };

  const handleCreateWithdrawRequest = async () => {
    if (amount <= 0) {
      setError("Please enter a valid amount greater than 0");
      return;
    }
    if (!user) {
      setError("User not found. Please log in again");
      return;
    }
    if (amount > user.Balance) {
      setError("Insufficient balance for this withdrawal");
      return;
    }

    try {
      const result = await createWithdrawRequest({ Amount: amount }).unwrap();
      setAmount(0);
      setError("");
      setViewMode("history");
      refetch();
    } catch (error: any) {
      console.error("Withdraw request failed:", error);
      setError(
        error?.data?.Message ||
          error?.message ||
          "Withdraw request failed. Please try again."
      );
    }
  };

  const currentBalance = user?.Balance || 0;

  return (
    <div className="w-full h-full flex flex-col items-start gap-5 font-poppins">
      <p className="text-5xl m-8 font-bold text-white">
        <span className="text-[#FEA863]">Withdraw</span> Money
      </p>

      <div className="w-1/2 mx-8 p-4 rounded-2xl bg-linear-to-r from-[#FEA863]/20 to-[#E9B286]/50 border border-[#FEA863]/30 backdrop-blur-sm shadow-2xl">
        <p className="text-black text-sm mb-2 font-bold">
          Account Current Balance
        </p>
        <div className="flex items-center gap-2">
          <MTPCoinOutline size={30} color="#FEA863" />
          <span className="text-3xl font-bold text-white">
            {currentBalance.toLocaleString()}
          </span>
        </div>
      </div>

      <div className="w-full px-8 py-2 flex items-center gap-5">
        <div
          onClick={() => setViewMode("withdraw")}
          className={`rounded-md px-5 py-2 cursor-pointer shadow-md transition-all duration-500 ${
            viewMode === "withdraw"
              ? "bg-[#FEA863] text-white font-bold"
              : "text-white font-semibold border border-white/60 hover:bg-[#FEA863] hover:text-white"
          }`}
        >
          <p>Withdraw Money</p>
        </div>
        <div
          onClick={() => setViewMode("history")}
          className={`rounded-md px-5 py-2 cursor-pointer shadow-md transition-all duration-500 ${
            viewMode === "history"
              ? "bg-[#FEA863] text-white font-bold"
              : "text-white font-semibold hover:text-white border border-white/60 hover:bg-[#FEA863]"
          }`}
        >
          <p>Withdraw History</p>
        </div>
      </div>

      {/* Main Container */}
      <div className="w-full px-8">
        {viewMode === "withdraw" ? (
          <div className="backdrop-blur-xl bg-white/10 border border-white/20 rounded-3xl p-8 shadow-2xl">
            {/* Header */}
            <div className="mb-8">
              <p className="text-white/60 text-sm">
                Request to withdraw money from your account balance. Please
                enter the amount you wish to withdraw.
              </p>
            </div>

            {/* Amount Input */}
            <div className="mb-6">
              <label className="block text-white/80 text-sm font-medium mb-3">
                Amount to Withdraw
              </label>
              <div className="relative">
                <input
                  type="number"
                  value={amount || ""}
                  onChange={(e) => handleAmountChange(Number(e.target.value))}
                  placeholder="Enter amount to withdraw"
                  className="w-full px-4 py-3 bg-white/5 border border-white/20 rounded-xl text-white placeholder-white/40 focus:outline-none focus:ring-2 focus:ring-red-400 focus:border-transparent transition-all"
                />
                <div className="absolute right-4 top-3.5 flex items-center justify-center">
                  <MTPCoinOutline size={20} color="#FEA863" />
                </div>
              </div>
              {error && <p className="text-[#FEA863] text-xs mt-2">{error}</p>}
            </div>

            {/* Preset Amounts */}
            <div className="mb-8">
              <p className="text-white/70 text-xs font-medium mb-3 uppercase tracking-wide">
                Quick Select Amount
              </p>
              <div className="grid grid-cols-5 gap-2">
                {presetAmounts.map((preset) => (
                  <button
                    key={preset}
                    onClick={() => handlePresetAmount(preset)}
                    disabled={preset > currentBalance}
                    className={`py-2 px-3 rounded-lg font-semibold text-sm transition-all duration-200 ${
                      amount === preset
                        ? "bg-[#FEA863] text-white shadow-lg shadow-[#FEA863]/50"
                        : preset > currentBalance
                        ? "bg-white/5 text-white/30 border border-white/10 cursor-not-allowed"
                        : "bg-white/10 text-white border border-white/20 hover:bg-white/15 hover:border-white/30"
                    }`}
                  >
                    {preset.toLocaleString()}
                  </button>
                ))}
              </div>
            </div>

            {/* Confirm Button */}
            <button
              onClick={handleCreateWithdrawRequest}
              disabled={
                isCreatingWithdrawRequest ||
                amount <= 0 ||
                amount > currentBalance
              }
              className="w-full py-3 px-4 bg-linear-to-r from-[#FEA863] to-[#E9B286]/80 hover:from-[#FEA863]/90 hover:to-[#FEA863]/70 disabled:opacity-50 disabled:cursor-not-allowed text-white font-bold rounded-xl transition-all duration-200 shadow-lg shadow-[#FEA863]/30 hover:shadow-xl hover:shadow-[#FEA863]/40"
            >
              {isCreatingWithdrawRequest
                ? "Processing..."
                : "Confirm Withdrawal"}
            </button>
          </div>
        ) : (
          <div className="backdrop-blur-xl bg-white/10 border border-white/20 rounded-3xl p-8 shadow-2xl">
            <div className="mb-6">
              <p className="text-2xl font-bold text-white mb-2">
                Withdraw History
              </p>
              <p className="text-white/60 text-sm">
                View all your previous withdrawal requests
              </p>
            </div>

            {isHistoryLoading ? (
              <div className="flex flex-col items-center justify-center py-16 gap-4">
                <div className="w-12 h-12 border-4 border-[#FEA863] border-t-transparent rounded-full animate-spin"></div>
                <p className="text-white/60">Loading transaction history...</p>
              </div>
            ) : withdrawHistoryData &&
              withdrawHistoryData.AccountBalanceWithdrawalRequestList.length >
                0 ? (
              <div className="space-y-3">
                {withdrawHistoryData.AccountBalanceWithdrawalRequestList.map(
                  (transaction) => (
                    <div
                      key={transaction.Id}
                      className="bg-white/5 border border-white/10 rounded-xl p-4 hover:bg-white/10 transition-all"
                    >
                      <div className="flex items-center justify-between">
                        <div className="flex-1">
                          <div className="flex items-center gap-3 mb-2">
                            <TbCoinFilled className="w-5 h-5 text-[#FEA863]" />
                            <p className="text-white font-bold text-lg">
                              -{transaction.Amount.toLocaleString()} VND
                            </p>
                          </div>
                          <div className="flex items-center gap-4 text-sm text-white/60">
                            <p>
                              Type:{" "}
                              <span className="text-white/80">
                                Withdrawal Request
                              </span>
                            </p>
                            <p>
                              Status:{" "}
                              <span
                                className={`font-semibold ${
                                  transaction.CompletedAt
                                    ? "text-mystic-green"
                                    : !transaction.IsRejected
                                    ? "text-yellow-400"
                                    : "text-[#FEA863]"
                                }`}
                              >
                                {transaction.CompletedAt
                                  ? "Completed"
                                  : !transaction.IsRejected
                                  ? "Pending"
                                  : "Rejected"}
                              </span>
                            </p>
                          </div>
                        </div>
                        <div className="text-right">
                          <p className="text-xs text-white/40 mb-1">
                            Created at
                          </p>
                          <p className="text-sm text-white/80">
                            {new Date(transaction.CreatedAt).toLocaleDateString(
                              "vi-VN",
                              {
                                year: "numeric",
                                month: "2-digit",
                                day: "2-digit",
                                hour: "2-digit",
                                minute: "2-digit",
                              }
                            )}
                          </p>
                        </div>
                      </div>
                    </div>
                  )
                )}
              </div>
            ) : (
              <div className="flex flex-col items-center justify-center py-16 gap-3">
                <TbCoinFilled className="w-20 h-20 text-white/20" />
                <p className="text-white/60 text-lg">
                  No transaction history found
                </p>
                <p className="text-white/40 text-sm">
                  Your withdrawal requests will appear here
                </p>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default WithDrawPage;

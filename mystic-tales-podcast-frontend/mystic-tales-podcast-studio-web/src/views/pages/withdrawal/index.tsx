
import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2";
import { useSagaPolling } from "@/core/hooks/useSagaPolling";
import { getHistoryWithdrawal, withdrawalSubscription } from "@/core/services/transaction/transaction.service";
import { RootState } from "@/redux/rootReducer";
import { Coin } from "phosphor-react";
import { useEffect, useState } from "react";
import { useSelector } from "react-redux";
import { toast } from "react-toastify";
import './styles.scss'
const presetAmounts = [20000, 50000, 100000, 200000, 500000];

const WithDrawPage = () => {
  // REDUX
  const user = useSelector((state: RootState) => state.auth.user);

  // STATES
  const [viewMode, setViewMode] = useState<"withdraw" | "history">("withdraw");
  const [amount, setAmount] = useState<number>(0);
  const [error, setError] = useState<string>("");

  const [isCreatingWithdrawRequest, setIsCreatingWithdrawRequest] = useState<boolean>(false);
  const [isHistoryLoading, setIsHistoryLoading] = useState<boolean>(false);
  const [withdrawHistoryData, setWithdrawHistoryData] = useState<any>(null);
  const { startPolling } = useSagaPolling({
    timeoutSeconds: 10,
    intervalSeconds: 0.5,
  })
  // //HOOKS
  // const {
  //   data: withdrawHistoryData,
  //   isLoading: isHistoryLoading,
  //   refetch,
  // } = useGetTransactionHistoryQuery(
  //   { getEnum: "MoneyOut" },
  //   { skip: viewMode !== "history" }
  // );

  // const [createWithdrawRequest, { isLoading: isCreatingWithdrawRequest }] =
  //   useCreateWithdrawRequestMutation();

  const fetchTransactionHistory = async () => {
    setIsHistoryLoading(true);
    try {
      const res = await getHistoryWithdrawal(loginRequiredAxiosInstance);
      console.log("Fetched tone lisst :", res.data);
      if (res.success && res.data) {
        setWithdrawHistoryData(res.data);
      } else {
        console.error('API Error:', res.message);
      }
    } catch (error) {
      console.error('Lỗi khi fetch detail:', error);
    } finally {
      setIsHistoryLoading(false);
    }
  }
  useEffect(() => {
    if (viewMode === "history") {
      fetchTransactionHistory();
    }
  }, [viewMode]);

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
      setIsCreatingWithdrawRequest(true);
      const res = await withdrawalSubscription(loginRequiredAxiosInstance, amount);
      const sagaId = res?.data.SagaInstanceId
      if (!sagaId) {
        toast.error("Withdrawal failed, please try again.")
        return
      }
      await startPolling(sagaId, loginRequiredAxiosInstance, {
        onSuccess: async () => {
          toast.success('Withdrawal successful');
          setAmount(0);
          setError("");
          setViewMode("history");
          await fetchTransactionHistory();
        },
        onFailure: (err) => toast.error(err || "Saga failed!"),
        onTimeout: () => toast.error("System not responding, please try again."),
      })
    } catch (err) {
      console.error(err);
      toast.error('Error withdrawing');
    } finally {
      setIsCreatingWithdrawRequest(false);
    }
  };
  const currentBalance = user?.Balance || 0;

  return (
    <div className="w-full h-full flex flex-col items-start mt-8 gap-5 font-poppins">
      <div className="flex items-center justify-between  w-full pr-10"> 
      <p className="text-5xl m-8 font-bold text-white">
        <span className="text-[#ff9800]">Withdraw</span> Money
      </p>
      <div className="flex items-center gap-2">
        <Coin className="w-6 h-6 text-[#ff9800]" />
        <span className="text-3xl font-bold text-white">
          {currentBalance.toLocaleString()}
        </span>
      </div>
</div>

      <div className="w-full px-8 py-2 flex items-center gap-5">
        <div
          onClick={() => setViewMode("withdraw")}
          className={`rounded-md px-5 py-2 cursor-pointer shadow-md transition-all duration-500 ${viewMode === "withdraw"
            ? "bg-[#ff9800] text-white font-bold"
            : "text-white font-semibold border border-white/60 hover:bg-[#ff9800] hover:text-white"
            }`}
        >
          <p>Withdraw Money</p>
        </div>
        <div
          onClick={() => setViewMode("history")}
          className={`rounded-md px-5 py-2 cursor-pointer shadow-md transition-all duration-500 ${viewMode === "history"
            ? "bg-[#ff9800] text-white font-bold"
            : "text-white font-semibold hover:text-white border border-white/60 hover:bg-[#ff9800]"
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
                  className="w-full px-4 py-3 bg-white/5 border border-white/20 rounded-xl text-white placeholder-white/40 focus:outline-none focus:ring-2 focus:ring-[#ff9800] focus:border-transparent transition-all"
                />
                <Coin className="absolute right-4 top-3.5 w-5 h-5 text-[#ff9800]/60" />
              </div>
              {error && <p className="text-[#ff9800] text-xs mt-2">{error}</p>}
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
                    className={`py-2 px-3 rounded-lg font-semibold text-sm transition-all duration-200 ${amount === preset
                      ? "bg-[#ff9800] text-white shadow-lg shadow-[#ff9800]/50"
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
              className="action-btn--unpublish w-full py-3 px-4 "
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
                <div className="w-12 h-12 border-4 border-[#ff9800] border-t-transparent rounded-full animate-spin"></div>
                <p className="text-white/60">Loading transaction history...</p>
              </div>
            ) : withdrawHistoryData &&
              withdrawHistoryData.AccountBalanceWithdrawalRequestList.length > 0 ? (
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
                            <Coin className="w-5 h-5 text-[#ff9800]" />
                            <p className="text-white font-bold text-lg">
                              -{transaction.Amount.toLocaleString()} VND
                            </p>
                          </div>
                          <div className="flex items-center gap-4 text-sm text-white/60">  
                            <p>
                              Status:{" "}
                              {(() => {
                                const isRejected = transaction.IsRejected === true;
                                const isCompleted = transaction.CompletedAt != null;
                                const statusText = isRejected
                                  ? "Rejected"
                                  : isCompleted
                                  ? "Completed"
                                  : "Pending";
                                const statusColor = isRejected
                                  ? "text-[#ff9800]" // orange for rejected
                                  : isCompleted
                                  ? "text-[#aee339]" // green for completed
                                  : "text-yellow-400"; // yellow for pending
                                return (
                                  <span className={`font-semibold ${statusColor}`}>
                                    {statusText}
                                  </span>
                                );
                              })()}
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
                <Coin className="w-20 h-20 text-white/20" />
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

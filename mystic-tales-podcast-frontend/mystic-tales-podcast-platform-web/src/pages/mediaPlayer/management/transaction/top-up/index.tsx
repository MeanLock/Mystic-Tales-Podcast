import {
  useCreatePaymentLinkMutation,
  useGetTransactionHistoryQuery,
} from "@/core/services/transaction/transaction.service";
import type { RootState } from "@/redux/store";
import { useEffect, useState } from "react";
import { useSelector } from "react-redux";
import { TbCoinFilled } from "react-icons/tb";
import "./style.css";
import MTPCoinOutline from "@/components/coinIcons/CoinIconOutline";
const presetAmounts = [20000, 50000, 100000, 200000, 500000];

const TopUpPage = () => {
  // REDUX STATES
  const user = useSelector((state: RootState) => state.auth.user);

  // LOCAL STORAGE
  const neededAmount = localStorage.getItem("neededTopUpAmount");

  // STATES
  const [amount, setAmount] = useState<number>(0);
  const [error, setError] = useState<string>("");
  const [viewMode, setViewMode] = useState<"top-up" | "history">("top-up");

  // HOOKS
  const [createPaymentLinkMutation, { isLoading }] =
    useCreatePaymentLinkMutation();

  const {
    data: topUpHistoryData,
    isLoading: isHistoryLoading,
    refetch,
  } = useGetTransactionHistoryQuery(
    { getEnum: "MoneyIn" },
    { skip: viewMode !== "history" }
  );

  useEffect(() => {
    if (viewMode === "history") {
      refetch();
    }
  }, [viewMode, refetch]);

  useEffect(() => {
    if (neededAmount) {
      const parsedAmount = parseInt(neededAmount);
      if (isNaN(parsedAmount)) {
        setAmount(0);
      } else {
        if (parsedAmount >= 10000) {
          setAmount(parsedAmount);
        } else {
          setAmount(10000);
        }
      }
    }
  }, [neededAmount]);

  // FUNCTIONS
  const handleAmountChange = (value: number) => {
    setAmount(value);
    if (value < 10000) {
      setError("Số tiền tối thiểu là 10,000");
    } else {
      setError("");
    }
  };

  const handlePresetAmount = (preset: number) => {
    handleAmountChange(preset);
  };

  const handleGetPaymentLink = async () => {
    if (amount < 10000) {
      setError("Số tiền tối thiểu là 10,000");
      return;
    }
    const backUrl =
      localStorage.getItem("paymentBackUrl") ||
      "/media-player/management/transactions/top-up";

    try {
      const randomId = crypto.randomUUID();
      console.log("DEBUG top-up: set topUpSessionId =", randomId);

      localStorage.setItem("topUpSessionId", randomId);
      const description = `Top up ${amount.toLocaleString()}`;
      const returnUrl = `/media-player/management/transactions/payment-result?feCode=${randomId}&backUrl=${encodeURIComponent(
        backUrl
      )}&amount=${amount}`;
      const cancelUrl =
        "/media-player/management/transactions/payment-result?status=cancel";

      const response = await createPaymentLinkMutation({
        amount,
        description,
        returnUrl,
        cancelUrl,
      }).unwrap();

      console.log("Response from createPaymentLinkMutation:", response);

      if (!response || !response.PaymentLinkUrl) {
        console.error("Invalid response:", response);
        setError("Không thể tạo link thanh toán. Vui lòng thử lại.");
        return;
      }

      const paymentLink = response.PaymentLinkUrl;
      console.log("Payment Link:", paymentLink);
      window.location.href = paymentLink;
      // window.open(paymentLink, "_blank");
    } catch (error) {
      console.error("Error creating payment link:", error);
      setError("Có lỗi xảy ra. Vui lòng thử lại.");
    }
  };

  const currentBalance = user?.Balance || 0;

  return (
    <div className="w-full h-full flex flex-col items-start gap-5 font-poppins">
      <p className="text-5xl m-8 font-bold text-white">
        <span className="text-mystic-green">Top-up</span> Money
      </p>
      <div className="w-1/2 mx-8 p-4 rounded-2xl bg-linear-to-r from-mystic-green/20 to-mystic-green/50 border border-mystic-green/30 backdrop-blur-sm shadow-2xl">
        <p className="text-black text-sm mb-2 font-bold">
          Account Current Balance
        </p>
        <div className="flex items-center gap-2">
          <MTPCoinOutline size={30} />
          <span className="text-3xl font-bold text-white animate-aurora">
            {currentBalance.toLocaleString()}
          </span>
        </div>
      </div>

      <div className="w-full px-8 py-2 flex items-center gap-5">
        <div
          onClick={() => setViewMode("top-up")}
          className={`rounded-md px-5 py-2 cursor-pointer shadow-md transition-all duration-500 ${
            viewMode === "top-up"
              ? " bg-mystic-green text-black font-bold"
              : "text-white font-semibold border border-white/60 hover:bg-mystic-green hover:text-black"
          }`}
        >
          <p>Top Up Money</p>
        </div>
        <div
          onClick={() => setViewMode("history")}
          className={`rounded-md px-5 py-2 cursor-pointer shadow-md transition-all duration-500 ${
            viewMode === "history"
              ? " bg-mystic-green text-black font-bold"
              : "text-white font-semibold  hover:text-black border border-white/60 hover:bg-mystic-green"
          }`}
        >
          <p>Top Up History</p>
        </div>
      </div>
      {/* Main Container */}
      <div className="w-full px-8">
        {/* Glassmorphism Card */}
        {viewMode === "top-up" ? (
          <div className="backdrop-blur-xl bg-white/10 border border-white/20 rounded-3xl p-8 shadow-2xl">
            {/* Header */}
            <div className="mb-8">
              {/* <p className="text-2xl font-bold text-white mb-2">Top-up Form</p> */}
              <p className="text-white/60 text-sm">
                You will be navigate to payment gateway to complete your top-up
                transaction.
              </p>
            </div>

            {/* Current Balance Section */}

            {/* Amount Input */}
            <div className="mb-6">
              <label className="block text-white/80 text-sm font-medium mb-3">
                Amount to Top-up
              </label>
              <div className="relative">
                <input
                  type="number"
                  value={amount || ""}
                  onChange={(e) => handleAmountChange(Number(e.target.value))}
                  placeholder="Nhập số tiền (tối thiểu 10,000)"
                  className="w-full px-4 py-3 bg-white/5 border border-white/20 rounded-xl text-white placeholder-white/40 focus:outline-none focus:ring-2 focus:ring-mystic-green focus:border-transparent transition-all"
                />
                <div className="absolute right-4 top-3.5 flex items-center justify-center">
                  <MTPCoinOutline size={20} />
                </div>
              </div>
              {error && <p className="text-red-400 text-xs mt-2">{error}</p>}
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
                    className={`py-2 px-3 rounded-lg font-semibold text-sm transition-all duration-200 ${
                      amount === preset
                        ? "bg-mystic-green text-slate-900 shadow-lg shadow-mystic-green/50"
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
              onClick={handleGetPaymentLink}
              disabled={isLoading || amount < 10000}
              className="w-full py-3 px-4 bg-linear-to-r from-mystic-green to-mystic-green/80 hover:from-mystic-green/90 hover:to-mystic-green/70 disabled:opacity-50 disabled:cursor-not-allowed text-slate-900 font-bold rounded-xl transition-all duration-200 shadow-lg shadow-mystic-green/30 hover:shadow-xl hover:shadow-mystic-green/40"
            >
              {isLoading ? "Processing..." : "Confirm Top-up"}
            </button>
          </div>
        ) : (
          <div className="backdrop-blur-xl bg-white/10 border border-white/20 rounded-3xl p-8 shadow-2xl">
            <div className="mb-6">
              <p className="text-2xl font-bold text-white mb-2">
                Top-up History
              </p>
              <p className="text-white/60 text-sm">
                View all your previous top-up transactions
              </p>
            </div>

            {isHistoryLoading ? (
              <div className="flex flex-col items-center justify-center py-16 gap-4">
                <div className="w-12 h-12 border-4 border-mystic-green border-t-transparent rounded-full animate-spin"></div>
                <p className="text-white/60">Loading transaction history...</p>
              </div>
            ) : topUpHistoryData &&
              topUpHistoryData.AccountBalanceTransactionList.length > 0 ? (
              <div className="space-y-3">
                {topUpHistoryData.AccountBalanceTransactionList.map(
                  (transaction) => (
                    <div
                      key={transaction.Id}
                      className="bg-white/5 border border-white/10 rounded-xl p-4 hover:bg-white/10 transition-all"
                    >
                      <div className="flex items-center justify-between">
                        <div className="flex-1">
                          <div className="flex items-center gap-3 mb-2">
                            <TbCoinFilled className="w-5 h-5 text-mystic-green" />
                            <p className="text-white font-bold text-lg">
                              +{transaction.Amount.toLocaleString()} VND
                            </p>
                          </div>
                          <div className="flex items-center gap-4 text-sm text-white/60">
                            <p>
                              Type:{" "}
                              <span className="text-white/80">
                                {transaction.TransactionType.Name}
                              </span>
                            </p>
                            <p>
                              Status:{" "}
                              <span
                                className={`font-semibold ${
                                  transaction.TransactionStatus.Name ===
                                  "Success"
                                    ? "text-mystic-green"
                                    : transaction.TransactionStatus.Name ===
                                      "Pending"
                                    ? "text-yellow-400"
                                    : "text-red-400"
                                }`}
                              >
                                {transaction.TransactionStatus.Name}
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
                  Your top-up transactions will appear here
                </p>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default TopUpPage;

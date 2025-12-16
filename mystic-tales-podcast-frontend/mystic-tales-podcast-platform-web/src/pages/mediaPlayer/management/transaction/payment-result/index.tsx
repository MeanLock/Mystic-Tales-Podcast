import { FireworksBackground } from "@/components/ui/shadcn-io/fireworks-background";
import { LiquidButton } from "@/components/ui/shadcn-io/liquid-button";
import { useEffect, useRef, useState } from "react";
import { FaCheck, FaRegDizzy } from "react-icons/fa";
import { TbCoinFilled} from "react-icons/tb";
import { useNavigate, useSearchParams } from "react-router-dom";

const PaymentResultPage = () => {
  const [searchParams] = useSearchParams();
  const status = searchParams.get("status");
  const backUrl = searchParams.get("backUrl");
  const topUpSessionId = searchParams.get("feCode");
  const amount = searchParams.get("amount");

  // STATES
  const [isPaymentSuccess, setIsPaymentSuccess] = useState(false);
  const [amountPaid, setAmountPaid] = useState<number | null>(null);

  // HOOKS
  const navigate = useNavigate();
  const effectRan = useRef(false);

  useEffect(() => {
    // Trong React 18 + StrictMode, effect mount sẽ chạy 2 lần ở dev.
    // Dùng cờ để ignore lần 2.
    if (effectRan.current) return;
    effectRan.current = true;

    const sessionId = localStorage.getItem("topUpSessionId");

    // 1. Thiếu feCode hoặc không có session → invalid
    if (!topUpSessionId || !sessionId) {
      navigate("/media-player/management/transactions/top-up", {
        replace: true,
      });
      return;
    }

    // 2. Không khớp → invalid
    if (topUpSessionId !== sessionId) {
      alert("Invalid payment session. Redirecting to Top-up page.");
      navigate("/media-player/management/transactions/top-up", {
        replace: true,
      });
      return;
    }

    // 3. Dùng xong thì xóa
    localStorage.removeItem("topUpSessionId");

    // 4. Xử lý status
    if (!status || status === "") {
      navigate("/media-player/management/transactions/top-up", {
        replace: true,
      });
      return;
    }

    if (status === "cancel" || status !== "PAID") {
      setIsPaymentSuccess(false);
      return;
    }

    // Đến đây chắc chắn PAID
    setIsPaymentSuccess(true);

    if (amount) {
      const amt = parseInt(amount, 10);
      if (!isNaN(amt)) setAmountPaid(amt);
    }
  }, [topUpSessionId, status, amount, backUrl, navigate]);

  // FUNCTIONS
  const handleContinue = () => {
    if (backUrl && backUrl !== "") {
      navigate(backUrl);
    } else {
      navigate("/media-player/management/transactions/top-up");
    }
  };

  return (
    <div className="w-full h-full relative">
      {isPaymentSuccess ? (
        <>
          {/* Background layer */}
          <div className="absolute inset-0 z-0">
            <FireworksBackground
              population={1}
              color={["#d65a5a", "#6dbf6d", "#3da0c4", "#ebed6d"]}
              fireworkSpeed={{ min: 4, max: 8 }}
              particleSize={{ min: 2, max: 6 }}
            />
          </div>
          {/* Content layer */}
          <div className="relative z-10 w-full h-full flex flex-col gap-5 items-center justify-center text-white p-8">
            <div className="flex justify-center transition-all">
              <span className="relative flex h-16 w-16 animate-bounce">
                <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-gradient-to-r from-[#1D976C] to-[#93F9B9] opacity-40"></span>
                <span className="relative inline-flex items-center justify-center h-16 w-16 rounded-full bg-gradient-to-r from-[#1D976C] to-[#93F9B9]">
                  <FaCheck size={30} color="white" />
                </span>
              </span>
            </div>
            <p className="text-white text-6xl font-poppins font-bold">
              Payment{" "}
              <span className="bg-gradient-to-r from-[#1D976C] to-[#93F9B9] bg-clip-text text-transparent">
                Successful!
              </span>
            </p>
            {amountPaid && (
              <div className="flex items-center gap-1 text-2xl font-poppins font-semibold">
                <p>Your Account Balance has received: </p>

                <p className="bg-gradient-to-r from-[#FF8008] to-[#FFC837] bg-clip-text text-transparent">
                  {amountPaid.toLocaleString()}
                </p>
                <TbCoinFilled color="#FFC837" />
              </div>
            )}
            <div className="flex items-center justify-center p-5 gap-10">
              <div
                onClick={() =>
                  navigate("/media-player/management/transactions/top-up")
                }
                className="flex flex-col items-center gap-3"
              >
                <LiquidButton variant="default">Deposit More</LiquidButton>
              </div>
              <div
                onClick={() => handleContinue()}
                className="flex flex-col items-center gap-3"
              >
                <LiquidButton variant="colored">Continue</LiquidButton>
              </div>
            </div>
          </div>
        </>
      ) : (
        <div className="w-full h-full flex flex-col items-center justify-center gap-5">
          <div className="flex justify-center transition-all">
            <span className="relative flex h-16 w-16">
              <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-gradient-to-r from-[#ED213A] to-[#93291E] opacity-40"></span>
              <span className="relative inline-flex items-center justify-center h-16 w-16 rounded-full bg-gradient-to-r from-[#ED213A] to-[#93291E]">
                <FaRegDizzy size={50} color="white" />
              </span>
            </span>
          </div>
          <p className="text-white text-6xl font-poppins font-bold">
            Payment{" "}
            <span className="bg-gradient-to-r from-[#ED213A] to-[#93291E] bg-clip-text text-transparent">
              Failed!
            </span>
          </p>
          <p className="text-xl font-light italic text-[#d9d9d9]">
            Seems like something went wrong. Please try again.
          </p>
          <div className="flex items-center justify-center p-5 gap-10">
            <div
              onClick={() =>
                navigate("/media-player/management/transactions/top-up")
              }
              className="flex flex-col items-center gap-3"
            >
              <LiquidButton variant="default">Try Again</LiquidButton>
            </div>
            <div
              onClick={() => navigate("/media-player/discovery")}
              className="flex flex-col items-center gap-3"
            >
              <LiquidButton variant="colored">Home</LiquidButton>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default PaymentResultPage;

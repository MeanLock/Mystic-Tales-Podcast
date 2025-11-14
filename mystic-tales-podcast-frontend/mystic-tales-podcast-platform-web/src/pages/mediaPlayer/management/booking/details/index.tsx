/* eslint-disable @typescript-eslint/no-unused-vars */
// @ts-nocheck

import Loading from "@/components/loading";

import type { BookingDetailsFromAPI } from "@/core/types/booking";
import { skipToken } from "@reduxjs/toolkit/query";
import { useEffect, useState } from "react";
import { IoIosArrowBack } from "react-icons/io";
import { useNavigate, useParams } from "react-router-dom";
import BookingStatusTrackingBar from "./components/BookingStatusTrackingBar";
import { TimeUtil } from "@/core/utils/time";
import { TbCoinFilled } from "react-icons/tb";
import RequirementCard from "./components/RequirementCard";
import RequirementCardWithWordCount from "./components/RequirementCardWithWordCounts";
import {
  useConfirmAndDepositMutation,
  useGetBookingDetailQuery,
} from "@/core/services/booking/booking.service";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import { useSelector } from "react-redux";
import type { RootState } from "@/redux/store";
import {
  resolveFiles,
  type FileResolveConfig,
} from "@/core/utils/fileResolver.util";

export function renderDescriptionHTML(description: string | null) {
  if (!description) return "";

  // --- Tách link ---
  const linkRegex = /\$-\[link\]\$-([\s\S]*?)\$-\[link\]\$-/;
  const linkMatch = description.match(linkRegex);
  const link = linkMatch ? linkMatch[1].trim() : null;

  // --- Tách script ---
  const scriptRegex = /\$-\[script\]\$-([\s\S]*?)\$-\[script\]\$-/;
  const scriptMatch = description.match(scriptRegex);
  const scriptContent = scriptMatch ? scriptMatch[1].trim() : null;

  // --- Loại bỏ các phần đặc biệt khỏi phần mô tả còn lại ---
  let cleanDescription = description
    .replace(linkRegex, "")
    .replace(scriptRegex, "")
    .trim();

  // --- Tạo HTML ---
  let html = `<p>${cleanDescription}</p>`;

  if (link) {
    html += `
    <p><strong>Link</strong>: <a href="${link}" target="_blank" rel="noopener noreferrer">${link}</a></p>`;
  }

  if (scriptContent) {
    html += `
    <p><strong>Script</strong>:</p>
    <div style="margin-top: 10px; border: 1px solid #ccc; padding: 10px; border-radius: 5px; background-color: #f9f9f9;">
      ${scriptContent}
    </div>
    `;
  }

  return html.trim();
}

const fileConfig: FileResolveConfig[] = [
  {
    path: "PodcastBuddy.MainImageFileKey",
    type: "AccountPublic",
    output: "PodcastBuddy.ImageUrl",
  },
];

// Config cho requirements - sẽ được apply cho từng item trong array
const requirementFileConfig: FileResolveConfig[] = [
  {
    path: "RequirementDocumentFileKey",
    type: "BookingPublic",
    output: "RequirementDocumentFileUrl",
  },
];

const BookingDetailsPage = () => {
  const { id } = useParams<{ id: string }>();
  const user = useSelector((state: RootState) => state.auth.user);

  // STATES
  const [viewMode, setViewMode] = useState<string>("informations");
  const [resolvedBooking, setResolvedBooking] = useState<any>(null);
  const [isResolvingFiles, setIsResolvingFiles] = useState(false);
  const [isTopUpDialogOpen, setIsTopUpDialogOpen] = useState(false);
  const [neededTopUpAmount, setNeededTopUpAmount] = useState<number>(0);

  // HOOKS
  const navigate = useNavigate();

  // FUNCTIONS
  const getTotalWordCount = (booking: any) => {
    return booking.Booking.BookingRequirementFileList.reduce(
      (count: any, requirement: any) => count + requirement.WordCount,
      0
    );
  };

  // MỞ LẠI LOGIC SAU
  const {
    data: booking,
    isLoading,
    isError,
    refetch,
  } = useGetBookingDetailQuery(id ? { id: Number(id) } : skipToken);

  const [confirmDeal, { isLoading: isConfirming }] =
    useConfirmAndDepositMutation();

  // EFFECT: Resolve files khi có booking data
  useEffect(() => {
    const resolveBookingFiles = async () => {
      if (!booking) return;

      setIsResolvingFiles(true);
      try {
        // 1. Resolve main booking files (PodcastBuddy avatar)
        const { resolvedData: bookingWithAvatarRaw } = await resolveFiles(
          booking.Booking,
          fileConfig
        );

        // Cast về single object (không phải array) vì booking.Booking là object đơn
        const bookingWithAvatar = bookingWithAvatarRaw as BookingDetailsFromAPI;

        // 3. Gắn requirements đã resolved vào booking
        const finalBooking = {
          ...booking,
          Booking: {
            ...bookingWithAvatar,
          },
        };

        // console.log("Final Booking: ", finalBooking);

        setResolvedBooking(finalBooking);
      } catch (err) {
        console.error("Error resolving files:", err);
        // Fallback: dùng booking gốc nếu resolve fail
        setResolvedBooking(booking);
      } finally {
        setIsResolvingFiles(false);
      }
    };

    resolveBookingFiles();
  }, [booking]);

  const handleConfirmDeal = async () => {
    if (!booking || !user) return;

    const requiredDeposit = booking.Booking.Price / 2;
    const currentBalance = user.Balance || 0;

    if (currentBalance < requiredDeposit) {
      // Không đủ tiền -> mở dialog hỏi nạp thêm
      const needed = requiredDeposit - currentBalance;
      setNeededTopUpAmount(needed);
      setIsTopUpDialogOpen(true);

      // không làm gì nữa, chỉ mở dialog
      return;
    }

    // Đủ tiền -> confirm luôn
    try {
      await confirmDeal({
        BookingId: booking.Booking.Id,
        Amount: requiredDeposit,
      }).unwrap();

      // sau khi confirm thành công, refetch lại booking
      refetch && (await refetch());
    } catch (err) {
      alert((err as any)?.message || "Confirm failed");
    }
  };

  const handleConfirmTopUp = () => {
    if (!booking) return;

    // lưu số tiền cần nạp & backUrl rồi chuyển qua trang top-up
    localStorage.setItem("neededTopUpAmount", neededTopUpAmount.toString());
    localStorage.setItem(
      "paymentBackUrl",
      `/media-player/management/bookings/${booking.Booking.Id}`
    );

    setIsTopUpDialogOpen(false);
    navigate("/media-player/management/transactions/top-up");
  };

  // LOADING STATE
  if (isLoading || isResolvingFiles) {
    return (
      <div className="w-full h-full flex flex-col items-center justify-center gap-5">
        <Loading />
        <p className="text-[#D9D9D9] font-poppins font-bold">
          {isLoading
            ? "Getting Your Booking Details..."
            : "Loading Resources..."}
        </p>
      </div>
    );
  }

  // NO DATA STATE
  if (!resolvedBooking) {
    return (
      <div className="w-full h-full flex items-center justify-center">
        <p className="text-white font-poppins">
          Seems like the booking you're looking for doesn't exist
        </p>
      </div>
    );
  }

  if (isError) {
    <div>
      <p>Somethings wrong happened, please try again later</p>
    </div>;
  }

  if (!booking) {
    return (
      <div>
        <p>Seems like the booking you looking for doesn't exists</p>
      </div>
    );
  } else {
    return (
      <div className="w-full p-8 flex flex-col gap-5">
        <div
          onClick={() => navigate(-1)}
          className="w-full font-poppins cursor-pointer flex items-center hover:underline text-white"
        >
          <IoIosArrowBack size={20} />
          <p className="font-light font-poppins">Back</p>
        </div>

        <p className="text-3xl font-poppins font-bold text-white">
          Booking Details: #{booking.Booking.Id}
        </p>

        <div className="w-full flex flex-col">
          {/* Status Tracking */}
          <div className="w-full px-5 flex items-center bg-transparent  backdrop-blur-[1px] shadow-2xl border-b-[#d9d9d9]">
            <BookingStatusTrackingBar
              currentStatus={booking.Booking.CurrentStatus}
              statusTracking={booking.Booking.StatusTracking}
            />
          </div>

          <div className="w-full flex items-center gap-5 pt-5 pb-3">
            <div
              onClick={() => setViewMode("informations")}
              className={`transition-all duration-500 ease-out hover:-translate-y-1 cursor-pointer rounded-full px-5 py-2 border-2 ${
                viewMode === "informations"
                  ? "border-mystic-green bg-mystic-green/20 "
                  : "border-[#d9d9d9] bg-[#d9d9d9]/20"
              }`}
            >
              <p className="font-bold text-white">Informations</p>
            </div>
            {booking.Booking.CurrentStatus.Id === 2 && (
              <div
                onClick={() => setViewMode("dealing")}
                className={`transition-all duration-500 ease-out hover:-translate-y-1 cursor-pointer rounded-full px-5 py-2 border-2 ${
                  viewMode === "dealing"
                    ? "border-mystic-green bg-mystic-green/20 "
                    : "border-[#d9d9d9] bg-[#d9d9d9]/20"
                }`}
              >
                <p className="font-bold text-white">Quotation Dealing</p>
              </div>
            )}
            {booking.Booking.CurrentStatus.Id >= 5 && (
              <div
                onClick={() => setViewMode("producingRequest")}
                className={`transition-all duration-500 ease-out hover:-translate-y-1 cursor-pointer rounded-full px-5 py-2 border-2 ${
                  viewMode === "producingRequest"
                    ? "border-mystic-green bg-mystic-green/20 "
                    : "border-[#d9d9d9] bg-[#d9d9d9]/20"
                }`}
              >
                <p className="font-bold text-white">Producing Requests</p>
              </div>
            )}
          </div>

          {/* Informations Mode */}
          {viewMode === "informations" && (
            <div className="w-full flex flex-col p-5 gap-5">
              <p className="font-poppins font-bold text-white text-2xl">
                Booking Informations
              </p>
              {/* Title */}
              <div className="w-full flex flex-col">
                <p className="font-poppins text-white font-semibold text-lg">
                  Title
                </p>
                <div className="py-3 text-white border-b-[1px] border-white">
                  <p>{resolvedBooking.Booking.Title}</p>
                </div>
              </div>
              {/* Deadline & Price & Podcaster */}
              <div className="w-full grid grid-cols-3">
                {/* Deadline */}
                <div className="flex flex-col">
                  <p className="font-poppins font-semibold text-white text-lg">
                    Deadline
                  </p>
                  <div className="w-1/2 py-2 text-white border-b-[1px] border-white">
                    {resolvedBooking.Booking.Deadline ? (
                      <p>
                        {TimeUtil.formatDate(
                          resolvedBooking.Booking.Deadline,
                          "DD/MM/YYYY"
                        )}
                      </p>
                    ) : (
                      <p>Not Yet</p>
                    )}
                  </div>
                </div>
                {/* Price */}
                <div className="flex flex-col">
                  <p className="font-poppins text-white font-semibold text-lg">
                    Price
                  </p>
                  <div className="w-1/2 flex items-center gap-1 py-2 text-white border-b-[1px]  border-white">
                    {resolvedBooking.Booking.Price ? (
                      <>
                        <p>{booking.Booking.Price.toLocaleString()}</p>
                        <TbCoinFilled />
                      </>
                    ) : (
                      <p>Not Yet</p>
                    )}
                  </div>
                </div>
                {/* Podcaster */}
                <div className="flex flex-col">
                  <p className="font-poppins text-white font-semibold text-lg">
                    Podcaster
                  </p>
                  <div className="w-1/2 flex items-center gap-1 py-2 text-white border-b-[1px]  border-white">
                    <img
                      src={
                        resolvedBooking.Booking.PodcastBuddy.ImageUrl
                          ? resolvedBooking.Booking.PodcastBuddy.ImageUrl
                          : "/images/unknown/user.jpg"
                      }
                      className="w-8 h-8 rounded-full aspect-square object-cover"
                    />
                    <p className="font-semibold">
                      {resolvedBooking.Booking.PodcastBuddy.FullName}
                    </p>
                  </div>
                </div>
              </div>

              {/* Description */}
              <div className="w-full flex flex-col">
                <p className="font-poppins text-white font-semibold text-lg">
                  Description
                </p>
                <div className="py-3 text-white border-b-[1px]  border-white">
                  <div
                    dangerouslySetInnerHTML={{
                      __html: renderDescriptionHTML(
                        booking.Booking.Description
                      ),
                    }}
                  />
                </div>
              </div>

              {/* Requirements List */}
              {resolvedBooking.Booking.BookingRequirementFileList.map(
                (requirement: any, index: number) => (
                  <RequirementCard
                    key={`${index}-${requirement.Id}`}
                    requirement={requirement}
                  />
                )
              )}
            </div>
          )}

          {viewMode === "dealing" && (
            <div className="w-full flex flex-col p-5 gap-5">
              <p className="font-poppins font-bold text-white text-2xl">
                Podcaster Dealing
              </p>

              <div className="w-full flex flex-col gap-5">
                {/* Info Cards */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
                  <div className="bg-white/10 backdrop-blur-sm border border-slate-600/30 rounded-lg p-4">
                    <p className="text-sm text-white font-bold font-poppins mb-1">
                      Deadline Days
                    </p>
                    <p className="text-2xl font-bold text-white">
                      <span className="text-mystic-green">
                        {booking.Booking.DeadlineDays}{" "}
                      </span>{" "}
                      days
                    </p>
                  </div>

                  <div className="bg-white/10  backdrop-blur-sm border border-slate-600/30 rounded-lg p-4">
                    <p className="text-sm text-white font-bold font-poppins mb-1">
                      Total Word Count
                    </p>
                    <p className="text-2xl font-bold text-white">
                      <span className="text-mystic-green">
                        {getTotalWordCount(booking).toLocaleString()}
                      </span>{" "}
                      words
                    </p>
                  </div>

                  <div className="bg-white/10 backdrop-blur-sm border border-slate-600/30 rounded-lg p-4">
                    <p className="text-sm text-white font-bold font-poppins mb-1">
                      Total Price
                    </p>
                    <div className="flex items-center gap-2">
                      <p className="text-2xl font-bold text-white">
                        <span className="text-mystic-green">
                          {booking.Booking.Price.toLocaleString()}
                        </span>
                      </p>
                      <TbCoinFilled className="w-5 h-5 text-mystic-green" />
                    </div>
                  </div>
                </div>
                {resolvedBooking.Booking.BookingRequirementFileList.map(
                  (requirement: any, index: number) => (
                    <RequirementCardWithWordCount
                      key={index}
                      requirement={requirement}
                    />
                  )
                )}
              </div>
              <div className="w-full flex items-center gap-5 justify-end">
                <div className="cursor-pointer px-5 font-bold py-2 bg-red-600 rounded-sm text-white font-poppins shadow-xl transition-all duration-500 ease-out hover:-translate-y-1">
                  Cancel
                </div>
                <div
                  onClick={() => handleConfirmDeal()}
                  className="cursor-pointer px-5 font-bold py-2 bg-mystic-green rounded-sm text-white font-poppins shadow-xl transition-all duration-500 ease-out hover:-translate-y-1"
                >
                  {isConfirming ? "Confirming..." : "Confirm Deal"}
                </div>
              </div>
            </div>
          )}
        </div>

        <Dialog open={isTopUpDialogOpen} onOpenChange={setIsTopUpDialogOpen}>
          <DialogContent className="bg-black/50 backdrop-blur-sm text-white border border-white/10">
            <DialogHeader>
              <DialogTitle className="text-xl font-bold">
                Account Balance Not Enough!
              </DialogTitle>
              <DialogDescription className="text-slate-300">
                Your account balance is not sufficient to place a deposit for
                this booking.
              </DialogDescription>
            </DialogHeader>

            <div className="mt-4 space-y-2 text-sm">
              <p>
                <span className="text-slate-400">Needed Amount: </span>
                <span className="font-semibold text-mystic-green">
                  {booking.Booking.Price / 2
                    ? (booking.Booking.Price / 2).toLocaleString()
                    : 0}{" "}
                  Coins
                </span>
              </p>
              <p>
                <span className="text-slate-400">Current Balance: </span>
                <span className="font-semibold text-[#d9d9d9]">
                  {user?.Balance?.toLocaleString() ?? 0} Coins
                </span>
              </p>
              <p>
                <span className="text-slate-400">
                  Additional Top-Up Amount:{" "}
                </span>
                <span className="font-semibold text-yellow-300">
                  {neededTopUpAmount.toLocaleString()} Coins
                </span>
              </p>
            </div>

            <DialogFooter className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                onClick={() => setIsTopUpDialogOpen(false)}
                className="px-4 py-2 rounded-md border border-slate-600 text-sm text-slate-200 hover:bg-slate-800 transition"
              >
                Later
              </button>
              <button
                type="button"
                onClick={handleConfirmTopUp}
                className="px-4 py-2 rounded-md bg-mystic-green text-sm font-semibold text-black hover:bg-mystic-green/90 transition"
              >
                Top Up
              </button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>
    );
  }

  // if (error) {
  //   <div>
  //     <p>Có lỗi xảy ra</p>
  //   </div>;
  // }
};

export default BookingDetailsPage;

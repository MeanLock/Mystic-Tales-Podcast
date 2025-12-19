import Loading from "@/components/loading";
import { skipToken } from "@reduxjs/toolkit/query";
import { useState } from "react";
import { IoIosArrowBack } from "react-icons/io";
import { useNavigate, useParams } from "react-router-dom";
import BookingStatusTrackingBar from "./components/BookingStatusTrackingBar";
import { TimeUtil } from "@/core/utils/time";
import RequirementCard from "./components/RequirementCard";
import RequirementCardWithWordCount from "./components/RequirementCardWithWordCounts";
import {
  useAcceptBookingAndPayTheRestMutation,
  useCancelBookingManuallyMutation,
  useConfirmAndDepositMutation,
  useCreateCancelBookingRequestMutation,
  useGetBookingDetailQuery,
  useGetBookingProducingRequestDetailsQuery,
  useGetManunalCancelReasonOptionsQuery,
  useSendNewEditRequestMutation,
} from "@/core/services/booking/booking.service";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import { useDispatch, useSelector } from "react-redux";
import type { RootState } from "@/redux/store";
import { IoPause, IoPlay } from "react-icons/io5";
import { LiquidButton } from "@/components/ui/shadcn-io/liquid-button";
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/components/ui/select";
import { Input } from "@/components/ui/input";
import { setError } from "@/redux/slices/errorSlice/errorSlice";
import { usePlayer } from "@/core/services/player/usePlayer";
import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";
import MTPCoinOutline from "@/components/coinIcons/CoinIconOutline";

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

const BookingDetailsPage = () => {
  const { id } = useParams<{ id: string }>();
  const user = useSelector((state: RootState) => state.auth.user);

  const { play, pause, playBookingTrack, state: playerUiState } = usePlayer();
  const { data: bookingManualCancelReasons } =
    useGetManunalCancelReasonOptionsQuery();

  // STATES
  const [viewMode, setViewMode] = useState<string>("informations");
  const [isTopUpDialogOpen, setIsTopUpDialogOpen] = useState(false);
  const [neededTopUpAmount, setNeededTopUpAmount] = useState<number>(0);
  const [selectedProducingRequestId, setSelectedProducingRequestId] = useState<
    string | null
  >(null);
  const [isEditRequestDialogOpen, setIsEditRequestDialogOpen] = useState(false);
  const [selectedTrackIds, setSelectedTrackIds] = useState<string[]>([]);
  const [editNote, setEditNote] = useState("");
  const [deadlineDayCount, setDeadlineDayCount] = useState<number>(1);
  const [
    isCancelBookingConfirmationDialogOpen,
    setIsCancelBookingConfirmationDialogOpen,
  ] = useState(false);
  // Cancel booking states
  const [cancelDescription, setCancelDescription] = useState("");
  const [cancelReason, setCancelReason] = useState("");
  const [customCancelReason, setCustomCancelReason] = useState("");
  const [cancelType, setCancelType] = useState<1 | 2>(1);

  // HOOKS
  const navigate = useNavigate();

  // FUNCTIONS
  const getTotalWordCount = (booking: any) => {
    return booking.Booking.BookingRequirementFileList.reduce(
      (count: any, requirement: any) => count + requirement.WordCount,
      0
    );
  };

  const {
    data: booking,
    isLoading,
    isError,
    refetch,
  } = useGetBookingDetailQuery(id ? { id: Number(id) } : skipToken);

  const [confirmDeal, { isLoading: isConfirming }] =
    useConfirmAndDepositMutation();

  const [sendNewEditRequest, { isLoading: isSendingEditRequest }] =
    useSendNewEditRequestMutation();

  const [cancelManuallyBooking] = useCancelBookingManuallyMutation();
  const [createCancelBookingRequest] = useCreateCancelBookingRequestMutation();
  const [acceptBooking] = useAcceptBookingAndPayTheRestMutation();

  // LẤY CHI TIẾT PRODUCING REQUEST NẾU CẦN
  const {
    data: producingRequestDetails,
    isLoading: isLoadingRequestDetails,
    refetch: refetchProducingRequestDetails,
  } = useGetBookingProducingRequestDetailsQuery(
    selectedProducingRequestId
      ? { BookingProducingRequestId: selectedProducingRequestId }
      : skipToken,
    {
      refetchOnMountOrArgChange: true,
      refetchOnFocus: true,
      refetchOnReconnect: true,
    }
  );

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
      setViewMode("informations");
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

  const handleViewRequestDetails = (requestId: string) => {
    const selectedId = selectedProducingRequestId;
    if (selectedId === requestId) {
      setSelectedProducingRequestId(null);
    } else {
      setSelectedProducingRequestId(requestId);
    }
  };

  // const handleCloseProducingRequestDialog = () => {
  //   setSelectedProducingRequestId(null);
  // };

  const handleOpenCreateEditRequestForm = () => {
    setIsEditRequestDialogOpen(true);
    setSelectedTrackIds([]);
    setEditNote("");
    setDeadlineDayCount(1);
  };

  const handleToggleTrackSelection = (trackId: string) => {
    setSelectedTrackIds((prev) =>
      prev.includes(trackId)
        ? prev.filter((id) => id !== trackId)
        : [...prev, trackId]
    );
  };

  const handleSubmitEditRequest = async () => {
    if (!booking) return;

    // Validation
    if (selectedTrackIds.length === 0) {
      alert("Please select at least one track to edit");
      return;
    }
    if (!editNote.trim()) {
      alert("Please enter a note for the edit request");
      return;
    }
    if (deadlineDayCount <= 0) {
      alert("Deadline day count must be greater than 0");
      return;
    }

    try {
      await sendNewEditRequest({
        BookingId: booking.Booking.Id,
        Note: editNote.trim(),
        DeadlineDayCount: deadlineDayCount,
        BookingPodcastTrackIds: selectedTrackIds,
      }).unwrap();

      // Success - close dialog and refetch
      setIsEditRequestDialogOpen(false);
      await refetch();
    } catch (error) {
      alert(`Error sending edit request: ${error}`);
    }
  };

  const handleCancelBooking = () => {
    // Check điều kiện hủy booking
    let description = "";

    if (!booking || !booking.Booking) {
      return;
    }

    const bookingData = booking.Booking;
    const statusId = bookingData.CurrentStatus.Id;

    // Status 1-4: Quotation stages - no penalty
    if (statusId >= 1 && statusId <= 4) {
      setCancelType(1);
      description = "You will not be charged any penalty fee.";
    }
    // Status 5+: Producing stages
    else if (statusId >= 5) {
      // Nếu chỉ có 1 producing request
      if (bookingData.BookingProducingRequestList.length === 1) {
        // Check xem Podcast Buddy có trễ deadline không
        const deadlineString =
          bookingData.BookingProducingRequestList[0].Deadline;
        const deadlineDate = new Date(deadlineString);
        const today = new Date();

        if (today > deadlineDate) {
          // Podcast Buddy trễ deadline -> cancel type 1, không phạt
          description =
            "Seems like Podcast Buddy is late on delivering your request. You will be returned 100% of the booking deposit.";
          setCancelType(1);
        } else {
          // Chưa trễ deadline nhưng đang producing -> cần review
          description =
            "We will review your cancel request and decide the penalty fee based on the progress of your booking.";
          setCancelType(2);
        }
      }
      // Có nhiều hơn 1 producing request -> phức tạp, cần review
      else if (bookingData.BookingProducingRequestList.length > 1) {
        description =
          "We will review your cancel request and decide the penalty fee based on the progress of your booking.";
        setCancelType(2);
      }
      // Không có producing request nào nhưng status >= 5 -> cho cancel type 1
      else {
        description = "You will not be charged any penalty fee.";
        setCancelType(1);
      }
    }
    // Các status khác (8-12: các status cancel/complete) -> không cho cancel
    else {
      description = "This booking cannot be cancelled at this stage.";
      setCancelType(1);
    }

    setCancelDescription(description);
    setIsCancelBookingConfirmationDialogOpen(true);
  };

  const handleSetCancelReason = (value: string) => {
    setCancelReason(value);
  };

  const dispatch = useDispatch();

  const handleConfirmCancelBooking = async () => {
    let finalCancelReason = cancelReason;
    if (cancelReason === "other") {
      finalCancelReason = customCancelReason;
    }
    // Gọi API hủy booking ở đây, truyền vào cancelReason
    try {
      if (cancelType === 1) {
        await cancelManuallyBooking({
          BookingId: booking!.Booking.Id,
          CancelReason: finalCancelReason,
        }).unwrap();
      } else if (cancelType === 2) {
        await createCancelBookingRequest({
          BookingId: booking!.Booking.Id,
          CancelReason: finalCancelReason,
        }).unwrap();
      }
      // Sau khi hủy xong thì đóng dialog và refetch lại booking
      setIsCancelBookingConfirmationDialogOpen(false);
      await refetch();
    } catch (error) {
      dispatch(
        setError({
          message: error as string,
          autoClose: 10,
        })
      );
    }
  };

  const handlePlayPauseBookingPodcastTrack = async (trackId: string) => {
    if (!booking) {
      return;
    } else {
      if (
        playerUiState.currentAudio &&
        playerUiState.currentAudio.id === trackId
      ) {
        if (playerUiState.isPlaying) {
          pause();
        } else {
          play();
        }
      } else {
        await playBookingTrack({
          bookingId: booking.Booking.Id,
          bookingTrackId: trackId,
        });
        // Tự gọi lại details để cập nhật lượt nghe
        await refetchProducingRequestDetails();
      }
    }
  };

  const handleAcceptBooking = async () => {
    if (!booking) return;
    try {
      await acceptBooking({ BookingId: booking.Booking.Id }).unwrap();
      // sau khi accept thành công, refetch navigate lại trang bookings
      navigate("/media-player/management/bookings");
    } catch (err) {
      dispatch(
        setError({
          message: (err as string) || "Accepting booking failed",
          autoClose: 10,
        })
      );
    }
  };

  // LOADING STATE
  if (isLoading) {
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
  if (!booking) {
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

          <div className="w-full flex items-center pt-5 pb-3 justify-between">
            <div className="flex items-center gap-5 ">
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
            <div className="flex-1 flex items-center justify-end">
              {booking.Booking.CurrentStatus.Id !== 4 &&
                booking.Booking.CurrentStatus.Id !== 8 &&
                booking.Booking.CurrentStatus.Id !== 9 &&
                booking.Booking.CurrentStatus.Id !== 10 &&
                booking.Booking.CurrentStatus.Id !== 11 &&
                booking.Booking.CurrentStatus.Id !== 12 && (
                  <LiquidButton
                    onClick={() => handleCancelBooking()}
                    variant="danger"
                  >
                    <p>Cancel</p>
                  </LiquidButton>
                )}
            </div>
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
                <div className="py-3 text-white border-b border-white">
                  <p>{booking.Booking.Title}</p>
                </div>
              </div>
              {/* Deadline & Price & Podcaster */}
              <div className="w-full grid grid-cols-3">
                {/* Deadline */}
                <div className="flex flex-col">
                  <p className="font-poppins font-semibold text-white text-lg">
                    Deadline
                  </p>
                  <div className="w-1/2 py-2 text-white border-b border-white">
                    {booking.Booking.Deadline ? (
                      <p>
                        {TimeUtil.formatDate(
                          booking.Booking.Deadline,
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
                  <div className="w-1/2 flex items-center gap-1 py-2 text-white border-b  border-white">
                    {booking.Booking.Price ? (
                      <>
                        <p>{booking.Booking.Price.toLocaleString()}</p>
                        <MTPCoinOutline size={16} color="#fff" />
                      </>
                    ) : (
                      <p>Not Yet</p>
                    )}
                  </div>
                </div>
                {/* Podcaster */}
                <div className="flex flex-col">
                  <p className="font-poppins text-white font-semibold text-lg">
                    Buddy
                  </p>
                  <div className="w-1/2 flex items-center gap-1 py-2 text-white border-b  border-white">
                    <AutoResolveImage
                      FileKey={booking.Booking.PodcastBuddy.MainImageFileKey}
                      type="AccountPublicSource"
                      className="w-8 h-8 rounded-full aspect-square object-cover"
                    />
                    <p className="font-semibold line-clamp-1">
                      {booking.Booking.PodcastBuddy.FullName}
                    </p>
                  </div>
                </div>
              </div>

              {/* Description */}
              <div className="w-full flex flex-col">
                <p className="font-poppins text-white font-semibold text-lg">
                  Description
                </p>
                <div className="py-3 text-white border-b border-white">
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
              {/* Sort theo Order requirement.Order */}
              {[...booking.Booking.BookingRequirementFileList]
                .sort((a, b) => a.Order - b.Order)
                .map((requirement: any, index: number) =>
                  requirement.WordCount === null ||
                  requirement.WordCount === 0 ||
                  requirement.WordCount === undefined ? (
                    <RequirementCard
                      key={`${index}-${requirement.Id}`}
                      requirement={requirement}
                    />
                  ) : (
                    <RequirementCardWithWordCount
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
                        {booking.Booking.DeadlineDays
                          ? booking.Booking.DeadlineDays
                          : "Not yet"}{" "}
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
                      <MTPCoinOutline size={20} color="#aee339" />
                    </div>
                  </div>
                </div>
                {[...booking.Booking.BookingRequirementFileList]
                  .sort((a, b) => a.Order - b.Order)
                  .map((requirement: any, index: number) => (
                    <RequirementCardWithWordCount
                      key={index}
                      requirement={requirement}
                    />
                  ))}
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

          {viewMode === "producingRequest" && (
            <div className="w-full flex flex-col gap-5 p-5">
              <p className="font-poppins font-bold text-white text-2xl">
                Producing Requests
              </p>
              {booking.Booking.BookingProducingRequestList.length === 0 ? (
                <div className="w-full p-8 bg-white/5 backdrop-blur-sm rounded-xl border border-white/10 text-center">
                  <p className="text-white/70">No producing requests yet</p>
                </div>
              ) : (
                (() => {
                  return booking.Booking.BookingProducingRequestList.map(
                    (request, index) => {
                      return (
                        <div
                          key={request.Id}
                          className="w-full p-6 bg-white/5 backdrop-blur-sm rounded-xl border border-white/10 hover:border-mystic-green/50 transition-all duration-300"
                        >
                          <div className="flex items-start justify-between">
                            <div className="flex-1 space-y-3">
                              <div className="flex items-center gap-3">
                                <span className="px-3 py-1 bg-mystic-green/20 text-mystic-green rounded-full text-sm font-semibold">
                                  Request #
                                  {booking.Booking.BookingProducingRequestList
                                    .length - index}
                                </span>
                                {request.IsAccepted === true ? (
                                  <span className="px-3 py-1 bg-green-500/20 text-green-400 rounded-full text-sm font-semibold">
                                    Accepted
                                  </span>
                                ) : request.IsAccepted === false ? (
                                  <span className="px-3 py-1 bg-red-500/20 text-red-200 rounded-full text-sm font-semibold">
                                    Rejected
                                  </span>
                                ) : (
                                  <span className="px-3 py-1 bg-yellow-500/20 text-yellow-400 rounded-full text-sm font-semibold">
                                    Pending
                                  </span>
                                )}
                              </div>

                              <div className="grid grid-cols-2 gap-4">
                                <div>
                                  <p className="text-white/60 text-sm">
                                    Created At
                                  </p>
                                  <p className="text-white font-semibold">
                                    {TimeUtil.formatDate(
                                      request.CreatedAt,
                                      "DD/MM/YYYY"
                                    )}
                                  </p>
                                </div>
                                <div>
                                  <p className="text-white/60 text-sm">
                                    Deadline
                                  </p>
                                  <p className="text-white font-semibold">
                                    {/* {TimeUtil.formatISOStringToDate(
                                  request.Deadline
                                )} */}
                                    {TimeUtil.formatDate(
                                      request.Deadline,
                                      "DD/MM/YYYY"
                                    )}
                                  </p>
                                </div>
                              </div>

                              {request.Note && (
                                <div>
                                  <p className="text-white/60 text-sm">Note</p>
                                  <p className="text-white">{request.Note}</p>
                                </div>
                              )}

                              {request.RejectReason && (
                                <div>
                                  <p className="text-red-400 text-sm">
                                    Reject Reason
                                  </p>
                                  <p className="text-white">
                                    {request.RejectReason}
                                  </p>
                                </div>
                              )}

                              {request.FinishedAt && (
                                <div>
                                  <p className="text-white/60 text-sm">
                                    Finished At
                                  </p>
                                  <p className="text-white font-semibold">
                                    {/* {TimeUtil.formatISOStringToDate(
                                  
                                )} */}
                                    {TimeUtil.formatDate(
                                      request.FinishedAt,
                                      "DD/MM/YYYY"
                                    )}
                                  </p>
                                </div>
                              )}
                            </div>

                            {booking.Booking.CurrentStatus.Id !== 8 &&
                              booking.Booking.CurrentStatus.Id !== 9 &&
                              booking.Booking.CurrentStatus.Id !== 10 &&
                              booking.Booking.CurrentStatus.Id !== 11 &&
                              booking.Booking.CurrentStatus.Id !== 12 && (
                                <button
                                  onClick={() =>
                                    handleViewRequestDetails(request.Id)
                                  }
                                  className="px-4 py-2 bg-mystic-green hover:bg-mystic-green/80 text-black font-semibold rounded-lg transition-all duration-300 hover:scale-105"
                                >
                                  {selectedProducingRequestId === request.Id
                                    ? "Hide Details"
                                    : "View Details"}
                                </button>
                              )}
                          </div>

                          {/* Details */}
                          {selectedProducingRequestId === request.Id && (
                            <div className="w-full py-10 flex items-center justify-center">
                              {isLoadingRequestDetails ? (
                                <div className="w-full h-64 flex items-center justify-center flex-col gap-5">
                                  <Loading />
                                  <p className="font-poppins text-[#D9D9D9] font-bold">
                                    Loading request details...
                                  </p>
                                </div>
                              ) : (
                                producingRequestDetails && (
                                  <div className="w-full h-full flex flex-col gap-6">
                                    <div className="grid grid-cols-4 gap-4">
                                      <div className="p-4 bg-white/5 rounded-lg">
                                        <p className="text-white/60 text-sm mb-1">
                                          Status
                                        </p>
                                        <p
                                          className={`font-semibold ${
                                            producingRequestDetails
                                              .BookingProducingRequest
                                              .IsAccepted
                                              ? "text-mystic-green"
                                              : producingRequestDetails.BookingProducingRequest
                                              ? "text-red-300"
                                              : "text-blue-400"
                                          }`}
                                        >
                                          {producingRequestDetails
                                            .BookingProducingRequest.IsAccepted
                                            ? "Accepted"
                                            : producingRequestDetails
                                                .BookingProducingRequest
                                                .RejectReason
                                            ? "Rejected"
                                            : "Pending"}
                                        </p>
                                      </div>
                                      <div className="p-4 bg-white/5 rounded-lg">
                                        <p className="text-white/60 text-sm mb-1">
                                          Deadline Days
                                        </p>
                                        {producingRequestDetails
                                          .BookingProducingRequest
                                          .DeadlineDays ? (
                                          <p className="text-white font-semibold">
                                            {
                                              producingRequestDetails
                                                .BookingProducingRequest
                                                .DeadlineDays
                                            }{" "}
                                            days
                                          </p>
                                        ) : (
                                          <p className="text-[#d9d9d9] font-semibold">
                                            Unknown
                                          </p>
                                        )}
                                      </div>
                                      <div className="p-4 bg-white/5 rounded-lg">
                                        <p className="text-white/60 text-sm mb-1">
                                          Created At
                                        </p>
                                        <p className="text-white font-semibold">
                                          {TimeUtil.formatDate(
                                            producingRequestDetails
                                              .BookingProducingRequest
                                              .CreatedAt,
                                            "hh:mm:ssDD/MM/YYYY"
                                          )}
                                        </p>
                                      </div>
                                      <div className="p-4 bg-white/5 rounded-lg">
                                        <p className="text-white/60 text-sm mb-1">
                                          Deadline
                                        </p>
                                        <p className="text-white font-semibold">
                                          {TimeUtil.formatDate(
                                            producingRequestDetails
                                              .BookingProducingRequest.Deadline,
                                            "hh:mm:ssDD/MM/YYYY"
                                          )}
                                        </p>
                                      </div>
                                    </div>

                                    {/* Note */}
                                    {producingRequestDetails
                                      .BookingProducingRequest.Note && (
                                      <div className="p-4 bg-white/5 rounded-lg">
                                        <p className="text-white/60 text-sm mb-2">
                                          Note
                                        </p>
                                        <div
                                          className="text-white text-base leading-relaxed"
                                          dangerouslySetInnerHTML={{
                                            __html: renderDescriptionHTML(
                                              producingRequestDetails
                                                .BookingProducingRequest.Note
                                            ),
                                          }}
                                        />
                                      </div>
                                    )}

                                    {/* Reject Reason */}
                                    {producingRequestDetails
                                      .BookingProducingRequest.RejectReason && (
                                      <div className="p-4 bg-red-500/10 border border-red-500/20 rounded-lg">
                                        <p className="text-red-400 text-sm mb-2 font-semibold">
                                          Reject Reason
                                        </p>
                                        <p className="text-white">
                                          {
                                            producingRequestDetails
                                              .BookingProducingRequest
                                              .RejectReason
                                          }
                                        </p>
                                      </div>
                                    )}

                                    {/* Podcast Tracks */}
                                    {producingRequestDetails
                                      .BookingProducingRequest
                                      .BookingPodcastTracks &&
                                      producingRequestDetails
                                        .BookingProducingRequest
                                        .BookingPodcastTracks.length > 0 && (
                                        <div>
                                          <h3 className="text-lg font-semibold text-white mb-3">
                                            Podcast Tracks (
                                            {
                                              producingRequestDetails
                                                .BookingProducingRequest
                                                .BookingPodcastTracks.length
                                            }
                                            )
                                          </h3>
                                          <div className="space-y-3">
                                            {/* Cần sort theo track.BookingRequirement.Order */}
                                            {[
                                              ...producingRequestDetails
                                                .BookingProducingRequest
                                                .BookingPodcastTracks,
                                            ]
                                              .sort(
                                                (a, b) =>
                                                  a.BookingRequirement.Order -
                                                  b.BookingRequirement.Order
                                              )
                                              .map((track) => (
                                                <div
                                                  key={track.Id}
                                                  className="p-4 bg-white/5 border border-white/10 rounded-lg"
                                                >
                                                  <div className="flex items-center justify-between mb-2">
                                                    <span className="text-mystic-green font-semibold">
                                                      Track #
                                                      {
                                                        track.BookingRequirement
                                                          .Order
                                                      }
                                                      :{" "}
                                                      {
                                                        track.BookingRequirement
                                                          .Name
                                                      }
                                                    </span>
                                                    <span className="text-white/60 text-sm">
                                                      {Math.floor(
                                                        track.AudioLength / 60
                                                      )}
                                                      :
                                                      {String(
                                                        track.AudioLength % 60
                                                      ).padStart(2, "0")}{" "}
                                                      s
                                                    </span>
                                                  </div>
                                                  <div className="grid grid-cols-4 gap-3 text-sm">
                                                    <div>
                                                      <p className="text-white/60">
                                                        File Size
                                                      </p>
                                                      <p className="text-white">
                                                        {(
                                                          track.AudioFileSize /
                                                          1024 /
                                                          1024
                                                        ).toFixed(2)}{" "}
                                                        MB
                                                      </p>
                                                    </div>
                                                    <div>
                                                      <p className="text-white/60">
                                                        Preview Slots
                                                      </p>
                                                      <p className="text-white">
                                                        {
                                                          track.RemainingPreviewListenSlot
                                                        }
                                                      </p>
                                                    </div>
                                                    <div>
                                                      <p className="text-white/60">
                                                        Requirement ID
                                                      </p>
                                                      <p className="text-white text-xs truncate">
                                                        {
                                                          track
                                                            .BookingRequirement
                                                            .Id
                                                        }
                                                      </p>
                                                    </div>
                                                    {track.RemainingPreviewListenSlot >
                                                      0 && (
                                                      <div className="flex items-center justify-end">
                                                        <div
                                                          onClick={() =>
                                                            handlePlayPauseBookingPodcastTrack(
                                                              track.Id
                                                            )
                                                          }
                                                          className="px-3 py-2 rounded-full flex items-center justify-center bg-mystic-green text-black font-bold transition-all duration-500 ease-out hover:-translate-y-1 gap-2 cursor-pointer"
                                                        >
                                                          {playerUiState.isPlaying &&
                                                          playerUiState.currentAudio &&
                                                          playerUiState
                                                            .currentAudio.id ===
                                                            track.Id ? (
                                                            <IoPause />
                                                          ) : (
                                                            <IoPlay />
                                                          )}
                                                          <p>
                                                            {playerUiState.isPlaying &&
                                                            playerUiState.currentAudio &&
                                                            playerUiState
                                                              .currentAudio
                                                              .id === track.Id
                                                              ? "Pause Track"
                                                              : "Play Track"}
                                                          </p>
                                                        </div>
                                                      </div>
                                                    )}
                                                  </div>
                                                </div>
                                              ))}
                                          </div>
                                        </div>
                                      )}

                                    {/* Edit Requirements */}
                                    {producingRequestDetails
                                      .BookingProducingRequest
                                      .EditRequirementList &&
                                      producingRequestDetails
                                        .BookingProducingRequest
                                        .EditRequirementList.length > 0 && (
                                        <div>
                                          <h3 className="text-lg font-semibold text-white mb-3">
                                            Edit Requirements (
                                            {
                                              producingRequestDetails
                                                .BookingProducingRequest
                                                .EditRequirementList.length
                                            }
                                            )
                                          </h3>
                                          <div className="space-y-2">
                                            {producingRequestDetails.BookingProducingRequest.EditRequirementList.map(
                                              (edit, idx) => (
                                                <div
                                                  key={edit.Id}
                                                  className="p-3 bg-yellow-500/10 border border-yellow-500/20 rounded-lg"
                                                >
                                                  <p className="text-yellow-400 font-semibold text-sm">
                                                    Edit #{idx + 1}: {edit.Name}
                                                  </p>
                                                </div>
                                              )
                                            )}
                                          </div>
                                        </div>
                                      )}
                                  </div>
                                )
                              )}
                            </div>
                          )}

                          {/* Action Buttons */}
                          {producingRequestDetails &&
                            producingRequestDetails.BookingProducingRequest
                              .BookingPodcastTracks &&
                            producingRequestDetails.BookingProducingRequest
                              .BookingPodcastTracks.length > 0 &&
                            booking.Booking.CurrentStatus.Id === 6 &&
                            producingRequestDetails.BookingProducingRequest
                              .Id === request.Id && (
                              <div className="w-full  flex items-center justify-end gap-3">
                                <button
                                  onClick={handleOpenCreateEditRequestForm}
                                  className="px-6 py-2 bg-blue-400/10 hover:bg-blue-400/20 text-blue-400 border-blue-400 border rounded-lg transition-all duration-300"
                                >
                                  Send New Edit Request
                                </button>
                                <button
                                  onClick={() => handleAcceptBooking()}
                                  className="px-6 py-2 bg-green-400/10 hover:bg-green-400/20 text-green-400 border-green-400 border rounded-lg transition-all duration-300"
                                >
                                  Accept and Pay The Rest
                                </button>
                              </div>
                            )}
                        </div>
                      );
                    }
                  );
                })()
              )}
            </div>
          )}
        </div>

        {/* ----- EDIT REQUEST DIALOG ----- */}
        <Dialog
          open={isEditRequestDialogOpen}
          onOpenChange={setIsEditRequestDialogOpen}
        >
          <DialogContent className="z-9999 max-w-3xl max-h-[80vh] overflow-y-auto bg-black/50 backdrop-blur-sm text-white border border-white/10">
            <DialogHeader>
              <DialogTitle className="text-2xl font-bold text-mystic-green">
                Send New Edit Request
              </DialogTitle>
              <DialogDescription className="text-white/70">
                Select tracks you want to edit and provide details for the edit
                request
              </DialogDescription>
            </DialogHeader>

            <div className="space-y-6 mt-4">
              {/* Track Selection */}
              <div>
                <h3 className="text-lg font-semibold text-white mb-3">
                  Select Tracks to Edit
                </h3>
                {producingRequestDetails &&
                producingRequestDetails.BookingProducingRequest
                  .BookingPodcastTracks &&
                producingRequestDetails.BookingProducingRequest
                  .BookingPodcastTracks.length > 0 ? (
                  <div className="space-y-2">
                    {producingRequestDetails.BookingProducingRequest.BookingPodcastTracks.map(
                      (track, idx) => (
                        <div
                          key={track.Id}
                          onClick={() => handleToggleTrackSelection(track.Id)}
                          className={`p-4 rounded-lg border cursor-pointer transition-all duration-300 ${
                            selectedTrackIds.includes(track.Id)
                              ? "bg-mystic-green/20 border-mystic-green"
                              : "bg-white/5 border-white/10 hover:border-white/30"
                          }`}
                        >
                          <div className="flex items-center justify-between">
                            <div className="flex items-center gap-3">
                              <input
                                type="checkbox"
                                checked={selectedTrackIds.includes(track.Id)}
                                onChange={(e) => {
                                  e.stopPropagation();
                                  handleToggleTrackSelection(track.Id);
                                }}
                                onClick={(e) => e.stopPropagation()}
                                className="w-5 h-5 cursor-pointer accent-mystic-green"
                              />
                              <span className="text-white font-semibold">
                                Track #{idx + 1}
                              </span>
                            </div>
                            <span className="text-white/60 text-sm">
                              {Math.floor(track.AudioLength / 60)}:
                              {String(track.AudioLength % 60).padStart(2, "0")}{" "}
                              min
                            </span>
                          </div>
                          <div className="grid grid-cols-2 gap-3 mt-2 text-sm">
                            <div>
                              <p className="text-white/60">File Size</p>
                              <p className="text-white">
                                {(track.AudioFileSize / 1024 / 1024).toFixed(2)}{" "}
                                MB
                              </p>
                            </div>
                            <div>
                              <p className="text-white/60">Preview Slots</p>
                              <p className="text-white">
                                {track.RemainingPreviewListenSlot}
                              </p>
                            </div>
                          </div>
                        </div>
                      )
                    )}
                  </div>
                ) : (
                  <div className="p-8 bg-white/5 rounded-lg text-center">
                    <p className="text-white/60">No tracks available</p>
                  </div>
                )}
              </div>

              {/* Note Input */}
              <div>
                <label className="block text-white font-semibold mb-2">
                  Note <span className="text-red-400">*</span>
                </label>
                <textarea
                  value={editNote}
                  onChange={(e) => setEditNote(e.target.value)}
                  placeholder="Describe what needs to be edited..."
                  rows={4}
                  className="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-lg text-white placeholder-white/40 focus:outline-none focus:border-mystic-green transition-all"
                />
              </div>

              {/* Deadline Day Count */}
              <div>
                <label className="block text-white font-semibold mb-2">
                  Deadline (Days) <span className="text-red-400">*</span>
                </label>
                <input
                  type="number"
                  min="1"
                  value={deadlineDayCount}
                  onChange={(e) => setDeadlineDayCount(Number(e.target.value))}
                  className="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-lg text-white focus:outline-none focus:border-mystic-green transition-all"
                />
              </div>
            </div>

            <DialogFooter className="mt-6 flex gap-3">
              <button
                onClick={() => setIsEditRequestDialogOpen(false)}
                className="px-6 py-2 bg-white/10 hover:bg-white/20 text-white rounded-lg transition-all duration-300"
                disabled={isSendingEditRequest}
              >
                Cancel
              </button>
              <button
                onClick={handleSubmitEditRequest}
                disabled={isSendingEditRequest}
                className="px-6 py-2 bg-mystic-green hover:bg-mystic-green/80 text-black font-semibold rounded-lg transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isSendingEditRequest ? "Sending..." : "Send Edit Request"}
              </button>
            </DialogFooter>
          </DialogContent>
        </Dialog>

        {/* ----- ALERT BÁO KHÔNG ĐỦ TIỀN ----- */}
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

        {/* DIALOG XÁC NHẬN CANCEL */}
        <Dialog
          open={isCancelBookingConfirmationDialogOpen}
          onOpenChange={setIsCancelBookingConfirmationDialogOpen}
        >
          <DialogContent className="z-9999 bg-black/50 backdrop-blur-sm text-white border border-white/10">
            <DialogHeader>
              <DialogTitle className="text-xl font-bold">
                Are You Sure You Want to Cancel This Booking?
              </DialogTitle>
              <DialogDescription className="text-slate-300">
                {cancelDescription}
              </DialogDescription>
            </DialogHeader>

            <div className="flex flex-col py-2 gap-5 border-t border-mystic-green">
              <p className="text-[#D9D9D9] text-lg font-bold">
                Please Choose Cancel Reason To Submit
              </p>
              {/* Select or Choose Other to submit cancel reason by customer */}
              <Select
                value={cancelReason || undefined}
                onValueChange={handleSetCancelReason}
              >
                <SelectTrigger className="w-full">
                  <SelectValue placeholder="Select category" />
                </SelectTrigger>

                <SelectContent className="z-9999">
                  {bookingManualCancelReasons?.OptionalManualCancelReasonList.map(
                    (reason) => (
                      <SelectItem key={reason} value={reason}>
                        {reason}
                      </SelectItem>
                    )
                  )}
                  <SelectItem key="other" value="other">
                    Other
                  </SelectItem>
                </SelectContent>
              </Select>

              {cancelReason === "other" && (
                <Input
                  value={customCancelReason}
                  onChange={(e) => setCustomCancelReason(e.target.value)}
                  placeholder="Enter your own category"
                  className="bg-slate-900/40 border-slate-700/60"
                />
              )}
            </div>

            <DialogFooter className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                onClick={() => setIsCancelBookingConfirmationDialogOpen(false)}
                className="px-4 py-2 rounded-md border border-slate-600 text-sm text-slate-200 hover:bg-slate-800 transition"
              >
                Cancel
              </button>
              <button
                type="button"
                onClick={handleConfirmCancelBooking}
                className="px-4 py-2 rounded-md bg-red-500 text-sm font-semibold text-white hover:bg-red-600 transition"
              >
                Confirm Cancel
              </button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>
    );
  }
};

export default BookingDetailsPage;

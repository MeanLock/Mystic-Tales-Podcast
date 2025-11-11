import Loading from "@/components/loading";

import type { BookingDetailsUI } from "@/core/types/booking";
import { skipToken } from "@reduxjs/toolkit/query";
import { useEffect, useState } from "react";
import { IoIosArrowBack, IoIosArrowRoundBack } from "react-icons/io";
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
import { useSelector } from "react-redux";
import type { RootState } from "@/redux/store";

const mockBookingDetails1: BookingDetailsUI = {
  Id: 1,
  Title: "Đặt làm voice-over cho podcast về lucid dream",
  Description: `<div><div><em><strong>T&ocirc;i cần một <span style="color: #ff6600;">giọng đọc ấm &aacute;p</span> v&agrave; cuốn h&uacute;t để l&agrave;m voice-over cho tập podcast về lucid dream của m&igrave;nh.</strong></em></div></div>`,
  AccountId: 1,
  Podcaster: {
    Id: 1,
    Email: "thinhngu@gmail.com",
    FullName: "Bé Thịnh Pé Pỏng",
    ImageUrl:
      "https://i.pinimg.com/736x/4f/f1/d5/4ff1d52b884997affaa7bc5dc885cbde.jpg",
    PricePerBookingWord: 10,
  },
  Price: null,
  Deadline: null,
  DeadlineDays: 3,
  DemoAudioFileKey: "",
  BookingManualCancelledReason: null,
  BookingAutoCancelReason: null,
  CreatedAt: "2025-11-07T08:15:52.397Z",
  UpdatedAt: "2025-11-07T08:15:52.397Z",
  BookingRequirementFileList: [
    {
      Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      BookingId: 1,
      Name: "Đoạn 1 - Giới thiệu về lucid dream",
      Description: `Đoạn này cần một giọng đọc nhẹ nhàng và thu hút để giới thiệu về khái niệm lucid dream. /\$-\[script\]\$-<p><em><strong>Trading l&agrave; một hoạt động kinh doanh được thực hiện bởi những người chuy&ecirc;n mua v&agrave; b&aacute;n t&agrave;i sản, h&agrave;ng h&oacute;a hoặc tiền tệ với mục đ&iacute;ch kiếm lợi nhuận</strong></em>. Đ&acirc;y l&agrave; một trong những hoạt động đầu tư phổ biến v&agrave; c&oacute; t&iacute;nh rủi ro cao. Với sự ph&aacute;t triển của c&ocirc;ng nghệ, việc giao dịch trở n&ecirc;n dễ d&agrave;ng hơn bao giờ hết, cho ph&eacute;p bất kỳ ai c&oacute; thể tham gia v&agrave;o thị trường t&agrave;i ch&iacute;nh v&agrave; trở th&agrave;nh một nh&agrave; giao dịch. Tuy nhi&ecirc;n, để th&agrave;nh c&ocirc;ng trong trading, bạn cần hiểu r&otilde; về kh&aacute;i niệm trading l&agrave; g&igrave; cũng như c&aacute;c phong c&aacute;ch trading phổ biến. B&agrave;i viết n&agrave;y sẽ gi&uacute;p bạn t&igrave;m hiểu về trading v&agrave; những điều cần lưu &yacute; khi tham gia v&agrave;o hoạt động n&agrave;y.</p>
<h2><strong>Trading l&agrave; g&igrave;?</strong></h2>
<p>Trading hay c&ograve;n được gọi l&agrave; giao dịch chứng kho&aacute;n, l&agrave; hoạt động mua b&aacute;n t&agrave;i sản, h&agrave;ng h&oacute;a hoặc tiền tệ tr&ecirc;n thị trường t&agrave;i ch&iacute;nh. Mục đ&iacute;ch của trading l&agrave; kiếm lợi nhuận từ việc mua v&agrave;o v&agrave; b&aacute;n ra c&aacute;c t&agrave;i sản với gi&aacute; kh&aacute;c nhau. Người tham gia trading được gọi l&agrave; nh&agrave; giao dịch, họ c&oacute; thể l&agrave; c&aacute; nh&acirc;n, tổ chức hoặc c&aacute;c c&ocirc;ng ty đầu tư chuy&ecirc;n nghiệp.</p>
<p>Thị trường t&agrave;i ch&iacute;nh l&agrave; nơi c&aacute;c nh&agrave; giao dịch gặp gỡ v&agrave; thực hiện c&aacute;c giao dịch mua b&aacute;n. Thị trường n&agrave;y bao gồm nhiều loại t&agrave;i sản, từ cổ phiếu, tr&aacute;i phiếu, h&agrave;ng h&oacute;a đến tiền tệ. C&aacute;c giao dịch tr&ecirc;n thị trường t&agrave;i ch&iacute;nh được thực hiện th&ocirc;ng qua c&aacute;c s&agrave;n giao dịch, v&iacute; dụ như s&agrave;n chứng kho&aacute;n hay s&agrave;n ngoại hối.</p>
<p>Một trong những đặc điểm của trading l&agrave; t&iacute;nh thanh khoản cao, cho ph&eacute;p nh&agrave; giao dịch c&oacute; thể mua b&aacute;n t&agrave;i sản một c&aacute;ch nhanh ch&oacute;ng v&agrave; dễ d&agrave;ng. Tuy nhi&ecirc;n, điều n&agrave;y cũng đồng nghĩa với việc trading c&oacute; t&iacute;nh rủi ro cao. Nếu kh&ocirc;ng c&oacute; kế hoạch v&agrave; chiến lược đ&uacute;ng, bạn c&oacute; thể mất tiền nhanh ch&oacute;ng trong trading.</p>
<h2><strong>Khung thời gian giao dịch l&agrave; g&igrave;?</strong></h2>
<p>Nếu bạn đ&atilde; nắm được cơ bản về trading l&agrave; g&igrave; th&igrave; thứ tiếp theo bạn cần phải biết ch&iacute;nh l&agrave; khung thời gian trading. C&oacute; thể hiểu rằng, khung thời gian trading l&agrave; khoảng thời gian m&agrave; nh&agrave; giao dịch sẽ giữ một vị thế tr&ecirc;n thị trường. C&oacute; nhiều loại khung thời gian trading kh&aacute;c nhau, t&ugrave;y thuộc v&agrave;o mục đ&iacute;ch v&agrave; phong c&aacute;ch giao dịch của từng nh&agrave; giao dịch. Dưới đ&acirc;y l&agrave; một số khung thời gian trading phổ biến:</p>
<ul>
<li><strong>Position Trading</strong>: Đ&acirc;y l&agrave; loại trading d&agrave;i hạn, trong đ&oacute; nh&agrave; giao dịch giữ một vị thế trong khoảng từ v&agrave;i th&aacute;ng đến v&agrave;i năm. Nh&agrave; giao dịch sẽ t&igrave;m kiếm c&aacute;c xu hướng lớn tr&ecirc;n thị trường v&agrave; đưa ra quyết định mua hoặc b&aacute;n dựa tr&ecirc;n những thay đổi lớn trong thị trường.</li>
<li><strong>Swing Trading</strong>: Đ&acirc;y l&agrave; loại trading trung hạn, trong đ&oacute; nh&agrave; giao dịch giữ một vị thế trong khoảng từ v&agrave;i ng&agrave;y đến v&agrave;i tuần. Nh&agrave; giao dịch sẽ t&igrave;m kiếm c&aacute;c cơ hội giao dịch trong xu hướng ngắn hạn v&agrave; đưa ra quyết định mua hoặc b&aacute;n dựa tr&ecirc;n những thay đổi trong xu hướng n&agrave;y.</li>
<li><strong>Day Trading</strong>: Đ&acirc;y l&agrave; loại trading ngắn hạn, trong đ&oacute; nh&agrave; giao dịch mở v&agrave; đ&oacute;ng c&aacute;c vị thế trong c&ugrave;ng một ng&agrave;y. Nh&agrave; giao dịch sẽ t&igrave;m kiếm c&aacute;c cơ hội giao dịch trong khoảng thời gian ngắn v&agrave; đưa ra quyết định mua hoặc b&aacute;n dựa tr&ecirc;n những biến động ngắn hạn của thị trường.</li>
<li><strong>Scalp Trading</strong>: Đ&acirc;y l&agrave; loại trading rất ngắn hạn, trong đ&oacute; nh&agrave; giao dịch giữ c&aacute;c vị thế chỉ trong v&agrave;i ph&uacute;t hoặc v&agrave;i gi&acirc;y. Nh&agrave; giao dịch sẽ t&igrave;m kiếm c&aacute;c cơ hội giao dịch trong khoảng thời gian cực ngắn v&agrave; đưa ra quyết định mua hoặc b&aacute;n dựa tr&ecirc;n những biến động nhỏ của thị trường.</li>
</ul>
<p>Việc lựa chọn khung thời gian trading ph&ugrave; hợp với m&igrave;nh l&agrave; điều rất quan trọng để th&agrave;nh c&ocirc;ng trong trading. Bạn cần x&aacute;c định r&otilde; mục ti&ecirc;u v&agrave; phong c&aacute;ch giao dịch của m&igrave;nh để c&oacute; thể chọn được khung thời gian ph&ugrave; hợp.</p>\$-\[script\]\$-/`,
      RequirementFile: null,
      Order: 1,
      WordCount: 5000,
      PodcastBookingTone: {
        Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        Name: "string",
        Description: "string",
        PodcastBookingToneCategory: {
          Id: 0,
          Name: "string",
        },
        CreatedAt: "2025-11-07T08:15:52.397Z",
        UpdatedAt: "2025-11-07T08:15:52.397Z",
      },
    },
    {
      Id: "3fa85f64-5717-4562-b3fc-2c963f66af21",
      BookingId: 1,
      Name: "Đoạn 2 - Nội dung cuốn sách",
      Description: `<div><div><strong>Đ&acirc;y l&agrave; t&agrave;i liệu chi tiết về Trading, bạn cần đọc thật <span style="color: #ff0000;">r&otilde; v&agrave; to</span></strong></div></div>`,
      RequirementFile:
        "https://www.junkybooks.com/book/reader.php?book=thebooks/6483a9c6a2e4d-the-handbook-of-international-trade-and-finance.pdf",
      Order: 2,
      WordCount: 20000000,
      PodcastBookingTone: {
        Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        Name: "Giọng học thuật",
        Description: "",
        PodcastBookingToneCategory: {
          Id: 0,
          Name: "string",
        },
        CreatedAt: "2025-11-07T08:15:52.397Z",
        UpdatedAt: "2025-11-07T08:15:52.397Z",
      },
    },
  ],
  BookingProducingRequestList: [],
  CurrentStatus: {
    Id: 2,
    Name: "Quotation Dealing",
  },
  StatusTracking: [
    {
      id: "status-tracking-1",
      bookingId: 1,
      bookingStatusId: 1,
      createdAt: "2025-11-11T04:07:11.405Z",
    },
    {
      id: "status-tracking-2",
      bookingId: 1,
      bookingStatusId: 2,
      createdAt: "2025-11-12T04:07:11.405Z",
    },
  ],
};

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

function encodeDescription(
  description: string,
  link: string | null = null,
  script: string | null = null
) {
  let encoded = description?.trim() || "";

  if (link) {
    encoded += `\n$-[link]$-${link}$-[link]$-`;
  }

  if (script) {
    encoded += `\n$-[script]$-${script}$-[script]$-`;
  }

  return encoded.trim();
}

const ViewModes = ["informations", "dealing", "producingRequest"];

const BookingDetailsPage = () => {
  const { id } = useParams<{ id: string }>();
  const user = useSelector((state: RootState) => state.auth.user);
  // STATES
  const [viewMode, setViewMode] = useState<string>("informations");
  const [isActionError, setIsActionError] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

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
    error,
    refetch,
  } = useGetBookingDetailQuery(id ? { id: Number(id) } : skipToken);
  const [confirmDeal] = useConfirmAndDepositMutation();

  const handleConfirmDeal = async () => {
    if (!booking) {
      return;
    } else {
      if (!user) {
        return;
      } else {
        if (!booking.Booking.Price) {
          setIsActionError(true);
          setErrorMessage("Booking Hasn't Been Set Price Yet, Please Wait!");
          return;
        } else {
          if (user.Balance < booking.Booking.Price / 2) {
            setIsActionError(true);
            setErrorMessage("Your Account Balance Is Not Enough!");
          } else {
            try {
              const response = await confirmDeal({
                BookingId: booking.Booking.Id,
                Amount: booking.Booking.Price / 2,
              }).unwrap();

              // after successful confirm, refetch booking details to get updated state
              refetch && (await refetch());
            } catch (err) {
              // handle error (could set an action error)
              setIsActionError(true);
              setErrorMessage((err as any)?.message || "Confirm failed");
            }
          }
        }
      }
    }
  };

  if (isLoading) {
    return (
      <div className="w-full h-full flex flex-col items-center justify-center gap-5">
        <Loading />
        <p className="text-[#D9D9D9] font-poppins font-bold">
          Getting Your Booking Details ...
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
                  <div className="w-1/2 py-2 text-white border-b-[1px] border-white">
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
                  <div className="w-1/2 flex items-center gap-1 py-2 text-white border-b-[1px]  border-white">
                    {booking.Booking.Price ? (
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
                      src={booking.Booking.Podcaster.ImageUrl}
                      className="w-8 h-8 rounded-full aspect-square object-cover"
                    />
                    <p className="font-semibold">
                      {booking.Booking.Podcaster.FullName}
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
              {booking.Booking.BookingRequirementFileList.map(
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
                          {(
                            getTotalWordCount(booking) *
                            booking.Booking.Podcaster.PricePerBookingWord
                          ).toLocaleString()}
                        </span>
                      </p>
                      <TbCoinFilled className="w-5 h-5 text-mystic-green" />
                    </div>
                  </div>
                </div>
                {booking.Booking.BookingRequirementFileList.map(
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
                  Confirm
                </div>
              </div>
            </div>
          )}
        </div>
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

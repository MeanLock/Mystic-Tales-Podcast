import { Button } from "@/components/ui/button";
import {
  Carousel,
  CarouselContent,
  CarouselItem,
} from "@/components/ui/carousel";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import type { ShowDetailsUI } from "@/core/types/show";
import { useState } from "react";
import { FaPlus } from "react-icons/fa6";
import { FaPlay } from "react-icons/fa6";
import { IoPlay } from "react-icons/io5";
import { useNavigate } from "react-router-dom";

// Mock data theo hình ảnh "Kỳ Ẩn 101"
export const mockdata: ShowDetailsUI = {
  Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  Name: "Kỳ Án 101",
  Description:
    "Nơi những ám ảnh bùa nhập hồi hồn, những mảnh manh mặt tối của chúng vô nghĩa nổi vập vào dẫn đến sự thật kinh hoàng. Đây là nơi bạn sẽ gặp những kỳ án tóm sự, những vu ấn lạc đầu cảnh đau xa chỉ hóa bao nên gia đình thôi phép đoan vào trạng thật trong ngươi tầm. Tôi không cây không để mức tủ nhực để từa gì nhỏ không lấy lè hạt trong nhật bản",
  Language: "Vietnamese",
  ReleaseDate: "2025-11-05T05:59:24.492Z",
  IsReleased: true,
  Copyright: "© 2025 SAMURICE",
  UploadFrequency: "Weekly",
  RatingCount: 1540,
  AverageRating: 4.8,
  ImageUrl: `https://picsum.photos/300/300?random=1`,
  TrailerAudioFileKey: "/public/audio/1.mp3",
  TotalFollow: 23000,
  ListenCount: 198000,
  EpisodeCount: 4,
  Podcaster: {
    Id: 1,
    FullName: "SAMURICE",
    Email: "samurice@example.com",
    ImageUrl: "/public/images/podcaster.jpg",
  },
  PodcastCategory: {
    Id: 1,
    Name: "Mystery",
  },
  PodcastSubCategory: {
    Id: 2,
    Name: "True Crime",
    PodcastCategoryId: 1,
  },
  PodcastShowSubscriptionType: {
    Id: 1,
    Name: "Free",
  },
  PodcastChannel: {
    Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    Name: "Mystery Channel",
    ImageUrl: "/public/images/channel.jpg",
  },
  Hashtags: [
    { Id: 1, Name: "#mystery" },
    { Id: 2, Name: "#truecrime" },
  ],
  RatingList: [
    {
      Id: "1a2b3c4d-1234-5678-9abc-def012345678",
      Title: "Âm thanh tuyệt vời!",
      Content:
        "Chất lượng âm thanh rất trong và rõ ràng. Tôi thực sự hài lòng với trải nghiệm nghe nhạc lần này.",
      Rating: 5,
      Account: {
        Id: 101,
        FullName: "Nguyễn Văn A",
        Email: "nguyenvana@example.com",
        ImageUrl: "https://randomuser.me/api/portraits/men/32.jpg",
      },
      IsDeleted: false,
      CreatedAt: "2025-11-01T08:30:00Z",
    },
    {
      Id: "2b3c4d5e-2345-6789-abcd-ef0123456789",
      Title: "Ổn nhưng chưa hoàn hảo",
      Content: "Mọi thứ đều ổn, nhưng bass hơi yếu và cần được cải thiện thêm.",
      Rating: 3.5,
      Account: {
        Id: 102,
        FullName: "Trần Thị B",
        Email: "tranthib@example.com",
        ImageUrl: "https://randomuser.me/api/portraits/women/45.jpg",
      },
      IsDeleted: false,
      CreatedAt: "2025-10-28T14:15:00Z",
    },
    {
      Id: "3c4d5e6f-3456-789a-bcde-f01234567890",
      Title: "Không đáng tiền",
      Content:
        "Sản phẩm không giống mô tả, chất lượng âm thanh kém và pin yếu.",
      Rating: 1.5,
      Account: {
        Id: 103,
        FullName: "Lê Văn C",
        Email: "levanc@example.com",
        ImageUrl: "https://randomuser.me/api/portraits/men/77.jpg",
      },
      IsDeleted: false,
      CreatedAt: "2025-10-20T10:00:00Z",
    },
    {
      Id: "4d5e6f7g-4567-89ab-cdef-012345678901",
      Title: "Trải nghiệm tốt",
      Content:
        "Dịch vụ chăm sóc khách hàng rất tốt, sản phẩm hoạt động mượt mà.",
      Rating: 4,
      Account: {
        Id: 104,
        FullName: "Phạm Thảo D",
        Email: "phamthaod@example.com",
        ImageUrl: "https://randomuser.me/api/portraits/women/21.jpg",
      },
      IsDeleted: false,
      CreatedAt: "2025-11-03T16:45:00Z",
    },
  ],
  TakenDownReason: "",
  CreatedAt: "2025-11-05T05:59:24.492Z",
  UpdatedAt: "2025-11-05T05:59:24.492Z",
  CurrentStatus: {
    Id: 1,
    Name: "Active",
  },
  PodcastSubscriptionList: [],
  EpisodeList: [
    {
      Id: "ep-001",
      Name: "Vụ mất tích trong căn phòng khóa kín",
      Description:
        "Một người đàn ông biến mất trong chính căn phòng khóa kín từ bên trong. Tìm kiếm sự thật về vụ án bí ẩn này và thám hiểm những manh mối ẩn giấu sau cánh cửa đóng.",
      ExplicitContent: false,
      ReleaseDate: "2025-10-10T05:59:24.492Z",
      EpisodeOrder: 1,
      IsReleased: true,
      ImageUrl: `https://picsum.photos/300/300?random=2`,
      AudioFileKey: "/public/audio/episode1.mp3",
      AudioFileSize: 24000000,
      AudioLength: 1800, // 30 min
      PodcastEpisodeSubscriptionType: {
        Id: 1,
        Name: "Free",
      },
      PodcastShow: {
        Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        Name: "Kỳ Ẩn 101",
        ImageUrl: "/public/images/show-cover.jpg",
      },
      Hashtags: [{ Id: 1, Name: "#mystery" }],
      SeasonNumber: 1,
      TotalSave: 340,
      ListenCount: 18500,
      IsAudioPublishable: true,
      TakenDownReason: "",
      CreatedAt: "2025-10-10T05:59:24.492Z",
      UpdatedAt: "2025-10-15T05:59:24.492Z",
      CurrentStatus: {
        Id: 1,
        Name: "Active",
      },
    },
  ],
};

// Additional episodes for display (extending the mockdata)
const additionalEpisodes = [
  {
    Id: "ep-002",
    Name: "Vụ mất tích trong căn phòng khóa kín",
    Description:
      "Một người đàn ông biến mất trong chính căn phòng khóa kín từ bên trong. Tìm kiếm sự thật về vụ án bí ẩn này và thám hiểm những manh mối ẩn giấu sau cánh cửa đóng.",
    ExplicitContent: false,
    ReleaseDate: "2025-10-17T05:59:24.492Z",
    EpisodeOrder: 2,
    IsReleased: true,
    ImageUrl: `https://picsum.photos/300/300?random=2`,
    AudioFileKey: "/public/audio/episode2.mp3",
    AudioFileSize: 24000000,
    AudioLength: 1800,
    PodcastEpisodeSubscriptionType: {
      Id: 1,
      Name: "Free",
    },
    PodcastShow: {
      Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      Name: "Kỳ Án 101",
      ImageUrl: "/public/images/show-cover.jpg",
    },
    Hashtags: [{ Id: 1, Name: "#mystery" }],
    SeasonNumber: 1,
    TotalSave: 280,
    ListenCount: 16200,
    IsAudioPublishable: true,
    TakenDownReason: "",
    CreatedAt: "2025-10-17T05:59:24.492Z",
    UpdatedAt: "2025-10-17T05:59:24.492Z",
    CurrentStatus: {
      Id: 1,
      Name: "Active",
    },
  },
  {
    Id: "ep-003",
    Name: "Vụ mất tích trong căn phòng khóa kín",
    Description:
      "Một người đàn ông biến mất trong chính căn phòng khóa kín từ bên trong. Tìm kiếm sự thật về vụ án bí ẩn này và thám hiểm những manh mối ẩn giấu sau cánh cửa đóng.",
    ExplicitContent: false,
    ReleaseDate: "2025-10-24T05:59:24.492Z",
    EpisodeOrder: 3,
    IsReleased: true,
    ImageUrl: "/public/images/episode3.jpg",
    AudioFileKey: "/public/audio/episode3.mp3",
    AudioFileSize: 24000000,
    AudioLength: 1800,
    PodcastEpisodeSubscriptionType: {
      Id: 1,
      Name: "Free",
    },
    PodcastShow: {
      Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      Name: "Kỳ Ẩn 101",
      ImageUrl: "/public/images/show-cover.jpg",
    },
    Hashtags: [{ Id: 1, Name: "#mystery" }],
    SeasonNumber: 1,
    TotalSave: 320,
    ListenCount: 17800,
    IsAudioPublishable: true,
    TakenDownReason: "",
    CreatedAt: "2025-10-24T05:59:24.492Z",
    UpdatedAt: "2025-10-24T05:59:24.492Z",
    CurrentStatus: {
      Id: 1,
      Name: "Active",
    },
  },
  {
    Id: "ep-004",
    Name: "Vụ mất tích trong căn phòng khóa kín",
    Description:
      "Một người đàn ông biến mất trong chính căn phòng khóa kín từ bên trong. Tìm kiếm sự thật về vụ án bí ẩn này và thám hiểm những manh mối ẩn giấu sau cánh cửa đóng.",
    ExplicitContent: false,
    ReleaseDate: "2025-10-31T05:59:24.492Z",
    EpisodeOrder: 4,
    IsReleased: true,
    ImageUrl: "/public/images/episode4.jpg",
    AudioFileKey: "/public/audio/episode4.mp3",
    AudioFileSize: 24000000,
    AudioLength: 1800,
    PodcastEpisodeSubscriptionType: {
      Id: 1,
      Name: "Free",
    },
    PodcastShow: {
      Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      Name: "Kỳ Ẩn 101",
      ImageUrl: "/public/images/show-cover.jpg",
    },
    Hashtags: [{ Id: 1, Name: "#mystery" }],
    SeasonNumber: 1,
    TotalSave: 290,
    ListenCount: 15600,
    IsAudioPublishable: true,
    TakenDownReason: "",
    CreatedAt: "2025-10-31T05:59:24.492Z",
    UpdatedAt: "2025-10-31T05:59:24.492Z",
    CurrentStatus: {
      Id: 1,
      Name: "Active",
    },
  },
];

// Helper function to format duration
const formatDuration = (seconds: number): string => {
  const minutes = Math.floor(seconds / 60);
  return `${minutes} min`;
};

// Helper function to format time ago
const getTimeAgo = (dateString: string): string => {
  const date = new Date(dateString);
  const now = new Date();
  const diffInDays = Math.floor(
    (now.getTime() - date.getTime()) / (1000 * 60 * 60 * 24)
  );

  if (diffInDays === 0) return "Today";
  if (diffInDays === 1) return "1 day ago";
  return `${diffInDays} days ago`;
};

const ShowDetailsPage = () => {
  const showData = mockdata;
  const [isReviewDialogOpen, setIsReviewDialogOpen] = useState(false);
  const [rating, setRating] = useState(0);
  const [hover, setHover] = useState(0);
  // Combine the main episode with additional episodes
  const allEpisodes = [...showData.EpisodeList, ...additionalEpisodes];

  const navigate = useNavigate();
  return (
    <div className="min-h-screen text-white py-6">
      {/* Back Button */}
      <button
        onClick={() => navigate(-1)}
        className="px-12 flex items-center text-white  mb-6 cursor-pointer hover:-translate-y-1 "
      >
        <svg
          className="w-5 h-5 mr-2"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M15 19l-7-7 7-7"
          />
        </svg>
        Previous
      </button>

      {/* Header Section */}
      <div className="flex gap-10 mb-8 px-12 ">
        {/* Show Image */}
        <div className="w-80 h-80 bg-gray-800 rounded-lg overflow-hidden flex-shrink-0">
          <img
            src={showData.ImageUrl}
            alt={showData.Name}
            className="w-full h-full object-cover"
            onError={(e) => {
              e.currentTarget.style.display = "none";
              e.currentTarget.parentElement!.innerHTML = `
                <div class="w-full h-full bg-gradient-to-br from-gray-700 to-gray-800 flex items-center justify-center">
                  <svg class="w-24 h-24 text-gray-500" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M4 3a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V5a2 2 0 00-2-2H4zm12 12H4l4-8 3 6 2-4 3 6z" clip-rule="evenodd"/>
                  </svg>
                </div>
              `;
            }}
          />
        </div>

        {/* Show Info */}
        <div className="flex-1">
          <h1 className="text-4xl font-medium text-white mb-4">
            {showData.Name}
          </h1>
          <p className="text-xl text-white mb-1 ">
            {showData.Podcaster.FullName}
          </p>
          <span className="text-sm text-white">
            ⭐ {showData.AverageRating} ({showData.RatingCount}) -{" "}
            {showData.PodcastCategory.Name} - {showData.PodcastSubCategory.Name}
          </span>
          <p className="text-gray-300 text-base leading-relaxed my-6 max-w-2xl line-clamp-4">
            {showData.Description}
          </p>

          {/* Action Buttons */}
          <div className="flex items-center justify-between ">
            <Button className="bg-mystic-green hover:bg-lime-400 transition-all duration-700 ease-out  hover:-translate-y-1 cursor-pointer  text-black font-semibold px-6 py-2 rounded-sm">
              <FaPlay />
              Latest Episode
            </Button>
            <div className="flex items-center gap-5">
              <Button
                variant="outline"
                className="bg-mystic-green text-black hover:bg-lime-400 transition-all duration-700 ease-out hover:-translate-y-1 border-none px-4 py-0 rounded-full flex items-center gap-2"
              >
                + Follow
              </Button>
              <Button className="bg-mystic-green cursor-pointer hover:bg-lime-400   text-black rounded-full w-8 h-8 flex items-center justify-center text-sm font-bold">
                ⋯
              </Button>
            </div>
          </div>
        </div>
      </div>
      <div>
        <h2 className="text-2xl font-medium mb-2 mt-16 px-12 ">Trailer</h2>
        <audio
          controls
          controlsList="nodownload noplaybackrate"
          className="w-full px-12 pt-4"
        >
          <source src={showData.TrailerAudioFileKey} type="audio/mpeg" />
        </audio>
      </div>
      {/* Episodes Section */}
      <div>
        <h2 className="text-2xl font-medium mb-8 mt-12 px-12 ">Episodes</h2>

        <div className="space-y-10 px-3">
          {allEpisodes.map((episode) => (
            <div
              key={episode.Id}
              className="px-12 flex h-28 items-center gap-10 p-2 rounded-lg hover:bg-white/10  transition-colors group cursor-pointer"
            >
              <div className="relative aspect-square h-full bg-gray-700 rounded-lg overflow-hidden flex-shrink-0 ">
                <img
                  src={episode.ImageUrl}
                  alt={episode.Name}
                  className="w-full h-full aspect-square object-cover"
                  onError={(e) => {
                    e.currentTarget.style.display = "none";
                    e.currentTarget.parentElement!.innerHTML = `
                      <div class="w-full h-full bg-gradient-to-br from-gray-600 to-gray-700 flex items-center justify-center">
                        <svg class="w-6 h-6 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                          <path fill-rule="evenodd" d="M4 3a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V5a2 2 0 00-2-2H4zm12 12H4l4-8 3 6 2-4 3 6z" clip-rule="evenodd"/>
                        </svg>
                      </div>
                    `;
                  }}
                />
                <div className="absolute inset-0 hidden group-hover:inline-flex bg-black/30 items-center justify-center">
                  <div className="p-2  rounded-full bg-gray-400 flex items-center justify-center hover:bg-mystic-green ">
                    <IoPlay size={25} color="#ffffff" />
                  </div>
                </div>
              </div>
              <div className="flex-1 flex items-center justify-between gap-20">
                <div className="flex-1 min-w-0">
                  <div className="flex items-start justify-between">
                    <div className="flex-1 min-w-0">
                      <p className="text-sm text-[#d9d9d9] mb-1">
                        {getTimeAgo(episode.ReleaseDate)}
                      </p>
                      <h4 className="font-bold text-lg text-white mb-2 leading-tight">
                        {episode.Name}
                      </h4>
                      <p className="text-white font-light text-sm line-clamp-2 leading-relaxed">
                        {episode.Description}
                      </p>
                    </div>
                  </div>
                </div>

                <p className="text-white font-bold text-sm">
                  {formatDuration(episode.AudioLength)}
                </p>
                <button className="text-[#d9d9d9] text-mystic-green cursor-pointer">
                  <svg
                    className="w-5 h-5"
                    fill="currentColor"
                    viewBox="0 0 20 20"
                  >
                    <path d="M10 6a2 2 0 110-4 2 2 0 010 4zM10 12a2 2 0 110-4 2 2 0 010 4zM10 18a2 2 0 110-4 2 2 0 010 4z" />
                  </svg>
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>
      {/* Ratings & Reviews Section */}
      <div className="px-12">
        <div className="flex items-center gap-4 mb-8 mt-16">
          <h2 className="text-2xl font-medium">Ratings & Reviews</h2>
          <Dialog
            open={isReviewDialogOpen}
            onOpenChange={setIsReviewDialogOpen}
          >
            <DialogTrigger asChild>
              <button className="cursor-pointer w-8 h-8 rounded-full bg-mystic-green hover:bg-lime-400 flex items-center justify-center transition-all duration-300 hover:scale-110">
                <FaPlus className="text-black" size={16} />
              </button>
            </DialogTrigger>
            <DialogContent className="backdrop-blur-md bg-white/10 border border-white/20 rounded-2xl p-6 shadow-xl">
              <DialogHeader>
                <DialogTitle className="text-2xl font-semibold text-white mb-2">
                  Write a Review
                </DialogTitle>
              </DialogHeader>

              {/* Rating & Review Form */}
              <div className="mt-4">
                <div className="mb-4">
                  <label className="block text-white text-sm font-medium mb-3">
                    Your Rating
                  </label>
                  <div className="flex gap-1">
                    {Array.from({ length: 5 }).map((_, i) => {
                      const index = i + 1;
                      return (
                        <button
                          key={index}
                          type="button"
                          onClick={() => setRating(index)}
                          onMouseEnter={() => setHover(index)}
                          onMouseLeave={() => setHover(0)}
                          className="w-10 h-10 transition-transform duration-200 hover:scale-110"
                        >
                          <svg
                            className={`w-full h-full cursor-pointer transition-colors duration-200 ${
                              index <= (hover || rating)
                                ? "text-yellow-400"
                                : "text-gray-400"
                            }`}
                            fill="currentColor"
                            viewBox="0 0 20 20"
                          >
                            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                          </svg>
                        </button>
                      );
                    })}
                  </div>
                </div>

                {/* Title Input */}
                <div className="mb-4">
                  <label className="block text-white text-sm font-medium mb-2">
                    Review Title
                  </label>
                  <input
                    type="text"
                    placeholder="Give your review a title..."
                    className="w-full px-4 py-3 bg-white/5 backdrop-blur-sm border border-white/20 rounded-xl text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-mystic-green/50 focus:border-mystic-green/50 transition-all duration-300"
                  />
                </div>

                {/* Content Textarea */}
                <div className="mb-6">
                  <label className="block text-white text-sm font-medium mb-2">
                    Review Content
                  </label>
                  <textarea
                    rows={5}
                    placeholder="Share your thoughts about this podcast..."
                    className="w-full px-4 py-3 bg-white/5 backdrop-blur-sm border border-white/20 rounded-xl text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-mystic-green/50 focus:border-mystic-green/50 transition-all duration-300 resize-none"
                  />
                </div>

                {/* Submit Button */}
                <Button
                  onClick={() => setIsReviewDialogOpen(false)}
                  className="w-full bg-mystic-green hover:bg-lime-400 text-black font-semibold py-3 rounded-xl transition-all duration-300 hover:shadow-lg hover:shadow-mystic-green/25"
                >
                  Submit Review
                </Button>
              </div>
            </DialogContent>
          </Dialog>
        </div>

        <div className="flex gap-12 mb-14">
          {/* Left side - Overall Rating */}
          <div className="flex gap-12 items-center">
            <div className="">
              <p className="text-6xl font-bold text-mystic-green mb-2">
                {showData.AverageRating}
              </p>
              <p className="text-white text-center text-lg  mb-1">Out of 5</p>
            </div>

            {/* Rating bars */}
            <div className="space-y-2 w-100">
              {[5, 4, 3, 2, 1].map((rating) => {
                // Calculate percentage for each rating
                const count = showData.RatingList.filter(
                  (r) => Math.floor(r.Rating) === rating
                ).length;
                const percentage =
                  showData.RatingList.length > 0
                    ? (count / showData.RatingList.length) * 100
                    : 0;

                return (
                  <div key={rating} className="flex items-center gap-3">
                    {/* Stars */}
                    <div className="flex min-w-20">
                      {Array.from({ length: rating }).map((_, i) => (
                        <svg
                          key={i}
                          className="w-4 h-4 text-white"
                          fill="currentColor"
                          viewBox="0 0 20 20"
                        >
                          <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                        </svg>
                      ))}
                    </div>

                    {/* Progress bar */}
                    <div className="flex-1 bg-[#d9d9d9]  h-1">
                      <div
                        className="bg-mystic-green h-1  transition-all duration-300"
                        style={{ width: `${percentage}%` }}
                      ></div>
                    </div>
                  </div>
                );
              })}
              <div className="text-white text-xs font-light text-right ">
                {showData.RatingCount.toLocaleString()} ratings
              </div>
            </div>
          </div>
        </div>
        <Carousel
          opts={{
            align: "start",
            loop: false,
          }}
          className="w-full"
        >
          <CarouselContent className="-ml-4">
            {showData.RatingList.map((review) => (
              <CarouselItem key={review.Id} className="pl-4 basis-1/3">
                <div
                  className=" rounded-2xl p-6 h-full "
                  style={{ backgroundColor: "rgba(255, 255, 255, 0.1)" }}
                >
                  {/* Header with user info and date */}
                  <div className="flex justify-between items-start mb-2">
                    <div className="text-xs text-gray-200">
                      {getTimeAgo(review.CreatedAt)}
                    </div>
                    <div className="text-xs text-gray-200">
                      {review.Account.FullName}
                    </div>
                  </div>

                  {/* Title */}
                  <h4 className="font-semibold text-white text-base mb-3">
                    {review.Title}
                  </h4>

                  {/* Rating stars */}
                  <div className="flex mb-4">
                    {Array.from({ length: 5 }).map((_, i) => (
                      <svg
                        key={i}
                        className={`w-4 h-4 ${
                          i < Math.floor(review.Rating)
                            ? "text-yellow-400"
                            : "text-gray-400"
                        }`}
                        fill="currentColor"
                        viewBox="0 0 20 20"
                      >
                        <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                      </svg>
                    ))}
                  </div>

                  {/* Review content */}
                  <p className="text-gray-100 text-sm leading-relaxed">
                    {review.Content}
                  </p>
                </div>
              </CarouselItem>
            ))}
          </CarouselContent>
        </Carousel>
      </div>

      {/* Information Section */}
      <div className="px-12">
        <h2 className="text-2xl font-medium mb-8 mt-14">Information</h2>

        {/* Grid Layout - 3 columns */}
        <div className="grid grid-cols-3 gap-x-16 gap-y-6 mb-8">
          {/* Creator */}
          <div>
            <h3 className="text-gray-400 text-sm mb-2">Creator</h3>
            <p className="text-white text-base">
              {showData.Podcaster.FullName}
            </p>
          </div>

          {/* Seasons */}
          <div>
            <h3 className="text-gray-400 text-sm mb-2">Seasons</h3>
            <p className="text-white text-base">
              {showData.EpisodeList[0]?.SeasonNumber || 1}
            </p>
          </div>

          {/* Rating */}
          <div>
            <h3 className="text-gray-400 text-sm mb-2">Upload Frequency</h3>
            <p className="text-white text-base">{showData.UploadFrequency}</p>
          </div>

          {/* Copyright */}
          <div>
            <h3 className="text-gray-400 text-sm mb-2">Copyright</h3>
            <p className="text-white text-base">{showData.Copyright}</p>
          </div>

          {/* Show Website */}

          {/* Provider */}
          <div>
            <h3 className="text-gray-400 text-sm mb-2">Release Date</h3>
            <p className="text-white text-base">{showData.ReleaseDate}</p>
          </div>
        </div>

        {/* Full Description */}
        <div className="mt-8 pt-8 border-t border-white/10">
          <p className="text-white text-base leading-relaxed">
            {showData.Description}
          </p>
        </div>
      </div>
    </div>
  );
};

export default ShowDetailsPage;

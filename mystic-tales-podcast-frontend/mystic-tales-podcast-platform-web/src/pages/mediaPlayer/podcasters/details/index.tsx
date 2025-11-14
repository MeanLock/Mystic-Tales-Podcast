/* eslint-disable @typescript-eslint/no-unused-vars */
// @ts-nocheck

import {
  Carousel,
  CarouselContent,
  CarouselItem,
} from "@/components/ui/carousel";
import type { ChannelUI } from "@/core/types/channel";
import type { PodcasterUI } from "@/core/types/podcaster";
import Autoplay from "embla-carousel-autoplay";
import { useEffect, useState } from "react";
import { IoIosArrowBack } from "react-icons/io";
import { useNavigate } from "react-router-dom";
import ChannelCard from "./components/ChannelCard";
import type { ShowUI } from "@/core/types/show";
import ShowCard from "./components/ShowCard";

const PodcasterDetails: PodcasterUI = {
  Id: 1,
  Email: "lauvloveuso@gmail.com",
  Role: { Id: 1, Name: "Customer" },
  FullName: "Ari Staprans Leff",
  Dob: "08-08-1994",
  Gender: "male",
  Address: "",
  Phone: "",
  Balance: 0,
  ImageUrl:
    "https://i.pinimg.com/736x/72/26/04/722604c09eef53416bf4e38460deb4ba.jpg",
  IsVerified: true,
  GoogleId: null,
  PodcastListenSlot: 0,
  ViolationPoint: 0,
  ViolationLevel: 0,
  LastViolationPointChanged: null,
  LastViolationLevelChanged: null,
  LastPodcastListenSlotChanged: "",
  DeactivatedAt: null,
  CreatedAt: "",
  UpdatedAt: "",
  PodcasterProfile: {
    AccountId: 1,
    Name: "Lauv",
    Description:
      'Ari Staprans Leff, known professionally as Lauv, is an American musician best known for his breakout hit "I Like Me Better".',
    AverageRating: 4.9,
    RatingCount: 2340111,
    TotalFollow: 1200000,
    ListenCount: 30215521456,
    CommitmentDocumentFileKey: "",
    BuddyAudioFileKey: "",
    OwnedBookingStorageSize: 0,
    UsedBookingStorageSize: 0,
    IsVerified: true,
    CreatedAt: "",
    UpdatedAt: "",
  },
  ReviewList: [
    {
      Id: "1",
      Title: "",
      Content: "",
      Rating: 0,
      Account: { Id: 0, FullName: "", Email: "", ImageUrl: "" },
      DeletedAt: null,
      UpdatedAt: null,
    },
  ],
};

const MockPodcasterChannels: ChannelUI[] = [
  {
    Id: "1", //Chỉnh cái này
    Name: "How I'm Feeling", //Chỉnh cái này
    Description: "",
    BackgroundImageUrl:
      "https://i.pinimg.com/736x/7e/2a/2d/7e2a2d2852d1f7d5fd4d5cd169e18eec.jpg",
    ImageUrl:
      "https://i.pinimg.com/736x/7e/2a/2d/7e2a2d2852d1f7d5fd4d5cd169e18eec.jpg",
    TotalFavorite: 0,
    ListenCount: 0,
    ShowCount: 10, //Chỉnh cái này
    Podcaster: {
      Id: 1,
      FullName: "Lauv",
      Email: "lauvloveuso@gmail.com",
      ImageUrl: "",
    },
    PodcastCategory: {
      Id: 0,
      Name: "",
    },
    PodcastSubCategory: {
      Id: 0,
      Name: "",
      PodcastCategoryId: 0,
    },
    Hashtags: [{ Id: 1, Name: "" }],
    CreatedAt: "",
    UpdatedAt: "",
    CurrentStatus: {
      Id: 0,
      Name: "",
    },
  },
  {
    Id: "2", //Chỉnh cái này
    Name: "All 4 Nothing", //Chỉnh cái này
    Description: "",
    BackgroundImageUrl:
      "https://i.pinimg.com/736x/7e/2a/2d/7e2a2d2852d1f7d5fd4d5cd169e18eec.jpg",
    ImageUrl:
      "https://i.pinimg.com/736x/f3/7c/a3/f37ca35f111962f1c36d1f92294281e6.jpg",
    TotalFavorite: 0,
    ListenCount: 0,
    ShowCount: 25, //Chỉnh cái này
    Podcaster: {
      Id: 1,
      FullName: "Lauv",
      Email: "lauvloveuso@gmail.com",
      ImageUrl: "",
    },
    PodcastCategory: {
      Id: 0,
      Name: "",
    },
    PodcastSubCategory: {
      Id: 0,
      Name: "",
      PodcastCategoryId: 0,
    },
    Hashtags: [{ Id: 1, Name: "" }],
    CreatedAt: "",
    UpdatedAt: "",
    CurrentStatus: {
      Id: 0,
      Name: "",
    },
  },
  {
    Id: "3", //Chỉnh cái này
    Name: "I Met You When I Was 18", //Chỉnh cái này
    Description: "",
    BackgroundImageUrl:
      "https://i.pinimg.com/736x/7e/2a/2d/7e2a2d2852d1f7d5fd4d5cd169e18eec.jpg",
    ImageUrl:
      "https://i.pinimg.com/1200x/1c/26/1f/1c261fce5ebf0e5d17b42e94ab17eaf6.jpg",
    TotalFavorite: 0,
    ListenCount: 0,
    ShowCount: 8, //Chỉnh cái này
    Podcaster: {
      Id: 1,
      FullName: "Lauv",
      Email: "lauvloveuso@gmail.com",
      ImageUrl: "",
    },
    PodcastCategory: {
      Id: 0,
      Name: "",
    },
    PodcastSubCategory: {
      Id: 0,
      Name: "",
      PodcastCategoryId: 0,
    },
    Hashtags: [{ Id: 1, Name: "" }],
    CreatedAt: "",
    UpdatedAt: "",
    CurrentStatus: {
      Id: 0,
      Name: "",
    },
  },
  {
    Id: "4", //Chỉnh cái này
    Name: "Lost In The Light", //Chỉnh cái này
    Description: "",
    BackgroundImageUrl:
      "https://i.pinimg.com/736x/7e/2a/2d/7e2a2d2852d1f7d5fd4d5cd169e18eec.jpg",
    ImageUrl:
      "https://i.pinimg.com/736x/0d/1c/30/0d1c304caf89d0dc52607ea56c86958d.jpg",
    TotalFavorite: 0,
    ListenCount: 0,
    ShowCount: 14, //Chỉnh cái này
    Podcaster: {
      Id: 1,
      FullName: "Lauv",
      Email: "lauvloveuso@gmail.com",
      ImageUrl: "",
    },
    PodcastCategory: {
      Id: 0,
      Name: "",
    },
    PodcastSubCategory: {
      Id: 0,
      Name: "",
      PodcastCategoryId: 0,
    },
    Hashtags: [{ Id: 1, Name: "" }],
    CreatedAt: "",
    UpdatedAt: "",
    CurrentStatus: {
      Id: 0,
      Name: "",
    },
  },
  {
    Id: "5", //Chỉnh cái này
    Name: "Without You", //Chỉnh cái này
    Description: "",
    BackgroundImageUrl:
      "https://i.pinimg.com/736x/7e/2a/2d/7e2a2d2852d1f7d5fd4d5cd169e18eec.jpg",
    ImageUrl:
      "https://i.pinimg.com/1200x/41/f5/94/41f5941aa56b7d5b1bb8da8fdc6b9673.jpg",
    TotalFavorite: 0,
    ListenCount: 0,
    ShowCount: 20, //Chỉnh cái này
    Podcaster: {
      Id: 1,
      FullName: "Lauv",
      Email: "lauvloveuso@gmail.com",
      ImageUrl: "",
    },
    PodcastCategory: {
      Id: 0,
      Name: "",
    },
    PodcastSubCategory: {
      Id: 0,
      Name: "",
      PodcastCategoryId: 0,
    },
    Hashtags: [{ Id: 1, Name: "" }],
    CreatedAt: "",
    UpdatedAt: "",
    CurrentStatus: {
      Id: 0,
      Name: "",
    },
  },
  {
    Id: "6", //Chỉnh cái này
    Name: "WORK OUT", //Chỉnh cái này
    Description: "",
    BackgroundImageUrl:
      "https://i.pinimg.com/736x/7e/2a/2d/7e2a2d2852d1f7d5fd4d5cd169e18eec.jpg",
    ImageUrl:
      "https://i.pinimg.com/736x/52/94/db/5294db7f01a897b982c70f932bbbcf3e.jpg",
    TotalFavorite: 0,
    ListenCount: 0,
    ShowCount: 30, //Chỉnh cái này
    Podcaster: {
      Id: 1,
      FullName: "Lauv",
      Email: "lauvloveuso@gmail.com",
      ImageUrl: "",
    },
    PodcastCategory: {
      Id: 0,
      Name: "",
    },
    PodcastSubCategory: {
      Id: 0,
      Name: "",
      PodcastCategoryId: 0,
    },
    Hashtags: [{ Id: 1, Name: "" }],
    CreatedAt: "",
    UpdatedAt: "",
    CurrentStatus: {
      Id: 0,
      Name: "",
    },
  },
];

const MockPodcasterShows: ShowUI[] = [
  {
    Id: "1", //Chỉnh cái này
    Name: "Driving Vibes", //Chỉnh cái này
    Description: "",
    Language: "",
    ReleaseDate: "",
    IsReleased: true,
    Copyright: "",
    UploadFrequency: "",
    RatingCount: 0,
    AverageRating: 0,
    // Chỉnh cái này
    ImageUrl:
      "https://i.pinimg.com/736x/05/bf/55/05bf55e90aa862c6e5461c4fe9060760.jpg",
    TrailerAudioFileKey: "",
    TotalFollow: 0,
    ListenCount: 0,
    EpisodeCount: 12,
    Podcaster: {
      Id: 0,
      FullName: "",
      Email: "",
      ImageUrl: "",
    },
    PodcastCategory: {
      Id: 0,
      Name: "",
    },
    PodcastSubCategory: {
      Id: 0,
      Name: "",
      PodcastCategoryId: 0,
    },
    PodcastShowSubscriptionType: {
      Id: 0,
      Name: "",
    },
    PodcastChannel: {
      Id: "",
      Name: "",
      ImageUrl: "",
    },
    Hashtags: [{ Id: 0, Name: "" }],
    TakenDownReason: "",
    CreatedAt: "",
    UpdatedAt: "",
    CurrentStatus: {
      Id: 0,
      Name: "",
    },
  },
  {
    Id: "2", //Chỉnh cái này
    Name: "Party Vibes", //Chỉnh cái này
    Description: "",
    Language: "",
    ReleaseDate: "",
    IsReleased: true,
    Copyright: "",
    UploadFrequency: "",
    RatingCount: 0,
    AverageRating: 0,
    // Chỉnh cái này
    ImageUrl:
      "https://i.pinimg.com/736x/79/4c/34/794c34a501f130bb629bcb9ca303c8fb.jpg",
    TrailerAudioFileKey: "",
    TotalFollow: 0,
    ListenCount: 0,
    EpisodeCount: 12,
    Podcaster: {
      Id: 0,
      FullName: "",
      Email: "",
      ImageUrl: "",
    },
    PodcastCategory: {
      Id: 0,
      Name: "",
    },
    PodcastSubCategory: {
      Id: 0,
      Name: "",
      PodcastCategoryId: 0,
    },
    PodcastShowSubscriptionType: {
      Id: 0,
      Name: "",
    },
    PodcastChannel: {
      Id: "",
      Name: "",
      ImageUrl: "",
    },
    Hashtags: [{ Id: 0, Name: "" }],
    TakenDownReason: "",
    CreatedAt: "",
    UpdatedAt: "",
    CurrentStatus: {
      Id: 0,
      Name: "",
    },
  },
  {
    Id: "3", //Chỉnh cái này
    Name: "LAUV in Hong Kong", //Chỉnh cái này
    Description: "",
    Language: "",
    ReleaseDate: "",
    IsReleased: true,
    Copyright: "",
    UploadFrequency: "",
    RatingCount: 0,
    AverageRating: 0,
    // Chỉnh cái này
    ImageUrl:
      "https://i.pinimg.com/736x/1f/8c/82/1f8c821836aed693442ebebf97b4a1a7.jpg",
    TrailerAudioFileKey: "",
    TotalFollow: 0,
    ListenCount: 0,
    EpisodeCount: 12,
    Podcaster: {
      Id: 0,
      FullName: "",
      Email: "",
      ImageUrl: "",
    },
    PodcastCategory: {
      Id: 0,
      Name: "",
    },
    PodcastSubCategory: {
      Id: 0,
      Name: "",
      PodcastCategoryId: 0,
    },
    PodcastShowSubscriptionType: {
      Id: 0,
      Name: "",
    },
    PodcastChannel: {
      Id: "",
      Name: "",
      ImageUrl: "",
    },
    Hashtags: [{ Id: 0, Name: "" }],
    TakenDownReason: "",
    CreatedAt: "",
    UpdatedAt: "",
    CurrentStatus: {
      Id: 0,
      Name: "",
    },
  },
  {
    Id: "4", //Chỉnh cái này
    Name: "LAUV Your Self", //Chỉnh cái này
    Description: "",
    Language: "",
    ReleaseDate: "",
    IsReleased: true,
    Copyright: "",
    UploadFrequency: "",
    RatingCount: 0,
    AverageRating: 0,
    // Chỉnh cái này
    ImageUrl:
      "https://i.pinimg.com/736x/df/a7/ee/dfa7eeb5699a7f8477e5e8f61d864457.jpg",
    TrailerAudioFileKey: "",
    TotalFollow: 0,
    ListenCount: 0,
    EpisodeCount: 12,
    Podcaster: {
      Id: 0,
      FullName: "",
      Email: "",
      ImageUrl: "",
    },
    PodcastCategory: {
      Id: 0,
      Name: "",
    },
    PodcastSubCategory: {
      Id: 0,
      Name: "",
      PodcastCategoryId: 0,
    },
    PodcastShowSubscriptionType: {
      Id: 0,
      Name: "",
    },
    PodcastChannel: {
      Id: "",
      Name: "",
      ImageUrl: "",
    },
    Hashtags: [{ Id: 0, Name: "" }],
    TakenDownReason: "",
    CreatedAt: "",
    UpdatedAt: "",
    CurrentStatus: {
      Id: 0,
      Name: "",
    },
  },
];

const PodcasterDetailsPage = () => {
  const navigate = useNavigate();

  const [podcaster, setPodcaster] = useState<PodcasterUI | null>(null);
  const [channels, setChannels] = useState<ChannelUI[]>([]);
  const [shows, setShows] = useState<ShowUI[]>([]);
  const [isFollowed, setIsFollowed] = useState(false);
  const [isBookingAvailable, setIsBookingAvailable] = useState(true);

  useEffect(() => {
    setChannels(MockPodcasterChannels);
    setShows(MockPodcasterShows);
  }, []);

  const handleCreateBooking = () => {
    // store a small podcaster object as a JSON string in localStorage
    const payload = {
      Id: PodcasterDetails.Id,
      FullName: PodcasterDetails.FullName,
      ImageUrl: PodcasterDetails.ImageUrl,
    };
    try {
      localStorage.setItem("selectedPodcaster", JSON.stringify(payload));
      navigate("/media-player/management/bookings/create");
    } catch (e) {
      // storage may be full or unavailable in some privacy modes
      console.error("Failed to save selected podcaster to localStorage", e);
    }
  };

  return (
    <div className="w-full flex flex-col">
      <div className="w-full flex items-center px-5 py-2">
        <div
          onClick={() => navigate(-1)}
          className="h-12 flex items-center gap-3 text-white hover:underline cursor-pointer"
        >
          <IoIosArrowBack size={20} />
          <p className="font-light font-poppins">Back</p>
        </div>
      </div>

      {/* Customer Informations with strongly blurred background image */}
      <div className="w-full h-[400px] flex px-10 relative overflow-hidden">
        {/* blurred background image (covers full area) */}
        <img
          src={PodcasterDetails.ImageUrl}
          alt={PodcasterDetails.FullName}
          className="absolute inset-0 w-full h-full object-cover filter blur-xl scale-110 opacity-80"
          style={{
            // ensure it fills and the blur is strong; scale a bit to avoid edges showing
            transformOrigin: "center",
          }}
        />

        {/* dark overlay to keep foreground readable */}
        <div className="absolute inset-0 bg-black/50" />

        {/* actual content goes above the background */}
        <div className="relative z-10 w-full flex items-center gap-10">
          <div className="w-[312px] h-[312px] rounded-full overflow-hidden shadow-xl">
            <img
              src={PodcasterDetails.ImageUrl}
              alt={PodcasterDetails.FullName}
              className="w-full h-full object-cover"
            />
          </div>

          <div className="flex-1 flex flex-col justify-start text-white gap-3">
            <p className="text-[96px] font-bold p-0 m-0">
              {PodcasterDetails.PodcasterProfile?.Name.toLocaleUpperCase()}
            </p>
            <p className="font-poppins font-bold text-gray-300">
              {PodcasterDetails.PodcasterProfile.TotalFollow.toLocaleString()}{" "}
              Followers
            </p>
            <p className="text-sm text-gray-400 mt-2 line-clamp-4 w-2/3">
              {PodcasterDetails.PodcasterProfile.Description}
            </p>
            <div className="w-full  flex items-center justify-start gap-5 mt-5">
              {!isFollowed && (
                <div className="px-5 h-[30px] border-mystic-green border-2 bg-transparent cursor-pointer transition-all hover:-translate-y-1 ease-in-out duration-500 flex items-center justify-center rounded-full font-semibold">
                  <p className="text-mystic-green">Follow</p>
                </div>
              )}
              {isBookingAvailable && (
                <div
                  onClick={() => handleCreateBooking()}
                  className="px-5 h-[30px] bg-mystic-green text-black cursor-pointer transition-all hover:-translate-y-1 ease-in-out duration-500 flex items-center justify-center rounded-full font-bold"
                >
                  <p>Book This Podcaster</p>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Podcasters Channels */}
      <div className="w-full mt-20 flex flex-col gap-5 px-8">
        <p className="text-4xl font-bold text-white">
          My <span className="text-mystic-green">Channels</span>
        </p>
        <div className="w-full">
          <Carousel
            opts={{
              align: "start",
              loop: true,
            }}
            plugins={[
              Autoplay({
                delay: 4000,
              }),
            ]}
            className="w-full"
          >
            <CarouselContent>
              {channels.map((channel) => (
                <CarouselItem
                  key={channel.Id}
                  className="basis-1/2 md:basis-1/3 lg:basis-1/4"
                >
                  <div
                    onClick={() =>
                      navigate(`/media-player/channels/${channel.Id}`)
                    }
                    className="p-5"
                  >
                    <ChannelCard channel={channel} />
                  </div>
                </CarouselItem>
              ))}
            </CarouselContent>
          </Carousel>
        </div>
      </div>

      {/* Podcasters Shows */}
      <div className="w-full mt-20 flex flex-col gap-5 px-8 mb-20">
        <p className="text-3xl font-bold text-white">
          My <span className="text-mystic-green">Shows</span>
        </p>
        <div className="w-full">
          <Carousel
            opts={{
              align: "start",
              loop: true,
            }}
            plugins={[
              Autoplay({
                delay: 4000,
              }),
            ]}
            className="w-full"
          >
            <CarouselContent>
              {shows.map((show) => (
                <CarouselItem
                  key={show.Id}
                  className="basis-1/2 md:basis-1/3 lg:basis-1/5"
                >
                  <div className="p-1">
                    <ShowCard show={show} />
                  </div>
                </CarouselItem>
              ))}
            </CarouselContent>
          </Carousel>
        </div>
      </div>
    </div>
  );
};
export default PodcasterDetailsPage;

import {
  Carousel,
  CarouselContent,
  CarouselItem,
} from "@/components/ui/carousel";
import { Skeleton } from "@/components/ui/skeleton";
import { useGetTrendingFeedQuery } from "@/core/services/feed/feed.service";
import type {
  CategoryX,
  CategoryXUI,
  HotChannels,
  HotChannelsUI,
  HotPodcasters,
  HotPodcastersUI,
  HotShows,
  HotShowsUI,
  NewEpisodes,
  NewEpisodesUI,
  PopularChannels,
  PopularChannelsUI,
  PopularEpisodes,
  PopularEpisodesUI,
  PopularPodcasters,
  PopularPodcastersUI,
  PopularShows,
  PopularShowsUI,
  TrendingData,
  TrendingDataUI,
} from "@/core/types/feed";
import {
  resolveFiles,
  type FileResolveConfig,
} from "@/core/utils/fileResolver.util";
import Autoplay from "embla-carousel-autoplay";
import { useEffect, useState } from "react";
import PodcasterCard from "./components/PodcasterCard";
import ChannelCard from "./components/ChannelCard";
import ShowCard from "./components/ShowCard";
import EpisodeCard from "./components/EpisodeCard";

// File config for Podcasters sections (Popular & Hot)
const podcastersFileConfig: FileResolveConfig[] = [
  {
    type: "AccountPublic",
    path: "PodcasterList[].MainImageFileKey",
    output: "PodcasterList[].ImageUrl",
  },
];

// File config for Category sections (Category1-6)
const categoryFileConfig: FileResolveConfig[] = [
  // Show main image
  {
    type: "PodcastPublic",
    path: "ShowList[].MainImageFileKey",
    output: "ShowList[].ImageUrl",
  },
  // Podcaster avatar
  {
    type: "AccountPublic",
    path: "ShowList[].Podcaster.MainImageFileKey",
    output: "ShowList[].Podcaster.ImageUrl",
  },
  // Channel image
  {
    type: "PodcastPublic",
    path: "ShowList[].PodcastChannel.MainImageFileKey",
    output: "ShowList[].PodcastChannel.ImageUrl",
  },
];

// File config for Channels sections (Popular & Hot)
const channelsFileConfig: FileResolveConfig[] = [
  // Channel main image & background
  {
    type: "PodcastPublic",
    path: "ChannelList[].MainImageFileKey",
    output: "ChannelList[].ImageUrl",
  },
  {
    type: "PodcastPublic",
    path: "ChannelList[].BackgroundImageFileKey",
    output: "ChannelList[].BackgroundImageUrl",
  },
  // Channel podcaster avatar
  {
    type: "AccountPublic",
    path: "ChannelList[].Podcaster.MainImageFileKey",
    output: "ChannelList[].Podcaster.ImageUrl",
  },
];

// File config for Shows sections (Popular & Hot)
const showsFileConfig: FileResolveConfig[] = [
  {
    type: "PodcastPublic",
    path: "ShowList[].MainImageFileKey",
    output: "ShowList[].ImageUrl",
  },
  {
    type: "AccountPublic",
    path: "ShowList[].Podcaster.MainImageFileKey",
    output: "ShowList[].Podcaster.ImageUrl",
  },
  {
    type: "PodcastPublic",
    path: "ShowList[].PodcastChannel.MainImageFileKey",
    output: "ShowList[].PodcastChannel.ImageUrl",
  },
];

// File config for Episodes sections (New & Popular)
const episodesFileConfig: FileResolveConfig[] = [
  // Episode main image
  {
    type: "PodcastPublic",
    path: "EpisodeList[].MainImageFileKey",
    output: "EpisodeList[].ImageUrl",
  },
  // Episode's Show image
  {
    type: "PodcastPublic",
    path: "EpisodeList[].PodcastShow.MainImageFileKey",
    output: "EpisodeList[].PodcastShow.ImageUrl",
  },
];

const mockTrendingDataUI: TrendingDataUI = {
  PopularPodcasters: {
    PodcasterList: [
      {
        Id: 1,
        FullName: "Thống",
        Email: "alice@example.com",
        ImageUrl:
          "https://i.pinimg.com/1200x/a4/ed/4a/a4ed4a424e39bc91432cfcee14248d98.jpg",
      },
      {
        Id: 2,
        FullName: "David Tran",
        Email: "david@example.com",
        ImageUrl:
          "https://i.pinimg.com/736x/ba/71/e9/ba71e90f7049d0709d744b3affb04998.jpg",
      },
      {
        Id: 3,
        FullName: "Mina Le",
        Email: "mina@example.com",
        ImageUrl:
          "https://i.pinimg.com/736x/09/33/07/093307cc82e08760b9c7124249759385.jpg",
      },
    ],
  },

  Category1: {
    PodcastCategory: { Id: 10, Name: "Terrify" },
    ShowList: [
      {
        Id: "show-001",
        Name: "Art The Clown",
        Description: "Daily habits to improve your life.",
        Language: "English",
        ReleaseDate: "2025-11-16T07:27:46.917Z",
        IsReleased: true,
        Copyright: "© Better Studio",
        UploadFrequency: "Weekly",
        RatingCount: 1200,
        AverageRating: 4.8,
        ImageUrl:
          "https://i.pinimg.com/736x/3a/d7/98/3ad798aaac18d8f47aed31959082dc09.jpg",
        TrailerAudioFileKey: "better-everyday-trailer.mp3",
        TotalFollow: 45000,
        ListenCount: 560000,
        EpisodeCount: 86,
        Podcaster: {
          Id: 1,
          FullName: "Alice Nguyen",
          Email: "alice@example.com",
          ImageUrl: "/images/podcasters/alice.jpg",
        },
        PodcastCategory: { Id: 10, Name: "Terrify" },
        PodcastSubCategory: {
          Id: 101,
          Name: "Motivation",
          PodcastCategoryId: 10,
        },
        PodcastShowSubscriptionType: { Id: 1, Name: "Free" },
        PodcastChannel: {
          Id: "channel-001",
          Name: "Alice Network",
          ImageUrl:
            "https://i.pinimg.com/736x/26/08/ea/2608ea2548e5135069517f711f07de35.jpg",
        },
        Hashtags: [{ Id: 1, Name: "#motivation" }],
        TakenDownReason: "",
        CreatedAt: "2025-11-01T07:00:00.000Z",
        UpdatedAt: "2025-11-10T07:00:00.000Z",
        CurrentStatus: { Id: 1, Name: "Active" },
      },
    ],
  },

  HotPodcasters: {
    PodcasterList: [
      {
        Id: 4,
        FullName: "Kiana",
        Email: "henry@example.com",
        ImageUrl:
          "https://i.pinimg.com/1200x/22/b2/b6/22b2b6400a15cd557d9761d4713848cc.jpg",
      },
      {
        Id: 5,
        FullName: "Holly Henry",
        Email: "henry@example.com",
        ImageUrl:
          "https://i.pinimg.com/736x/5b/cf/fc/5bcffc5de7d4898d4a44536172d80594.jpg",
      },
      {
        Id: 6,
        FullName: "Vợ Iu",
        Email: "henry@example.com",
        ImageUrl:
          "https://i.pinimg.com/736x/83/d6/56/83d65678c01c214be75d320a9208c45e.jpg",
      },
      {
        Id: 7,
        FullName: "Đan Phượng",
        Email: "henry@example.com",
        ImageUrl:
          "https://i.pinimg.com/736x/ef/ea/79/efea79d9dc4ac23659f44a368ef8d41f.jpg",
      },
      {
        Id: 8,
        FullName: "Quỳnh Bei",
        Email: "henry@example.com",
        ImageUrl:
          "https://i.pinimg.com/736x/2a/06/a7/2a06a7cef1b87e4aab1c1243c144d89e.jpg",
      },
      {
        Id: 9,
        FullName: "Trần Hà Linh",
        Email: "henry@example.com",
        ImageUrl:
          "https://i.pinimg.com/736x/7a/72/f2/7a72f27aca239195e2ad74397b70af8b.jpg",
      },
    ],
  },

  Category2: {
    PodcastCategory: { Id: 20, Name: "Society & Culture" },
    ShowList: [
      {
        Id: "show-002",
        Name: "Voices of Vietnam",
        Description: "Stories of people across the country.",
        Language: "Vietnamese",
        ReleaseDate: "2025-11-16T07:27:46.917Z",
        IsReleased: true,
        Copyright: "© VOV Studio",
        UploadFrequency: "Daily",
        RatingCount: 800,
        AverageRating: 4.6,
        ImageUrl: "/images/shows/voices-vietnam.jpg",
        TrailerAudioFileKey: "voices-trailer.mp3",
        TotalFollow: 30000,
        ListenCount: 450000,
        EpisodeCount: 150,
        Podcaster: {
          Id: 2,
          FullName: "David Tran",
          Email: "david@example.com",
          ImageUrl: "/images/podcasters/david.jpg",
        },
        PodcastCategory: { Id: 20, Name: "Society & Culture" },
        PodcastSubCategory: {
          Id: 201,
          Name: "Culture",
          PodcastCategoryId: 20,
        },
        PodcastShowSubscriptionType: { Id: 1, Name: "Free" },
        PodcastChannel: {
          Id: "channel-002",
          Name: "Viet Stories",
          ImageUrl: "/images/channels/viet-stories.jpg",
        },
        Hashtags: [{ Id: 5, Name: "#culture" }],
        TakenDownReason: "",
        CreatedAt: "2025-10-10T07:27:46.917Z",
        UpdatedAt: "2025-11-15T07:27:46.917Z",
        CurrentStatus: { Id: 1, Name: "Active" },
      },
    ],
  },

  PopularChannels: {
    ChannelList: [
      {
        Id: "channel-001",
        Name: "Alice Network",
        Description: "A hub for self-improvement shows.",
        BackgroundImageUrl: "/images/channels/bg-alice.jpg",
        ImageUrl:
          "https://i.pinimg.com/736x/26/08/ea/2608ea2548e5135069517f711f07de35.jpg",
        TotalFavorite: 25000,
        ListenCount: 800000,
        ShowCount: 4,
        Podcaster: {
          Id: 1,
          FullName: "Alice Nguyen",
          Email: "alice@example.com",
          ImageUrl:
            "https://i.pinimg.com/736x/26/08/ea/2608ea2548e5135069517f711f07de35.jpg",
        },
        PodcastCategory: { Id: 10, Name: "Self Improvement" },
        PodcastSubCategory: {
          Id: 101,
          Name: "Motivation",
          PodcastCategoryId: 10,
        },
        Hashtags: [{ Id: 1, Name: "#growth" }],
        CreatedAt: "2025-09-01T10:00:00.000Z",
        UpdatedAt: "2025-11-10T10:00:00.000Z",
        CurrentStatus: { Id: 1, Name: "Active" },
      },
      {
        Id: "channel-002",
        Name: "Hoàng Hôn của Chư Thần",
        Description: "A hub for self-improvement shows.",
        BackgroundImageUrl: "/images/channels/bg-alice.jpg",
        ImageUrl:
          "https://i.pinimg.com/736x/2e/b6/34/2eb6347e904b73644f2b5ffa2d4cce36.jpg",
        TotalFavorite: 12000,
        ListenCount: 800000,
        ShowCount: 4,
        Podcaster: {
          Id: 1,
          FullName: "Numerion Zack",
          Email: "alice@example.com",
          ImageUrl:
            "https://i.pinimg.com/736x/26/08/ea/2608ea2548e5135069517f711f07de35.jpg",
        },
        PodcastCategory: { Id: 10, Name: "Self Improvement" },
        PodcastSubCategory: {
          Id: 101,
          Name: "Motivation",
          PodcastCategoryId: 10,
        },
        Hashtags: [{ Id: 1, Name: "#growth" }],
        CreatedAt: "2025-09-01T10:00:00.000Z",
        UpdatedAt: "2025-11-10T10:00:00.000Z",
        CurrentStatus: { Id: 1, Name: "Active" },
      },
      {
        Id: "channel-003",
        Name: "Mạn Đà La",
        Description: "A hub for self-improvement shows.",
        BackgroundImageUrl: "/images/channels/bg-alice.jpg",
        ImageUrl:
          "https://i.pinimg.com/736x/33/f1/9e/33f19e21107fdda07c96bbd1329ef0ed.jpg",
        TotalFavorite: 200000000,
        ListenCount: 800000,
        ShowCount: 4,
        Podcaster: {
          Id: 1,
          FullName: "Rick",
          Email: "alice@example.com",
          ImageUrl:
            "https://i.pinimg.com/736x/26/08/ea/2608ea2548e5135069517f711f07de35.jpg",
        },
        PodcastCategory: { Id: 10, Name: "Self Improvement" },
        PodcastSubCategory: {
          Id: 101,
          Name: "Motivation",
          PodcastCategoryId: 10,
        },
        Hashtags: [{ Id: 1, Name: "#growth" }],
        CreatedAt: "2025-09-01T10:00:00.000Z",
        UpdatedAt: "2025-11-10T10:00:00.000Z",
        CurrentStatus: { Id: 1, Name: "Active" },
      },
    ],
  },

  Category3: {
    PodcastCategory: { Id: 30, Name: "Technology" },
    ShowList: [
      {
        Id: "show-003",
        Name: "Tech Tomorrow",
        Description: "Future tech trends.",
        Language: "English",
        ReleaseDate: "2025-10-16T07:27:46.917Z",
        IsReleased: true,
        Copyright: "© TechHub",
        UploadFrequency: "Weekly",
        RatingCount: 600,
        AverageRating: 4.5,
        ImageUrl: "/images/shows/tech-tomorrow.jpg",
        TrailerAudioFileKey: "tech-tomorrow-trailer.mp3",
        TotalFollow: 20000,
        ListenCount: 300000,
        EpisodeCount: 60,
        Podcaster: {
          Id: 5,
          FullName: "Chris Do",
          Email: "chris@example.com",
          ImageUrl: "/images/podcasters/chris.jpg",
        },
        PodcastCategory: { Id: 30, Name: "Technology" },
        PodcastSubCategory: {
          Id: 301,
          Name: "Innovation",
          PodcastCategoryId: 30,
        },
        PodcastShowSubscriptionType: { Id: 1, Name: "Free" },
        PodcastChannel: {
          Id: "channel-003",
          Name: "Tech Hub",
          ImageUrl: "/images/channels/tech-hub.jpg",
        },
        Hashtags: [{ Id: 12, Name: "#technology" }],
        TakenDownReason: "",
        CreatedAt: "2025-08-21T07:00:00.000Z",
        UpdatedAt: "2025-11-12T07:00:00.000Z",
        CurrentStatus: { Id: 1, Name: "Active" },
      },
    ],
  },

  HotChannels: {
    ChannelList: [
      {
        Id: "channel-002",
        Name: "Những Kẻ Cô Đơn",
        Description: "Vietnamese cultural podcasts.",
        BackgroundImageUrl: "/images/channels/bg-viet.jpg",
        ImageUrl:
          "https://i.pinimg.com/1200x/59/c7/af/59c7af7242f0de6157b149c93f0f1dc6.jpg",
        TotalFavorite: 18000,
        ListenCount: 420000,
        ShowCount: 2,
        Podcaster: {
          Id: 2,
          FullName: "David Tran",
          Email: "david@example.com",
          ImageUrl: "/images/podcasters/david.jpg",
        },
        PodcastCategory: { Id: 20, Name: "Society & Culture" },
        PodcastSubCategory: {
          Id: 201,
          Name: "Culture",
          PodcastCategoryId: 20,
        },
        Hashtags: [{ Id: 5, Name: "#culture" }],
        CreatedAt: "2025-09-02T08:00:00.000Z",
        UpdatedAt: "2025-11-11T08:00:00.000Z",
        CurrentStatus: { Id: 1, Name: "Active" },
      },
    ],
  },

  Category4: {
    PodcastCategory: { Id: 40, Name: "Comedy" },
    ShowList: [
      {
        Id: "show-004",
        Name: "Laugh Now",
        Description: "Funny moments and stories.",
        Language: "English",
        ReleaseDate: "2025-11-01T07:27:46.917Z",
        IsReleased: true,
        Copyright: "© LaughHub",
        UploadFrequency: "Weekly",
        RatingCount: 300,
        AverageRating: 4.3,
        ImageUrl: "/images/shows/laugh-now.jpg",
        TrailerAudioFileKey: "laugh-now-trailer.mp3",
        TotalFollow: 12000,
        ListenCount: 180000,
        EpisodeCount: 45,
        Podcaster: {
          Id: 6,
          FullName: "Tom Lee",
          Email: "tom@example.com",
          ImageUrl: "/images/podcasters/tom.jpg",
        },
        PodcastCategory: { Id: 40, Name: "Comedy" },
        PodcastSubCategory: {
          Id: 401,
          Name: "Stand-up",
          PodcastCategoryId: 40,
        },
        PodcastShowSubscriptionType: { Id: 1, Name: "Free" },
        PodcastChannel: {
          Id: "channel-004",
          Name: "LaughHub",
          ImageUrl: "/images/channels/laughhub.jpg",
        },
        Hashtags: [{ Id: 7, Name: "#funny" }],
        TakenDownReason: "",
        CreatedAt: "2025-10-05T07:00:00.000Z",
        UpdatedAt: "2025-11-15T07:00:00.000Z",
        CurrentStatus: { Id: 1, Name: "Active" },
      },
    ],
  },

  PopularShows: {
    ShowList: [
      {
        Id: "show-002",
        Name: "Dune: The Raise Of Desert",
        Description: "Stories of Vietnamese lives.",
        Language: "Vietnamese",
        ReleaseDate: "2025-11-16T07:27:46.917Z",
        IsReleased: true,
        Copyright: "© VOV",
        UploadFrequency: "Daily",
        RatingCount: 800,
        AverageRating: 4.6,
        ImageUrl:
          "https://i.pinimg.com/1200x/78/29/09/782909bb6a2a778a17ec33dd7d053609.jpg",
        TrailerAudioFileKey: "voices-trailer.mp3",
        TotalFollow: 30000,
        ListenCount: 450000,
        EpisodeCount: 150,
        Podcaster: {
          Id: 2,
          FullName: "David Tran",
          Email: "david@example.com",
          ImageUrl: "/images/podcasters/david.jpg",
        },
        PodcastCategory: { Id: 20, Name: "Society & Culture" },
        PodcastSubCategory: {
          Id: 201,
          Name: "Culture",
          PodcastCategoryId: 20,
        },
        PodcastShowSubscriptionType: { Id: 1, Name: "Free" },
        PodcastChannel: {
          Id: "channel-002",
          Name: "Viet Stories",
          ImageUrl: "/images/channels/viet-stories.jpg",
        },
        Hashtags: [{ Id: 5, Name: "#culture" }],
        TakenDownReason: "",
        CreatedAt: "2025-10-10T07:27:46.917Z",
        UpdatedAt: "2025-11-12T07:27:46.917Z",
        CurrentStatus: { Id: 1, Name: "Active" },
      },
      {
        Id: "show-005",
        Name: "Can't Stop ?",
        Description: "Stories of Vietnamese lives.",
        Language: "Vietnamese",
        ReleaseDate: "2025-11-16T07:27:46.917Z",
        IsReleased: true,
        Copyright: "© VOV",
        UploadFrequency: "Daily",
        RatingCount: 800,
        AverageRating: 4.6,
        ImageUrl:
          "https://i.pinimg.com/1200x/75/c2/30/75c230b51e628d93ebadb5d7b73538a4.jpg",
        TrailerAudioFileKey: "voices-trailer.mp3",
        TotalFollow: 30000,
        ListenCount: 450000,
        EpisodeCount: 150,
        Podcaster: {
          Id: 2,
          FullName: "David Tran",
          Email: "david@example.com",
          ImageUrl: "/images/podcasters/david.jpg",
        },
        PodcastCategory: { Id: 20, Name: "Society & Culture" },
        PodcastSubCategory: {
          Id: 201,
          Name: "Culture",
          PodcastCategoryId: 20,
        },
        PodcastShowSubscriptionType: { Id: 1, Name: "Free" },
        PodcastChannel: {
          Id: "channel-002",
          Name: "Viet Stories",
          ImageUrl: "/images/channels/viet-stories.jpg",
        },
        Hashtags: [{ Id: 5, Name: "#culture" }],
        TakenDownReason: "",
        CreatedAt: "2025-10-10T07:27:46.917Z",
        UpdatedAt: "2025-11-12T07:27:46.917Z",
        CurrentStatus: { Id: 1, Name: "Active" },
      },
      {
        Id: "show-003",
        Name: "Blade Runner: Shadows of the Past",
        Description: "Stories of Vietnamese lives.",
        Language: "Vietnamese",
        ReleaseDate: "2025-11-16T07:27:46.917Z",
        IsReleased: true,
        Copyright: "© VOV",
        UploadFrequency: "Daily",
        RatingCount: 800,
        AverageRating: 4.6,
        ImageUrl:
          "https://i.pinimg.com/1200x/36/13/fe/3613fe3dc4b77c7e1e863ede71ea3f59.jpg",
        TrailerAudioFileKey: "voices-trailer.mp3",
        TotalFollow: 30000,
        ListenCount: 450000,
        EpisodeCount: 150,
        Podcaster: {
          Id: 2,
          FullName: "David Tran",
          Email: "david@example.com",
          ImageUrl: "/images/podcasters/david.jpg",
        },
        PodcastCategory: { Id: 20, Name: "Society & Culture" },
        PodcastSubCategory: {
          Id: 201,
          Name: "Culture",
          PodcastCategoryId: 20,
        },
        PodcastShowSubscriptionType: { Id: 1, Name: "Free" },
        PodcastChannel: {
          Id: "channel-002",
          Name: "Viet Stories",
          ImageUrl: "/images/channels/viet-stories.jpg",
        },
        Hashtags: [{ Id: 5, Name: "#culture" }],
        TakenDownReason: "",
        CreatedAt: "2025-10-10T07:27:46.917Z",
        UpdatedAt: "2025-11-12T07:27:46.917Z",
        CurrentStatus: { Id: 1, Name: "Active" },
      },
      {
        Id: "show-004",
        Name: "The Matrix: Rebirth Chronicles",
        Description: "Stories of Vietnamese lives.",
        Language: "Vietnamese",
        ReleaseDate: "2025-11-16T07:27:46.917Z",
        IsReleased: true,
        Copyright: "© VOV",
        UploadFrequency: "Daily",
        RatingCount: 800,
        AverageRating: 4.6,
        ImageUrl:
          "https://i.pinimg.com/1200x/da/0a/e4/da0ae4666fe2f97280dacde2fac03841.jpg",
        TrailerAudioFileKey: "voices-trailer.mp3",
        TotalFollow: 30000,
        ListenCount: 450000,
        EpisodeCount: 150,
        Podcaster: {
          Id: 2,
          FullName: "David Tran",
          Email: "david@example.com",
          ImageUrl: "/images/podcasters/david.jpg",
        },
        PodcastCategory: { Id: 20, Name: "Society & Culture" },
        PodcastSubCategory: {
          Id: 201,
          Name: "Culture",
          PodcastCategoryId: 20,
        },
        PodcastShowSubscriptionType: { Id: 1, Name: "Free" },
        PodcastChannel: {
          Id: "channel-002",
          Name: "Viet Stories",
          ImageUrl: "/images/channels/viet-stories.jpg",
        },
        Hashtags: [{ Id: 5, Name: "#culture" }],
        TakenDownReason: "",
        CreatedAt: "2025-10-10T07:27:46.917Z",
        UpdatedAt: "2025-11-12T07:27:46.917Z",
        CurrentStatus: { Id: 1, Name: "Active" },
      },
    ],
  },

  Category5: {
    PodcastCategory: { Id: 50, Name: "Business" },
    ShowList: [
      {
        Id: "show-005",
        Name: "Startup Talks",
        Description: "Interviews with startup founders.",
        Language: "English",
        ReleaseDate: "2025-09-16T07:27:46.917Z",
        IsReleased: true,
        Copyright: "© StartupLab",
        UploadFrequency: "Weekly",
        RatingCount: 900,
        AverageRating: 4.7,
        ImageUrl: "/images/shows/startup-talks.jpg",
        TrailerAudioFileKey: "startup-talks-trailer.mp3",
        TotalFollow: 35000,
        ListenCount: 500000,
        EpisodeCount: 72,
        Podcaster: {
          Id: 8,
          FullName: "Jenny Ha",
          Email: "jenny@example.com",
          ImageUrl: "/images/podcasters/jenny.jpg",
        },
        PodcastCategory: { Id: 50, Name: "Business" },
        PodcastSubCategory: {
          Id: 501,
          Name: "Entrepreneurship",
          PodcastCategoryId: 50,
        },
        PodcastShowSubscriptionType: { Id: 2, Name: "Premium" },
        PodcastChannel: {
          Id: "channel-005",
          Name: "BizCast",
          ImageUrl: "/images/channels/bizcast.jpg",
        },
        Hashtags: [{ Id: 9, Name: "#startup" }],
        TakenDownReason: "",
        CreatedAt: "2025-06-12T07:27:46.917Z",
        UpdatedAt: "2025-11-16T07:27:46.917Z",
        CurrentStatus: { Id: 1, Name: "Active" },
      },
    ],
  },

  HotShows: {
    ShowList: [
      {
        Id: "show-004",
        Name: "Laugh Now",
        Description: "Funny stories.",
        Language: "English",
        ReleaseDate: "2025-11-01T07:27:46.917Z",
        IsReleased: true,
        Copyright: "© LaughHub",
        UploadFrequency: "Weekly",
        RatingCount: 300,
        AverageRating: 4.3,
        ImageUrl:
          "https://i.pinimg.com/736x/fd/e8/78/fde878035995e9238d0e9bd258a26c67.jpg",
        TrailerAudioFileKey: "laugh-now-trailer.mp3",
        TotalFollow: 12000,
        ListenCount: 180000,
        EpisodeCount: 45,
        Podcaster: {
          Id: 6,
          FullName: "Tom Lee",
          Email: "tom@example.com",
          ImageUrl: "/images/podcasters/tom.jpg",
        },
        PodcastCategory: { Id: 40, Name: "Comedy" },
        PodcastSubCategory: {
          Id: 401,
          Name: "Stand-up",
          PodcastCategoryId: 40,
        },
        PodcastShowSubscriptionType: { Id: 1, Name: "Free" },
        PodcastChannel: {
          Id: "channel-004",
          Name: "LaughHub",
          ImageUrl: "/images/channels/laughhub.jpg",
        },
        Hashtags: [{ Id: 7, Name: "#funny" }],
        TakenDownReason: "",
        CreatedAt: "2025-10-05T07:27:46.917Z",
        UpdatedAt: "2025-11-15T07:27:46.917Z",
        CurrentStatus: { Id: 1, Name: "Active" },
      },
    ],
  },

  Category6: {
    PodcastCategory: { Id: 60, Name: "Health" },
    ShowList: [
      {
        Id: "show-006",
        Name: "Mindful Living",
        Description: "Mental health and mindfulness tips.",
        Language: "English",
        ReleaseDate: "2025-10-11T07:27:46.917Z",
        IsReleased: true,
        Copyright: "© HealthHub",
        UploadFrequency: "Twice a week",
        RatingCount: 700,
        AverageRating: 4.9,
        ImageUrl: "/images/shows/mindful-living.jpg",
        TrailerAudioFileKey: "mindful-living-trailer.mp3",
        TotalFollow: 40000,
        ListenCount: 600000,
        EpisodeCount: 92,
        Podcaster: {
          Id: 9,
          FullName: "Sarah Lee",
          Email: "sarah@example.com",
          ImageUrl: "/images/podcasters/sarah.jpg",
        },
        PodcastCategory: { Id: 60, Name: "Health" },
        PodcastSubCategory: {
          Id: 601,
          Name: "Mental Health",
          PodcastCategoryId: 60,
        },
        PodcastShowSubscriptionType: { Id: 1, Name: "Free" },
        PodcastChannel: {
          Id: "channel-006",
          Name: "HealthCast",
          ImageUrl: "/images/channels/healthcast.jpg",
        },
        Hashtags: [{ Id: 11, Name: "#mindfulness" }],
        TakenDownReason: "",
        CreatedAt: "2025-07-30T07:27:46.917Z",
        UpdatedAt: "2025-11-15T07:27:46.917Z",
        CurrentStatus: { Id: 1, Name: "Active" },
      },
    ],
  },

  NewEpisodes: {
    EpisodeList: [
      {
        Id: "ep-001",
        Name: "The Fallen Lord",
        Description: "Small steps that change your life.",
        ExplicitContent: false,
        ReleaseDate: "2025-11-16T07:27:46.917Z",
        EpisodeOrder: 87,
        IsReleased: true,
        ImageUrl:
          "https://i.pinimg.com/1200x/eb/5e/19/eb5e1990238de8f3be2786da5313e87b.jpg",
        AudioFileKey: "better-ep87.mp3",
        AudioFileSize: 12345678,
        AudioLength: 1420,
        PodcastEpisodeSubscriptionType: { Id: 1, Name: "Free" },
        PodcastShow: {
          Id: "show-001",
          Name: "Better Everyday",
          ImageUrl:
            "https://i.pinimg.com/736x/04/96/fd/0496fd29c4805dfe2d294d9efb2e1fb1.jpg",
          ReleaseDate: "2025-11-16",
          IsReleased: true,
        },
        Hashtags: [{ Id: 1, Name: "#motivation" }],
        SeasonNumber: 3,
        TotalSave: 2000,
        ListenCount: 35000,
        IsAudioPublishable: true,
        TakenDownReason: "",
        CreatedAt: "2025-11-16T07:00:00.000Z",
        UpdatedAt: "2025-11-16T07:10:00.000Z",
        CurrentStatus: { Id: 1, Name: "Active" },
      },
      {
        Id: "ep-002",
        Name: "Paradise Lost",
        Description: "Small steps that change your life.",
        ExplicitContent: false,
        ReleaseDate: "2025-11-16T07:27:46.917Z",
        EpisodeOrder: 87,
        IsReleased: true,
        ImageUrl:
          "https://i.pinimg.com/1200x/93/c5/f0/93c5f0be8fee7a9a36a8c72cd9f22857.jpg",
        AudioFileKey: "better-ep87.mp3",
        AudioFileSize: 12345678,
        AudioLength: 1420,
        PodcastEpisodeSubscriptionType: { Id: 1, Name: "Free" },
        PodcastShow: {
          Id: "show-001",
          Name: "Better Everyday",
          ImageUrl:
            "https://i.pinimg.com/736x/04/96/fd/0496fd29c4805dfe2d294d9efb2e1fb1.jpg",
          ReleaseDate: "2025-11-16",
          IsReleased: true,
        },
        Hashtags: [{ Id: 1, Name: "#motivation" }],
        SeasonNumber: 3,
        TotalSave: 2000,
        ListenCount: 35000,
        IsAudioPublishable: true,
        TakenDownReason: "",
        CreatedAt: "2025-11-16T07:00:00.000Z",
        UpdatedAt: "2025-11-16T07:10:00.000Z",
        CurrentStatus: { Id: 1, Name: "Active" },
      },
      {
        Id: "ep-003",
        Name: "Demi Gods",
        Description: "Small steps that change your life.",
        ExplicitContent: false,
        ReleaseDate: "2025-11-16T07:27:46.917Z",
        EpisodeOrder: 87,
        IsReleased: true,
        ImageUrl:
          "https://i.pinimg.com/1200x/bd/a0/a8/bda0a8fc73b515663b742033bba8eba6.jpg",
        AudioFileKey: "better-ep87.mp3",
        AudioFileSize: 12345678,
        AudioLength: 1420,
        PodcastEpisodeSubscriptionType: { Id: 1, Name: "Free" },
        PodcastShow: {
          Id: "show-001",
          Name: "Better Everyday",
          ImageUrl:
            "https://i.pinimg.com/736x/04/96/fd/0496fd29c4805dfe2d294d9efb2e1fb1.jpg",
          ReleaseDate: "2025-11-16",
          IsReleased: true,
        },
        Hashtags: [{ Id: 1, Name: "#motivation" }],
        SeasonNumber: 3,
        TotalSave: 2000,
        ListenCount: 35000,
        IsAudioPublishable: true,
        TakenDownReason: "",
        CreatedAt: "2025-11-16T07:00:00.000Z",
        UpdatedAt: "2025-11-16T07:10:00.000Z",
        CurrentStatus: { Id: 1, Name: "Active" },
      },
      {
        Id: "ep-004",
        Name: "Chúa Trời",
        Description: "Small steps that change your life.",
        ExplicitContent: false,
        ReleaseDate: "2025-11-16T07:27:46.917Z",
        EpisodeOrder: 87,
        IsReleased: true,
        ImageUrl:
          "https://i.pinimg.com/1200x/3c/2a/40/3c2a407ed85bb8b28f347a730a2417f9.jpg",
        AudioFileKey: "better-ep87.mp3",
        AudioFileSize: 12345678,
        AudioLength: 1420,
        PodcastEpisodeSubscriptionType: { Id: 1, Name: "Free" },
        PodcastShow: {
          Id: "show-001",
          Name: "Better Everyday",
          ImageUrl:
            "https://i.pinimg.com/736x/04/96/fd/0496fd29c4805dfe2d294d9efb2e1fb1.jpg",
          ReleaseDate: "2025-11-16",
          IsReleased: true,
        },
        Hashtags: [{ Id: 1, Name: "#motivation" }],
        SeasonNumber: 3,
        TotalSave: 2000,
        ListenCount: 35000,
        IsAudioPublishable: true,
        TakenDownReason: "",
        CreatedAt: "2025-11-16T07:00:00.000Z",
        UpdatedAt: "2025-11-16T07:10:00.000Z",
        CurrentStatus: { Id: 1, Name: "Active" },
      },
      {
        Id: "ep-005",
        Name: "Tai Ương Season 5",
        Description: "Small steps that change your life.",
        ExplicitContent: false,
        ReleaseDate: "2025-11-16T07:27:46.917Z",
        EpisodeOrder: 87,
        IsReleased: true,
        ImageUrl:
          "https://i.pinimg.com/736x/63/b4/8c/63b48cff428bb51072b7e1207820931d.jpg",
        AudioFileKey: "better-ep87.mp3",
        AudioFileSize: 12345678,
        AudioLength: 1420,
        PodcastEpisodeSubscriptionType: { Id: 1, Name: "Free" },
        PodcastShow: {
          Id: "show-001",
          Name: "Better Everyday",
          ImageUrl:
            "https://i.pinimg.com/736x/04/96/fd/0496fd29c4805dfe2d294d9efb2e1fb1.jpg",
          ReleaseDate: "2025-11-16",
          IsReleased: true,
        },
        Hashtags: [{ Id: 1, Name: "#motivation" }],
        SeasonNumber: 3,
        TotalSave: 2000,
        ListenCount: 35000,
        IsAudioPublishable: true,
        TakenDownReason: "",
        CreatedAt: "2025-11-16T07:00:00.000Z",
        UpdatedAt: "2025-11-16T07:10:00.000Z",
        CurrentStatus: { Id: 1, Name: "Active" },
      },
    ],
  },

  PopularEpisodes: {
    EpisodeList: [
      {
        Id: "ep-002",
        Name: "The Culture of Central Vietnam",
        Description: "Exploring Hue and Da Nang traditions.",
        ExplicitContent: false,
        ReleaseDate: "2025-11-15T07:27:46.917Z",
        EpisodeOrder: 151,
        IsReleased: true,
        ImageUrl: "/images/episodes/viet-ep151.jpg",
        AudioFileKey: "viet-ep151.mp3",
        AudioFileSize: 14322332,
        AudioLength: 1800,
        PodcastEpisodeSubscriptionType: { Id: 1, Name: "Free" },
        PodcastShow: {
          Id: "show-002",
          Name: "Voices of Vietnam",
          ImageUrl: "/images/shows/voices-vietnam.jpg",
          ReleaseDate: "2025-11-16",
          IsReleased: true,
        },
        Hashtags: [{ Id: 5, Name: "#culture" }],
        SeasonNumber: 5,
        TotalSave: 5000,
        ListenCount: 80000,
        IsAudioPublishable: true,
        TakenDownReason: "",
        CreatedAt: "2025-11-15T07:00:00.000Z",
        UpdatedAt: "2025-11-15T07:20:00.000Z",
        CurrentStatus: { Id: 1, Name: "Active" },
      },
    ],
  },
};

const TrendingPage = () => {
  // STATES
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [trendingData, setTrendingData] = useState<TrendingDataUI | null>(null);

  // Data For Rendering
  // Popular Podcasters
  const [popularPodcasters, setPopularPodcasters] =
    useState<PopularPodcastersUI | null>(null);
  // Hot Podcasters
  const [hotPodcasters, setHotPodcasters] = useState<HotPodcastersUI | null>(
    null
  );
  // Popular Channels
  const [popularChannels, setPopularChannels] =
    useState<PopularChannelsUI | null>(null);
  // Hot Channels
  const [hotChannels, setHotChannels] = useState<HotChannelsUI | null>(null);
  // Popular Shows
  const [popularShows, setPopularShows] = useState<PopularShowsUI | null>(null);
  // Hot Shows
  const [hotShows, setHotShows] = useState<HotShowsUI | null>(null);
  // Popular Episodes
  const [popularEpisodes, setPopularEpisodes] =
    useState<PopularEpisodesUI | null>(null);
  // New Episodes
  const [newEpisodes, setNewEpisodes] = useState<NewEpisodesUI | null>(null);
  // Categories
  const [category1, setCategory1] = useState<CategoryXUI | null>(null);
  const [category2, setCategory2] = useState<CategoryXUI | null>(null);
  const [category3, setCategory3] = useState<CategoryXUI | null>(null);
  const [category4, setCategory4] = useState<CategoryXUI | null>(null);
  const [category5, setCategory5] = useState<CategoryXUI | null>(null);
  const [category6, setCategory6] = useState<CategoryXUI | null>(null);

  // HOOKS
  const { data: trendingDataFromAPI, isLoading: isTrendingDataLoading } =
    useGetTrendingFeedQuery();

  useEffect(() => {
    const resolveEachSection = async () => {
      if (!trendingDataFromAPI) return;
      setIsLoading(true);

      try {
        // Resolve Popular Podcasters
        if (trendingDataFromAPI.PopularPodcasters) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.PopularPodcasters,
            podcastersFileConfig
          );
          const apiData = resolvedData as unknown as PopularPodcastersUI;
          // Merge with mock data
          setPopularPodcasters({
            PodcasterList: [
              ...apiData.PodcasterList,
              ...mockTrendingDataUI.PopularPodcasters.PodcasterList,
            ],
          });
        } else if (mockTrendingDataUI.PopularPodcasters) {
          setPopularPodcasters(mockTrendingDataUI.PopularPodcasters);
        }

        // Resolve Category1
        if (trendingDataFromAPI.Category1) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.Category1,
            categoryFileConfig
          );
          const apiData = resolvedData as unknown as CategoryXUI;
          // Merge with mock data
          setCategory1({
            ...apiData,
            ShowList: [
              ...apiData.ShowList,
              ...mockTrendingDataUI.Category1.ShowList,
            ],
          });
        } else if (mockTrendingDataUI.Category1) {
          setCategory1(mockTrendingDataUI.Category1);
        }

        // Resolve Hot Podcasters
        if (trendingDataFromAPI.HotPodcasters) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.HotPodcasters,
            podcastersFileConfig
          );
          const apiData = resolvedData as unknown as HotPodcastersUI;
          // Merge with mock data
          setHotPodcasters({
            PodcasterList: [
              ...apiData.PodcasterList,
              ...mockTrendingDataUI.HotPodcasters.PodcasterList,
            ],
          });
        } else if (mockTrendingDataUI.HotPodcasters) {
          setHotPodcasters(mockTrendingDataUI.HotPodcasters);
        }

        // Resolve Category2
        if (trendingDataFromAPI.Category2) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.Category2,
            categoryFileConfig
          );
          const apiData = resolvedData as unknown as CategoryXUI;
          // Merge with mock data
          setCategory2({
            ...apiData,
            ShowList: [
              ...apiData.ShowList,
              ...mockTrendingDataUI.Category2.ShowList,
            ],
          });
        } else if (mockTrendingDataUI.Category2) {
          setCategory2(mockTrendingDataUI.Category2);
        }

        // Resolve Popular Channels
        if (trendingDataFromAPI.PopularChannels) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.PopularChannels,
            channelsFileConfig
          );
          const apiData = resolvedData as unknown as PopularChannelsUI;
          // Merge with mock data
          setPopularChannels({
            ChannelList: [
              ...apiData.ChannelList,
              ...mockTrendingDataUI.PopularChannels.ChannelList,
            ],
          });
        } else if (mockTrendingDataUI.PopularChannels) {
          setPopularChannels(mockTrendingDataUI.PopularChannels);
        }

        // Resolve Category3
        if (trendingDataFromAPI.Category3) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.Category3,
            categoryFileConfig
          );
          const apiData = resolvedData as unknown as CategoryXUI;
          // Merge with mock data
          setCategory3({
            ...apiData,
            ShowList: [
              ...apiData.ShowList,
              ...mockTrendingDataUI.Category3.ShowList,
            ],
          });
        } else if (mockTrendingDataUI.Category3) {
          setCategory3(mockTrendingDataUI.Category3);
        }

        // Resolve Hot Channels
        if (trendingDataFromAPI.HotChannels) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.HotChannels,
            channelsFileConfig
          );
          const apiData = resolvedData as unknown as HotChannelsUI;
          // Merge with mock data
          setHotChannels({
            ChannelList: [
              ...apiData.ChannelList,
              ...mockTrendingDataUI.HotChannels.ChannelList,
            ],
          });
        } else if (mockTrendingDataUI.HotChannels) {
          setHotChannels(mockTrendingDataUI.HotChannels);
        }

        // Resolve Category4
        if (trendingDataFromAPI.Category4) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.Category4,
            categoryFileConfig
          );
          const apiData = resolvedData as unknown as CategoryXUI;
          // Merge with mock data
          setCategory4({
            ...apiData,
            ShowList: [
              ...apiData.ShowList,
              ...mockTrendingDataUI.Category4.ShowList,
            ],
          });
        } else if (mockTrendingDataUI.Category4) {
          setCategory4(mockTrendingDataUI.Category4);
        }

        // Resolve Popular Shows
        if (trendingDataFromAPI.PopularShows) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.PopularShows,
            showsFileConfig
          );
          const apiData = resolvedData as unknown as PopularShowsUI;
          // Merge with mock data
          setPopularShows({
            ShowList: [
              ...apiData.ShowList,
              ...mockTrendingDataUI.PopularShows.ShowList,
            ],
          });
        } else if (mockTrendingDataUI.PopularShows) {
          setPopularShows(mockTrendingDataUI.PopularShows);
        }

        // Resolve Category5
        if (trendingDataFromAPI.Category5) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.Category5,
            categoryFileConfig
          );
          const apiData = resolvedData as unknown as CategoryXUI;
          // Merge with mock data
          setCategory5({
            ...apiData,
            ShowList: [
              ...apiData.ShowList,
              ...mockTrendingDataUI.Category5.ShowList,
            ],
          });
        } else if (mockTrendingDataUI.Category5) {
          setCategory5(mockTrendingDataUI.Category5);
        }

        // Resolve Hot Shows
        if (trendingDataFromAPI.HotShows) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.HotShows,
            showsFileConfig
          );
          const apiData = resolvedData as unknown as HotShowsUI;
          // Merge with mock data
          setHotShows({
            ShowList: [
              ...apiData.ShowList,
              ...mockTrendingDataUI.HotShows.ShowList,
            ],
          });
        } else if (mockTrendingDataUI.HotShows) {
          setHotShows(mockTrendingDataUI.HotShows);
        }

        // Resolve Category6
        if (trendingDataFromAPI.Category6) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.Category6,
            categoryFileConfig
          );
          const apiData = resolvedData as unknown as CategoryXUI;
          // Merge with mock data
          setCategory6({
            ...apiData,
            ShowList: [
              ...apiData.ShowList,
              ...mockTrendingDataUI.Category6.ShowList,
            ],
          });
        } else if (mockTrendingDataUI.Category6) {
          setCategory6(mockTrendingDataUI.Category6);
        }

        // Resolve New Episodes
        if (trendingDataFromAPI.NewEpisodes) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.NewEpisodes,
            episodesFileConfig
          );
          const apiData = resolvedData as unknown as NewEpisodesUI;
          // Merge with mock data
          setNewEpisodes({
            EpisodeList: [
              ...apiData.EpisodeList,
              ...mockTrendingDataUI.NewEpisodes.EpisodeList,
            ],
          });
        } else if (mockTrendingDataUI.NewEpisodes) {
          setNewEpisodes(mockTrendingDataUI.NewEpisodes);
        }

        // Resolve Popular Episodes
        if (trendingDataFromAPI.PopularEpisodes) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.PopularEpisodes,
            episodesFileConfig
          );
          const apiData = resolvedData as unknown as PopularEpisodesUI;
          // Merge with mock data
          setPopularEpisodes({
            EpisodeList: [
              ...apiData.EpisodeList,
              ...mockTrendingDataUI.PopularEpisodes.EpisodeList,
            ],
          });
        } else if (mockTrendingDataUI.PopularEpisodes) {
          setPopularEpisodes(mockTrendingDataUI.PopularEpisodes);
        }

        console.log("✅ All trending sections resolved successfully");
      } catch (error) {
        console.error("❌ Error resolving trending data:", error);
      } finally {
        setIsLoading(false);
      }
    };

    void resolveEachSection();
  }, [trendingDataFromAPI]);

  // FUNCTIONS
  const checkResolvedData = () => {
    console.log("🔍 Resolved Trending Data:");
    console.log("Popular Podcasters:", popularPodcasters);
    console.log("Category1:", category1);
    console.log("Hot Podcasters:", hotPodcasters);
    console.log("Category2:", category2);
    console.log("Popular Channels:", popularChannels);
    console.log("Category3:", category3);
    console.log("Hot Channels:", hotChannels);
    console.log("Category4:", category4);
    console.log("Popular Shows:", popularShows);
    console.log("Category5:", category5);
    console.log("Hot Shows:", hotShows);
    console.log("Category6:", category6);
    console.log("New Episodes:", newEpisodes);
    console.log("Popular Episodes:", popularEpisodes);
  };

  return (
    <div
      className="
      flex flex-col items-center gap-10 mb-20 p-8
    "
    >
      <div className="w-full flex flex-col items-start justify-center mb-10">
        <p className="text-9xl pb-4 font-poppins font-bold text-transparent bg-clip-text bg-gradient-to-r from-[#DBE6F6] to-[#C5796D]">
          Trending
        </p>
        <p className="font-poppins text-white font-bold">
          Stay in tune with the podcast community.
        </p>
        <p className="w-2/3 font-poppins text-[#d9d9d9]">
          Trending highlights everything that’s gaining attention: rising
          podcasters, popular channels, standout shows, and episodes that are
          making waves.
        </p>
        <p className="font-poppins text-[#d9d9d9]">
          <span className="font-bold text-white">Updated constantly</span> —
          just explore and dive into what inspires you.
        </p>
      </div>

      {/* Popular Podcasters */}
      {popularPodcasters && popularPodcasters.PodcasterList.length > 0 && (
        <div className="w-full flex flex-col gap-4 mb-10">
          <div className="w-full flex items-center justify-between">
            <p className="font-poppins font-bold text-3xl text-white">
              <span className="bg-gradient-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                Popular
              </span>{" "}
              Podcasters
            </p>
            <p className="font-poppins font-semibold hover:underline hover:text-mystic-green cursor-pointer text-[#d9d9d9]">
              See all
            </p>
          </div>

          {isLoading ? (
            <Carousel
              opts={{
                align: "start",
                loop: false,
              }}
              className="w-full"
            >
              <CarouselContent>
                {popularPodcasters.PodcasterList.map((podcaster, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/4 lg:basis-1/4"
                  >
                    <div className="p-1 flex flex-col items-center">
                      <Skeleton className="w-full h-full aspect-square rounded-full mb-2" />
                      <p className="text-white font-poppins">
                        {podcaster.FullName}
                      </p>
                      {/* <Skeleton className="w-8/12 h-3 rounded-xs" /> */}
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          ) : (
            <Carousel
              opts={{
                align: "start",
                loop: true,
              }}
              plugins={[
                Autoplay({
                  delay: 3000,
                }),
              ]}
              className="w-full"
            >
              <CarouselContent>
                {popularPodcasters.PodcasterList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/4 lg:basis-1/4"
                  >
                    <PodcasterCard podcaster={card} />
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* Hot Podcasters */}
      {hotPodcasters && hotPodcasters.PodcasterList.length > 0 && (
        <div className="w-full flex flex-col gap-4">
          <div className="w-full flex items-center justify-between">
            <p className="font-poppins font-bold text-3xl text-white">
              <span className="bg-gradient-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                Hot
              </span>{" "}
              Podcasters
            </p>
            <p className="font-poppins font-semibold hover:underline hover:text-mystic-green cursor-pointer text-[#d9d9d9]">
              See all
            </p>
          </div>

          {isLoading ? (
            <Carousel
              opts={{
                align: "start",
                loop: false,
              }}
              className="w-full"
            >
              <CarouselContent>
                {hotPodcasters.PodcasterList.map((podcaster, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/5 lg:basis-1/5"
                  >
                    <div className="p-1 flex flex-col items-center">
                      <Skeleton className="w-full h-full aspect-square rounded-full mb-2" />
                      <p className="text-white font-poppins">
                        {podcaster.FullName}
                      </p>
                      {/* <Skeleton className="w-8/12 h-3 rounded-xs" /> */}
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          ) : (
            <Carousel
              opts={{
                align: "start",
                loop: true,
              }}
              plugins={[
                Autoplay({
                  delay: 3000,
                }),
              ]}
              className="w-full"
            >
              <CarouselContent>
                {hotPodcasters.PodcasterList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/5 lg:basis-1/5"
                  >
                    <PodcasterCard podcaster={card} />
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* Popular Channels */}
      {popularChannels && popularChannels.ChannelList.length > 0 && (
        <div className="w-full flex flex-col gap-4">
          <div className="w-full flex items-center justify-between">
            <p className="font-poppins font-bold text-3xl text-white">
              <span className="bg-gradient-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                Popular
              </span>{" "}
              Channels
            </p>
            <p className="font-poppins font-semibold hover:underline hover:text-mystic-green cursor-pointer text-[#d9d9d9]">
              See all
            </p>
          </div>

          {isLoading ? (
            <Carousel
              opts={{
                align: "start",
                loop: false,
              }}
              className="w-full py-5"
            >
              <CarouselContent>
                {popularChannels.ChannelList.map((channel, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/4 lg:basis-1/4"
                  >
                    <div className="p-3 flex flex-col items-center">
                      <Skeleton className="w-full h-full aspect-square rounded-md mb-2" />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          ) : (
            <Carousel
              opts={{
                align: "start",
                loop: true,
              }}
              plugins={[
                Autoplay({
                  delay: 3000,
                }),
              ]}
              className="w-full py-5"
            >
              <CarouselContent>
                {popularChannels.ChannelList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/4 lg:basis-1/4"
                  >
                    <div className="p-3 flex flex-col items-center">
                      <ChannelCard channel={card} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* Hot Channels */}
      {hotChannels && hotChannels.ChannelList.length > 0 && (
        <div className="w-full flex flex-col gap-4">
          <div className="w-full flex items-center justify-between">
            <p className="font-poppins font-bold text-3xl text-white">
              <span className="bg-gradient-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                Hot
              </span>{" "}
              Channels
            </p>
            <p className="font-poppins font-semibold hover:underline hover:text-mystic-green cursor-pointer text-[#d9d9d9]">
              See all
            </p>
          </div>

          {isLoading ? (
            <Carousel
              opts={{
                align: "start",
                loop: false,
              }}
              className="w-full py-5"
            >
              <CarouselContent>
                {hotChannels.ChannelList.map((channel, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/5 lg:basis-1/5"
                  >
                    <div className="p-3 flex flex-col items-center">
                      <Skeleton className="w-full h-full aspect-square rounded-md" />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          ) : (
            <Carousel
              opts={{
                align: "start",
                loop: true,
              }}
              plugins={[
                Autoplay({
                  delay: 3000,
                }),
              ]}
              className="w-full py-5"
            >
              <CarouselContent>
                {hotChannels.ChannelList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/5 lg:basis-1/5"
                  >
                    <div className="p-3 flex flex-col items-center">
                      <ChannelCard channel={card} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* Popular Shows */}
      {popularShows && popularShows.ShowList.length > 0 && (
        <div className="w-full flex flex-col gap-4">
          <div className="w-full flex items-center justify-between">
            <p className="font-poppins font-bold text-3xl text-white">
              <span className="bg-gradient-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                Popular
              </span>{" "}
              Shows
            </p>
            <p className="font-poppins font-semibold hover:underline hover:text-mystic-green cursor-pointer text-[#d9d9d9]">
              See all
            </p>
          </div>

          {isLoading ? (
            <Carousel
              opts={{
                align: "start",
                loop: false,
              }}
              className="w-full"
            >
              <CarouselContent>
                {popularShows.ShowList.map((show, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/3 lg:basis-1/3"
                  >
                    <div className="p-1 flex flex-col items-center">
                      <Skeleton className="w-full h-full aspect-video rounded-md mb-2" />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          ) : (
            <Carousel
              opts={{
                align: "start",
                loop: true,
              }}
              plugins={[
                Autoplay({
                  delay: 3000,
                }),
              ]}
              className="w-full"
            >
              <CarouselContent>
                {popularShows.ShowList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/3 lg:basis-1/3"
                  >
                    <div className="flex items-center justify-center py-2 px-1">
                      <ShowCard show={card} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* Hot Shows */}
      {hotShows && hotShows.ShowList.length > 0 && (
        <div className="w-full flex flex-col gap-4">
          <div className="w-full flex items-center justify-between">
            <p className="font-poppins font-bold text-3xl text-white">
              <span className="bg-gradient-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                Hot
              </span>{" "}
              Shows
            </p>
            <p className="font-poppins font-semibold hover:underline hover:text-mystic-green cursor-pointer text-[#d9d9d9]">
              See all
            </p>
          </div>

          {isLoading ? (
            <Carousel
              opts={{
                align: "start",
                loop: false,
              }}
              className="w-full"
            >
              <CarouselContent>
                {hotShows.ShowList.map((show, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/3 lg:basis-1/3"
                  >
                    <div className="p-1 flex flex-col items-center">
                      <Skeleton className="w-full h-full aspect-video rounded-md mb-2" />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          ) : (
            <Carousel
              opts={{
                align: "start",
                loop: true,
              }}
              plugins={[
                Autoplay({
                  delay: 3000,
                }),
              ]}
              className="w-full"
            >
              <CarouselContent>
                {hotShows.ShowList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/3 lg:basis-1/3"
                  >
                    <ShowCard show={card} />
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* New Episodes */}
      {newEpisodes && newEpisodes.EpisodeList.length > 0 && (
        <div className="w-full flex flex-col gap-4">
          <div className="w-full flex items-center justify-between">
            <p className="font-poppins font-bold text-3xl text-white">
              <span className="bg-gradient-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                New
              </span>{" "}
              Episodes
            </p>
            <p className="font-poppins font-semibold hover:underline hover:text-mystic-green cursor-pointer text-[#d9d9d9]">
              See all
            </p>
          </div>

          {isLoading ? (
            <Carousel
              opts={{
                align: "start",
                loop: false,
              }}
              className="w-full"
            >
              <CarouselContent>
                {newEpisodes.EpisodeList.map((episode, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/5 lg:basis-1/5"
                  >
                    <div className="p-1 flex flex-col items-center">
                      <Skeleton className="w-full h-full aspect-square rounded-full mb-2" />
                      <p className="text-white font-poppins">{episode.Name}</p>
                      {/* <Skeleton className="w-8/12 h-3 rounded-xs" /> */}
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          ) : (
            <Carousel
              opts={{
                align: "start",
                loop: true,
              }}
              plugins={[
                Autoplay({
                  delay: 3000,
                }),
              ]}
              className="w-full"
            >
              <CarouselContent>
                {newEpisodes.EpisodeList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/5 lg:basis-1/5"
                  >
                    <div className="p-1 flex flex-col items-center">
                      <EpisodeCard episode={card} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* Popular Episodes */}
      {popularEpisodes && popularEpisodes.EpisodeList.length > 0 && (
        <div className="w-full flex flex-col gap-4">
          <div className="w-full flex items-center justify-between">
            <p className="font-poppins font-bold text-3xl text-white">
              <span className="bg-gradient-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                Popular
              </span>{" "}
              Episodes
            </p>
            <p className="font-poppins font-semibold hover:underline hover:text-mystic-green cursor-pointer text-[#d9d9d9]">
              See all
            </p>
          </div>

          {isLoading ? (
            <Carousel
              opts={{
                align: "start",
                loop: false,
              }}
              className="w-full"
            >
              <CarouselContent>
                {popularEpisodes.EpisodeList.map((episode, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/5 lg:basis-1/5"
                  >
                    <div className="p-1 flex flex-col items-center">
                      <Skeleton className="w-full h-full aspect-square rounded-full mb-2" />
                      <p className="text-white font-poppins">{episode.Name}</p>
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          ) : (
            <Carousel
              opts={{
                align: "start",
                loop: true,
              }}
              plugins={[
                Autoplay({
                  delay: 3000,
                }),
              ]}
              className="w-full"
            >
              <CarouselContent>
                {popularEpisodes.EpisodeList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/5 lg:basis-1/5"
                  >
                    <div className="p-1 flex flex-col items-center">
                      <EpisodeCard episode={card} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}
    </div>
  );
};

export default TrendingPage;

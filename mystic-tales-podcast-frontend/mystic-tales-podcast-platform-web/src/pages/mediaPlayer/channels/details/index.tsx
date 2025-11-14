/* eslint-disable @typescript-eslint/no-unused-vars */
// @ts-nocheck

import {
  Carousel,
  CarouselContent,
  CarouselItem,
} from "@/components/ui/carousel";
import type { ChannelDetailsUI } from "@/core/types/channel";
import type { ShowUI } from "@/core/types/show";
import Autoplay from "embla-carousel-autoplay";
import { useEffect, useState } from "react";
import { IoIosArrowBack, IoIosMore } from "react-icons/io";
import { IoPaperPlane } from "react-icons/io5";

import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { Button } from "@/components/ui/button";
import { Check } from "lucide-react";

import { useNavigate, useParams } from "react-router-dom";
import ShowCard from "./components/ShowCard";

const ACCENT = "#aee339";

const formatVND = (n: number) =>
  n.toLocaleString("vi-VN", { maximumFractionDigits: 0 });

const cycleSuffix = (cycleName: string) => {
  const n = (cycleName || "").toLowerCase();
  if (n.includes("month")) return "/month";
  if (n.includes("year") || n.includes("annual")) return "/year";
  return "/cycle";
};

type SubscriptionData = {
  Price: number;
  CycleName: string;
  Benefits: {
    Id: number;
    Name: string;
  }[];
};

interface CurrentSubscription {
  Title: string;
  Description: string;
  Data: SubscriptionData[];
}

type ShowMaps = {
  CategoryId: number;
  Name: string;
  Shows: ShowUI[];
};

const mockChannelDetailsData: ChannelDetailsUI = {
  Channel: {
    Id: "uuid",
    Name: "CÂU CHUYỆN NGOÀI VŨ TRỤ",
    Description:
      "Bạn từng nhìn lên bầu trời và tự hỏi: “Ngoài kia có ai đang nhìn lại mình không? “Câu chuyện về vũ trụ” đưa bạn bay qua những dải ngân hà, xuyên qua thời gian và không gian để lắng nghe những câu chuyện kỳ thú về nguồn gốc, năng lượng và tương lai của vũ trụ — bằng giọng kể sinh động, dễ hiểu và đầy cảm xúc.",
    BackgroundImageUrl: "",
    ImageUrl:
      "https://i.pinimg.com/736x/ca/ea/ae/caeaaedb6b6a54ed402ed3ada4ade457.jpg",
    TotalFavorite: 17456,
    ListenCount: 2001224,
    ShowCount: 8,
    Podcaster: {
      Id: 1,
      FullName: "SAMURICE",
      Email: "samugao@gmail.com",
      ImageUrl:
        "https://scontent.fsgn2-6.fna.fbcdn.net/v/t39.30808-6/542618171_24917648541194226_1925287616611657518_n.jpg?_nc_cat=111&ccb=1-7&_nc_sid=6ee11a&_nc_ohc=pRGBFRolJJUQ7kNvwFC9g0a&_nc_oc=AdlxEOucGKq3u5KN9F7yFLwojTH8Io_-jHjETj3E6MfE1luqv7NgMmZ-JeWJZVsAjI4&_nc_zt=23&_nc_ht=scontent.fsgn2-6.fna&_nc_gid=vq2j8yROhnMpJeqWL-gIPw&oh=00_Afi45ydaDlGDqq8xNjGWxky18gT_mjGou47tXwC448h1LQ&oe=690CC7E3",
    },
    PodcastCategory: {
      Id: 1,
      Name: "Science",
    },
    PodcastSubCategory: {
      Id: 2,
      Name: "Space",
      PodcastCategoryId: 1,
    },
    Hashtags: [
      {
        Id: 1,
        Name: "#Ben_Ngoai_Vu_Tru",
      },
      {
        Id: 2,
        Name: "#Thien_Ha",
      },
      {
        Id: 3,
        Name: "#Space_Knowledge",
      },
      {
        Id: 4,
        Name: "#Alien",
      },
    ],
    CreatedAt: "2023-11-18T05:21:15.276Z",
    UpdatedAt: "2025-11-02T05:21:15.276Z",
    CurrentStatus: {
      Id: 1,
      Name: "Publish",
    },
    PodcastSubscriptionList: [
      {
        Id: 1,
        Name: "Gói Phi Hành Gia 1",
        Description:
          "Đăng ký ngay để lên chuyến tàu khám phá các câu chuyện ngoài vũ trụ cùng SAMURICE nhé!",
        PodcastChannelId: "uuid",
        PodcastShowId: null,
        IsActive: false,
        CurrentVersion: 1,
        DeletedAt: null,
        CreatedAt: "2023-11-19T05:21:15.276Z",
        UpdatedAt: "2023-12-19T05:21:15.276Z",
        PodcastSubscriptionCycleTypePriceList: [
          {
            PodcastSubscriptionId: 1,
            SubscriptionCycleType: {
              Id: 1,
              Name: "Daily",
            },
            Version: 1,
            Price: 10000,
            CreatedAt: "2023-11-19T05:21:15.276Z",
            UpdatedAt: "2023-12-19T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 1,
            SubscriptionCycleType: {
              Id: 1,
              Name: "Monthly",
            },
            Version: 1,
            Price: 250000,
            CreatedAt: "2023-11-19T05:21:15.276Z",
            UpdatedAt: "2023-12-19T05:21:15.276Z",
          },
        ],
        PodcastSubscriptionBenefitMappingList: [
          {
            PodcastSubscriptionId: 1,
            PodcastSubscriptionBenefit: {
              Id: 1,
              Name: "Unlimited Listen Count",
            },
            Version: 1,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 1,
            PodcastSubscriptionBenefit: {
              Id: 2,
              Name: "Show for Subscriber-Only",
            },
            Version: 1,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 1,
            PodcastSubscriptionBenefit: {
              Id: 3,
              Name: "Episode for Subscriber-Only",
            },
            Version: 1,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 1,
            PodcastSubscriptionBenefit: {
              Id: 4,
              Name: "Early Access",
            },
            Version: 1,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 1,
            PodcastSubscriptionBenefit: {
              Id: 5,
              Name: "Archive Access",
            },
            Version: 1,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 1,
            PodcastSubscriptionBenefit: {
              Id: 6,
              Name: "Bonus Episodes",
            },
            Version: 1,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            PodcastSubscriptionBenefit: {
              Id: 1,
              Name: "Unlimited Listen Count",
            },
            Version: 1,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            PodcastSubscriptionBenefit: {
              Id: 2,
              Name: "Show for Subscriber-Only",
            },
            Version: 1,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            PodcastSubscriptionBenefit: {
              Id: 3,
              Name: "Episode for Subscriber-Only",
            },
            Version: 1,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            PodcastSubscriptionBenefit: {
              Id: 4,
              Name: "Early Access",
            },
            Version: 1,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            PodcastSubscriptionBenefit: {
              Id: 5,
              Name: "Archive Access",
            },
            Version: 1,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            PodcastSubscriptionBenefit: {
              Id: 1,
              Name: "Unlimited Listen Count",
            },
            Version: 2,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            PodcastSubscriptionBenefit: {
              Id: 2,
              Name: "Show for Subscriber-Only",
            },
            Version: 2,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            PodcastSubscriptionBenefit: {
              Id: 3,
              Name: "Episode for Subscriber-Only",
            },
            Version: 2,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            PodcastSubscriptionBenefit: {
              Id: 4,
              Name: "Early Access",
            },
            Version: 2,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            PodcastSubscriptionBenefit: {
              Id: 5,
              Name: "Archive Access",
            },
            Version: 2,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            PodcastSubscriptionBenefit: {
              Id: 6,
              Name: "Bonus Episodes",
            },
            Version: 2,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
        ],
      },
      {
        Id: 2,
        Name: "Trở thành nhà thám hiểm",
        Description:
          "Đăng ký ngay để lên chuyến tàu khám phá các câu chuyện ngoài vũ trụ cùng SAMURICE nhé!",
        PodcastChannelId: "uuid",
        PodcastShowId: null,
        IsActive: true,
        CurrentVersion: 2,
        DeletedAt: null,
        CreatedAt: "2024-05-20T05:21:15.276Z",
        UpdatedAt: "2024-12-20T05:21:15.276Z",
        PodcastSubscriptionCycleTypePriceList: [
          {
            PodcastSubscriptionId: 1,
            SubscriptionCycleType: {
              Id: 1,
              Name: "Daily",
            },
            Version: 1,
            Price: 10000,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 1,
            SubscriptionCycleType: {
              Id: 1,
              Name: "Monthly",
            },
            Version: 1,
            Price: 250000,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            SubscriptionCycleType: {
              Id: 1,
              Name: "Daily",
            },
            Version: 2,
            Price: 12000,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            SubscriptionCycleType: {
              Id: 2,
              Name: "Monthly",
            },
            Version: 2,
            Price: 260000,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            SubscriptionCycleType: {
              Id: 3,
              Name: "Yearly",
            },
            Version: 2,
            Price: 1000000,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
        ],
        PodcastSubscriptionBenefitMappingList: [
          {
            PodcastSubscriptionId: 1,
            PodcastSubscriptionBenefit: {
              Id: 1,
              Name: "Unlimited Listen Count",
            },
            Version: 1,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 1,
            PodcastSubscriptionBenefit: {
              Id: 2,
              Name: "Show for Subscriber-Only",
            },
            Version: 1,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 1,
            PodcastSubscriptionBenefit: {
              Id: 3,
              Name: "Episode for Subscriber-Only",
            },
            Version: 1,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 1,
            PodcastSubscriptionBenefit: {
              Id: 4,
              Name: "Early Access",
            },
            Version: 1,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 1,
            PodcastSubscriptionBenefit: {
              Id: 5,
              Name: "Archive Access",
            },
            Version: 1,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 1,
            PodcastSubscriptionBenefit: {
              Id: 6,
              Name: "Bonus Episodes",
            },
            Version: 1,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            PodcastSubscriptionBenefit: {
              Id: 1,
              Name: "Unlimited Listen Count",
            },
            Version: 1,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            PodcastSubscriptionBenefit: {
              Id: 2,
              Name: "Show for Subscriber-Only",
            },
            Version: 1,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            PodcastSubscriptionBenefit: {
              Id: 3,
              Name: "Episode for Subscriber-Only",
            },
            Version: 1,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            PodcastSubscriptionBenefit: {
              Id: 4,
              Name: "Early Access",
            },
            Version: 1,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            PodcastSubscriptionBenefit: {
              Id: 5,
              Name: "Archive Access",
            },
            Version: 1,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            PodcastSubscriptionBenefit: {
              Id: 1,
              Name: "Unlimited Listen Count",
            },
            Version: 2,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            PodcastSubscriptionBenefit: {
              Id: 2,
              Name: "Show for Subscriber-Only",
            },
            Version: 2,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            PodcastSubscriptionBenefit: {
              Id: 3,
              Name: "Episode for Subscriber-Only",
            },
            Version: 2,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            PodcastSubscriptionBenefit: {
              Id: 4,
              Name: "Early Access",
            },
            Version: 2,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            PodcastSubscriptionBenefit: {
              Id: 5,
              Name: "Archive Access",
            },
            Version: 2,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
          {
            PodcastSubscriptionId: 2,
            PodcastSubscriptionBenefit: {
              Id: 6,
              Name: "Bonus Episodes",
            },
            Version: 2,
            CreatedAt: "2024-05-20T05:21:15.276Z",
            UpdatedAt: "2024-12-20T05:21:15.276Z",
          },
        ],
      },
    ],
    ShowList: [
      {
        Id: "uuid-show-1",
        Name: "CÂU CHUYỆN NGOÀI VŨ TRỤ SEASON 1 | SAMURICE",
        Description: "",
        Language: "Vietnamese",
        ReleaseDate: "2023-11-18T05:21:15.276Z",
        IsReleased: true,
        Copyright: "© 036563",
        UploadFrequency: "2 times/week",
        RatingCount: 15000,
        AverageRating: 4.9,
        ImageUrl:
          "https://i.pinimg.com/736x/32/07/45/320745d19b18177bacb279a6e7ff7c30.jpg",
        TrailerAudioFileKey: "",
        TotalFollow: 1500,
        ListenCount: 2400,
        EpisodeCount: 62,
        Podcaster: {
          Id: 1,
          FullName: "SAMURICE",
          Email: "samugao@gmail.com",
          ImageUrl:
            "https://scontent.fsgn2-6.fna.fbcdn.net/v/t39.30808-6/542618171_24917648541194226_1925287616611657518_n.jpg?_nc_cat=111&ccb=1-7&_nc_sid=6ee11a&_nc_ohc=pRGBFRolJJUQ7kNvwFC9g0a&_nc_oc=AdlxEOucGKq3u5KN9F7yFLwojTH8Io_-jHjETj3E6MfE1luqv7NgMmZ-JeWJZVsAjI4&_nc_zt=23&_nc_ht=scontent.fsgn2-6.fna&_nc_gid=vq2j8yROhnMpJeqWL-gIPw&oh=00_Afi45ydaDlGDqq8xNjGWxky18gT_mjGou47tXwC448h1LQ&oe=690CC7E3",
        },
        PodcastCategory: {
          Id: 1,
          Name: "Science",
        },
        PodcastSubCategory: {
          Id: 1,
          Name: "Space",
          PodcastCategoryId: 1,
        },
        PodcastShowSubscriptionType: {
          Id: 2,
          Name: "Subscriber-Only",
        },
        PodcastChannel: {
          Id: "",
          Name: "",
          ImageUrl: "",
        },
        Hashtags: [
          {
            Id: 0,
            Name: "",
          },
        ],
        TakenDownReason: "",
        CreatedAt: "",
        UpdatedAt: "",
        CurrentStatus: {
          Id: 3,
          Name: "Published",
        },
      },
      {
        Id: "uuid-show-2",
        Name: "CÂU CHUYỆN NGOÀI VŨ TRỤ SEASON 2 | SAMURICE",
        Description: "",
        Language: "Vietnamese",
        ReleaseDate: "2023-12-18T05:21:15.276Z",
        IsReleased: true,
        Copyright: "© 036563",
        UploadFrequency: "2 times/week",
        RatingCount: 15000,
        AverageRating: 4.9,
        ImageUrl:
          "https://i.pinimg.com/1200x/d4/aa/b8/d4aab8e755be5dcd25da197f97d5d624.jpg",
        TrailerAudioFileKey: "",
        TotalFollow: 1500,
        ListenCount: 2400,
        EpisodeCount: 62,
        Podcaster: {
          Id: 1,
          FullName: "SAMURICE",
          Email: "samugao@gmail.com",
          ImageUrl:
            "https://scontent.fsgn2-6.fna.fbcdn.net/v/t39.30808-6/542618171_24917648541194226_1925287616611657518_n.jpg?_nc_cat=111&ccb=1-7&_nc_sid=6ee11a&_nc_ohc=pRGBFRolJJUQ7kNvwFC9g0a&_nc_oc=AdlxEOucGKq3u5KN9F7yFLwojTH8Io_-jHjETj3E6MfE1luqv7NgMmZ-JeWJZVsAjI4&_nc_zt=23&_nc_ht=scontent.fsgn2-6.fna&_nc_gid=vq2j8yROhnMpJeqWL-gIPw&oh=00_Afi45ydaDlGDqq8xNjGWxky18gT_mjGou47tXwC448h1LQ&oe=690CC7E3",
        },
        PodcastCategory: {
          Id: 1,
          Name: "Science",
        },
        PodcastSubCategory: {
          Id: 1,
          Name: "Space",
          PodcastCategoryId: 1,
        },
        PodcastShowSubscriptionType: {
          Id: 2,
          Name: "Subscriber-Only",
        },
        PodcastChannel: {
          Id: "",
          Name: "",
          ImageUrl: "",
        },
        Hashtags: [
          {
            Id: 0,
            Name: "",
          },
        ],
        TakenDownReason: "",
        CreatedAt: "",
        UpdatedAt: "",
        CurrentStatus: {
          Id: 3,
          Name: "Published",
        },
      },
      {
        Id: "uuid-show-3",
        Name: "CÂU CHUYỆN NGOÀI VŨ TRỤ SEASON 3 | SAMURICE",
        Description: "",
        Language: "Vietnamese",
        ReleaseDate: "2024-01-18T05:21:15.276Z",
        IsReleased: true,
        Copyright: "© 036563",
        UploadFrequency: "2 times/week",
        RatingCount: 15000,
        AverageRating: 4.9,
        ImageUrl:
          "https://i.pinimg.com/736x/3f/1c/fe/3f1cfe72ff35d1519075fc75897ff3bc.jpg",
        TrailerAudioFileKey: "",
        TotalFollow: 1500,
        ListenCount: 2400,
        EpisodeCount: 62,
        Podcaster: {
          Id: 1,
          FullName: "SAMURICE",
          Email: "samugao@gmail.com",
          ImageUrl:
            "https://scontent.fsgn2-6.fna.fbcdn.net/v/t39.30808-6/542618171_24917648541194226_1925287616611657518_n.jpg?_nc_cat=111&ccb=1-7&_nc_sid=6ee11a&_nc_ohc=pRGBFRolJJUQ7kNvwFC9g0a&_nc_oc=AdlxEOucGKq3u5KN9F7yFLwojTH8Io_-jHjETj3E6MfE1luqv7NgMmZ-JeWJZVsAjI4&_nc_zt=23&_nc_ht=scontent.fsgn2-6.fna&_nc_gid=vq2j8yROhnMpJeqWL-gIPw&oh=00_Afi45ydaDlGDqq8xNjGWxky18gT_mjGou47tXwC448h1LQ&oe=690CC7E3",
        },
        PodcastCategory: {
          Id: 1,
          Name: "Science",
        },
        PodcastSubCategory: {
          Id: 1,
          Name: "Space",
          PodcastCategoryId: 1,
        },
        PodcastShowSubscriptionType: {
          Id: 2,
          Name: "Subscriber-Only",
        },
        PodcastChannel: {
          Id: "",
          Name: "",
          ImageUrl: "",
        },
        Hashtags: [
          {
            Id: 0,
            Name: "",
          },
        ],
        TakenDownReason: "",
        CreatedAt: "",
        UpdatedAt: "",
        CurrentStatus: {
          Id: 3,
          Name: "Published",
        },
      },
      {
        Id: "uuid-show-2.5",
        Name: "CÂU CHUYỆN NGOÀI VŨ TRỤ SEASON 2.5 | SAMURICE",
        Description: "",
        Language: "Vietnamese",
        ReleaseDate: "2023-11-18T05:21:15.276Z",
        IsReleased: true,
        Copyright: "© 036563",
        UploadFrequency: "2 times/week",
        RatingCount: 15000,
        AverageRating: 4.9,
        ImageUrl:
          "https://i.pinimg.com/736x/32/07/45/320745d19b18177bacb279a6e7ff7c30.jpg",
        TrailerAudioFileKey: "",
        TotalFollow: 1500,
        ListenCount: 2400,
        EpisodeCount: 62,
        Podcaster: {
          Id: 1,
          FullName: "SAMURICE",
          Email: "samugao@gmail.com",
          ImageUrl:
            "https://scontent.fsgn2-6.fna.fbcdn.net/v/t39.30808-6/542618171_24917648541194226_1925287616611657518_n.jpg?_nc_cat=111&ccb=1-7&_nc_sid=6ee11a&_nc_ohc=pRGBFRolJJUQ7kNvwFC9g0a&_nc_oc=AdlxEOucGKq3u5KN9F7yFLwojTH8Io_-jHjETj3E6MfE1luqv7NgMmZ-JeWJZVsAjI4&_nc_zt=23&_nc_ht=scontent.fsgn2-6.fna&_nc_gid=vq2j8yROhnMpJeqWL-gIPw&oh=00_Afi45ydaDlGDqq8xNjGWxky18gT_mjGou47tXwC448h1LQ&oe=690CC7E3",
        },
        PodcastCategory: {
          Id: 1,
          Name: "Science",
        },
        PodcastSubCategory: {
          Id: 1,
          Name: "Space",
          PodcastCategoryId: 1,
        },
        PodcastShowSubscriptionType: {
          Id: 2,
          Name: "Subscriber-Only",
        },
        PodcastChannel: {
          Id: "",
          Name: "",
          ImageUrl: "",
        },
        Hashtags: [
          {
            Id: 0,
            Name: "",
          },
        ],
        TakenDownReason: "",
        CreatedAt: "",
        UpdatedAt: "",
        CurrentStatus: {
          Id: 2,
          Name: "Ready To Release",
        },
      },
      {
        Id: "uuid-show-4",
        Name: "CÂU CHUYỆN NGOÀI VŨ TRỤ SEASON 4 | SAMURICE",
        Description: "",
        Language: "Vietnamese",
        ReleaseDate: "2024-02-18T05:21:15.276Z",
        IsReleased: true,
        Copyright: "© 036563",
        UploadFrequency: "2 times/week",
        RatingCount: 15000,
        AverageRating: 4.9,
        ImageUrl:
          "https://i.pinimg.com/736x/b4/8b/88/b48b88ae6c1d82ed53aad07d6d07fcef.jpg",
        TrailerAudioFileKey: "",
        TotalFollow: 1500,
        ListenCount: 2400,
        EpisodeCount: 62,
        Podcaster: {
          Id: 1,
          FullName: "SAMURICE",
          Email: "samugao@gmail.com",
          ImageUrl:
            "https://scontent.fsgn2-6.fna.fbcdn.net/v/t39.30808-6/542618171_24917648541194226_1925287616611657518_n.jpg?_nc_cat=111&ccb=1-7&_nc_sid=6ee11a&_nc_ohc=pRGBFRolJJUQ7kNvwFC9g0a&_nc_oc=AdlxEOucGKq3u5KN9F7yFLwojTH8Io_-jHjETj3E6MfE1luqv7NgMmZ-JeWJZVsAjI4&_nc_zt=23&_nc_ht=scontent.fsgn2-6.fna&_nc_gid=vq2j8yROhnMpJeqWL-gIPw&oh=00_Afi45ydaDlGDqq8xNjGWxky18gT_mjGou47tXwC448h1LQ&oe=690CC7E3",
        },
        PodcastCategory: {
          Id: 1,
          Name: "Science",
        },
        PodcastSubCategory: {
          Id: 1,
          Name: "Space",
          PodcastCategoryId: 1,
        },
        PodcastShowSubscriptionType: {
          Id: 2,
          Name: "Subscriber-Only",
        },
        PodcastChannel: {
          Id: "",
          Name: "",
          ImageUrl: "",
        },
        Hashtags: [
          {
            Id: 0,
            Name: "",
          },
        ],
        TakenDownReason: "",
        CreatedAt: "",
        UpdatedAt: "",
        CurrentStatus: {
          Id: 3,
          Name: "Published",
        },
      },
      {
        Id: "uuid-show-5",
        Name: "CÂU CHUYỆN NGOÀI VŨ TRỤ SEASON 5 | SAMURICE",
        Description: "",
        Language: "Vietnamese",
        ReleaseDate: "2024-04-18T05:21:15.276Z",
        IsReleased: true,
        Copyright: "© 036563",
        UploadFrequency: "2 times/week",
        RatingCount: 15000,
        AverageRating: 4.9,
        ImageUrl:
          "https://i.pinimg.com/736x/aa/45/d2/aa45d2009244b72ced03a261f5202207.jpg",
        TrailerAudioFileKey: "",
        TotalFollow: 1500,
        ListenCount: 2400,
        EpisodeCount: 62,
        Podcaster: {
          Id: 1,
          FullName: "SAMURICE",
          Email: "samugao@gmail.com",
          ImageUrl:
            "https://scontent.fsgn2-6.fna.fbcdn.net/v/t39.30808-6/542618171_24917648541194226_1925287616611657518_n.jpg?_nc_cat=111&ccb=1-7&_nc_sid=6ee11a&_nc_ohc=pRGBFRolJJUQ7kNvwFC9g0a&_nc_oc=AdlxEOucGKq3u5KN9F7yFLwojTH8Io_-jHjETj3E6MfE1luqv7NgMmZ-JeWJZVsAjI4&_nc_zt=23&_nc_ht=scontent.fsgn2-6.fna&_nc_gid=vq2j8yROhnMpJeqWL-gIPw&oh=00_Afi45ydaDlGDqq8xNjGWxky18gT_mjGou47tXwC448h1LQ&oe=690CC7E3",
        },
        PodcastCategory: {
          Id: 1,
          Name: "Science",
        },
        PodcastSubCategory: {
          Id: 1,
          Name: "Space",
          PodcastCategoryId: 1,
        },
        PodcastShowSubscriptionType: {
          Id: 2,
          Name: "Subscriber-Only",
        },
        PodcastChannel: {
          Id: "",
          Name: "",
          ImageUrl: "",
        },
        Hashtags: [
          {
            Id: 0,
            Name: "",
          },
        ],
        TakenDownReason: "",
        CreatedAt: "",
        UpdatedAt: "",
        CurrentStatus: {
          Id: 3,
          Name: "Published",
        },
      },
      {
        Id: "uuid-show-6",
        Name: "CÂU CHUYỆN NGOÀI VŨ TRỤ SEASON 6 | SAMURICE",
        Description: "",
        Language: "Vietnamese",
        ReleaseDate: "2024-05-18T05:21:15.276Z",
        IsReleased: true,
        Copyright: "© 036563",
        UploadFrequency: "2 times/week",
        RatingCount: 15000,
        AverageRating: 4.9,
        ImageUrl:
          "https://i.pinimg.com/736x/5b/a4/7c/5ba47c37f30be28690432cc79a9222e5.jpg",
        TrailerAudioFileKey: "",
        TotalFollow: 1500,
        ListenCount: 2400,
        EpisodeCount: 62,
        Podcaster: {
          Id: 1,
          FullName: "SAMURICE",
          Email: "samugao@gmail.com",
          ImageUrl:
            "https://scontent.fsgn2-6.fna.fbcdn.net/v/t39.30808-6/542618171_24917648541194226_1925287616611657518_n.jpg?_nc_cat=111&ccb=1-7&_nc_sid=6ee11a&_nc_ohc=pRGBFRolJJUQ7kNvwFC9g0a&_nc_oc=AdlxEOucGKq3u5KN9F7yFLwojTH8Io_-jHjETj3E6MfE1luqv7NgMmZ-JeWJZVsAjI4&_nc_zt=23&_nc_ht=scontent.fsgn2-6.fna&_nc_gid=vq2j8yROhnMpJeqWL-gIPw&oh=00_Afi45ydaDlGDqq8xNjGWxky18gT_mjGou47tXwC448h1LQ&oe=690CC7E3",
        },
        PodcastCategory: {
          Id: 1,
          Name: "Science",
        },
        PodcastSubCategory: {
          Id: 1,
          Name: "Space",
          PodcastCategoryId: 1,
        },
        PodcastShowSubscriptionType: {
          Id: 2,
          Name: "Subscriber-Only",
        },
        PodcastChannel: {
          Id: "",
          Name: "",
          ImageUrl: "",
        },
        Hashtags: [
          {
            Id: 0,
            Name: "",
          },
        ],
        TakenDownReason: "",
        CreatedAt: "",
        UpdatedAt: "",
        CurrentStatus: {
          Id: 3,
          Name: "Published",
        },
      },
      {
        Id: "uuid-show-7",
        Name: "CÂU CHUYỆN NGOÀI VŨ TRỤ SEASON 7 | SAMURICE",
        Description: "",
        Language: "Vietnamese",
        ReleaseDate: "2024-06-18T05:21:15.276Z",
        IsReleased: true,
        Copyright: "© 036563",
        UploadFrequency: "2 times/week",
        RatingCount: 15000,
        AverageRating: 4.9,
        ImageUrl:
          "https://i.pinimg.com/736x/5e/98/2a/5e982acca78006fc8d8af9be5996fb69.jpg",
        TrailerAudioFileKey: "",
        TotalFollow: 1500,
        ListenCount: 2400,
        EpisodeCount: 62,
        Podcaster: {
          Id: 1,
          FullName: "SAMURICE",
          Email: "samugao@gmail.com",
          ImageUrl:
            "https://scontent.fsgn2-6.fna.fbcdn.net/v/t39.30808-6/542618171_24917648541194226_1925287616611657518_n.jpg?_nc_cat=111&ccb=1-7&_nc_sid=6ee11a&_nc_ohc=pRGBFRolJJUQ7kNvwFC9g0a&_nc_oc=AdlxEOucGKq3u5KN9F7yFLwojTH8Io_-jHjETj3E6MfE1luqv7NgMmZ-JeWJZVsAjI4&_nc_zt=23&_nc_ht=scontent.fsgn2-6.fna&_nc_gid=vq2j8yROhnMpJeqWL-gIPw&oh=00_Afi45ydaDlGDqq8xNjGWxky18gT_mjGou47tXwC448h1LQ&oe=690CC7E3",
        },
        PodcastCategory: {
          Id: 1,
          Name: "Science",
        },
        PodcastSubCategory: {
          Id: 1,
          Name: "Space",
          PodcastCategoryId: 1,
        },
        PodcastShowSubscriptionType: {
          Id: 2,
          Name: "Subscriber-Only",
        },
        PodcastChannel: {
          Id: "",
          Name: "",
          ImageUrl: "",
        },
        Hashtags: [
          {
            Id: 0,
            Name: "",
          },
        ],
        TakenDownReason: "",
        CreatedAt: "",
        UpdatedAt: "",
        CurrentStatus: {
          Id: 3,
          Name: "Published",
        },
      },
      {
        Id: "uuid-show-8",
        Name: "CÂU CHUYỆN NGOÀI VŨ TRỤ SEASON 8 | SAMURICE",
        Description: "",
        Language: "Vietnamese",
        ReleaseDate: "2024-07-18T05:21:15.276Z",
        IsReleased: true,
        Copyright: "© 036563",
        UploadFrequency: "2 times/week",
        RatingCount: 15000,
        AverageRating: 4.9,
        ImageUrl:
          "https://i.pinimg.com/1200x/79/f7/2e/79f72efb8ac49baeff8014c9070631c0.jpg",
        TrailerAudioFileKey: "",
        TotalFollow: 1500,
        ListenCount: 2400,
        EpisodeCount: 62,
        Podcaster: {
          Id: 1,
          FullName: "SAMURICE",
          Email: "samugao@gmail.com",
          ImageUrl:
            "https://scontent.fsgn2-6.fna.fbcdn.net/v/t39.30808-6/542618171_24917648541194226_1925287616611657518_n.jpg?_nc_cat=111&ccb=1-7&_nc_sid=6ee11a&_nc_ohc=pRGBFRolJJUQ7kNvwFC9g0a&_nc_oc=AdlxEOucGKq3u5KN9F7yFLwojTH8Io_-jHjETj3E6MfE1luqv7NgMmZ-JeWJZVsAjI4&_nc_zt=23&_nc_ht=scontent.fsgn2-6.fna&_nc_gid=vq2j8yROhnMpJeqWL-gIPw&oh=00_Afi45ydaDlGDqq8xNjGWxky18gT_mjGou47tXwC448h1LQ&oe=690CC7E3",
        },
        PodcastCategory: {
          Id: 1,
          Name: "Science",
        },
        PodcastSubCategory: {
          Id: 1,
          Name: "Space",
          PodcastCategoryId: 1,
        },
        PodcastShowSubscriptionType: {
          Id: 2,
          Name: "Subscriber-Only",
        },
        PodcastChannel: {
          Id: "",
          Name: "",
          ImageUrl: "",
        },
        Hashtags: [
          {
            Id: 0,
            Name: "",
          },
        ],
        TakenDownReason: "",
        CreatedAt: "",
        UpdatedAt: "",
        CurrentStatus: {
          Id: 3,
          Name: "Published",
        },
      },
    ],
  },
};

const RenderSubscriptionSection = ({
  subscription,
  onOpen,
}: {
  subscription: CurrentSubscription;
  onOpen: () => void;
}) => {
  const renderPriceOptions = (data?: SubscriptionData[]) => {
    const list = data ?? subscription?.Data ?? [];
    if (!list.length) return "";
    const cheapest = list.reduce(
      (min, cur) => (cur.Price < min.Price ? cur : min),
      list[0]
    );
    return `Only from ${formatVND(cheapest.Price)}đ for ${cheapest.CycleName}`;
  };

  return (
    <div
      className="z-20 w-[300px] absolute right-5 bottom-5 flex flex-col gap-2 p-3 rounded-xl
                 bg-[rgba(174,227,57,0.85)] text-black shadow-[15px_15px_20px_#0000008c]
                 backdrop-blur-md"
    >
      <div className="flex items-center justify-between w-full">
        <p className="text-xs font-bold line-clamp-1">
          {subscription.Title.toUpperCase()}
        </p>
        <p className="text-[9px] text-black/70 font-bold">Subscription</p>
      </div>
      <p className="text-[12px] font-semibold line-clamp-1">
        {subscription.Description}
      </p>
      <div className="text-[10px] italic w-full flex items-center justify-between mt-3">
        <p>{renderPriceOptions(subscription.Data)}</p>
        <button
          onClick={onOpen}
          className="group w-8 h-8 p-1 bg-white text-black flex items-center justify-center
                     rounded-full shadow-[5px_5px_20px_#0000008c] hover:scale-105 transition"
          aria-label="Open subscription dialog"
        >
          <IoPaperPlane
            className="group-hover:-rotate-90 transition-all duration-700 ease-out"
            size={15}
          />
        </button>
      </div>
    </div>
  );
};

const ChannelDetailsPage = () => {
  const { id } = useParams();

  // STATES
  const [isLoading, setIsLoading] = useState(true);
  const [isNotFound, setIsNotFound] = useState(false);
  const [channel, setChannel] = useState<ChannelDetailsUI | null>(null);
  const [shows, setShows] = useState<ShowMaps[]>([]);
  const [isSubscriptionDialogOpen, setIsSubscriptionDialogOpen] =
    useState(false);

  const [currentSubscription, setCurrentSubscription] =
    useState<CurrentSubscription | null>(null);

  // HOOKS
  const navigate = useNavigate();

  useEffect(() => {
    if (!id) {
      navigate("media-player/channels");
    } else {
      // giả lập loading
      setTimeout(() => {
        fetchChannel(id);
      }, 3000);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // FUNCTIONS
  const fetchChannel = (channelId: string) => {
    setIsLoading(true);

    const data = mockChannelDetailsData;

    if (!data || !data.Channel) {
      setIsNotFound(true);
      setIsLoading(false);
      return;
    }

    // ---- 1) Tìm subscription đang active ----
    const subs = data.Channel.PodcastSubscriptionList ?? [];
    const activeSub = subs.find((s) => s.IsActive === true) || null;

    let mappedCurrentSubscription: CurrentSubscription | null = null;

    if (activeSub) {
      const versionToUse = activeSub.CurrentVersion;
      // ---- 2) Lọc price & benefit theo SubscriptionId + Version ----
      const prices = activeSub.PodcastSubscriptionCycleTypePriceList.filter(
        (p) =>
          p.PodcastSubscriptionId === activeSub.Id && p.Version === versionToUse
      );

      const benefits =
        (activeSub.PodcastSubscriptionBenefitMappingList ?? [])
          .filter(
            (bm) =>
              bm.PodcastSubscriptionId === activeSub.Id &&
              bm.Version === versionToUse
          )
          .map((bm) => bm.PodcastSubscriptionBenefit) ?? [];

      if (prices.length > 0) {
        const priceData: SubscriptionData[] = prices.map((p) => ({
          Price: p.Price,
          CycleName: p.SubscriptionCycleType?.Name ?? "",
          Benefits: benefits,
        }));

        mappedCurrentSubscription = {
          Title: activeSub.Name,
          Description: activeSub.Description,
          Data: priceData,
        };
      }
    } else {
      // Không có active subscription → null
      console.log("Is Not Subscription");
    }

    // ---- 3) Lọc shows published và gom theo category ----
    const publishedShows: ShowUI[] = (data.Channel.ShowList ?? []).filter(
      (show) => show?.CurrentStatus?.Id === 3
    );

    // group shows by PodcastCategory.Id
    const groupsMap = new Map<number, ShowMaps>();
    for (const s of publishedShows) {
      const cat = s?.PodcastCategory;
      if (!cat) continue;
      const catId = Number(cat.Id) || 0;
      const existing = groupsMap.get(catId);
      if (existing) {
        existing.Shows.push(s);
      } else {
        groupsMap.set(catId, {
          CategoryId: catId,
          Name: cat.Name ?? "",
          Shows: [s],
        });
      }
    }

    const groupedShows: ShowMaps[] = Array.from(groupsMap.values());

    // ---- 4) Set state & log ----
    setChannel(data);
    setCurrentSubscription(mappedCurrentSubscription);
    setShows(groupedShows);

    console.log("[Channel]", data);
    console.log("[Current Subscription]", mappedCurrentSubscription);
    console.log("[Grouped Published Shows]", groupedShows);

    setIsLoading(false);
  };

  if (isLoading) {
    return (
      <div className="flex-1 flex items-center justify-center">
        <p>Loading ...</p>
      </div>
    );
  }

  if (isNotFound) {
    return (
      <div className="flex-1 flex items-center justify-center">
        <p>Not Found :(</p>
      </div>
    );
  }

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
      <div className="w-full h-[400px] flex flex-col items-center justify-center px-10 relative overflow-hidden">
        {/* blurred background image (covers full area) */}
        <img
          src={channel?.Channel.ImageUrl}
          alt={channel?.Channel.Name}
          className="absolute inset-0 w-full h-full object-cover filter blur-xl scale-110 opacity-80"
          style={{
            // ensure it fills and the blur is strong; scale a bit to avoid edges showing
            transformOrigin: "center",
          }}
        />

        <div className="w-full h-full flex-1 flex flex-col items-center justify-center gap-2 relative z-10">
          <img
            src={channel?.Channel.ImageUrl}
            alt={channel?.Channel.Name}
            className="aspect-square w-[175px] object-cover rounded-md shadow-[10px_10px_20px_#0000008c]"
          />

          <p className="text-white text-2xl font-bold mt-5">
            {channel?.Channel.Name.toUpperCase()}
          </p>
          <div className="text-[#ababab] text-sm font-md w-1/3 overflow-ellipsis line-clamp-3 text-justify">
            <p>{channel?.Channel.Description}</p>
          </div>

          <div className="text-xs flex items-center justify-center gap-2 text-[#d9d9d9] font-semibold overflow-ellipsis line-clamp-1">
            <p className="hover:text-mystic-green hover:underline cursor-pointer">
              {channel?.Channel.PodcastCategory.Name.toUpperCase()}
            </p>{" "}
            •{" "}
            <p className="hover:text-mystic-green hover:underline cursor-pointer">
              {channel?.Channel.PodcastSubCategory.Name.toUpperCase()}
            </p>{" "}
            • <p>{channel?.Channel.ShowCount.toLocaleString()} shows</p>
          </div>
        </div>

        {currentSubscription && (
          <RenderSubscriptionSection
            subscription={currentSubscription}
            onOpen={() => setIsSubscriptionDialogOpen(true)}
          />
        )}
      </div>
      <div className="w-full p-8 flex flex-col">
        {shows.map((group, index) => (
          <div className="w-full flex flex-col">
            <div className="w-full flex items-center justify-between">
              <p className="font-bold text-3xl text-white">
                <span className="text-mystic-green">{group.Name}</span> Shows
              </p>

              <p className="font-poppins font-semibold cursor-pointer text-white hover:underline hover:text-mystic-green text-sm">
                See all ({group.Shows.length})
              </p>
            </div>
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
                  {group.Shows.map((show) => (
                    <CarouselItem
                      key={show.Id}
                      className="basis-1/2 md:basis-1/3 lg:basis-1/5"
                    >
                      <div
                        onClick={() =>
                          navigate(`/media-player/shows/${show.Id}`)
                        }
                        className="p-5"
                      >
                        <ShowCard show={show} />
                      </div>
                    </CarouselItem>
                  ))}
                </CarouselContent>
              </Carousel>
            </div>
          </div>
        ))}
      </div>
      {currentSubscription && (
        <Dialog
          open={isSubscriptionDialogOpen}
          onOpenChange={setIsSubscriptionDialogOpen}
        >
          <DialogContent
            className="w-[500px] px-8 py-12 border border-white/10 bg-[#0f1115]/50 text-white
                 backdrop-blur-xl shadow-2xl rounded-2xl"
          >
            <DialogHeader>
              <DialogTitle className="text-3xl text-mystic-green font-bold tracking-tight">
                {currentSubscription.Title}
              </DialogTitle>
              <DialogDescription className="text-white/70 ">
                {currentSubscription.Description}
              </DialogDescription>
            </DialogHeader>

            {/* Tabs cho các cycle: Monthly, Annually, ... */}
            <Tabs
              defaultValue={
                currentSubscription.Data?.[0]?.CycleName || "Monthly"
              }
              className="w-full mt-4"
            >
              <TabsList
                className="w-full transition-all duration-200 ease-out flex items-center md:inline-flex gap-2 bg-white/5 p-1 rounded-full
                     ring-1 ring-white/10"
              >
                {currentSubscription.Data.map((d) => (
                  <TabsTrigger
                    key={d.CycleName}
                    value={d.CycleName}
                    className="data-[state=active]:bg-[var(--accent)]
                         data-[state=active]:text-black data-[state=active]:shadow
                         rounded-full px-5 py-2 text-sm font-semibold
                         text-white
                         hover:bg-white/10 transition"
                    style={{ ["--accent" as any]: ACCENT }}
                  >
                    {d.CycleName}
                  </TabsTrigger>
                ))}
              </TabsList>

              {currentSubscription.Data.map((d) => (
                <TabsContent
                  key={d.CycleName}
                  value={d.CycleName}
                  className="mt-6 space-y-6"
                >
                  {/* Card giá */}
                  <div
                    className="rounded-2xl p-6 md:p-8 border border-white/10
                         bg-gradient-to-b from-white/5 to-transparent"
                  >
                    <div className="flex items-end gap-3">
                      <span className="text-4xl md:text-5xl text-mystic-green font-extrabold leading-none">
                        {formatVND(d.Price)}đ
                      </span>
                      <span className="text-white/60 mb-1">
                        {cycleSuffix(d.CycleName)}
                      </span>
                    </div>

                    {/* Benefits */}
                    {d.Benefits?.length > 0 && (
                      <ul className="mt-5 flex flex-col gap-3">
                        {d.Benefits.map((b) => (
                          <li key={b.Id} className="flex items-start gap-3">
                            <span
                              className="mt-0.5 inline-flex h-5 w-5 items-center justify-center rounded-full
                                   ring-1 ring-white/15"
                              style={{
                                backgroundColor: "rgba(174,227,57,0.15)",
                                color: "#cde97a",
                              }}
                            >
                              <Check size={14} />
                            </span>
                            <span className="text-sm text-white/90">
                              {b.Name}
                            </span>
                          </li>
                        ))}
                      </ul>
                    )}

                    <DialogFooter className="mt-8 flex items-center justify-center">
                      <Button
                        className="w-full mx-auto md:w-auto font-bold rounded-xl px-6 py-6
                             text-black hover:brightness-95"
                        style={{ backgroundColor: ACCENT }}
                      >
                        Subscribe now for only {formatVND(d.Price)}đ
                        {cycleSuffix(d.CycleName)}
                      </Button>
                    </DialogFooter>
                  </div>
                </TabsContent>
              ))}
            </Tabs>
          </DialogContent>
        </Dialog>
      )}
    </div>
  );
};

export default ChannelDetailsPage;

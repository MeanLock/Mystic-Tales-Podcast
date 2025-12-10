import { Channel } from "./channel.type";
import type { PodcastCategory, PodcastSubCategory } from "./category.type";
import { Show } from "./show.type";
import { Episode } from "./episode.type";

// DISCOVERY TYPES
export type DiscoveryData = {
  ContinueListening: ContinueListening;
  BasedOnYourTaste: BaseOnYourTaste;
  NewReleases: NewReleases;
  HotThisWeek: HotThisWeek;
  TopSubCategory: TopSubCategory;
  TopPodcasters: TopPodcasters;
  RandomCategory: RandomCategory;
  TalentedRookies: TalentedRookies;
};

export type ContinueListening = {
  ListenSessionList: {
    Episode: {
      Id: string;
      Name: string;
      MainImageFileKey: string;
      ReleaseDate: string;
      AudioLength: number;
      IsReleased: boolean;
    };
    Podcaster: {
      Id: number;
      FullName: string;
      Email: string;
      MainImageFileKey: string;
    };
    PodcastEpisodeListenSession: {
      Id: string;
      LastListenDurationSeconds: number;
    };
  }[];
};

export type BaseOnYourTaste = {
  ShowList: Show[];
};

export type NewReleases = {
  ShowList: Show[];
};

export type HotThisWeek = {
  ChannelList: Channel[];
  ShowList: Show[];
};

export type TopSubCategory = {
  PodcastSubCategory: PodcastSubCategory; //Chưa có nè
  ShowList: Show[];
};

export type TopPodcasters = {
  PodcasterList: {
    Id: number;
    FullName: string;
    Email: string;
    MainImageFileKey: string;
  }[];
};

export type RandomCategory = {
  PodcastCategory: PodcastCategory;
  ShowList: Show[];
};

export type TalentedRookies = {
  PodcasterList: {
    Id: number;
    FullName: string;
    Email: string;
    MainImageFileKey: string;
  }[];
};

// -----------------------------------------------------------------------------
// -----------------------------------------------------------------------------
// -----------------------------------------------------------------------------
// -----------------------------------------------------------------------------

// TRENDING TYPES
export type TrendingData = {
  PopularPodcasters: PopularPodcasters;
  Category1: CategoryX;
  HotPodcasters: HotPodcasters;
  Category2: CategoryX;
  PopularChannels: PopularChannels;
  Category3: CategoryX;
  HotChannels: HotChannels;
  Category4: CategoryX;
  PopularShows: PopularShows;
  Category5: CategoryX;
  HotShows: HotShows;
  Category6: CategoryX;
  NewEpisodes: NewEpisodes;
  PopularEpisodes: PopularEpisodes;
};


export type PopularPodcasters = {
  PodcasterList: {
    Id: number;
    FullName: string;
    Email: string;
    MainImageFileKey: string;
  }[];
};

export type CategoryX = {
  PodcastCategory: PodcastCategory;
  ShowList: Show[];
};

export type HotPodcasters = {
  PodcasterList: {
    Id: number;
    FullName: string;
    Email: string;
    MainImageFileKey: string;
  }[];
};

export type HotPodcastersUI = {
  PodcasterList: {
    Id: number;
    FullName: string;
    Email: string;
    ImageUrl: string;
  }[];
};

export type PopularChannels = {
  ChannelList: Channel[];
};

export type HotChannels = {
  ChannelList: Channel[];
};

export type PopularShows = {
  ShowList: Show[];
};

export type HotShows = {
  ShowList: Show[];
};

export type NewEpisodes = {
  EpisodeList: Episode[];
};


export type PopularEpisodes = {
  EpisodeList: Episode[];
};


// -----------------------------------------------------------------------------
// -----------------------------------------------------------------------------
// -----------------------------------------------------------------------------
// -----------------------------------------------------------------------------

// CATEGORY FEED TYPES
export type CategoryFeedData = {
  PodcastCategory: {
    Id: number;
    Name: string;
    MainImageFileKey: string;
  };
  TopChannels: Channel[];
  TopShows: Show[];
  TopEpisodes: Episode[];
  HotShows: Show[];
  SubCategorySections: SubCategoriesFromAPI[];
};

export type SubCategoriesFromAPI = {
  PodcastSubCategory: {
    Id: number;
    Name: string;
    PodcastCategoryId: number;
  };
  ShowList: Show[];
};


// -----------------------------------------------------------------------------
// -----------------------------------------------------------------------------
// -----------------------------------------------------------------------------
// -----------------------------------------------------------------------------

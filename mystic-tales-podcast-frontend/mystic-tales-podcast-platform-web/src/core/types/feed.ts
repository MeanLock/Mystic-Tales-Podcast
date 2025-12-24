import type { ChannelFromAPI, ChannelUI } from "./channel";
import type { EpisodeFromAPI, EpisodeUI } from "./episode";
import type { PodcastCategory, PodcastSubCategory } from "./podcastCategory";
import type { ShowFromAPI, ShowUI } from "./show";

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

export type DiscoveryDataFileResolved = {
  ContinueListening: ContinueListeningUI;
  BasedOnYourTaste: BaseOnYourTasteUI;
  NewReleases: NewReleasesUI;
  HotThisWeek: HotThisWeekUI;
  TopSubCategory: TopSubCategoryUI;
  TopPodcasters: TopPodcastersUI;
  RandomCategory: RandomCategoryUI;
  TalentedRookies: TalentedRookiesUI;
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

export type ContinueListeningUI = {
  ListenSessionList: {
    Episode: {
      Id: string;
      Name: string;
      ImageUrl: string;
      ReleaseDate: string;
      AudioLength: number;
      IsReleased: boolean;
    };
    Podcaster: {
      Id: number;
      FullName: string;
      Email: string;
      ImageUrl: string;
    };
    PodcastEpisodeListenSession: {
      Id: string;
      LastListenDurationSeconds: number;
    };
  }[];
};

export type BaseOnYourTaste = {
  ShowList: ShowFromAPI[];
};

export type BaseOnYourTasteUI = {
  ShowList: ShowUI[];
};

export type NewReleases = {
  ShowList: ShowFromAPI[];
};

export type NewReleasesUI = {
  ShowList: ShowUI[];
};

export type HotThisWeek = {
  ChannelList: ChannelFromAPI[];
  ShowList: ShowFromAPI[];
};

export type HotThisWeekUI = {
  ChannelList: ChannelUI[];
  ShowList: ShowUI[];
};

export type TopSubCategory = {
  PodcastSubCategory: PodcastSubCategory;
  ShowList: ShowFromAPI[];
};

export type TopSubCategoryUI = {
  PodcastSubCategory: PodcastSubCategory;
  ShowList: ShowUI[];
};

export type TopPodcasters = {
  PodcasterList: {
    Id: number;
    FullName: string;
    Email: string;
    MainImageFileKey: string;
  }[];
};

export type TopPodcastersUI = {
  PodcasterList: {
    Id: number;
    FullName: string;
    Email: string;
    ImageUrl: string;
  }[];
};

export type RandomCategory = {
  PodcastCategory: PodcastCategory;
  ShowList: ShowFromAPI[];
};

export type RandomCategoryUI = {
  PodcastCategory: PodcastCategory;
  ShowList: ShowUI[];
};

export type TalentedRookies = {
  PodcasterList: {
    Id: number;
    FullName: string;
    Email: string;
    MainImageFileKey: string;
  }[];
};

export type TalentedRookiesUI = {
  PodcasterList: {
    Id: number;
    FullName: string;
    Email: string;
    ImageUrl: string;
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

export type TrendingDataUI = {
  PopularPodcasters: PopularPodcastersUI;
  Category1: CategoryXUI;
  HotPodcasters: HotPodcastersUI;
  Category2: CategoryXUI;
  PopularChannels: PopularChannelsUI;
  Category3: CategoryXUI;
  HotChannels: HotChannelsUI;
  Category4: CategoryXUI;
  PopularShows: PopularShowsUI;
  Category5: CategoryXUI;
  HotShows: HotShowsUI;
  Category6: CategoryXUI;
  NewEpisodes: NewEpisodesUI;
  PopularEpisodes: PopularEpisodesUI;
};

export type PopularPodcasters = {
  PodcasterList: {
    Id: number;
    FullName: string;
    Email: string;
    MainImageFileKey: string;
  }[];
};

export type PopularPodcastersUI = {
  PodcasterList: {
    Id: number;
    FullName: string;
    Email: string;
    ImageUrl: string;
  }[];
};

export type CategoryX = {
  PodcastCategory: PodcastCategory;
  ShowList: ShowFromAPI[];
};

export type CategoryXUI = {
  PodcastCategory: PodcastCategory;
  ShowList: ShowUI[];
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
  ChannelList: ChannelFromAPI[];
};

export type PopularChannelsUI = {
  ChannelList: ChannelUI[];
};

export type HotChannels = {
  ChannelList: ChannelFromAPI[];
};

export type HotChannelsUI = {
  ChannelList: ChannelUI[];
};

export type PopularShows = {
  ShowList: ShowFromAPI[];
};

export type PopularShowsUI = {
  ShowList: ShowUI[];
};

export type HotShows = {
  ShowList: ShowFromAPI[];
};

export type HotShowsUI = {
  ShowList: ShowUI[];
};

export type NewEpisodes = {
  EpisodeList: EpisodeFromTrending[];
};

export type EpisodeFromTrending = {
  Id: string;
  Name: string;
  Description: string;
  MainImageFileKey: string;
  IsReleased: boolean;
  ReleaseDate: string;
  AudioLength: number;
};

export type NewEpisodesUI = {
  EpisodeList: EpisodeUI[];
};

export type PopularEpisodes = {
  EpisodeList: EpisodeFromAPI[];
};

export type PopularEpisodesUI = {
  EpisodeList: EpisodeUI[];
};

// -----------------------------------------------------------------------------
// -----------------------------------------------------------------------------
// -----------------------------------------------------------------------------
// -----------------------------------------------------------------------------

// CATEGORY FEED TYPES
export type CategoryFeedDataFromAPI = {
  PodcastCategory: {
    Id: number;
    Name: string;
    MainImageFileKey: string;
  };
  TopChannels: ChannelFromAPI[];
  TopShows: ShowFromAPI[];
  TopEpisodes: EpisodeFromAPI[];
  HotShows: ShowFromAPI[];
  SubCategorySections: SubCategoriesFromAPI[];
};

export type SubCategoriesFromAPI = {
  PodcastSubCategory: {
    Id: number;
    Name: string;
    PodcastCategoryId: number;
  };
  ShowList: ShowFromAPI[];
};

export type CategoryFeedDataUI = {
  PodcastCategory: {
    Id: number;
    Name: string;
    MainImageFileKey: string;
  };
  TopChannels: ChannelUI[];
  TopShows: ShowUI[];
  TopEpisodes: EpisodeUI[];
  HotShows: ShowUI[];
  SubCategorySections: SubCategoriesUI[];
};

export type SubCategoriesUI = {
  PodcastSubCategory: {
    Id: number;
    Name: string;
    PodcastCategoryId: number;
  };
  ShowList: ShowUI[];
};

// -----------------------------------------------------------------------------
// -----------------------------------------------------------------------------
// -----------------------------------------------------------------------------
// -----------------------------------------------------------------------------

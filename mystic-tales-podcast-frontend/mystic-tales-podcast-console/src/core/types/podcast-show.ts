export type PodcastCategory = {
  Id: number;
  Name: string;
};

export type PodcastSubCategory = {
  Id: number;
  Name: string;
  PodcastCategoryId: number;
};

export type PodcastShowsSubscriptionType = {
  Id: number;
  Name: string;
};

export type PodcastSubscriptionCycleType = {
  Id: number;
  Name: string;
};

export type PodcastSubscriptionBenefit = {
  Id: number;
  Name: string;
};

export type PodcastSubscriptionCycleTypePrice = {
  PodcastSubscriptionId: number;
  SubscriptionCycleType: PodcastSubscriptionCycleType;
  Version: number;
  Price: number;
  CreatedAt: string;
  UpdatedAt: string;
};

export type PodcastSubscriptionBenefitMapping = {
  PodcastSubscriptionId: number;
  PodcastSubscriptionBenefit: PodcastSubscriptionBenefit;
  Version: number;
  CreatedAt: string;
  UpdatedAt: string;
};

export type ShowSubscription = {
  Id: number;
  Name: string;
  Description: string;
  PodcastChannelId: string;
  PodcastShowId: string;
  IsActive: boolean;
  CurrentVersion: number;
  DeletedAt: string;
  CreatedAt: string;
  UpdatedAt: string;
  PodcastSubscriptionCycleTypePriceList: PodcastSubscriptionCycleTypePrice[];
  PodcastSubscriptionBenefitMappingList: PodcastSubscriptionBenefitMapping[];
};

export type PodcastEpisodeSubscriptionType = {
  Id: number;
  Name: string;
};

export type ShowEpisode = {
  Id: string;
  Title: string;
  Description: string;
  ExplicitContent: boolean;
  ReleaseDate: string;
  IsReleased: boolean;
  MainImageFileKey: string;
  AudioFileKey: string;
  AudioFileSize: number;
  AudioLength: number;
  AudioFingerprint: string;
  PodcastEpisodeSubscriptionType: PodcastEpisodeSubscriptionType;
  PodcastShowId: string;
  SeasonNumber: number;
  TotalSave: number;
  ListenCount: number;
  IsAudioPublishable: boolean;
  TakenDownReason: string;
  DeletedAt: string;
  CreatedAt: string;
  UpdatedAt: string;
};

export type PodcastShow = {
  Id: number;
  Name: string;
  Description: string;
  Language: string;
  ReleaseDate: string;
  Copyright: string;
  UploadFrequency: string;
  AverageRating: number;
  RatingCount: number;
  MainImageFileKey: string;
  TrailerAudioFileKey: string;
  TotalFollow: number;
  ListenCount: number;
  PodcasterId: number;
  PodcastCategory: PodcastCategory;
  PodcastSubCategory: PodcastSubCategory;
  PodcastShowsSubscriptionType: PodcastShowsSubscriptionType;
  PodcastChannnelId: number;
  TakenDownReason: string;
  DeletedAt: string;
  CreateAt: string;
  UpdatedAt: string;
  ShowSubscriptionList: ShowSubscription[];
  ShowEpisodeList: ShowEpisode[];
};


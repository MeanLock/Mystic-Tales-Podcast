export type EpisodeDetailsFromAPI = {
  Id: string;
  Name: string;
  Description: string;
  ExplicitContent: boolean;
  ReleaseDate: string;
  EpisodeOrder: number;
  IsReleased: boolean;
  MainImageFileKey: string;
  AudioFileKey: string;
  AudioFileSize: number;
  AudioLength: number;
  PodcastEpisodeSubscriptionType: {
    Id: number;
    Name: string;
  };
  PodcastShow: {
    Id: string;
    Name: string;
    Description: string;
    MainImageFileKey: string;
    ReleaseDate: string;
    IsReleased: boolean;
  };
  Hashtags: {
    Id: number;
    Name: string;
  }[];
  SeasonNumber: number;
  TotalSave: number;
  ListenCount: number;
  IsAudioPublishable: boolean;
  TakenDownReason: string;
  CreatedAt: string;
  UpdatedAt: string;
  CurrentStatus: {
    Id: number;
    Name: string;
  };
  Podcaster: {
    Id: number;
    FullName: string;
    Email: string;
    MainImageFileKey: string;
  };
  IsSavedByCurrentUser: true;
};

export type EpisodeDetailsUI = {
  Id: string;
  Name: string;
  Description: string;
  ExplicitContent: boolean;
  ReleaseDate: string;
  EpisodeOrder: number;
  IsReleased: boolean;
  ImageUrl: string;
  AudioFileKey: string;
  AudioFileSize: number;
  AudioLength: number;
  PodcastEpisodeSubscriptionType: {
    Id: number;
    Name: string;
  };
  PodcastShow: {
    Id: string;
    Name: string;
    Description: string;
    ImageUrl: string;
    ReleaseDate: string;
    IsReleased: boolean;
  };
  Hashtags: {
    Id: number;
    Name: string;
  }[];
  SeasonNumber: number;
  TotalSave: number;
  ListenCount: number;
  IsAudioPublishable: boolean;
  TakenDownReason: string;
  CreatedAt: string;
  UpdatedAt: string;
  CurrentStatus: {
    Id: number;
    Name: string;
  };
  Podcaster: {
    Id: number;
    FullName: string;
    Email: string;
    ImageUrl: string;
  };
  IsSavedByCurrentUser: true;
};

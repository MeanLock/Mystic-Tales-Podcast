export type Episode = {
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
};

export type EpisodeFromShow = {
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
    MainImageFileKey: string;
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
};

export type EpisodeDetails = {
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

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

export type EpisodeFromAPI = {
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

export type EpisodeUI = {
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
    DeletedAt: string;
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

export type EpisodeFromShowUI = {
  Id: "string";
  Name: "string";
  Description: "string";
  ExplicitContent: true;
  ReleaseDate: "2025-12-01T08:08:43.536Z";
  EpisodeOrder: 0;
  IsReleased: true;
  MainImageFileKey: "string";
  AudioFileKey: "string";
  AudioFileSize: 0;
  AudioLength: 0;
  PodcastEpisodeSubscriptionType: {
    Id: 0;
    Name: "string";
  };
  PodcastShow: {
    Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6";
    Name: "string";
    Description: "string";
    MainImageFileKey: "string";
    ReleaseDate: "2025-12-01";
    IsReleased: true;
    DeletedAt: "2025-12-01T08:08:43.536Z";
  };
  Hashtags: [
    {
      Id: 0;
      Name: "string";
    }
  ];
  SeasonNumber: 0;
  TotalSave: 0;
  ListenCount: 0;
  IsAudioPublishable: true;
  TakenDownReason: "string";
  CreatedAt: "2025-12-01T08:08:43.536Z";
  UpdatedAt: "2025-12-01T08:08:43.536Z";
  CurrentStatus: {
    Id: 0;
    Name: "string";
  };
};

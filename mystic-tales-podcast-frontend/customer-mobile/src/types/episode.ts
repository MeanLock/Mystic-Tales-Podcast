export type EpisodeFromApi = {
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
  PodcastEpisodeSubscriptionType: PodcastEpisodeSubscription;
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

export type EpisodeWithImageUrl = {
  Id: string;
  Title: string;
  Description: string;
  ExplicitContent: boolean;
  ReleaseDate: string;
  IsReleased: boolean;
  ImageUrl: string;
  AudioFileKey: string;
  AudioFileSize: number;
  AudioLength: number;
  AudioFingerprint: string;
  PodcastEpisodeSubscriptionType: PodcastEpisodeSubscription;
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

export type PodcastEpisodeSubscriptionType = {
  Id: number;
  Name: string;
};
export type EpisodeCardWithImageProps = {
  Id: string;
  Title: string;
  Description: string;
  ExplicitContent: boolean;
  ReleaseDate: string;
  IsReleased: boolean;
  ImageUrl: string;
  AudioFileKey: string;
  AudioFileSize: number;
  AudioLength: number;
  AudioFingerprint: string;
  PodcastEpisodeSubscriptionType: PodcastEpisodeSubscription;
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

export type PodcastEpisodeSubscription = {
  Id: number;
  Name: string;
};

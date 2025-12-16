export type PodcastEpisodeSubscriptionType = {
  Id: number;
  Name: string;
};

export type PodcastEpisode = {
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


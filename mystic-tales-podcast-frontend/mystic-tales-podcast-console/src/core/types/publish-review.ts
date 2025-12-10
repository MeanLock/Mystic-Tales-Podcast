export type PublishReview = {
  Id: number;
  AssignedStaff: {
    Id: number;
    FullName: string;
    Email: string;
  };

  PodcastEpisode: {
    Id: string;
    Name: string;
    Description: string | null;
    ExplicitContent: boolean;
    ReleaseDate: string; // yyyy-MM-dd
    IsReleased: boolean;
    MainImageFileKey: string;
    AudioFileKey: string;
    AudioFileSize: number;
    AudioLength: number;
    AudioFingerPrint: string;
    AudioTranscript: string;
    AudioEncryptionKeyId: string;
    AudioEncryptionKeyFileKey: string;
    PodcastEpisodeSubscriptionTypeId: number;
    PodcastShowId: string;
    SeasonNumber: number;
    EpisodeOrder: number;
    TotalSave: number;
    ListenCount: number;
    IsAudioPublishable: boolean;
    TakenDownReason: string;
    DeletedAt: string;
    CreatedAt: string;
    UpdatedAt: string;
  };

  Note: string;
  ReReviewCount: number;
  Deadline: string;
  CreatedAt: string;
  UpdatedAt: string;

  PodcastIllegalContentTypeList: {
    Id: number;
    Name: string;
  }[];

  PublishDuplicateDetectedPodcastEpisodes: {
    Id: string;
    Name: string;
    Description: string | null;
    ExplicitContent: boolean;
    ReleaseDate: string;
    IsReleased: boolean;
    MainImageFileKey: string;
    AudioFileKey: string;
    AudioFileSize: number;
    AudioLength: number;
    AudioFingerPrint: string;
    AudioTranscript: string;
    AudioEncryptionKeyId: string;
    AudioEncryptionKeyFileKey: string;
    PodcastEpisodeSubscriptionTypeId: number;
    PodcastShowId: string;
    SeasonNumber: number;
    EpisodeOrder: number;
    TotalSave: number;
    ListenCount: number;
    IsAudioPublishable: boolean;
    TakenDownReason: string;
    DeletedAt: string;
    CreatedAt: string;
    UpdatedAt: string;
  }[];

  RestrictedTermFoundList: string[];

  PodcastChannel: {
    Id: string;
    Name: string;
    Description: string;
    MainImageFileKey: string;
    DeletedAt: string | null;
  };

  PodcastShow: {
    Id: string;
    Name: string;
    Description: string;
    MainImageFileKey: string;
    ReleaseDate: string;
    DeletedAt: string | null;
  };

  Podcaster: {
    Id: number;
    FullName: string;
    Email: string;
    MainImageFileKey: string;
    RoleId: number;
    IsVerified: boolean;
    ViolationLevel: number;
    ViolationPoint: number;
    PodcasterProfileName: string;
    PodcasterProfileIsVerified: boolean;
    HasVerifiedPodcasterProfile: boolean;
  };

  EpisodeCurrentStatus: {
    Id: number;
    Name: string;
  };

  ShowCurrentStatus: {
    Id: number;
    Name: string;
  };

  ChannelCurrentStatus: {
    Id: number;
    Name: string;
  };

  CurrentStatus: {
    Id: number;
    Name: string;
  };
};

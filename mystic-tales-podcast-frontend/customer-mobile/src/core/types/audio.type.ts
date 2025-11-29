// Lưu thông tin chi tiết của audio đang phát
export type CurrentAudio = {
  Id: string;
  Name: string;
  MainImageFileKey: string;
  PodcasterName: string;
  AudioLength: number; // giây
} | null;

// Lưu thông tin quản lý UI của Media Player
export type PlayerControl = {
  playStatus: "stop" | "pause" | "play" | "loading";
  nextMode: "Sequential" | "Random";
  sourceType: ProcedureSourceDetailType | null;
  isNextSessionNull: boolean;
  audioId: string | null;
  isAutoPlay: boolean;
  volume: number;
};

// Lưu thông tin phiên nghe hiện tại
export type ListenSession =
  | ListenSessionEpisodes
  | ListenSessionBookingTracks
  | null;

export type ListenSessionEpisodes = {
  PodcastEpisode: {
    Id: string;
    Name: string;
    Description: string;
    MainImageFileKey: string;
    AudioLength: number;
    ReleaseDate: string;
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
  Token: string;
  PlaylistFileKey: string;
  AudioFileUrl: string;
};

export type ListenSessionBookingTracks = {
  Booking: {
    Id: number;
    Title: string;
    Description: string;
  };
  BookingPodcastTrack: {
    Id: string;
    BookingRequirementName: string;
    BookingRequirementDescription: string;
  };
  BookingPodcastTrackListenSession: {
    Id: string;
    LastListenDurationSeconds: number;
  };
  PlaylistFileKey: string;
  AudioFileUrl: string;
};

// Lưu thông tin cách thức nghe hiện tại
export type ListenSessionProcedure = {
  Id: string;
  PlayOrderMode: PlayOrderMode;
  IsAutoPlay: boolean;
  SourceDetail: {
    Type: ProcedureSourceDetailType;
    PodcastShow: {
      Id: string;
      Name: string;
    };
    Booking: {
      BookingProducingRequestId: string;
      Title: string;
    };
  };
  ListenObjectsSequentialOrder:
    | {
        ListenObjectId: string;
        Order: number;
        IsListenable: boolean;
      }[]
    | null;
  ListenObjectsRandomOrder:
    | {
        ListenObjectId: string;
        Order: number;
        IsListenable: boolean;
      }[]
    | null;
  IsCompleted: boolean;
  CreatedAt: string;
} | null;

export type ProcedureSourceDetailType =
  | "SavedEpisodes"
  | "SpecifyShowEpisodes"
  | "BookingProducingTracks";

export type PlayOrderMode = "Sequential" | "Random";

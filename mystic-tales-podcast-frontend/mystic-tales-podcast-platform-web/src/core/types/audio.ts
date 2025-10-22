export type CurrentAudioFromApi = {
  Id: string;
  Name: string;
  LatestPosition: number; // giây
  AudioLength: number; // giây
  EpisodeNumber: number; // Số tập
  MainFileKey: string;
  MainImageFileKey: string;
  PodcasterName: string;
  Show: { Id: string; Name: string };
} | null;

export type CurrentAudioUI = {
  Id: string;
  Name: string;
  LatestPosition: number; // giây
  AudioLength: number; // giây
  EpisodeNumber: number; // Số tập
  FileUrl: string;
  ImageUrl: string;
  PodcasterName: string;
  Show: { Id: string; Name: string };
} | null;

export type QueuedAudio = {
  Index: number;
  Id: string;
  Name: string;
  AudioLength: number;
  EpisodeNumber: number; // Số tập
  FileUrl: string;
  ImageUrl: string;
  PodcasterName: string;
  Show: { Id: string; Name: string };
};

export type QueuedAudioWithNoIndex = Omit<QueuedAudio, "Index">;

export type AddToQueuePayload = {
  audio: QueuedAudioWithNoIndex;
  position: "to-last" | "to-top";
};

export type PlayMode = {
  playStatus: "stop" | "pause" | "play";
  nextMode: "normal" | "show" | "saved" | "bookings";
};

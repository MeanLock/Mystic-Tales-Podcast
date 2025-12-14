export type HlsLoadBaseOptions = {
  audio: HTMLAudioElement;
  baseUrl: string; // BASE_URL (không slash cuối)
  fileKey: string;

  seekTo?: number; // seconds
  isSeekThenPlay?: boolean; // true: load xong seek + play, false: load xong seek + pause

  accessToken?: string | null;
  onBufferingChange?: (buffering: boolean) => void;
  onSeekingChange?: (seeking: boolean) => void;
};

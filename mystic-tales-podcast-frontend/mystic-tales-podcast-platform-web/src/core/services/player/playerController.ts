// @ts-nocheck

// playerController.ts
import Hls from "hls.js";
import { loadEpisodeHls } from "./hlsEpisodeLoaderNew";
import { loadBookingHls } from "./hlsBookingLoaderNew";
import { BASE_URL } from "@/core/api/appApi";
import { getAccessToken } from "@/core/api/appApi/token";
import type {
  ListenSessionBookingTracks,
  ListenSessionEpisodes,
  ListenSessionProcedure,
} from "@/core/types/audio";

export type SourceType =
  | "SpecifyShowEpisodes"
  | "SavedEpisodes"
  | "BookingProducingTracks";

export type PlayerUiState = {
  isPlaying: boolean;
  buffering: boolean;
  seeking: boolean;
  currentTime: number;
  duration: number;
  currentAudio: {
    id: string;
    name: string;
    image?: string;
    podcasterName?: string;
  } | null;
  sourceType: SourceType | null;
  volume: number;
  isLoadingSession: boolean;
  loadingAudioId: string | null;
};

export type PlayerEvents = {
  onStateChange?: (state: PlayerUiState) => void;
  onError?: (error: unknown) => void;
  onEnded?: () => void;
};

class PlayerController {
  private static _instance: PlayerController | null = null;
  static getInstance() {
    if (!this._instance) this._instance = new PlayerController();
    return this._instance;
  }

  private audio: HTMLAudioElement;
  private hls: Hls | null = null;

  private events: PlayerEvents = {};
  private onStateChangeListeners: Array<(state: PlayerUiState) => void> = [];
  private onErrorListeners: Array<(error: unknown) => void> = [];
  private onEndedListeners: Array<() => void> = [];
  private isPlaying = false;
  private buffering = false;
  private seeking = false;
  private sourceType: SourceType | null = null;
  private currentAudioMeta: PlayerUiState["currentAudio"] = null;

  private currentSession:
    | ListenSessionEpisodes
    | ListenSessionBookingTracks
    | null = null;
  private currentProcedure: ListenSessionProcedure | null = null;
  private currentAudioId: string | null = null;
  private rafId: number | null = null;
  private isLoadingLatest = false; // Prevent concurrent playFromLatest calls
  private isLoadingSession = false; // Prevent concurrent play operations
  private loadingAudioId: string | null = null; // Track which audio is being loaded

  private constructor() {
    this.audio = new Audio();
    this.audio.preload = "auto";
    this.audio.crossOrigin = "anonymous";

    // === AUDIO EVENTS ===
    this.audio.addEventListener("timeupdate", () => this.emitState());
    this.audio.addEventListener("loadedmetadata", () => this.emitState());

    this.audio.addEventListener("seeking", () => {
      this.seeking = true;
      // thường khi seeking thì cũng đang chờ buffer
      this.buffering = true;
      this.emitState();
    });

    this.audio.addEventListener("seeked", () => {
      this.seeking = false;
      this.emitState();
    });

    // Đang chờ data (thiếu buffer)
    this.audio.addEventListener("waiting", () => {
      this.buffering = true;
      this.emitState();
    });

    // Đã có đủ data để play (kể cả không auto-play)
    this.audio.addEventListener("canplay", () => {
      this.buffering = false;
      this.emitState();
    });

    this.audio.addEventListener("progress", () => this.emitState());

    this.audio.addEventListener("playing", () => {
      console.log("Audio playing event");
      this.isPlaying = true;
      // Khi bắt đầu playing thì coi như hết buffering
      this.buffering = false;
      this.emitState();
      this.startTicker();
    });

    this.audio.addEventListener("pause", () => {
      console.log("Audio pause event");
      this.isPlaying = false;
      this.emitState();
      this.stopTicker();
    });

    this.audio.addEventListener("ended", () => {
      this.isPlaying = false;
      this.emitState();
      // Call all registered ended listeners
      this.onEndedListeners.forEach((listener) => {
        try {
          listener();
        } catch (err) {
          console.error("Error in onEnded listener:", err);
        }
      });
      console.log("Audio ended event");
      this.stopTicker();
    });
  }

  attachEvents(events: PlayerEvents) {
    if (events.onStateChange) {
      this.onStateChangeListeners.push(events.onStateChange);
    }
    if (events.onError) {
      this.onErrorListeners.push(events.onError);
    }
    if (events.onEnded) {
      this.onEndedListeners.push(events.onEnded);
    }

    return () => {
      if (events.onStateChange) {
        const idx = this.onStateChangeListeners.indexOf(events.onStateChange);
        if (idx > -1) this.onStateChangeListeners.splice(idx, 1);
      }
      if (events.onError) {
        const idx = this.onErrorListeners.indexOf(events.onError);
        if (idx > -1) this.onErrorListeners.splice(idx, 1);
      }
      if (events.onEnded) {
        const idx = this.onEndedListeners.indexOf(events.onEnded);
        if (idx > -1) this.onEndedListeners.splice(idx, 1);
      }
    };
  }

  getUiState(): PlayerUiState {
    return {
      isPlaying: this.isPlaying,
      buffering: this.buffering,
      seeking: this.seeking,
      currentTime: this.audio.currentTime || 0,
      duration: this.audio.duration || 0,
      currentAudio: this.currentAudioMeta,
      sourceType: this.sourceType,
      volume: this.audio.volume,
      isLoadingSession: this.isLoadingSession,
      loadingAudioId: this.loadingAudioId,
    };
  }

  // Basic controls
  async play() {
    try {
      await this.audio.play();
    } catch (err) {
      this.onErrorListeners.forEach((listener) => {
        try {
          listener(err);
        } catch (e) {
          console.error("Error in onError listener:", e);
        }
      });
    }
  }

  pause() {
    this.audio.pause();
  }

  stop() {
    console.log("Stopping playback and resetting player state");
    this.audio.pause();
    this.audio.currentTime = 0;
    this.audio.src = "";

    this.destroyHls();
    this.stopTicker();

    this.isPlaying = false;
    this.buffering = false;
    this.seeking = false;
    this.sourceType = null;
    this.currentAudioMeta = null;
    this.currentSession = null;
    this.currentProcedure = null;
    this.currentAudioId = null;

    this.emitState();
  }

  seek(seconds: number) {
    console.log("Seeking to:", Math.max(0, seconds));
    this.audio.currentTime = Math.max(0, seconds);
    // audio sẽ tự fire 'seeking'/'seeked', mình chỉ emit state thêm cho chắc
    this.emitState();
  }

  setVolume(v01: number) {
    this.audio.volume = Math.min(1, Math.max(0, v01));
    this.emitState();
  }

  // Reset toàn bộ state về trạng thái ban đầu (dùng khi cần fresh start)
  reset() {
    this.stop();
  }

  // Kiểm tra và set flag để prevent concurrent playFromLatest
  isLoadingLatestSession(): boolean {
    return this.isLoadingLatest;
  }

  setLoadingLatestSession(value: boolean): void {
    this.isLoadingLatest = value;
  }

  // Kiểm tra và set flag để prevent concurrent play operations
  isCurrentlyLoadingSession(): boolean {
    return this.isLoadingSession;
  }

  setLoadingSession(value: boolean, audioId?: string): void {
    this.isLoadingSession = value;
    this.loadingAudioId = value ? audioId || null : null;
    this.emitState();
  }

  // ====== API CHÍNH: nhận session đã có sẵn ======
  async playFromExistingSession(opts: {
    session: ListenSessionEpisodes | ListenSessionBookingTracks;
    procedure: ListenSessionProcedure;
    sourceType: SourceType;
    seekTo?: number;
    isSeekThenPlay?: boolean;
  }) {
    const {
      session,
      procedure,
      sourceType,
      seekTo,
      isSeekThenPlay = true,
    } = opts;

    this.sourceType = sourceType;
    this.currentSession = session;
    this.currentProcedure = procedure;

    if (
      sourceType === "SpecifyShowEpisodes" ||
      sourceType === "SavedEpisodes"
    ) {
      const s = session as ListenSessionEpisodes;
      this.currentAudioId = s.PodcastEpisode.Id.toString();
      this.currentAudioMeta = {
        id: s.PodcastEpisode.Id.toString(),
        name: s.PodcastEpisode.Name,
        image: s.PodcastEpisode.MainImageFileKey || "",
        podcasterName: s.Podcaster.FullName || "Unknown",
      };
      await this.loadFromEpisodeSession(s, { seekTo, isSeekThenPlay });
    } else {
      const s = session as ListenSessionBookingTracks;
      this.currentAudioId = s.BookingPodcastTrack.Id.toString();
      this.currentAudioMeta = {
        id: s.BookingPodcastTrack.Id.toString(),
        name: s.BookingPodcastTrack.BookingRequirementName,
        image: "",
        podcasterName: "Booking Track",
      };
      await this.loadFromBookingSession(s, { seekTo, isSeekThenPlay });
    }
  }

  async switchToSession(opts: {
    session: ListenSessionEpisodes | ListenSessionBookingTracks;
    procedure: ListenSessionProcedure;
    sourceType: SourceType;
    seekTo?: number;
    isSeekThenPlay?: boolean;
  }) {
    return this.playFromExistingSession(opts);
  }

  // INTERNAL
  private async loadFromEpisodeSession(
    session: ListenSessionEpisodes,
    opts: { seekTo?: number; isSeekThenPlay?: boolean }
  ) {
    this.destroyHls();
    // Bắt đầu load => bật buffering
    this.setBuffering(true);

    const accessToken = getAccessToken();
    const hasPlaylistFileKey = session.PlaylistFileKey;
    const hasAudioFileUrl = session.AudioFileUrl;

    // Case file trực tiếp (mp3/mp4)
    if (hasAudioFileUrl) {
      this.audio.src = session.AudioFileUrl;
      if (typeof opts.seekTo === "number" && opts.seekTo > 0) {
        this.audio.currentTime = opts.seekTo;
      }
      // Khi metadata + canplay, event sẽ tự tắt buffering
      if (opts.isSeekThenPlay !== false) {
        await this.audio.play().catch(() => {});
      } else {
        this.audio.pause();
      }
      return;
    }

    // Case HLS playlist
    if (hasPlaylistFileKey) {
      this.hls = await loadEpisodeHls({
        audio: this.audio,
        baseUrl: BASE_URL,
        fileKey: session.PlaylistFileKey,
        episodeId: session.PodcastEpisode.Id,
        token: session.Token,
        accessToken,
        seekTo: opts.seekTo,
        isSeekThenPlay: opts.isSeekThenPlay ?? true,
        // Giữ callback nhưng chủ yếu state sẽ được drive bởi audio events
        onBufferingChange: (b) => this.setBuffering(b),
        onSeekingChange: (s) => this.setSeeking(s),
      });
    }
  }

  private async loadFromBookingSession(
    session: ListenSessionBookingTracks,
    opts: { seekTo?: number; isSeekThenPlay?: boolean }
  ) {
    this.destroyHls();
    this.setBuffering(true);

    const accessToken = getAccessToken();
    const hasPlaylistFileKey = session.PlaylistFileKey;
    const hasAudioFileUrl = session.AudioFileUrl;

    if (hasAudioFileUrl) {
      this.audio.src = session.AudioFileUrl;
      if (typeof opts.seekTo === "number" && opts.seekTo > 0) {
        this.audio.currentTime = opts.seekTo;
      }
      if (opts.isSeekThenPlay !== false) {
        await this.audio.play().catch(() => {});
      } else {
        this.audio.pause();
      }
      return;
    }

    if (hasPlaylistFileKey) {
      this.hls = await loadBookingHls({
        audio: this.audio,
        baseUrl: BASE_URL,
        fileKey: session.PlaylistFileKey,
        bookingId: session.Booking.Id,
        trackId: session.BookingPodcastTrack.Id,
        accessToken,
        seekTo: opts.seekTo,
        isSeekThenPlay: opts.isSeekThenPlay ?? true,
        onBufferingChange: (b) => this.setBuffering(b),
        onSeekingChange: (s) => this.setSeeking(s),
      });
    }
  }

  private setBuffering(b: boolean) {
    this.buffering = b;
    this.emitState();
  }

  private setSeeking(s: boolean) {
    this.seeking = s;
    this.emitState();
  }

  private emitState() {
    const state = this.getUiState();
    this.onStateChangeListeners.forEach((listener) => {
      try {
        listener(state);
      } catch (err) {
        console.error("Error in onStateChange listener:", err);
      }
    });
  }

  private startTicker() {
    if (this.rafId != null) return;
    const tick = () => {
      this.emitState();
      this.rafId = requestAnimationFrame(tick);
    };
    this.rafId = requestAnimationFrame(tick);
  }

  private stopTicker() {
    if (this.rafId != null) {
      cancelAnimationFrame(this.rafId);
      this.rafId = null;
    }
  }

  private destroyHls() {
    if (this.hls) {
      try {
        this.hls.destroy();
      } catch {}
      this.hls = null;
    }
  }
}

export function getPlayerController() {
  return PlayerController.getInstance();
}

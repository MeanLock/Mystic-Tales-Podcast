// src/core/player/playerEngine.ts
import { Audio, AVPlaybackStatus } from "expo-av";
import { ListenSession, ListenSessionProcedure } from "../../types/audio.type";

export type SourceType =
  | "SpecifyShowEpisodes"
  | "SavedEpisodes"
  | "BookingProducingTracks";

export type PlayerTrack = {
  id: string;
  url: string;
  title?: string;
  artist?: string;
  artwork?: string | null;
};

export type PlayerUiState = {
  isPlaying: boolean;
  buffering: boolean;
  listenSession: ListenSession;
  listenSessionProcedure: ListenSessionProcedure;
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
  isAutoPlay: boolean;
  isAudioLoading: boolean;
  loadingAudioId: string | null;
};

export type PlayerStatus = {
  isLoaded: boolean;
  isPlaying: boolean;
  positionMs: number;
  durationMs: number;
  bufferedMs?: number;
  didJustFinish?: boolean;
  isBuffering?: boolean;
};

export type PlayerEngineListener = (status: PlayerStatus) => void;
export type PlayerUiStateListener = (state: PlayerUiState) => void;
export type OnAudioEndCallback = () => void;

class PlayerEngine {
  private sound: Audio.Sound | null = null;
  private currentTrack: PlayerTrack | null = null;
  private listener: PlayerEngineListener | null = null;

  // UI State tracking
  private uiStateListeners: PlayerUiStateListener[] = [];
  private buffering: boolean = false;
  private seeking: boolean = false;
  private sourceType: SourceType | null = null;
  private volume: number = 1.0;
  private listenSession: ListenSession | null = null;
  private listenSessionProcedure: ListenSessionProcedure | null = null;
  private isAutoPlay: boolean = false;
  private onAudioEndCallback: OnAudioEndCallback | null = null;
  private hasFinished: boolean = false;
  private isAudioLoading: boolean = false;
  private loadingAudioId: string | null = null;

  // Cache last known values to prevent reset on modal reopen
  private lastKnownDuration: number = 0;
  private lastKnownPosition: number = 0;

  addUiStateListener(listener: PlayerUiStateListener) {
    this.uiStateListeners.push(listener);

    // Try to get immediate status from sound's internal state
    let durationMs = this.lastKnownDuration;
    let positionMs = this.lastKnownPosition;
    let isPlaying = false;

    if (this.sound) {
      try {
        const soundInternal: any = this.sound;
        const lastStatus = soundInternal._lastStatusUpdate;

        if (lastStatus && lastStatus.isLoaded) {
          durationMs = lastStatus.durationMillis ?? this.lastKnownDuration;
          positionMs = lastStatus.positionMillis ?? this.lastKnownPosition;
          isPlaying = lastStatus.isPlaying ?? false;

          if (__DEV__) {
            console.log(
              "[PlayerEngine] Got sync status from _lastStatusUpdate:",
              {
                duration: durationMs / 1000,
                position: positionMs / 1000,
              }
            );
          }
        }
      } catch (e) {
        // Fallback to cached values
      }
    }

    if (__DEV__) {
      console.log("[PlayerEngine] addUiStateListener - Emit:", {
        duration: durationMs / 1000,
        position: positionMs / 1000,
        cached: {
          duration: this.lastKnownDuration / 1000,
          position: this.lastKnownPosition / 1000,
        },
      });
    }

    // CRITICAL: Only emit sync if we have valid duration
    // Otherwise wait for async to prevent emitting duration = 0
    if (durationMs > 0 || !this.sound) {
      const initialStatus: PlayerStatus = {
        isLoaded: this.sound !== null,
        isPlaying: isPlaying,
        positionMs: positionMs,
        durationMs: durationMs,
      };
      this.emitUiState(initialStatus);
    } else if (__DEV__) {
      console.log(
        "[PlayerEngine] Skipping sync emit - no duration, waiting for async"
      );
    }

    // Then refresh async to ensure accuracy (or emit if we skipped above)
    if (this.sound) {
      this.sound.getStatusAsync().then((status) => {
        if (status.isLoaded) {
          this.emit(status);
        }
      });
    }

    return () => {
      this.uiStateListeners = this.uiStateListeners.filter(
        (l) => l !== listener
      );
    };
  }

  private emitUiState(status: PlayerStatus) {
    // CRITICAL: Always use currentTrack from instance, never from status
    // This ensures currentAudio is preserved even when sound is paused/unloaded
    const uiState: PlayerUiState = {
      isPlaying: status.isPlaying,
      buffering: this.buffering || (status.isBuffering ?? false),
      seeking: this.seeking,
      currentTime: status.positionMs / 1000, // convert to seconds
      duration: status.durationMs / 1000, // convert to seconds
      currentAudio: this.currentTrack
        ? {
            id: this.currentTrack.id,
            name: this.currentTrack.title ?? "Unknown",
            image: this.currentTrack.artwork ?? undefined,
            podcasterName: this.currentTrack.artist ?? undefined,
          }
        : null,
      sourceType: this.sourceType,
      volume: this.volume,
      listenSession: this.listenSession,
      listenSessionProcedure: this.listenSessionProcedure,
      isAutoPlay: this.isAutoPlay,
      isAudioLoading: this.isAudioLoading,
      loadingAudioId: this.loadingAudioId,
    };

    this.uiStateListeners.forEach((listener) => {
      try {
        listener(uiState);
      } catch (e) {
        console.warn("Error in UI state listener", e);
      }
    });
  }

  setSourceType(type: SourceType | null) {
    this.sourceType = type;
  }

  setSeeking(seeking: boolean) {
    this.seeking = seeking;
  }

  setBuffering(buffering: boolean) {
    this.buffering = buffering;
  }

  setListener(listener: PlayerEngineListener | null) {
    this.listener = listener;
    if (this.sound) {
      this.sound.setOnPlaybackStatusUpdate(
        listener ? (st) => this.handleStatus(st) : null
      );
    }
  }

  getCurrentTrack() {
    return this.currentTrack;
  }

  private emit(status: AVPlaybackStatus) {
    const st: any = status;

    const playerStatus: PlayerStatus = {
      isLoaded: status.isLoaded ?? false,
      isPlaying: status.isLoaded ? status.isPlaying ?? false : false,
      positionMs: status.isLoaded
        ? status.positionMillis ?? 0
        : this.lastKnownPosition,
      durationMs: status.isLoaded
        ? status.durationMillis ?? 0
        : this.lastKnownDuration,
      bufferedMs: status.isLoaded
        ? status.playableDurationMillis ?? undefined
        : undefined,
      didJustFinish: status.isLoaded ? status.didJustFinish ?? false : false,
      isBuffering: status.isLoaded ? st.isBuffering ?? false : false,
    };

    // Update cache when loaded
    if (status.isLoaded) {
      if (status.durationMillis) {
        this.lastKnownDuration = status.durationMillis;
        if (__DEV__) {
          console.log(
            "[PlayerEngine] Cache updated - duration:",
            status.durationMillis / 1000,
            "s"
          );
        }
      }
      if (status.positionMillis !== undefined)
        this.lastKnownPosition = status.positionMillis;
    }

    // Always emit to UI state listeners - even when not loaded to keep currentAudio
    this.emitUiState(playerStatus);

    // Also emit to legacy listener if exists
    if (this.listener) {
      this.listener(playerStatus);
    }
  }

  private handleStatus(status: AVPlaybackStatus) {
    // Check if audio just finished
    if (status.isLoaded && status.didJustFinish) {
      console.log("🎵 Audio didJustFinish detected!");
      console.log("🎵 hasFinished:", this.hasFinished);
      console.log("🎵 onAudioEndCallback:", this.onAudioEndCallback !== null);

      if (!this.hasFinished && this.onAudioEndCallback) {
        console.log("🎵 Audio ended - triggering callback");
        this.hasFinished = true;
        this.onAudioEndCallback();
      }
    }

    // Always emit status, even if not loaded, to preserve currentAudio in UI
    this.emit(status);
  }

  async loadAndPlay(
    track: PlayerTrack,
    listenSession: ListenSession,
    listenSessionProcedure: ListenSessionProcedure,
    isSeekThenPlay: boolean,
    seekToSeconds?: number,
    isAutoPlay?: boolean
  ) {
    try {
      if (this.sound) {
        await this.sound.unloadAsync();
        this.sound.setOnPlaybackStatusUpdate(null);
        this.sound = null;
      }

      this.currentTrack = track;
      this.listenSession = listenSession;
      this.listenSessionProcedure = listenSessionProcedure;
      this.isAutoPlay = isAutoPlay ?? false;
      this.hasFinished = false; // Reset finish flag for new track

      // Reset cache for new track
      this.lastKnownDuration = 0;
      this.lastKnownPosition = 0;

      const sound = new Audio.Sound();

      // Load without auto-play if we need to seek first, OR if isSeekThenPlay is false
      const shouldAutoPlay =
        (seekToSeconds === undefined || seekToSeconds === 0) && isSeekThenPlay;

      await sound.loadAsync({ uri: track.url }, { shouldPlay: shouldAutoPlay });

      // Always set callback to ensure real-time updates
      sound.setOnPlaybackStatusUpdate((st) => this.handleStatus(st));

      this.sound = sound;

      // If seekTo is specified and > 0, seek first then play based on isSeekThenPlay
      if (seekToSeconds !== undefined && seekToSeconds > 0) {
        this.setSeeking(true);
        await sound.setPositionAsync(seekToSeconds * 1000); // convert to ms
        this.setSeeking(false);

        // After seeking, play or pause based on isSeekThenPlay
        if (isSeekThenPlay) {
          await sound.playAsync();
        } else {
          await sound.pauseAsync();
        }

        // Force emit status after seek operation
        const status = await sound.getStatusAsync();
        if (status.isLoaded) {
          this.emit(status);
        }
      }
    } catch (e) {
      console.warn("playerEngine.loadAndPlay error", e);
      this.setSeeking(false);
    }
  }

  async play() {
    if (!this.sound) return;
    try {
      await this.sound.playAsync();
      // Force emit UI state after play to ensure immediate UI update
      const status = await this.sound.getStatusAsync();
      if (status.isLoaded) {
        this.emit(status);
      }
    } catch (e) {
      console.warn("playerEngine.play error", e);
    }
  }

  async pause() {
    if (!this.sound) return;
    try {
      await this.sound.pauseAsync();
      // Force emit UI state after pause to ensure immediate UI update
      const status = await this.sound.getStatusAsync();
      if (status.isLoaded) {
        this.emit(status);
      }
    } catch (e) {
      console.warn("playerEngine.pause error", e);
    }
  }

  async togglePlayPause() {
    if (!this.sound) return;
    try {
      const status = await this.sound.getStatusAsync();
      if (!status.isLoaded) return;
      if (status.isPlaying) {
        await this.pause();
      } else {
        await this.play();
      }
    } catch (e) {
      console.warn("playerEngine.togglePlayPause error", e);
    }
  }

  async seekTo(positionMs: number) {
    if (!this.sound) return;
    try {
      this.setSeeking(true);
      await this.sound.setPositionAsync(positionMs);
      // Wait a bit for seek to settle
      setTimeout(() => this.setSeeking(false), 100);
    } catch (e) {
      console.warn("playerEngine.seekTo error", e);
      this.setSeeking(false);
    }
  }

  async setVolume(volume0to1: number) {
    if (!this.sound) return;
    try {
      this.volume = volume0to1;
      await this.sound.setVolumeAsync(volume0to1);
    } catch (e) {
      console.warn("playerEngine.setVolume error", e);
    }
  }

  setAutoPlay(isAutoPlay: boolean) {
    this.isAutoPlay = isAutoPlay;
    // Emit updated state to all listeners
    if (this.sound) {
      this.sound.getStatusAsync().then((status) => {
        if (status.isLoaded) {
          this.emit(status);
        }
      });
    }
  }

  setLoadingState(isLoading: boolean, audioId: string | null = null) {
    this.isAudioLoading = isLoading;
    this.loadingAudioId = audioId;
    // Force emit to update UI immediately
    const status: PlayerStatus = {
      isLoaded: this.sound !== null,
      isPlaying: false,
      positionMs: this.lastKnownPosition,
      durationMs: this.lastKnownDuration,
    };
    this.emitUiState(status);
  }

  getLoadingState() {
    return {
      isAudioLoading: this.isAudioLoading,
      loadingAudioId: this.loadingAudioId,
    };
  }

  setOnAudioEndCallback(callback: OnAudioEndCallback | null) {
    this.onAudioEndCallback = callback;
  }

  getState(): PlayerUiState {
    // Try to get current status from sound's internal state
    let durationSeconds = this.lastKnownDuration / 1000;
    let currentTimeSeconds = this.lastKnownPosition / 1000;
    let isPlaying = false;

    if (this.sound) {
      try {
        const soundInternal: any = this.sound;
        const lastStatus = soundInternal._lastStatusUpdate;

        if (lastStatus && lastStatus.isLoaded) {
          durationSeconds =
            (lastStatus.durationMillis ?? this.lastKnownDuration) / 1000;
          currentTimeSeconds =
            (lastStatus.positionMillis ?? this.lastKnownPosition) / 1000;
          isPlaying = lastStatus.isPlaying ?? false;
        }
      } catch (e) {
        // Fallback to cached values already set above
      }
    }

    return {
      isPlaying: isPlaying,
      buffering: this.buffering,
      listenSession: this.listenSession,
      listenSessionProcedure: this.listenSessionProcedure,
      seeking: this.seeking,
      currentTime: currentTimeSeconds,
      duration: durationSeconds,
      currentAudio: this.currentTrack
        ? {
            id: this.currentTrack.id,
            name: this.currentTrack.title ?? "Unknown",
            image: this.currentTrack.artwork ?? undefined,
            podcasterName: this.currentTrack.artist ?? undefined,
          }
        : null,
      sourceType: this.sourceType,
      volume: this.volume,
      isAutoPlay: this.isAutoPlay,
      isAudioLoading: this.isAudioLoading,
      loadingAudioId: this.loadingAudioId,
    };
  }

  async stopAndUnload() {
    if (!this.sound) {
      // Even if no sound, clear state and emit
      this.currentTrack = null;
      this.sourceType = null;
      this.seeking = false;
      this.buffering = false;
      this.listenSession = null;
      this.listenSessionProcedure = null;
      this.isAutoPlay = false;
      this.hasFinished = false;
      this.lastKnownDuration = 0;
      this.lastKnownPosition = 0;
      this.isAudioLoading = false;
      this.loadingAudioId = null;

      // Emit cleared state to UI
      this.emitUiState({
        isLoaded: false,
        isPlaying: false,
        positionMs: 0,
        durationMs: 0,
      });
      return;
    }

    try {
      await this.sound.stopAsync();
      await this.sound.unloadAsync();
      this.sound.setOnPlaybackStatusUpdate(null);
    } catch (e) {
      console.warn("playerEngine.stopAndUnload error", e);
    } finally {
      this.sound = null;
      this.currentTrack = null;
      this.sourceType = null;
      this.seeking = false;
      this.buffering = false;
      this.listenSession = null;
      this.listenSessionProcedure = null;
      this.isAutoPlay = false;
      this.hasFinished = false;
      this.lastKnownDuration = 0;
      this.lastKnownPosition = 0;
      this.isAudioLoading = false;
      this.loadingAudioId = null;

      // Emit cleared state to UI
      this.emitUiState({
        isLoaded: false,
        isPlaying: false,
        positionMs: 0,
        durationMs: 0,
      });
    }
  }
}

export const playerEngine = new PlayerEngine();

// src/core/player/playerEngine.ts
import { Audio, AVPlaybackStatus } from "expo-av";

export type PlayerTrack = {
  id: string;
  url: string;
  title?: string;
  artist?: string;
  artwork?: string | null;
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

class PlayerEngine {
  private sound: Audio.Sound | null = null;
  private currentTrack: PlayerTrack | null = null;
  private listener: PlayerEngineListener | null = null;

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
    if (!this.listener || !status.isLoaded) return;

    const st: any = status;

    this.listener({
      isLoaded: status.isLoaded,
      isPlaying: status.isPlaying ?? false,
      positionMs: status.positionMillis ?? 0,
      durationMs: status.durationMillis ?? 0,
      bufferedMs: status.playableDurationMillis ?? undefined,
      didJustFinish: status.didJustFinish ?? false,
      isBuffering: st.isBuffering ?? false,
    });
  }

  private handleStatus(status: AVPlaybackStatus) {
    if (!status.isLoaded) {
      // có thể log error nếu status.type === "error"
      return;
    }
    this.emit(status);
  }

  async loadAndPlay(track: PlayerTrack) {
    try {
      if (this.sound) {
        await this.sound.unloadAsync();
        this.sound.setOnPlaybackStatusUpdate(null);
        this.sound = null;
      }

      this.currentTrack = track;

      const sound = new Audio.Sound();
      await sound.loadAsync(
        { uri: track.url },
        { shouldPlay: true } // auto play khi load
      );

      sound.setOnPlaybackStatusUpdate(
        this.listener ? (st) => this.handleStatus(st) : null
      );

      this.sound = sound;
    } catch (e) {
      console.warn("playerEngine.loadAndPlay error", e);
    }
  }

  async play() {
    if (!this.sound) return;
    try {
      await this.sound.playAsync();
    } catch (e) {
      console.warn("playerEngine.play error", e);
    }
  }

  async pause() {
    if (!this.sound) return;
    try {
      await this.sound.pauseAsync();
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
      await this.sound.setPositionAsync(positionMs);
    } catch (e) {
      console.warn("playerEngine.seekTo error", e);
    }
  }

  async setVolume(volume0to1: number) {
    if (!this.sound) return;
    try {
      await this.sound.setVolumeAsync(volume0to1);
    } catch (e) {
      console.warn("playerEngine.setVolume error", e);
    }
  }

  async stopAndUnload() {
    if (!this.sound) return;
    try {
      await this.sound.stopAsync();
      await this.sound.unloadAsync();
      this.sound.setOnPlaybackStatusUpdate(null);
    } catch (e) {
      console.warn("playerEngine.stopAndUnload error", e);
    } finally {
      this.sound = null;
      this.currentTrack = null;
    }
  }
}

export const playerEngine = new PlayerEngine();

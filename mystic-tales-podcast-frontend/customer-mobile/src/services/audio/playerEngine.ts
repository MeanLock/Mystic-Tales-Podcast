// src/services/audio/playerEngine.ts
import { Audio, AVPlaybackStatusSuccess } from "expo-av";
import type { CurrentAudioType } from "@/src/features/mediaPlayer/playerSlice";
import { audioMap } from "./audioMap";

class PlayerEngine {
  private sound: Audio.Sound | null = null;
  private currentId: string | null = null;

  onStatus?: (s: AVPlaybackStatusSuccess) => void;

  private async unload() {
    if (this.sound) {
      try {
        await this.sound.unloadAsync();
      } catch {}
      this.sound = null;
      this.currentId = null;
    }
  }

  private getSource(key: string) {
    const mod = audioMap[key];
    if (!mod) throw new Error(`Audio key not found: ${key}`);
    return mod;
  }

  async playCurrent(audio: NonNullable<CurrentAudioType>) {
    console.log("Playing");
    if (!audio) {
      console.log("Không có audio");
      return;
    }
    const key = audio.MainFileKey;
    // Nếu đang phát cùng bài thì chỉ resume
    if (this.currentId === audio.Id && this.sound) {
      const status = await this.sound.getStatusAsync();
      if ((status as AVPlaybackStatusSuccess).isLoaded) {
        await this.sound.playAsync();
        return;
      }
    }
    // Bài khác → unload và load mới
    await this.unload();
    const source = this.getSource(key);
    const { sound } = await Audio.Sound.createAsync(
      source,
      {
        shouldPlay: true,
        positionMillis: (audio.LatestPosition ?? 0) * 1000,
        progressUpdateIntervalMillis: 250,
        volume: 1.0,
      },
      (status) => {
        if ("isLoaded" in status && status.isLoaded && this.onStatus) {
          this.onStatus(status);
        }
      }
    );
    this.sound = sound;
    this.currentId = audio.Id;
  }

  async pause() {
    if (!this.sound) return;
    const s = await this.sound.getStatusAsync();
    if ("isLoaded" in s && s.isLoaded && s.isPlaying) {
      await this.sound.pauseAsync();
    }
  }

  async stop() {
    await this.unload();
  }

  async seek(seconds: number) {
    if (!this.sound) return;
    const s = await this.sound.getStatusAsync();
    if ("isLoaded" in s && s.isLoaded) {
      await this.sound.setPositionAsync(seconds * 1000);
    }
  }

  async setVolume(vol01: number) {
    if (!this.sound) return;
    await this.sound.setVolumeAsync(Math.max(0, Math.min(1, vol01)));
  }
}

export const playerEngine = new PlayerEngine();

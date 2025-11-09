// src/services/audio/playerEngine.ts
import { Audio, AVPlaybackStatusSuccess } from "expo-av";
import type { CurrentAudioType } from "@/src/features/mediaPlayer/playerSlice";
import { audioMap } from "./audioMap";

class PlayerEngine {
  private sound: Audio.Sound | null = null;
  private currentId: string | null = null;
  private preloaded: { id: string; sound: Audio.Sound } | null = null;
  private seekSeq = 0; // tăng dần
  private lastAppliedSeq = 0;

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

  /** ✅ Preload KHÔNG gắn callback để tránh spam status */
  async preload(id: string, mainFileKey: string) {
    try {
      if (this.preloaded?.id === id) return;
      if (this.preloaded?.sound) {
        try {
          await this.preloaded.sound.unloadAsync();
        } catch {}
      }
      this.preloaded = null;

      const source = this.getSource(mainFileKey);
      const { sound } = await Audio.Sound.createAsync(
        source,
        { shouldPlay: false, progressUpdateIntervalMillis: 250, volume: 1.0 }
        /* no status listener here */
      );
      this.preloaded = { id, sound };
    } catch {
      /* ignore preload errors */
    }
  }

  /** Khi play từ preload, gắn callback TẠI ĐÂY */
  async playCurrent(audio: NonNullable<CurrentAudioType>) {
    if (!audio) return;

    // Resume cùng bài
    if (this.currentId === audio.Id && this.sound) {
      const st = await this.sound.getStatusAsync();
      if ((st as AVPlaybackStatusSuccess).isLoaded) {
        await this.sound.playAsync();
        return;
      }
    }

    // Dùng bản preload nếu có
    if (this.preloaded && this.preloaded.id === audio.Id) {
      console.log(
        "[engine] playCurrent",
        audio?.Name,
        "via",
        this.preloaded ? "preloaded" : "fresh"
      );

      await this.unload();
      this.sound = this.preloaded.sound;
      this.currentId = audio.Id;
      this.preloaded = null;

      // 👉 gắn callback cho bản đang phát
      this.sound.setOnPlaybackStatusUpdate?.((status) => {
        if ("isLoaded" in status && status.isLoaded && this.onStatus) {
          this.onStatus(status);
        }
      });

      try {
        await this.sound.setPositionAsync((audio.LatestPosition ?? 0) * 1000);
      } catch {}
      await this.sound.playAsync();
      return;
    }

    // Load thường
    await this.unload();
    const source = this.getSource(audio.MainFileKey);
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

  private getSource(key: string) {
    const mod = audioMap[key];
    if (!mod) throw new Error(`Audio key not found: ${key}`);

    // If the map value is a string, treat it as a remote uri for Expo Audio
    if (typeof mod === "string") {
      return { uri: mod };
    }

    // Otherwise assume it's a local require(...) (number) or already valid source
    return mod as any;
  }

  // async playCurrent(audio: NonNullable<CurrentAudioType>) {
  //   console.log("Playing");
  //   if (!audio) {
  //     console.log("Không có audio");
  //     return;
  //   }
  //   const key = audio.MainFileKey;
  //   // Nếu đang phát cùng bài thì chỉ resume
  //   if (this.currentId === audio.Id && this.sound) {
  //     const status = await this.sound.getStatusAsync();
  //     if ((status as AVPlaybackStatusSuccess).isLoaded) {
  //       await this.sound.playAsync();
  //       return;
  //     }
  //   }

  //   // Bài khác → unload và load mới
  //   await this.unload();
  //   const source = this.getSource(key);
  //   const { sound } = await Audio.Sound.createAsync(
  //     source,
  //     {
  //       shouldPlay: true,
  //       positionMillis: (audio.LatestPosition ?? 0) * 1000,
  //       progressUpdateIntervalMillis: 250,
  //       volume: 1.0,
  //     },
  //     (status) => {
  //       if ("isLoaded" in status && status.isLoaded && this.onStatus) {
  //         this.onStatus(status);
  //       }
  //     }
  //   );
  //   this.sound = sound;
  //   this.currentId = audio.Id;
  // }

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

    // Tạo seq cho lần seek này
    const mySeq = ++this.seekSeq;

    try {
      const s = await this.sound.getStatusAsync();
      if (!("isLoaded" in s) || !s.isLoaded) return;

      // Nếu có nhiều lệnh dồn, chỉ giữ lệnh cuối: lệnh cũ tự chạy nhưng seq nhỏ hơn sẽ bị bỏ qua kết quả
      await this.sound.setPositionAsync(seconds * 1000);
      this.lastAppliedSeq = Math.max(this.lastAppliedSeq, mySeq);
    } catch (e: any) {
      // Expo thường ném "Error: Seeking interrupted." khi có seek mới đến trước khi seek cũ xong
      const msg = String(e?.message || e);
      if (msg.includes("Seeking interrupted")) {
        // nuốt lỗi này cho sạch log
        return;
      }
      console.error(e);
    }
  }

  async setVolume(vol01: number) {
    if (!this.sound) return;
    await this.sound.setVolumeAsync(Math.max(0, Math.min(1, vol01)));
  }
}

export const playerEngine = new PlayerEngine();

import Hls from "hls.js";

type EngineListeners = {
  timeupdate?: (t: number) => void;
  duration?: (d: number) => void;
  ended?: () => void;
  canplay?: () => void;
};

class AudioEngine {
  private audio: HTMLAudioElement;
  private hls?: Hls;
  private listeners: EngineListeners = {};

  constructor() {
    this.audio = new Audio();
    this.audio.preload = "auto";
    this.audio.crossOrigin = "anonymous";

    this.audio.addEventListener("timeupdate", () => {
      this.listeners.timeupdate?.(this.audio.currentTime || 0);
    });
    this.audio.addEventListener("loadedmetadata", () => {
      this.listeners.duration?.(this.audio.duration || 0);
    });
    this.audio.addEventListener("canplay", () => {
      this.listeners.canplay?.();
    });
    this.audio.addEventListener("ended", () => {
      this.listeners.ended?.();
    });
  }

  attachListeners(l: EngineListeners) {
    this.listeners = l;
  }

  detachListeners() {
    this.listeners = {};
  }

  /**
   * Load source: supports HLS (.m3u8) via hls.js, else native <audio>
   */
  async load(src: string) {
    // cleanup previous Hls
    if (this.hls) {
      this.hls.destroy();
      this.hls = undefined;
    }

    const isHls = /\.m3u8($|\?)/i.test(src);

    if (isHls) {
      if (Hls.isSupported()) {
        this.hls = new Hls({
          // optional: tinh chỉnh buffer nếu cần
          // maxBufferLength: 30,
        });
        this.hls.loadSource(src);
        this.hls.attachMedia(this.audio);
      } else if (this.audio.canPlayType("application/vnd.apple.mpegurl")) {
        // Safari iOS/macOS
        this.audio.src = src;
      } else {
        throw new Error("HLS not supported in this browser");
      }
    } else {
      this.audio.src = src;
    }
  }

  play() {
    return this.audio.play();
  }
  pause() {
    this.audio.pause();
  }
  setVolume(v01: number) {
    this.audio.volume = Math.min(1, Math.max(0, v01));
  }
  seek(seconds: number) {
    this.audio.currentTime = Math.max(0, seconds);
  }
  getCurrentTime() {
    return this.audio.currentTime || 0;
  }
  getDuration() {
    return this.audio.duration || 0;
  }
}

let _engine: AudioEngine | null = null;
export function getAudioEngine() {
  if (!_engine) _engine = new AudioEngine();
  return _engine;
}

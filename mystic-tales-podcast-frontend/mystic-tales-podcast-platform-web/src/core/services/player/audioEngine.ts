import { BASE_URL } from "@/core/api/appApi";
import { getAccessToken } from "@/core/api/appApi/token";
import Hls from "hls.js";

type EngineListeners = {
  timeupdate?: (t: number) => void;
  duration?: (d: number) => void;
  ended?: () => void;
  canplay?: () => void;
};

// HLS Task update
type MysticHlsOptions = {
  /** Absolute .m3u8 url */
  playlistUrl: string;
  /** Episode Id hiện đang phát */
  episodeId: string;
  /** Token trả về từ /listen (hết hạn sau ~5 phút, không refresh) */
  token: string;
};

class AudioEngine {
  private audio: HTMLAudioElement;
  private hls?: Hls;
  private listeners: EngineListeners = {};

  // HLS Task update
  /** Track đang được load trong engine (để phân biệt resume vs load mới) */
  private currentTrackId: string | null = null;

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

  // HLS Task update
  /** Dọn Hls instance cũ */
  private destroyHls() {
    if (this.hls) {
      try {
        this.hls.destroy();
      } catch {}
      this.hls = undefined;
    }
  }

  /**
   * Load source: supports HLS (.m3u8) via hls.js, else native <audio>
   */
  /**
   * Load nguồn audio thường (non-HLS), hoặc Safari native HLS
   */
  async load(src: string, trackId?: string) {
    this.destroyHls();

    const isHls = /\.m3u8($|\?)/i.test(src);
    if (isHls) {
      if (Hls.isSupported()) {
        this.hls = new Hls();
        this.hls.loadSource(src);
        this.hls.attachMedia(this.audio);
      } else if (this.audio.canPlayType("application/vnd.apple.mpegurl")) {
        this.audio.src = src;
      } else {
        throw new Error("HLS not supported in this browser");
      }
    } else {
      this.audio.src = src;
    }

    this.currentTrackId = trackId ?? null;
  }

  /**
   * Load HLS với cơ chế xhrSetup như code mẫu (rewrite key/segment request)
   * Dùng cho flow Mystic Podcast.
   */
  async loadMysticHls(opts: MysticHlsOptions) {
    const { playlistUrl, episodeId, token } = opts;

    this.destroyHls();

    const keyParamToken = token; // token /listen -> gắn vào query cho key

    if (!keyParamToken) {
      alert("Ê");
    }
    if (Hls.isSupported()) {
      // -------------------------------------------
      const h = new Hls({
        enableWorker: true,
        maxBufferLength: 120, // buffer ~2 segment (60s)
        maxBufferHole: 0.5,
        maxMaxBufferLength: 300,
        maxBufferSize: 60 * 1000 * 1000,
        // XHR fallback (don’t call xhr.open() yourself)
        xhrSetup: (xhr: any, url: string) => {
          const originalOpen = xhr.open.bind(xhr);
          xhr.open = (method: string, u: string, async?: boolean) => {
            let next = u;
            try {
              // Resolve the request URL (u) relative to the manifest URL (url) so
              // relative URIs work as expected.
              let resolvedHref = u;
              try {
                // absolute URL
                new URL(u);
                resolvedHref = u;
              } catch {
                try {
                  const base = new URL(url);
                  resolvedHref = new URL(u, base).href;
                } catch {
                  resolvedHref = u;
                }
              }

              // Extract pathname for GUID detection
              let pathname = resolvedHref;
              try {
                pathname = new URL(resolvedHref).pathname;
              } catch {}

              // 1) KEY detection: find GUID segment anywhere in pathname
              const guidMatch = pathname.match(/([0-9a-fA-F-]{36})/);
              if (guidMatch) {
                const kid = guidMatch[1];
                const keyUrl = new URL(
                  `${BASE_URL}/api/podcast-service/api/episodes/${episodeId}/hls-encryption-key/${kid}`
                );
                if (keyParamToken)
                  keyUrl.searchParams.set("token", keyParamToken);
                next = keyUrl.href;
                console.debug("HLS xhrSetup -> KEY mapped", {
                  method,
                  u,
                  resolvedHref,
                  pathname,
                  kid,
                  next,
                });
              }
              // 2) SEGMENT mapping
              else if (resolvedHref.includes(".ts")) {
                const idx = resolvedHref.lastIndexOf(
                  "main_files/PodcastEpisodes/"
                );
                const fileKey = idx !== -1 ? resolvedHref.substring(idx) : null;
                console.debug("HLS xhrSetup -> SEGMENT candidate", {
                  method,
                  u,
                  resolvedHref,
                  fileKey,
                });
                if (fileKey) {
                  next = `${BASE_URL}/api/podcast-service/api/episodes/hls-segment/get-file-data/${fileKey}`;
                  console.debug("HLS xhrSetup -> SEGMENT mapped", { next });
                }
              }
            } catch (err) {
              next = u;
            }

            console.debug("HLS xhrSetup -> originalOpen", {
              method,
              u,
              next,
              async,
            });
            return originalOpen(method, next, async);
          };

          xhr.withCredentials = true;
          try {
            const accessToken =
              "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEwMTIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiVsWpIFRo4buLIEYiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJ2dXRoaWZAZW1haWwuY29tIiwiaWQiOiIxMDEyIiwicm9sZV9pZCI6IjEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDdXN0b21lciIsImJhbGFuY2UiOiIwLjAwIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9zZXJpYWxudW1iZXIiOiJhZDI0NGMwMy04ZWEyLTQ5NDUtOTZhNy1hZmRjZDIxZDE3ZTciLCJleHAiOjc3NjEzODUzOTUsImlzcyI6ImxvY2FsaG9zdCIsImF1ZCI6ImxvY2FsaG9zdCJ9.hKMmv2Ax0EhF1pwAw5LLLdv3YKXb-LaoVMIBu-hYUBhoJjoLHt0LdCa-yfxeHlF-0Kxuc6WG8VWci_oO9eEU8ySOAca9EsFF4LARqf1xXqwA295ync6TBrWMpM1Wjf5UCw_GxGq9iSfESYn9aMfxlEdvj33CRI39xD-xhaqgg_JpCk1BV1Iz0pYjEW_Jibo9Qm6VfxVadciZxfGa89h6YVEOOGTlaGWwCKYk0HuK5ygXKpcGGGjtswH-dhcwoBUl1XHK_g9czryS-tiHlTTPV6lPd1m7IWq4VhbIrtV_Qaz2OuGTT3NjOg8ARlgzNo5qWfNC2cIe-KBAaIENHTReiQ";
            if (accessToken)
              xhr.setRequestHeader("Authorization", `Bearer ${accessToken}`);
          } catch {}
          xhr.setRequestHeader("ngrok-skip-browser-warning", "69420");
        },
      });

      h.loadSource(`${playlistUrl}?t=${Date.now()}`);
      h.attachMedia(this.audio);
      this.hls = h;
    } else if (this.audio.canPlayType("application/vnd.apple.mpegurl")) {
      // Safari fallback (không can thiệp được xhrSetup)
      this.audio.src = playlistUrl;
    } else {
      throw new Error("HLS not supported in this browser");
    }

    this.currentTrackId = episodeId;
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

  // HLS Task update
  /** Cho biết engine đang giữ track nào để phân biệt resume vs load mới */
  getCurrentTrackId() {
    return this.currentTrackId;
  }
}

let _engine: AudioEngine | null = null;
export function getAudioEngine() {
  if (!_engine) _engine = new AudioEngine();
  return _engine;
}

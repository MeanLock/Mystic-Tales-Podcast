// hlsEpisodeLoader.ts
import Hls from "hls.js";
import type { HlsLoadBaseOptions } from "@/core/types/hls";

type EpisodeHlsOptions = HlsLoadBaseOptions & {
  episodeId: string;
  token: string; // Token /listen
};

// alo
/**
 * Đảm bảo baseUrl luôn có "/" ở cuối
 */
function normalizeBaseUrl(url: string): string {
  return url.endsWith("/") ? url : `${url}/`;
}

/**
 * Load HLS cho EPISODE:
 * - Tạo Hls instance (hoặc dùng native HLS của Safari)
 * - Map key/segment theo episode API
 * - Sau khi audio sẵn sàng:
 *    + Seek đến seekTo (nếu có)
 *    + Play hoặc pause tùy isSeekThenPlay
 * - Trạng thái seeking/buffering chủ yếu do <audio> events quản lý
 */
export async function loadEpisodeHls(
  opts: EpisodeHlsOptions
): Promise<Hls | null> {
  const {
    audio,
    baseUrl,
    fileKey,
    episodeId,
    token,
    accessToken,
    seekTo,
    isSeekThenPlay = true,
    onBufferingChange,
  } = opts;

  const normalizedBaseUrl = normalizeBaseUrl(baseUrl);
  // Thêm timestamp để bust cache mỗi lần load
  const cacheBuster = Date.now();
  const playlistUrl = `${normalizedBaseUrl}api/podcast-service/api/episodes/hls-playlist/get-file-data/${fileKey}?_t=${cacheBuster}`;

  const performSeekAndPlay = async () => {
    if (typeof seekTo === "number" && seekTo > 0) {
      audio.currentTime = seekTo;
    }

    if (isSeekThenPlay) {
      await audio.play().catch(() => {});
    } else {
      audio.pause();
    }
  };

  // === Safari native HLS ===
  if (
    !Hls.isSupported() &&
    audio.canPlayType("application/vnd.apple.mpegurl")
  ) {
    audio.src = playlistUrl;

    return new Promise((resolve) => {
      const onLoadedMetadata = async () => {
        const doWork = async () => {
          if (audio.readyState >= 2) {
            await performSeekAndPlay();
          } else {
            const onCanPlay = async () => {
              audio.removeEventListener("canplay", onCanPlay);
              await performSeekAndPlay();
            };
            audio.addEventListener("canplay", onCanPlay);
          }
        };

        audio.removeEventListener("loadedmetadata", onLoadedMetadata);
        await doWork();
        // Buffering state sẽ được quản lý bởi audio events (canplay/playing)
        onBufferingChange?.(false);
        resolve(null);
      };

      // Bắt đầu: coi như đang buffering
      onBufferingChange?.(true);
      audio.addEventListener("loadedmetadata", onLoadedMetadata);
    });
  }

  // === Browser dùng Hls.js ===
  const startPosition = typeof seekTo === "number" && seekTo > 0 ? seekTo : -1;

  const h = new Hls({
    enableWorker: true,
    maxBufferLength: 120,
    maxBufferHole: 0.5,
    maxMaxBufferLength: 300,
    maxBufferSize: 60 * 1000 * 1000,
    startPosition,
    xhrSetup: (xhr: any, url: string) => {
      const originalOpen = xhr.open.bind(xhr);
      xhr.open = (method: string, u: string, async?: boolean) => {
        let next = u;

        // alert("Đang gọi lấy key/segment HLS...");
        console.log("Token:", token);
        // KEY: pattern UUID ở cuối
        if (/[0-9a-fA-F-]{36}$/.test(u)) {
          const kid = u.split("/").pop();
          next = `${normalizedBaseUrl}api/podcast-service/api/episodes/${episodeId}/hls-encryption-key/${kid}?token=${token}`;
        }
        // SEGMENT: .ts trong folder PodcastEpisodes
        else if (u.includes(".ts")) {
          const idx = url.lastIndexOf("main_files/PodcastEpisodes/");
          if (idx !== -1) {
            const segmentFileKey = url.substring(idx);
            next = `${normalizedBaseUrl}api/podcast-service/api/episodes/hls-segment/get-file-data/${segmentFileKey}`;
          }
        }

        return originalOpen(method, next, async);
      };

      xhr.withCredentials = true;
      try {
        if (accessToken) {
          xhr.setRequestHeader("Authorization", `Bearer ${accessToken}`);
        }
      } catch {}
      xhr.setRequestHeader("ngrok-skip-browser-warning", "69420");
    },
  });

  return new Promise<Hls>((resolve, reject) => {
    let resolved = false;
    let retryCount = 0;
    const MAX_RETRIES = 1;

    const finishResolve = () => {
      if (!resolved) {
        resolved = true;
        resolve(h);
      }
    };

    const onLoadedMetadata = async () => {
      audio.removeEventListener("loadedmetadata", onLoadedMetadata);

      const doWork = async () => {
        if (audio.readyState >= 2) {
          await performSeekAndPlay();
        } else {
          const onCanPlay = async () => {
            audio.removeEventListener("canplay", onCanPlay);
            await performSeekAndPlay();
          };
          audio.addEventListener("canplay", onCanPlay);
        }
      };

      await doWork();
      onBufferingChange?.(false);
      finishResolve();
    };

    // Khi bắt đầu load HLS: bật buffering
    onBufferingChange?.(true);
    audio.addEventListener("loadedmetadata", onLoadedMetadata);

    h.on(Hls.Events.ERROR, (_, data) => {
      console.error("[HLS EPISODE] Error:", data);

      // Nếu bị stall do thiếu buffer => đang buffering
      if (data.details === Hls.ErrorDetails.BUFFER_STALLED_ERROR) {
        onBufferingChange?.(true);
      }

      if (data.fatal) {
        // Fatal: tắt spinner hiện tại, có thể implement UI error ngoài này
        onBufferingChange?.(false);

        if (retryCount < MAX_RETRIES) {
          retryCount++;
          console.warn(
            `[HLS EPISODE] Retrying... (${retryCount}/${MAX_RETRIES})`
          );

          // Exponential backoff: 1s, 2s, 3s
          setTimeout(() => {
            // Tạo URL mới với timestamp mới cho retry
            const retryUrl = playlistUrl.replace(
              /[?&]_t=\d+/,
              `&_t=${Date.now()}`
            );
            h.loadSource(retryUrl);
            h.startLoad();
          }, 1000 * retryCount);
        } else {
          console.error(
            `[HLS EPISODE] Max retries (${MAX_RETRIES}) reached. Stopping.`
          );
          if (!resolved) {
            resolved = true;
            reject(
              new Error(`Failed to load HLS after ${MAX_RETRIES} retries`)
            );
          }
        }
      }
    });

    h.attachMedia(audio);
    h.loadSource(playlistUrl);
  });
}

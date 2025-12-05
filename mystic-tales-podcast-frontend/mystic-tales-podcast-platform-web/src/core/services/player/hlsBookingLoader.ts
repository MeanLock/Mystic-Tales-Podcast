// // hlsBookingLoader.ts
// import Hls from "hls.js";
// import type { HlsLoadBaseOptions } from "@/core/types/hls";

// type BookingHlsOptions = HlsLoadBaseOptions & {
//   bookingId: number;
//   trackId: string;
// };

// export async function loadBookingHls(
//   opts: BookingHlsOptions
// ): Promise<Hls | null> {
//   const {
//     audio,
//     baseUrl,
//     fileKey,
//     bookingId,
//     trackId,
//     accessToken,
//     seekTo,
//     isSeekThenPlay = true,
//     onBufferingChange,
//     onSeekingChange,
//   } = opts;

//   const playlistUrl = `${baseUrl}api/booking-management-service/api/bookings/${bookingId}/booking-podcast-tracks/hls-playlist/get-file-data/${fileKey}`;

//   if (
//     !Hls.isSupported() &&
//     audio.canPlayType("application/vnd.apple.mpegurl")
//   ) {
//     audio.src = playlistUrl;
//     if (typeof onBufferingChange === "function") {
//       onBufferingChange(true);
//     }

//     return new Promise((resolve) => {
//       const onLoaded = async () => {
//         if (typeof onBufferingChange === "function") {
//           onBufferingChange(false);
//         }

//         // Wait for audio to be ready before seeking
//         const performSeekAndPlay = async () => {
//           if (typeof seekTo === "number" && seekTo > 0) {
//             if (typeof onSeekingChange === "function") {
//               onSeekingChange(true);
//             }
//             audio.currentTime = seekTo;
//             // Wait a bit for seek to settle
//             await new Promise((r) => setTimeout(r, 100));
//             if (typeof onSeekingChange === "function") {
//               onSeekingChange(false);
//             }
//           }

//           if (isSeekThenPlay) {
//             await audio.play().catch(() => {});
//           } else {
//             audio.pause();
//             // Double-check pause for Safari
//             setTimeout(() => {
//               if (!isSeekThenPlay) {
//                 audio.pause();
//               }
//             }, 50);
//           }

//           audio.removeEventListener("loadedmetadata", onLoaded);
//           resolve(null);
//         };

//         // If audio is already ready, perform immediately
//         if (audio.readyState >= 2) {
//           await performSeekAndPlay();
//         } else {
//           // Otherwise wait for canplay event
//           const onCanPlay = async () => {
//             audio.removeEventListener("canplay", onCanPlay);
//             await performSeekAndPlay();
//           };
//           audio.addEventListener("canplay", onCanPlay);
//         }
//       };

//       audio.addEventListener("loadedmetadata", onLoaded);
//     });
//   }

//   const h = new Hls({
//     enableWorker: true,
//     maxBufferLength: 120,
//     maxBufferHole: 0.5,
//     maxMaxBufferLength: 300,
//     maxBufferSize: 60 * 1000 * 1000,
//     xhrSetup: (xhr: any, url: string) => {
//       const originalOpen = xhr.open.bind(xhr);
//       xhr.open = (method: string, u: string, async?: boolean) => {
//         let next = u;

//         if (/[0-9a-fA-F-]{36}$/.test(u)) {
//           const kid = u.split("/").pop();
//           next = `${baseUrl}api/booking-management-service/api/bookings/${bookingId}/booking-podcast-tracks/${trackId}/hls-encryption-key/${kid}`;
//         } else if (u.includes(".ts")) {
//           const idx = url.lastIndexOf("main_files/Bookings/");
//           if (idx !== -1) {
//             const segmentFileKey = url.substring(idx);
//             next = `${baseUrl}api/booking-management-service/api/bookings/${bookingId}/booking-podcast-tracks/hls-segment/get-file-data/${segmentFileKey}`;
//           }
//         }

//         return originalOpen(method, next, async);
//       };

//       xhr.withCredentials = true;
//       try {
//         if (accessToken) {
//           xhr.setRequestHeader("Authorization", `Bearer ${accessToken}`);
//         }
//       } catch {}
//       xhr.setRequestHeader("ngrok-skip-browser-warning", "69420");
//     },
//   });

//   let firstFragHandled = false;

//   h.on(Hls.Events.MANIFEST_PARSED, () => {
//     firstFragHandled = false;
//     if (typeof onBufferingChange === "function") {
//       onBufferingChange(true);
//     }
//   });

//   h.on(Hls.Events.FRAG_LOADED, async () => {
//     if (!firstFragHandled) {
//       firstFragHandled = true;
//       if (typeof onBufferingChange === "function") {
//         onBufferingChange(false);
//       }

//       // Wait for audio to be ready before seeking
//       const performSeekAndPlay = async () => {
//         if (typeof seekTo === "number" && seekTo > 0) {
//           if (typeof onSeekingChange === "function") {
//             onSeekingChange(true);
//           }
//           audio.currentTime = seekTo;
//           // Wait a bit for seek to settle
//           await new Promise((r) => setTimeout(r, 100));
//           if (typeof onSeekingChange === "function") {
//             onSeekingChange(false);
//           }
//         }

//         if (isSeekThenPlay) {
//           await audio.play().catch(() => {});
//         } else {
//           // Ensure audio is paused after seek
//           audio.pause();
//           // Double-check pause after a small delay for browser compatibility
//           setTimeout(() => {
//             if (!isSeekThenPlay) {
//               audio.pause();
//             }
//           }, 50);
//         }
//       };

//       // If audio is already ready, perform immediately
//       if (audio.readyState >= 2) {
//         await performSeekAndPlay();
//       } else {
//         // Otherwise wait for canplay event
//         const onCanPlay = async () => {
//           audio.removeEventListener("canplay", onCanPlay);
//           await performSeekAndPlay();
//         };
//         audio.addEventListener("canplay", onCanPlay);
//       }
//     }
//   });

//   h.on(Hls.Events.ERROR, (_, data) => {
//     console.error("[HLS BOOKING] Error:", data);
//     if (data.fatal && typeof onBufferingChange === "function") {
//       onBufferingChange(false);
//       h.loadSource(`${playlistUrl}?t=${Date.now()}`);
//       h.startLoad();
//     }
//   });

//   h.attachMedia(audio);
//   h.loadSource(`${playlistUrl}?t=${Date.now()}`);

//   return h;
// }

// hlsBookingLoader.ts
import Hls from "hls.js";
import type { HlsLoadBaseOptions } from "@/core/types/hls";

type BookingHlsOptions = HlsLoadBaseOptions & {
  bookingId: number;
  trackId: string;
};

export async function loadBookingHls(
  opts: BookingHlsOptions
): Promise<Hls | null> {
  const {
    audio,
    baseUrl,
    fileKey,
    bookingId,
    trackId,
    accessToken,
    seekTo,
    isSeekThenPlay = true,
    onBufferingChange,
  } = opts;

  const playlistUrl = `${baseUrl}api/booking-management-service/api/bookings/${bookingId}/booking-podcast-tracks/hls-playlist/get-file-data/${fileKey}`;

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
        resolve(null);
      };

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

        if (/[0-9a-fA-F-]{36}$/.test(u)) {
          const kid = u.split("/").pop();
          next = `${baseUrl}api/booking-management-service/api/bookings/${bookingId}/booking-podcast-tracks/${trackId}/hls-encryption-key/${kid}`;
        } else if (u.includes(".ts")) {
          const idx = url.lastIndexOf("main_files/Bookings/");
          if (idx !== -1) {
            const segmentFileKey = url.substring(idx);
            next = `${baseUrl}api/booking-management-service/api/bookings/${bookingId}/booking-podcast-tracks/hls-segment/get-file-data/${segmentFileKey}`;
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

  return new Promise<Hls>((resolve) => {
    let resolved = false;

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

    onBufferingChange?.(true);
    audio.addEventListener("loadedmetadata", onLoadedMetadata);

    h.on(Hls.Events.ERROR, (_, data) => {
      console.error("[HLS BOOKING] Error:", data);

      if (data.details === Hls.ErrorDetails.BUFFER_STALLED_ERROR) {
        onBufferingChange?.(true);
      }

      if (data.fatal) {
        onBufferingChange?.(false);
        h.loadSource(`${playlistUrl}?t=${Date.now()}`);
        h.startLoad();
      }
    });

    h.attachMedia(audio);
    h.loadSource(`${playlistUrl}?t=${Date.now()}`);
  });
}

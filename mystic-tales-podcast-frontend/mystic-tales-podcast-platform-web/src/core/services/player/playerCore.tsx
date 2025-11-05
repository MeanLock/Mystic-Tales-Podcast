import { useEffect, useRef } from "react";
import { useDispatch, useSelector } from "react-redux";
import type { RootState } from "@/redux/store";
import {
  registerPlayerImpl,
  unregisterPlayerImpl,
  getAudioEngine,
} from "./playerBridge";
import axios from "axios";
import {
  pauseAudio,
  nextAudio,
} from "@/redux/slices/mediaPlayerSlice/mediaPlayerSlice";
// player.service hooks are not used here; PlayerCore uses axios to call /listen directly (mirrors Search page)
import { BASE_URL } from "@/core/api/appApi";

export default function PlayerCore() {
  const dispatch = useDispatch();
  const { currentAudio, playMode } = useSelector((s: RootState) => s.player);
  // We'll manage Hls + audio inside PlayerCore and register control functions
  // via playerBridge so UI components can call getAudioEngine().
  const engineRef = useRef<any>(null);
  const hlsRef = useRef<any>(null);
  const audioRef = useRef<HTMLAudioElement | null>(null);
  const listenersRef = useRef<any>({});

  // HLS Task update
  // Attach listeners (time/duration/end)
  useEffect(() => {
    // register bridge impl (no-op until audio created)
    registerPlayerImpl({
      play: async () => {
        try {
          await audioRef.current?.play();
        } catch {}
      },
      pause: () => audioRef.current?.pause(),
      seek: (s: number) => {
        if (audioRef.current) audioRef.current.currentTime = Math.max(0, s);
      },
      setVolume: (v: number) => {
        if (audioRef.current)
          audioRef.current.volume = Math.min(1, Math.max(0, v));
      },
      getCurrentTime: () => audioRef.current?.currentTime ?? 0,
      getDuration: () => audioRef.current?.duration ?? 0,
      attachListeners: (l: any) => {
        listenersRef.current = l || {};
      },
      detachListeners: () => {
        listenersRef.current = {};
      },
      getCurrentTrackId: () => engineRef.current?.currentTrackId ?? null,
    });

    return () => {
      unregisterPlayerImpl();
    };
  }, [dispatch]);

  // HLS Task update
  // =========================
  // CHỈ LOAD KHI NHẤN PLAY
  // =========================

  // RTK Query: kept reference for playlist endpoint if needed in future (unused now)
  // const [triggerGetPlaylist, playlistResult] = useLazyGetPlayListFileQuery();

  // Cache nhẹ: nhớ lại playlistUrl/token cho audio Id đang phát (trong 1 session)
  const lastLoadedIdRef = useRef<string | null>(null);
  const lastPlaylistUrlRef = useRef<string | null>(null);
  const lastListenTokenRef = useRef<string | null>(null);
  const isLoadingRef = useRef<boolean>(false);

  // Khi đổi bài (currentAudio)
  useEffect(() => {
    console.log(
      "PlayerCore.useEffect fired. playStatus=",
      playMode.playStatus,
      "currentAudio=",
      currentAudio
    );
    const engine = getAudioEngine();

    const ensureLoadedThenPlay = async () => {
      if (!currentAudio) return;

      // Nếu engine đã load đúng track này rồi -> resume play
      if (
        engine.getCurrentTrackId() === currentAudio.Id &&
        lastPlaylistUrlRef.current
      ) {
        try {
          await engine.play();
        } catch {}
        return;
      }

      // Nếu đang loading, tránh đúp
      if (isLoadingRef.current) return;

      isLoadingRef.current = true;
      try {
        // 1) /listen -> giống như SearchPage: call via axios to ensure headers/ngrok + withCredentials
        console.log("calling /listen for audioId=", currentAudio.Id);
        const accessToken =
          "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEwMTIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiVsWpIFRo4buLIEYiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJ2dXRoaWZAZW1haWwuY29tIiwiaWQiOiIxMDEyIiwicm9sZV9pZCI6IjEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDdXN0b21lciIsImJhbGFuY2UiOiIwLjAwIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9zZXJpYWxudW1iZXIiOiJhZDI0NGMwMy04ZWEyLTQ5NDUtOTZhNy1hZmRjZDIxZDE3ZTciLCJleHAiOjc3NjEzODUzOTUsImlzcyI6ImxvY2FsaG9zdCIsImF1ZCI6ImxvY2FsaG9zdCJ9.hKMmv2Ax0EhF1pwAw5LLLdv3YKXb-LaoVMIBu-hYUBhoJjoLHt0LdCa-yfxeHlF-0Kxuc6WG8VWci_oO9eEU8ySOAca9EsFF4LARqf1xXqwA295ync6TBrWMpM1Wjf5UCw_GxGq9iSfESYn9aMfxlEdvj33CRI39xD-xhaqgg_JpCk1BV1Iz0pYjEW_Jibo9Qm6VfxVadciZxfGa89h6YVEOOGTlaGWwCKYk0HuK5ygXKpcGGGjtswH-dhcwoBUl1XHK_g9czryS-tiHlTTPV6lPd1m7IWq4VhbIrtV_Qaz2OuGTT3NjOg8ARlgzNo5qWfNC2cIe-KBAaIENHTReiQ";
        const listenResp = await axios.get(
          `${BASE_URL}/api/podcast-service/api/episodes/${currentAudio.Id}/listen`,
          {
            headers: {
              ...(accessToken
                ? { Authorization: `Bearer ${accessToken}` }
                : {}),
              "ngrok-skip-browser-warning": "69420",
            },
            withCredentials: true,
          }
        );
        console.log("listen response:", listenResp);
        const listenRes = listenResp.data;
        const fileKey = listenRes.PlaylistFileKey;
        const token = listenRes.Token ?? null;
        lastListenTokenRef.current = token;

        // 2) Construct playlist URL the same way as the search page flow
        const playlistUrl = `${BASE_URL}/api/podcast-service/api/episodes/hls-playlist/get-file-data/${fileKey}`;
        lastPlaylistUrlRef.current = playlistUrl;

        // 3) Create audio element and Hls instance (mirror SearchPage logic)
        const HARD_CODED_ACCESS_TOKEN =
          "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEwMTIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiVsWpIFRo4buLIEYiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJ2dXRoaWZAZW1haWwuY29tIiwiaWQiOiIxMDEyIiwicm9sZV9pZCI6IjEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDdXN0b21lciIsImJhbGFuY2UiOiIwLjAwIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9zZXJpYWxudW1iZXIiOiJhZDI0NGMwMy04ZWEyLTQ5NDUtOTZhNy1hZmRjZDIxZDE3ZTciLCJleHAiOjc3NjEzODUzOTUsImlzcyI6ImxvY2FsaG9zdCIsImF1ZCI6ImxvY2FsaG9zdCJ9.hKMmv2Ax0EhF1pwAw5LLLdv3YKXb-LaoVMIBu-hYUBhoJjoLHt0LdCa-yfxeHlF-0Kxuc6WG8VWci_oO9eEU8ySOAca9EsFF4LARqf1xXqwA295ync6TBrWMpM1Wjf5UCw_GxGq9iSfESYn9aMfxlEdvj33CRI39xD-xhaqgg_JpCk1BV1Iz0pYjEW_Jibo9Qm6VfxVadciZxfGa89h6YVEOOGTlaGWwCKYk0HuK5ygXKpcGGGjtswH-dhcwoBUl1XHK_g9czryS-tiHlTTPV6lPd1m7IWq4VhbIrtV_Qaz2OuGTT3NjOg8ARlgzNo5qWfNC2cIe-KBAaIENHTReiQ";

        // create audio element if missing
        if (!audioRef.current) {
          const a = document.createElement("audio");
          a.preload = "auto";
          a.crossOrigin = "anonymous";
          audioRef.current = a;

          // attach listeners to propagate to registered consumers
          a.addEventListener("loadedmetadata", () => {
            listenersRef.current.duration?.(a.duration || 0);
          });
          a.addEventListener("timeupdate", () => {
            listenersRef.current.timeupdate?.(a.currentTime || 0);
          });
          a.addEventListener("ended", () => {
            // dispatch next
            dispatch(nextAudio());
            listenersRef.current.ended?.();
          });
        }

        // destroy previous hls
        try {
          if (hlsRef.current) {
            hlsRef.current.destroy();
            hlsRef.current = null;
          }
        } catch {}

        // setup Hls with xhrSetup similar to SearchPage
        const Hls = (await import("hls.js")).default;
        if (Hls.isSupported()) {
          const h = new Hls({
            enableWorker: true,
            maxBufferLength: 120,
            maxBufferHole: 0.5,
            maxMaxBufferLength: 300,
            maxBufferSize: 60 * 1000 * 1000,
            xhrSetup: (xhr: any, url: string) => {
              const originalOpen = xhr.open.bind(xhr);
              xhr.open = (method: string, u: string, async?: boolean) => {
                let next = u;
                if (/[0-9a-fA-F-]{36}$/.test(u)) {
                  const kid = u.split("/").pop();
                  next = `${BASE_URL}/api/podcast-service/api/episodes/${currentAudio.Id}/hls-encryption-key/${kid}?token=${token}`;
                } else if (u.includes(".ts")) {
                  const idx = url.lastIndexOf("main_files/PodcastEpisodes/");
                  const fileKey = idx !== -1 ? url.substring(idx) : null;
                  if (fileKey)
                    next = `${BASE_URL}/api/podcast-service/api/episodes/hls-segment/get-file-data/${fileKey}`;
                }
                return originalOpen(method, next, async);
              };
              xhr.withCredentials = true;
              try {
                if (HARD_CODED_ACCESS_TOKEN)
                  xhr.setRequestHeader(
                    "Authorization",
                    `Bearer ${HARD_CODED_ACCESS_TOKEN}`
                  );
              } catch {}
              xhr.setRequestHeader("ngrok-skip-browser-warning", "69420");
            },
          });

          h.loadSource(`${playlistUrl}?t=${Date.now()}`);
          h.attachMedia(audioRef.current!);
          hlsRef.current = h;
        } else if (
          audioRef.current.canPlayType("application/vnd.apple.mpegurl")
        ) {
          audioRef.current.src = playlistUrl;
        } else {
          throw new Error("HLS not supported in this browser");
        }

        // set current track id for bridge
        engineRef.current = { currentTrackId: currentAudio.Id };

        // 4) play
        try {
          await audioRef.current?.play();
        } catch {}
      } catch (e) {
        console.error("PlayerCore.ensureLoadedThenPlay error:", e);
        // If fail -> pause to reflect UI state
        dispatch(pauseAudio());
      } finally {
        isLoadingRef.current = false;
        lastLoadedIdRef.current = currentAudio.Id;
      }
    };

    if (playMode.playStatus === "play") {
      // YÊU CẦU: chỉ khi nhấn Play mới load source
      ensureLoadedThenPlay();
    } else if (playMode.playStatus === "pause") {
      engine.pause();
    } else if (playMode.playStatus === "stop") {
      engine.pause();
      // Không xoá cache để user có thể resume nhanh nếu muốn,
      // nhưng engine sẽ không tự play nếu không nhấn Play.
    }
  }, [playMode.playStatus, currentAudio, dispatch]);

  // Play/Pause từ Redux
  // useEffect(() => {
  //   const engine = engineRef.current;
  //   if (playMode.playStatus === "play") {
  //     engine.play().catch(() => void 0);
  //   } else if (playMode.playStatus === "pause") {
  //     engine.pause();
  //   } else if (playMode.playStatus === "stop") {
  //     engine.pause();
  //     // có thể clear src nếu muốn
  //   }
  // }, [playMode.playStatus]);

  // Volume (Redux 0..100 -> audio 0..1)
  useEffect(() => {
    const engine = getAudioEngine();

    // volume luôn sync
    engine.setVolume((playMode.volume ?? 100) / 100);
  }, [playMode.volume]);

  return null; // không render UI
}

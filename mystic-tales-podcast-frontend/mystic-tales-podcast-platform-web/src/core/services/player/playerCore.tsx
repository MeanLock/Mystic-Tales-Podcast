import { useEffect, useRef } from "react";
import { useDispatch, useSelector } from "react-redux";
import type { RootState } from "@/redux/store";
import {
  registerPlayerImpl,
  unregisterPlayerImpl,
  getAudioEngine,
} from "./playerBridge";
import {
  pauseAudio,
  nextAudio,
  setListenSession,
  setListenSessionProcedure,
  setCurrentAudio,
} from "@/redux/slices/mediaPlayerSlice/mediaPlayerSlice";
import {
  useListenToEpisodeMutation,
  useListenToBookingTrackMutation,
} from "./player.service";
import { useGetSubscriptionBenefitsMapListFromEpisodeIdQuery } from "@/core/services/subscription/subscription.service";
import { BASE_URL } from "@/core/api/appApi";
import { getAccessToken } from "@/core/api/appApi/token";
import type {
  ListenSessionEpisodes,
  ListenSessionBookingTracks,
} from "@/core/types/audio";

export default function PlayerCore() {
  const dispatch = useDispatch();
  const { playMode, listenSession, listenSessionProcedure } = useSelector(
    (s: RootState) => s.player
  );

  const engineRef = useRef<any>(null);
  const hlsRef = useRef<any>(null);
  const audioRef = useRef<HTMLAudioElement | null>(null);
  const listenersRef = useRef<any>({});

  // RTK Query hooks
  const [listenToEpisode] = useListenToEpisodeMutation();
  const [listenToBookingTrack] = useListenToBookingTrackMutation();

  // Get subscription benefits only when playing SpecifyShowEpisodes
  const shouldFetchBenefits =
    playMode.sourceType === "SpecifyShowEpisodes" && !!playMode.audioId;

  const { data: benefitsData, isLoading: isBenefitsLoading } =
    useGetSubscriptionBenefitsMapListFromEpisodeIdQuery(
      { PodcastEpisodeId: playMode.audioId! },
      { skip: !shouldFetchBenefits }
    );

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

  // Cache nhẹ: nhớ lại playlistUrl/token cho audio Id đang phát (trong 1 session)
  const lastLoadedIdRef = useRef<string | null>(null);
  const isLoadingRef = useRef<boolean>(false);

  // Khi có yêu cầu play audio mới
  useEffect(() => {
    const engine = getAudioEngine();

    const ensureLoadedThenPlay = async () => {
      // Cần có audioId và sourceType để bắt đầu
      if (!playMode.audioId || !playMode.sourceType) return;

      // Nếu đang fetch benefits cho SpecifyShowEpisodes, đợi xong đã
      if (playMode.sourceType === "SpecifyShowEpisodes" && isBenefitsLoading) {
        console.log("Waiting for benefits to load...");
        return;
      }

      // Nếu engine đã load đúng track này rồi -> resume play
      if (engine.getCurrentTrackId() === playMode.audioId && listenSession) {
        try {
          await engine.play();
        } catch {}
        return;
      }

      // Nếu đang loading, tránh đúp
      if (isLoadingRef.current) return;

      isLoadingRef.current = true;
      try {
        // 1) Gọi RTK Query để lấy listenSession
        let listenResult;
        const sourceType = playMode.sourceType;
        const audioId = playMode.audioId;

        if (
          sourceType === "SpecifyShowEpisodes" ||
          sourceType === "SavedEpisodes"
        ) {
          // Lấy benefits list để truyền vào API
          const benefitsList =
            sourceType === "SpecifyShowEpisodes" && benefitsData
              ? benefitsData.CurrentPodcastSubscriptionRegistrationBenefitList
              : [];

          console.log("Benefits List to send:", benefitsList);
          console.log("Benefits Data:", benefitsData);

          // Gọi listenToEpisode
          const response = await listenToEpisode({
            PodcastEpisodeId: audioId,
            SourceType: sourceType,
            CurrentPodcastSubscriptionRegistrationBenefitList: benefitsList,
          }).unwrap();

          listenResult = response.ListenSession as ListenSessionEpisodes;
          dispatch(setListenSession(listenResult));
          dispatch(setListenSessionProcedure(response.ListenSessionProcedure));
        } else if (sourceType === "BookingProducingTracks") {
          // Gọi listenToBookingTrack
          // Cần lấy BookingId từ listenSessionProcedure nếu có
          if (!listenSessionProcedure) {
            console.error("No listenSessionProcedure found for booking");
            dispatch(pauseAudio());
            return;
          }

          const bookingId =
            listenSessionProcedure.SourceDetail.Booking
              ?.BookingProducingRequestId;
          if (!bookingId) {
            console.error("No BookingId found");
            dispatch(pauseAudio());
            return;
          }

          const response = await listenToBookingTrack({
            BookingId: bookingId,
            BookingPodcastTrackId: audioId,
          }).unwrap();

          listenResult = response.ListenSession as ListenSessionBookingTracks;
          dispatch(setListenSession(listenResult));
          dispatch(setListenSessionProcedure(response.ListenSessionProcedure));
        }

        if (!listenResult) {
          console.error("No listen result");
          dispatch(pauseAudio());
          return;
        }

        // 2) Extract currentAudio from listenResult
        let currentAudioData;
        if (
          sourceType === "SpecifyShowEpisodes" ||
          sourceType === "SavedEpisodes"
        ) {
          const episodeSession = listenResult as ListenSessionEpisodes;
          currentAudioData = {
            Id: episodeSession.PodcastEpisode.Id,
            Name: episodeSession.PodcastEpisode.Name,
            ImageUrl: episodeSession.PodcastEpisode.MainImageFileKey || "", // Will be resolved later if needed
            PodcasterName: episodeSession.Podcaster.FullName || "Unknown",
            AudioLength:
              episodeSession.PodcastEpisodeListenSession
                .LastListenDurationSeconds || 0,
          };
        } else if (sourceType === "BookingProducingTracks") {
          const bookingSession = listenResult as ListenSessionBookingTracks;
          currentAudioData = {
            Id: bookingSession.BookingPodcastTrack.Id,
            Name: bookingSession.BookingPodcastTrack.BookingRequirementName,
            ImageUrl: "", // Booking tracks don't have images in current type
            PodcasterName: "Booking Track",
            AudioLength:
              bookingSession.BookingPodcastTrackListenSession
                .LastListenDurationSeconds || 0,
          };
        }

        if (currentAudioData) {
          dispatch(setCurrentAudio(currentAudioData));
        }

        // 3) Check xem có PlaylistFileKey hay AudioFileUrl
        const hasPlaylistFileKey =
          "PlaylistFileKey" in listenResult && listenResult.PlaylistFileKey;
        const hasAudioFileUrl =
          "AudioFileUrl" in listenResult && listenResult.AudioFileUrl;

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

        if (hasAudioFileUrl) {
          // Case 1: AudioFileUrl -> Play trực tiếp
          console.log("Playing from AudioFileUrl:", listenResult.AudioFileUrl);
          audioRef.current.src = listenResult.AudioFileUrl;
        } else if (hasPlaylistFileKey) {
          // Case 2: PlaylistFileKey -> Setup HLS
          console.log(
            "Playing from PlaylistFileKey:",
            listenResult.PlaylistFileKey
          );

          const Hls = (await import("hls.js")).default;
          const accessToken = getAccessToken();

          // ===================================
          // FLOW 1: EPISODES (SpecifyShowEpisodes hoặc SavedEpisodes)
          // ===================================
          if (
            sourceType === "SpecifyShowEpisodes" ||
            sourceType === "SavedEpisodes"
          ) {
            const episodeSession = listenResult as ListenSessionEpisodes;
            const token = episodeSession.Token;
            const fileKey = episodeSession.PlaylistFileKey;
            const episodeId = episodeSession.PodcastEpisode.Id;

            // URL: /api/podcast-service/api/episodes/hls-playlist/get-file-data/{FileKey}

            // NOTE: SỬA LẠI VỚI NGROK KHÔNG CÓ / Ở CUỐI
            // const playlistUrl = `${BASE_URL}/api/podcast-service/api/episodes/hls-playlist/get-file-data/${fileKey}`;
            const playlistUrl = `${BASE_URL}api/podcast-service/api/episodes/hls-playlist/get-file-data/${fileKey}`;
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

                    // Handle encryption key requests (UUID pattern)
                    // URL: /api/podcast-service/api/episodes/{PodcastEpisodeId}/hls-encryption-key/{KeyId}
                    if (/[0-9a-fA-F-]{36}$/.test(u)) {
                      const kid = u.split("/").pop();
                      // NOTE: SỬA LẠI VỚI NGROK KHÔNG CÓ / Ở CUỐI
                      next = `${BASE_URL}api/podcast-service/api/episodes/${episodeId}/hls-encryption-key/${kid}?token=${token}`;
                    }
                    // Handle segment requests (.ts files)
                    // URL: /api/podcast-service/api/episodes/hls-segment/get-file-data/{FileKey}
                    else if (u.includes(".ts")) {
                      const idx = url.lastIndexOf(
                        "main_files/PodcastEpisodes/"
                      );
                      if (idx !== -1) {
                        const segmentFileKey = url.substring(idx);
                        // NOTE: SỬA LẠI VỚI NGROK KHÔNG CÓ / Ở CUỐI
                        next = `${BASE_URL}api/podcast-service/api/episodes/hls-segment/get-file-data/${segmentFileKey}`;
                      }
                    }

                    return originalOpen(method, next, async);
                  };

                  xhr.withCredentials = true;
                  try {
                    if (accessToken) {
                      xhr.setRequestHeader(
                        "Authorization",
                        `Bearer ${accessToken}`
                      );
                    }
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
              // Native HLS support (Safari)
              audioRef.current.src = playlistUrl;
            } else {
              throw new Error("HLS not supported in this browser");
            }
          }
          // ===================================
          // FLOW 2: BOOKING TRACKS (BookingProducingTracks)
          // ===================================
          else if (sourceType === "BookingProducingTracks") {
            const bookingSession = listenResult as ListenSessionBookingTracks;
            const fileKey = bookingSession.PlaylistFileKey;
            const bookingId = bookingSession.Booking.Id;
            const trackId = bookingSession.BookingPodcastTrack.Id;

            // URL: /api/booking-management-service/api/bookings/{BookingId}/booking-podcast-tracks/hls-playlist/get-file-data/{FileKey}
            // const playlistUrl = `${BASE_URL}/api/booking-management-service/api/bookings/${bookingId}/booking-podcast-tracks/hls-playlist/get-file-data/${fileKey}`;
            // NOTE: SỬA LẠI VỚI NGROK KHÔNG CÓ / Ở CUỐI
            const playlistUrl = `${BASE_URL}api/booking-management-service/api/bookings/${bookingId}/booking-podcast-tracks/hls-playlist/get-file-data/${fileKey}`;

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

                    // Handle encryption key requests (UUID pattern)
                    // URL: /api/booking-management-service/api/bookings/{BookingId}/booking-podcast-tracks/{BookingPodcastTrackId}/hls-encryption-key/{KeyId}
                    if (/[0-9a-fA-F-]{36}$/.test(u)) {
                      const kid = u.split("/").pop();
                      // NOTE: SỬA LẠI VỚI NGROK KHÔNG CÓ / Ở CUỐI
                      // next = `${BASE_URL}/api/booking-management-service/api/bookings/${bookingId}/booking-podcast-tracks/${trackId}/hls-encryption-key/${kid}`;
                      next = `${BASE_URL}api/booking-management-service/api/bookings/${bookingId}/booking-podcast-tracks/${trackId}/hls-encryption-key/${kid}`;
                    }
                    // Handle segment requests (.ts files)
                    // URL: /api/booking-management-service/api/bookings/{BookingId}/booking-podcast-tracks/hls-segment/get-file-data/{FileKey}
                    else if (u.includes(".ts")) {
                      const idx = url.lastIndexOf(
                        "main_files/BookingPodcastTracks/"
                      );
                      if (idx !== -1) {
                        const segmentFileKey = url.substring(idx);
                        // NOTE: SỬA LẠI VỚI NGROK KHÔNG CÓ / Ở CUỐI

                        //next = `${BASE_URL}/api/booking-management-service/api/bookings/${bookingId}/booking-podcast-tracks/hls-segment/get-file-data/${segmentFileKey}`;
                        next = `${BASE_URL}api/booking-management-service/api/bookings/${bookingId}/booking-podcast-tracks/hls-segment/get-file-data/${segmentFileKey}`;
                      }
                    }

                    return originalOpen(method, next, async);
                  };

                  xhr.withCredentials = true;
                  try {
                    if (accessToken) {
                      xhr.setRequestHeader(
                        "Authorization",
                        `Bearer ${accessToken}`
                      );
                    }
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
              // Native HLS support (Safari)
              audioRef.current.src = playlistUrl;
            } else {
              throw new Error("HLS not supported in this browser");
            }
          } else {
            console.error("Unknown source type:", sourceType);
            dispatch(pauseAudio());
            return;
          }
        } else {
          console.error("No PlaylistFileKey or AudioFileUrl found");
          dispatch(pauseAudio());
          return;
        }

        // set current track id for bridge
        engineRef.current = { currentTrackId: audioId };

        // 4) play
        try {
          await audioRef.current?.play();
        } catch {}
      } catch (e) {
        console.error("PlayerCore.ensureLoadedThenPlay error:", e);
        dispatch(pauseAudio());
      } finally {
        isLoadingRef.current = false;
        lastLoadedIdRef.current = playMode.audioId || null;
      }
    };

    if (playMode.playStatus === "play") {
      ensureLoadedThenPlay();
    } else if (playMode.playStatus === "pause") {
      engine.pause();
    } else if (playMode.playStatus === "stop") {
      engine.pause();
    }
  }, [
    playMode.playStatus,
    playMode.audioId,
    playMode.sourceType,
    dispatch,
    listenSession,
    listenSessionProcedure,
    listenToEpisode,
    listenToBookingTrack,
    isBenefitsLoading,
    benefitsData,
  ]);

  // Volume (Redux 0..100 -> audio 0..1)
  useEffect(() => {
    const engine = getAudioEngine();
    engine.setVolume((playMode.volume ?? 100) / 100);
  }, [playMode.volume]);

  return null;
}

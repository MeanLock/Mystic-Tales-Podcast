import { useEffect, useRef, useCallback } from "react";
import { useDispatch, useSelector } from "react-redux";
import type { RootState } from "@/redux/store";
import {
  registerPlayerImpl,
  unregisterPlayerImpl,
  getAudioEngine,
} from "./playerBridge";
import {
  playAudio,
  pauseAudio,
  setListenSession,
  setListenSessionProcedure,
  setCurrentAudio,
  setIsNextSessionNull,
  setUIIsAutoPlay,
  setUIPlayOrderMode,
  setBuffering,
} from "@/redux/slices/mediaPlayerSlice/mediaPlayerSlice";
import {
  useListenToEpisodeMutation,
  useListenToBookingTrackMutation,
  useUpdateEpisodeLastDurationMutation,
  useUpdateBookingTrackLastDurationMutation,
  useNavigateEpisodeInProcedureMutation,
  useNavigateBookingTrackInProcedureMutation,
  playerApi,
} from "./player.service";
import { useGetSubscriptionBenefitsMapListFromEpisodeIdQuery } from "@/core/services/subscription/subscription.service";
import { BASE_URL } from "@/core/api/appApi";
import { getAccessToken } from "@/core/api/appApi/token";
import type {
  ListenSessionEpisodes,
  ListenSessionBookingTracks,
} from "@/core/types/audio";
import { setError } from "@/redux/slices/errorSlice/errorSlice";

export default function PlayerCore() {
  const dispatch = useDispatch();
  const {
    playMode,
    listenSession,
    listenSessionProcedure,
    seekTo,
    continue_listen_session_id,
    bookingId,
  } = useSelector((s: RootState) => s.player);

  const user = useSelector((state: RootState) => state.auth.user);

  const engineRef = useRef<any>(null);
  const hlsRef = useRef<any>(null);
  const audioRef = useRef<HTMLAudioElement | null>(null);
  const listenersRef = useRef<any>({});
  const playingIntervalRef = useRef<number | null>(null);
  const isAutoPlayRef = useRef<boolean>(false);
  const getLatestPlayerState = useRef<() => RootState["player"]>(() => ({
    playMode,
    listenSession,
    listenSessionProcedure,
    currentAudio: null,
    seekTo: null,
    continue_listen_session_id: null,
    bookingId: null,
    isBuffering: false,
  }));

  // RTK Query hooks
  const [listenToEpisode] = useListenToEpisodeMutation();
  const [listenToBookingTrack] = useListenToBookingTrackMutation();
  const [navigateEpisode] = useNavigateEpisodeInProcedureMutation();
  const [navigateBookingTrack] = useNavigateBookingTrackInProcedureMutation();

  // Get subscription benefits only when playing SpecifyShowEpisodes
  const shouldFetchBenefits =
    playMode.sourceType === "SpecifyShowEpisodes" && !!playMode.audioId;

  const { data: benefitsData, isLoading: isBenefitsLoading } =
    useGetSubscriptionBenefitsMapListFromEpisodeIdQuery(
      { PodcastEpisodeId: playMode.audioId! },
      { skip: !shouldFetchBenefits }
    );

  // Mutation hooks for update last duration
  const [updateLastDurationEpisode] = useUpdateEpisodeLastDurationMutation();
  const [updateLastDurationBookingTrack] =
    useUpdateBookingTrackLastDurationMutation();

  // Cache nhẹ: nhớ lại playlistUrl/token cho audio Id đang phát (trong 1 session)
  const lastLoadedIdRef = useRef<string | null>(null);
  const isLoadingRef = useRef<boolean>(false);
  const pendingSeekRef = useRef<number | null>(null);
  const hasLoadedFirstSegmentRef = useRef<boolean>(false);

  // Sync player state to ref để tránh closure issue
  useEffect(() => {
    getLatestPlayerState.current = () => ({
      playMode,
      listenSession,
      listenSessionProcedure,
      currentAudio: null,
      continue_listen_session_id: continue_listen_session_id,
      seekTo: seekTo,
      bookingId: bookingId,
      isBuffering: false,
    });
    isAutoPlayRef.current = playMode.isAutoPlay;
  }, [
    playMode,
    listenSession,
    listenSessionProcedure,
    continue_listen_session_id,
    seekTo,
    bookingId,
  ]);

  // Hàm update last duration - dùng chung cho cả interval và seek
  const updateLastDuration = useCallback(async () => {
    if (!listenSession || !listenSessionProcedure || !audioRef.current) {
      return;
    }

    const currentTime = Math.floor(audioRef.current.currentTime || 0);
    const sourceType = listenSessionProcedure.SourceDetail.Type;

    try {
      if (sourceType === "SpecifyShowEpisodes") {
        const benefitsList = benefitsData
          ? benefitsData.CurrentPodcastSubscriptionRegistrationBenefitList
          : [];

        await updateLastDurationEpisode({
          LastListenDurationSeconds: currentTime,
          PodcastEpisodeListenSessionId: (
            listenSession as ListenSessionEpisodes
          ).PodcastEpisodeListenSession.Id,
          CurrentPodcastSubscriptionRegistrationBenefitList: benefitsList,
        }).unwrap();
      } else if (sourceType === "SavedEpisodes") {
        await updateLastDurationEpisode({
          LastListenDurationSeconds: currentTime,
          PodcastEpisodeListenSessionId: (
            listenSession as ListenSessionEpisodes
          ).PodcastEpisodeListenSession.Id,
          CurrentPodcastSubscriptionRegistrationBenefitList: null,
        }).unwrap();
      } else if (sourceType === "BookingProducingTracks") {
        await updateLastDurationBookingTrack({
          LastListenDurationSeconds: currentTime,
          BookingPodcastTrackListenSessionId: (
            listenSession as ListenSessionBookingTracks
          ).BookingPodcastTrackListenSession.Id,
        }).unwrap();
      }
    } catch (error) {
      console.error("Error updating last duration:", error);
    }
  }, [
    listenSession,
    listenSessionProcedure,
    benefitsData,
    updateLastDurationEpisode,
    updateLastDurationBookingTrack,
  ]);

  // Hàm navigate để handle next/previous
  const handleNavigate = useCallback(
    async (navigateType: "Next" | "Previous") => {
      // Lấy state mới nhất từ ref để tránh closure
      const currentState = getLatestPlayerState.current();
      const currentListenSession = currentState.listenSession;
      const currentListenSessionProcedure = currentState.listenSessionProcedure;

      if (!currentListenSession || !currentListenSessionProcedure) {
        console.error("No listen session or procedure found");
        return;
      }

      try {
        const sourceType = currentListenSessionProcedure.SourceDetail.Type;

        if (
          sourceType === "SpecifyShowEpisodes" ||
          sourceType === "SavedEpisodes"
        ) {
          // Navigate cho Episodes
          const episodeSession = currentListenSession as ListenSessionEpisodes;

          // Lấy benefits list nếu là SpecifyShowEpisodes
          const benefitsList =
            sourceType === "SpecifyShowEpisodes" && benefitsData
              ? benefitsData.CurrentPodcastSubscriptionRegistrationBenefitList
              : null;

          if (sourceType === "SpecifyShowEpisodes") {
            if (benefitsList && benefitsList.length > 0) {
              if (!benefitsList.some((b) => b.Id === 1)) {
                if (user?.PodcastListenSlot === 0) {
                  dispatch(
                    setError({
                      message:
                        "You have no listen slots left.",
                      autoClose: 10,
                    })
                  );
                  return;
                }
              }
            }
          }
          const response = await navigateEpisode({
            ListenSessionNavigateType: navigateType,
            ListenSessionId: episodeSession.PodcastEpisodeListenSession.Id,
            ListenSessionProcedureId: currentListenSessionProcedure.Id,
            CurrentPodcastSubscriptionRegistrationBenefitList: benefitsList,
          }).unwrap();

          dispatch(setListenSessionProcedure(response.ListenSessionProcedure));
          // Cập nhật Redux state với session mới
          if (response.ListenSession) {
            dispatch(setListenSession(response.ListenSession));

            // Extract và set current audio
            console.log("Response ListenSession:", response);
            const newEpisodeSession =
              response.ListenSession as ListenSessionEpisodes;
            console.log("New Episode Session Episode Id:", newEpisodeSession);
            if (!newEpisodeSession.PodcastEpisode.Id) {
              alert("Đéo có");
            }
            const currentAudioData = {
              Id: newEpisodeSession.PodcastEpisode.Id,
              Name: newEpisodeSession.PodcastEpisode.Name,
              MainImageFileKey:
                newEpisodeSession.PodcastEpisode.MainImageFileKey || "",
              PodcasterName: newEpisodeSession.Podcaster.FullName || "Unknown",
              AudioLength:
                newEpisodeSession.PodcastEpisodeListenSession
                  .LastListenDurationSeconds || 0,
            };
            dispatch(setCurrentAudio(currentAudioData));

            // Reset currentTrackId để force reload audio mới
            if (engineRef.current) {
              engineRef.current.currentTrackId = null;
            }

            // Trigger play với audio mới
            dispatch(
              playAudio({
                sourceType: sourceType,
                audioId: newEpisodeSession.PodcastEpisode.Id,
              })
            );
          } else {
            // Không còn bài nào để navigate
            console.log("No more tracks to navigate");
            dispatch(setIsNextSessionNull(true));
            dispatch(pauseAudio());
          }
        } else if (sourceType === "BookingProducingTracks") {
          // Navigate cho Booking Tracks
          const bookingSession =
            currentListenSession as ListenSessionBookingTracks;

          const response = await navigateBookingTrack({
            ListenSessionNavigateType: navigateType,
            ListenSessionId: bookingSession.BookingPodcastTrackListenSession.Id,
            ListenSessionProcedureId: currentListenSessionProcedure.Id,
            CurrentPodcastSubscriptionRegistrationBenefitList: null,
          }).unwrap();

          // Cập nhật Redux state với session mới
          if (response.ListenSession) {
            dispatch(setListenSession(response.ListenSession));
            dispatch(
              setListenSessionProcedure(response.ListenSessionProcedure)
            );

            // Extract và set current audio
            const newBookingSession =
              response.ListenSession as ListenSessionBookingTracks;
            const currentAudioData = {
              Id: newBookingSession.BookingPodcastTrack.Id,
              Name: newBookingSession.BookingPodcastTrack
                .BookingRequirementName,
              MainImageFileKey: "",
              PodcasterName: "Booking Track",
              AudioLength:
                newBookingSession.BookingPodcastTrackListenSession
                  .LastListenDurationSeconds || 0,
            };
            dispatch(setCurrentAudio(currentAudioData));

            // Reset currentTrackId để force reload audio mới
            if (engineRef.current) {
              engineRef.current.currentTrackId = null;
            }

            // Trigger play với audio mới
            dispatch(
              playAudio({
                sourceType: "BookingProducingTracks",
                audioId: newBookingSession.BookingPodcastTrack.Id,
              })
            );
          } else {
            // Không còn track nào để navigate
            console.log("No more tracks to navigate");
            dispatch(setIsNextSessionNull(true));
            dispatch(pauseAudio());
          }
        }
      } catch (error) {
        console.error("Error navigating:", error);
        dispatch(pauseAudio());
      }
    },
    [benefitsData, navigateEpisode, navigateBookingTrack, dispatch]
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
        if (audioRef.current) {
          audioRef.current.currentTime = Math.max(0, s);
          // Cập nhật last duration ngay khi user seek
          updateLastDuration();
        }
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
      next: () => handleNavigate("Next"),
      previous: () => handleNavigate("Previous"),
    });

    return () => {
      unregisterPlayerImpl();
    };
  }, [dispatch, handleNavigate, updateLastDuration]);

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

      // Nếu đang loading, tránh đúp
      if (isLoadingRef.current) return;

      // Check xem đã load track này chưa
      if (
        lastLoadedIdRef.current === playMode.audioId &&
        engineRef.current?.currentTrackId === playMode.audioId
      ) {
        // Đã load rồi, chỉ cần resume play
        try {
          await audioRef.current?.play();
        } catch {}
        return;
      }

      isLoadingRef.current = true;
      try {
        // 1) Check xem listenSession hiện tại có khớp với audioId không
        let listenResult = listenSession;
        const sourceType = playMode.sourceType;
        const audioId = playMode.audioId;

        // Kiểm tra xem listenSession có match với audioId không
        let needNewSession = true;
        if (listenSession) {
          if (
            (sourceType === "SpecifyShowEpisodes" ||
              sourceType === "SavedEpisodes") &&
            "PodcastEpisode" in listenSession
          ) {
            const episodeSession = listenSession as ListenSessionEpisodes;
            if (episodeSession.PodcastEpisode.Id === audioId) {
              needNewSession = false;
            }
          } else if (
            sourceType === "BookingProducingTracks" &&
            "BookingPodcastTrack" in listenSession
          ) {
            const bookingSession = listenSession as ListenSessionBookingTracks;
            if (bookingSession.BookingPodcastTrack.Id === audioId) {
              needNewSession = false;
            }
          }
        }

        // Nếu cần session mới, gọi API
        if (needNewSession) {
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
              continue_listen_session_id:
                continue_listen_session_id || undefined,
            }).unwrap();

            listenResult = response.ListenSession as ListenSessionEpisodes;

            dispatch(
              setUIIsAutoPlay(
                response.ListenSessionProcedure
                  ? response.ListenSessionProcedure.IsAutoPlay
                  : false
              )
            );
            dispatch(
              setUIPlayOrderMode(
                response.ListenSessionProcedure
                  ? response.ListenSessionProcedure.PlayOrderMode
                  : "Sequential"
              )
            );

            dispatch(setListenSession(listenResult));
            dispatch(
              setListenSessionProcedure(response.ListenSessionProcedure)
            );
          } else if (sourceType === "BookingProducingTracks") {
            // Gọi listenToBookingTrack
            // Lấy BookingId từ Redux state
            if (!bookingId) {
              console.error("No BookingId found in Redux state");
              dispatch(pauseAudio());
              return;
            }

            const response = await listenToBookingTrack({
              BookingId: bookingId.toString(),
              BookingPodcastTrackId: audioId,
            }).unwrap();

            listenResult = response.ListenSession as ListenSessionBookingTracks;
            dispatch(setListenSession(listenResult));
            dispatch(
              setListenSessionProcedure(response.ListenSessionProcedure)
            );
            dispatch(
              setUIIsAutoPlay(
                response.ListenSessionProcedure
                  ? response.ListenSessionProcedure.IsAutoPlay
                  : false
              )
            );
            dispatch(
              setUIPlayOrderMode(
                response.ListenSessionProcedure
                  ? response.ListenSessionProcedure.PlayOrderMode
                  : "Sequential"
              )
            );
          }
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
            MainImageFileKey:
              episodeSession.PodcastEpisode.MainImageFileKey || "", // Will be resolved later if needed
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
            MainImageFileKey: "", // Booking tracks don't have images in current type
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

            // Seek đến vị trí LastListenDurationSeconds nếu có
            if (pendingSeekRef.current !== null && pendingSeekRef.current > 0) {
              const seekTo = pendingSeekRef.current;
              pendingSeekRef.current = null; // Clear pending seek

              console.log("[LOADEDMETADATA] Seeking to:", seekTo);
              // Seek tới vị trí
              a.currentTime = seekTo;

              // Pause lại và đợi user nhấn play
              dispatch(pauseAudio());
            }
          });
          a.addEventListener("timeupdate", () => {
            listenersRef.current.timeupdate?.(a.currentTime || 0);
          });

          // Buffering events for HTML5 audio - chỉ cho lần đầu
          a.addEventListener("loadstart", () => {
            // Reset flag khi bắt đầu load audio mới
            hasLoadedFirstSegmentRef.current = false;
          });

          a.addEventListener("waiting", () => {
            // Chỉ set buffering cho lần đầu
            if (!hasLoadedFirstSegmentRef.current) {
              console.log("[AUDIO] Waiting for data - Buffering started");
              dispatch(setBuffering(true));
            }
          });

          a.addEventListener("canplay", () => {
            // Chỉ set buffering ended cho lần đầu
            if (!hasLoadedFirstSegmentRef.current) {
              console.log("[AUDIO] Can play - Buffering ended");
              dispatch(setBuffering(false));
              hasLoadedFirstSegmentRef.current = true;
            }
          });

          a.addEventListener("playing", () => {
            // Đảm bảo tắt buffering khi đang play
            if (!hasLoadedFirstSegmentRef.current) {
              console.log("[AUDIO] Playing - Buffering ended");
              dispatch(setBuffering(false));
              hasLoadedFirstSegmentRef.current = true;
            }
          });

          a.addEventListener("ended", () => {
            // Kiểm tra xem có nên tự động next không
            // Check cả 2 sources:
            // 1. playMode.isAutoPlay (Redux UI state - để hoạt động ngay khi user toggle)
            // 2. listenSessionProcedure.IsAutoPlay (từ server - giá trị đã lưu)
            console.log("[ENDED EVENT] Audio ended, checking autoplay...");
            console.log(
              "[ENDED EVENT] playMode.isAutoPlay:",
              playMode.isAutoPlay
            );
            console.log(
              "[ENDED EVENT] isAutoPlayRef.current:",
              isAutoPlayRef.current
            );
            console.log(
              "[ENDED EVENT] listenSessionProcedure.IsAutoPlay:",
              listenSessionProcedure?.IsAutoPlay
            );

            // Ưu tiên playMode.isAutoPlay (UI state) để hoạt động ngay
            const shouldAutoPlay = playMode.isAutoPlay || isAutoPlayRef.current;

            if (
              shouldAutoPlay &&
              listenSession &&
              !playMode.isNextSessionNull &&
              listenSessionProcedure
            ) {
              const playOrder =
                playMode.nextMode === "Sequential"
                  ? listenSessionProcedure.ListenObjectsSequentialOrder
                  : listenSessionProcedure.ListenObjectsRandomOrder;

              const listenableCount =
                playOrder?.filter((item) => item.IsListenable).length || 0;

              if (listenableCount > 1) {
                console.log("[ENDED EVENT] Auto-playing next track...");
                handleNavigate("Next");
              } else {
                console.log(
                  "[ENDED EVENT] Not enough listenable tracks (count:",
                  listenableCount,
                  ")"
                );
                dispatch(pauseAudio());
              }
            } else {
              // Console log ra để xem cái gì làm sai điều kiện
              console.log("Is Autoplay:", isAutoPlayRef.current);
              console.log(
                "[ENDED EVENT] listenSession:",
                listenSession ? "exists" : "null"
              );
              console.log(
                "[ENDED EVENT] isNextSessionNull:",
                playMode.isNextSessionNull
              );
              console.log(
                "[ENDED EVENT] listenSessionProcedure:",
                listenSessionProcedure ? "exists" : "null"
              );
              console.log(
                "[ENDED EVENT] Auto-play disabled or no session available"
              );
              dispatch(pauseAudio());
            }
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

              // HLS Event Listeners for buffering control (EPISODES)
              h.on(Hls.Events.MANIFEST_PARSED, () => {
                console.log("[HLS] Manifest parsed");
                // Reset flag khi load audio mới
                hasLoadedFirstSegmentRef.current = false;
              });

              h.on(Hls.Events.FRAG_LOADING, () => {
                // Chỉ set buffering cho segment đầu tiên
                if (!hasLoadedFirstSegmentRef.current) {
                  console.log(
                    "[HLS] First fragment loading - Buffering started"
                  );
                  dispatch(setBuffering(true));
                }
              });

              h.on(Hls.Events.FRAG_LOADED, () => {
                // Chỉ set buffering ended cho segment đầu tiên
                if (!hasLoadedFirstSegmentRef.current) {
                  console.log("[HLS] First fragment loaded - Buffering ended");
                  dispatch(setBuffering(false));
                  hasLoadedFirstSegmentRef.current = true;
                }
              });

              h.on(Hls.Events.ERROR, (_, data) => {
                console.error("[HLS] Error:", data);
                if (data.fatal) {
                  dispatch(setBuffering(false));
                  switch (data.type) {
                    case Hls.ErrorTypes.NETWORK_ERROR:
                      console.error("[HLS] Fatal network error");
                      h.startLoad();
                      break;
                    case Hls.ErrorTypes.MEDIA_ERROR:
                      console.error("[HLS] Fatal media error");
                      h.recoverMediaError();
                      break;
                    default:
                      console.error("[HLS] Fatal error, cannot recover");
                      break;
                  }
                }
              });
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
                      const idx = url.lastIndexOf("main_files/Bookings/");
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

              // HLS Event Listeners for buffering control (BOOKING TRACKS)
              h.on(Hls.Events.MANIFEST_PARSED, () => {
                console.log("[HLS] Manifest parsed");
                // Reset flag khi load audio mới
                hasLoadedFirstSegmentRef.current = false;
              });

              h.on(Hls.Events.FRAG_LOADING, () => {
                // Chỉ set buffering cho segment đầu tiên
                if (!hasLoadedFirstSegmentRef.current) {
                  console.log(
                    "[HLS] First fragment loading - Buffering started"
                  );
                  dispatch(setBuffering(true));
                }
              });

              h.on(Hls.Events.FRAG_LOADED, () => {
                // Chỉ set buffering ended cho segment đầu tiên
                if (!hasLoadedFirstSegmentRef.current) {
                  console.log("[HLS] First fragment loaded - Buffering ended");
                  dispatch(setBuffering(false));
                  hasLoadedFirstSegmentRef.current = true;
                }
              });

              h.on(Hls.Events.ERROR, (_, data) => {
                console.error("[HLS] Error:", data);
                if (data.fatal) {
                  dispatch(setBuffering(false));
                  switch (data.type) {
                    case Hls.ErrorTypes.NETWORK_ERROR:
                      console.error("[HLS] Fatal network error");
                      h.startLoad();
                      break;
                    case Hls.ErrorTypes.MEDIA_ERROR:
                      console.error("[HLS] Fatal media error");
                      h.recoverMediaError();
                      break;
                    default:
                      console.error("[HLS] Fatal error, cannot recover");
                      break;
                  }
                }
              });
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

        // Lưu seekTo hoặc lastDuration vào ref để seek sau khi loadedmetadata
        let targetSeekPosition = 0;

        if (seekTo !== null && seekTo !== undefined) {
          // Nếu có seekTo từ Redux, ưu tiên dùng seekTo
          targetSeekPosition = seekTo;
        } else {
          // Nếu không có seekTo, dùng lastDuration từ session
          const lastDuration =
            sourceType === "SpecifyShowEpisodes" ||
            sourceType === "SavedEpisodes"
              ? (listenResult as ListenSessionEpisodes)
                  .PodcastEpisodeListenSession.LastListenDurationSeconds || 0
              : sourceType === "BookingProducingTracks"
              ? (listenResult as ListenSessionBookingTracks)
                  .BookingPodcastTrackListenSession.LastListenDurationSeconds ||
                0
              : 0;
          targetSeekPosition = lastDuration;
        }

        if (targetSeekPosition > 0) {
          pendingSeekRef.current = targetSeekPosition;
        }

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
    seekTo,
    continue_listen_session_id,
    bookingId,
  ]);

  // Volume (Redux 0..100 -> audio 0..1)
  useEffect(() => {
    const engine = getAudioEngine();
    engine.setVolume((playMode.volume ?? 100) / 100);
  }, [playMode.volume]);

  // Cập nhật last-duration tự động mỗi 5 giây khi đang play
  useEffect(() => {
    if (
      playMode.playStatus === "play" &&
      listenSession &&
      listenSessionProcedure
    ) {
      // Clear interval cũ nếu có
      if (playingIntervalRef.current) {
        clearInterval(playingIntervalRef.current);
      }

      // Tạo interval mới - gọi updateLastDuration mỗi 5 giây
      playingIntervalRef.current = window.setInterval(() => {
        updateLastDuration();
      }, 5000);
    } else {
      // Clear interval khi pause/stop
      if (playingIntervalRef.current) {
        clearInterval(playingIntervalRef.current);
        playingIntervalRef.current = null;
      }
    }

    // Cleanup khi component unmount
    return () => {
      if (playingIntervalRef.current) {
        clearInterval(playingIntervalRef.current);
      }
    };
  }, [
    playMode.playStatus,
    listenSession,
    listenSessionProcedure,
    updateLastDuration,
  ]);

  return null;
}

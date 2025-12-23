// @ts-nocheck
// usePlayer.ts
import { useEffect, useState, useCallback } from "react";
import {
  getPlayerController,
  type PlayerUiState,
} from "@/core/services/player/playerController";
import {
  useListenToEpisodeMutation,
  useListenToBookingTrackMutation,
  useNavigateEpisodeInProcedureMutation,
  useNavigateBookingTrackInProcedureMutation,
  useGetEpisodeLatestSessionMutation,
  useGetBookingLatestSessionMutation,
  useUpdatePlayModeMutation,
} from "@/core/services/player/player.service"; // file RTK Query của bạn
import type { SubscriptionBenefit } from "../subscription/subscription.service";
import type {
  ListenSessionBookingTracks,
  ListenSessionEpisodes,
  ListenSessionProcedure,
} from "@/core/types/audio";
import { useDispatch, useSelector } from "react-redux";
import {
  setListenSession,
  setListenSessionProcedure,
} from "@/redux/slices/mediaPlayerSlice/mediaPlayerSlice";
import type { RootState } from "@/redux/store";
import { useLazyCheckUserPodcastListenSlotQuery } from "../account/account.service";
import { showAlert } from "@/redux/slices/alertSlice/alertSlice";

import {
  getBookingLatestSession,
  getEpisodeLatestSession,
} from "./get-latest.service";

export function usePlayer() {
  const controller = getPlayerController();

  const [state, setState] = useState<PlayerUiState>(controller.getUiState());
  const player = useSelector((state: RootState) => state.player);
  const dispatch = useDispatch();

  // RTK Query hooks – CHỈ ĐƯỢC DÙNG Ở ĐÂY
  // Listen
  const [listenToEpisode] = useListenToEpisodeMutation();
  const [listenToBookingTrack] = useListenToBookingTrackMutation();

  // Lấy latest session dành cho Episode và Booking Track
  // Dùng mutation để tránh bị cancel khi gọi programmatically
  const [triggerBookingSession] = useGetBookingLatestSessionMutation();
  const [triggerEpisodeSession] = useGetEpisodeLatestSessionMutation();

  // Navigate
  const [navigateEpisode] = useNavigateEpisodeInProcedureMutation();
  const [navigateBookingTrack] = useNavigateBookingTrackInProcedureMutation();

  const [getPodcastListenSlotCount] = useLazyCheckUserPodcastListenSlotQuery();
  // Update PlayMode
  const [updatePlayMode] = useUpdatePlayModeMutation();

  useEffect(() => {
    const unsubscribe = controller.attachEvents({
      onStateChange: (nextState) => {
        // debug thử
        // console.log("[usePlayer] onStateChange", nextState);
        setState(nextState);
      },
      onError: (err) => console.error("[PlayerError]", err),
    });

    // optional: sync 1 phát nữa cho chắc
    setState(controller.getUiState());

    return () => {
      unsubscribe();
      // KHÔNG stop controller khi unmount vì nó là singleton
      // và cần giữ state xuyên suốt các component
    };
  }, [controller]);

  // ====== Các hàm public gọi từ UI ======

  // Hàm Listen To Episode From Specify Show
  // Gọi API xong là play luôn, và luôn play từ đầu (seekTo = 0)
  const playEpisodeFromSpecifyShow = useCallback(
    async (opts: { audioId: string; benefitsList: SubscriptionBenefit[] }) => {
      if (controller.isCurrentlyLoadingSession()) {
        console.warn("Already loading a session, please wait");
        return;
      }

      const { audioId, benefitsList = [] } = opts;

      // Set loading flag NGAY để chặn concurrent calls
      controller.setLoadingSession(true, audioId);

      let retryCount = 0;
      const MAX_RETRIES = 3;

      const attemptLoad = async (): Promise<boolean> => {
        try {
          const listenSlot = await getPodcastListenSlotCount().unwrap();

          if (benefitsList.length > 0) {
            if (!benefitsList.find((b) => b.Id === 1)) {
              if (listenSlot <= 0) {
                dispatch(
                  showAlert({
                    type: "warning",
                    title: "Listen Slot Exceeded",
                    description:
                      "You have used up all your free listen slots. Please wait for more slots to become available.",
                    isAutoClose: false,
                    isClosable: true,
                  })
                );
                return false;
              }
            }
          } else {
            if (listenSlot <= 0) {
              dispatch(
                showAlert({
                  type: "warning",
                  title: "Listen Slot Exceeded",
                  description:
                    "You have used up all your free listen slots. Please wait for more slots to become available.",
                  isAutoClose: false,
                  isClosable: true,
                })
              );
              return false;
            }
          }

          const resAPI = await listenToEpisode({
            PodcastEpisodeId: audioId,
            SourceType: "SpecifyShowEpisodes",
            CurrentPodcastSubscriptionRegistrationBenefitList: benefitsList,
          }).unwrap();

          const res = resAPI.data;

          if (res === null || res === undefined) {
            return;
          }

          const session = res.ListenSession as ListenSessionEpisodes;
          const procedure =
            res.ListenSessionProcedure as ListenSessionProcedure;

          // Lưu session và procedure vào redux store
          dispatch(setListenSession(session));
          dispatch(setListenSessionProcedure(procedure));

          await controller.playFromExistingSession({
            session,
            procedure,
            sourceType: "SpecifyShowEpisodes",
            seekTo: 0,
            isSeekThenPlay: true,
          });
          return true;
        } catch (error) {
          console.error(
            `Error in playEpisodeFromSpecifyShow (attempt ${retryCount + 1}):`,
            error
          );
          return false;
        }
      };

      try {
        while (retryCount < MAX_RETRIES) {
          const success = await attemptLoad();
          if (success) break;

          retryCount++;
          if (retryCount < MAX_RETRIES) {
            console.log(`Retrying... (${retryCount}/${MAX_RETRIES})`);
            await new Promise((resolve) =>
              setTimeout(resolve, 1000 * retryCount)
            );
          }
        }

        if (retryCount >= MAX_RETRIES) {
          console.error("Max retries reached. Clearing state.");
          controller.stop();
          dispatch(setListenSession(null));
          dispatch(setListenSessionProcedure(null));
          dispatch(
            showAlert({
              type: "error",
              title: "Playback Failed",
              description:
                "Unable to load audio after multiple attempts. Please try again later.",
              isAutoClose: true,
              isClosable: true,
            })
          );
        }
      } finally {
        controller.setLoadingSession(false);
      }
    },
    [listenToEpisode, controller, dispatch, getPodcastListenSlotCount]
  );

  // Hàm continue listening, đây là type của Specify Show Episodes
  const playContinueListening = useCallback(
    async (opts: {
      audioId: string;
      continueSessionId?: string;
      benefitsList: SubscriptionBenefit[];
      seekTo: number;
    }) => {
      if (controller.isCurrentlyLoadingSession()) {
        console.warn("Already loading a session, please wait");
        return;
      }

      const { audioId, continueSessionId, benefitsList = [], seekTo } = opts;

      controller.setLoadingSession(true, audioId);

      let retryCount = 0;
      const MAX_RETRIES = 3;

      const attemptLoad = async (): Promise<boolean> => {
        try {
          const res = await listenToEpisode({
            PodcastEpisodeId: audioId,
            SourceType: "SpecifyShowEpisodes",
            CurrentPodcastSubscriptionRegistrationBenefitList: benefitsList,
            continue_listen_session_id: continueSessionId,
          }).unwrap();

          const session = res.ListenSession as ListenSessionEpisodes;
          const procedure =
            res.ListenSessionProcedure as ListenSessionProcedure;

          dispatch(setListenSession(session));
          dispatch(setListenSessionProcedure(procedure));

          await controller.playFromExistingSession({
            session,
            procedure,
            sourceType: "SpecifyShowEpisodes",
            seekTo: seekTo,
            isSeekThenPlay: true,
          });
          return true;
        } catch (error) {
          console.error(
            `Error in playContinueListening (attempt ${retryCount + 1}):`,
            error
          );
          return false;
        }
      };

      try {
        while (retryCount < MAX_RETRIES) {
          const success = await attemptLoad();
          if (success) break;

          retryCount++;
          if (retryCount < MAX_RETRIES) {
            console.log(`Retrying... (${retryCount}/${MAX_RETRIES})`);
            await new Promise((resolve) =>
              setTimeout(resolve, 1000 * retryCount)
            );
          }
        }

        if (retryCount >= MAX_RETRIES) {
          console.error("Max retries reached. Clearing state.");
          controller.stop();
          dispatch(setListenSession(null));
          dispatch(setListenSessionProcedure(null));
          dispatch(
            showAlert({
              type: "error",
              title: "Playback Failed",
              description:
                "Unable to load audio after multiple attempts. Please try again later.",
              isAutoClose: true,
              isClosable: true,
            })
          );
        }
      } finally {
        controller.setLoadingSession(false);
      }
    },
    [listenToEpisode, controller, dispatch]
  );

  // Hàm Listen To Episode From Saved Episodes
  // Gọi API xong là play luôn, và luôn play từ đầu (seekTo = 0)
  const playEpisodeFromSavedEpisodes = useCallback(
    async (opts: { audioId: string; benefitsList: SubscriptionBenefit[] }) => {
      if (controller.isCurrentlyLoadingSession()) {
        console.warn("Already loading a session, please wait");
        return;
      }

      const { audioId, benefitsList } = opts;

      controller.setLoadingSession(true, audioId);

      let retryCount = 0;
      const MAX_RETRIES = 3;

      const attemptLoad = async (): Promise<boolean> => {
        try {
          const listenSlot = await getPodcastListenSlotCount().unwrap();

          if (benefitsList.length > 0) {
            if (!benefitsList.find((b) => b.Id === 1)) {
              if (listenSlot <= 0) {
                dispatch(
                  showAlert({
                    type: "warning",
                    title: "Listen Slot Exceeded",
                    description:
                      "You have used up all your free listen slots. Please wait for more slots to become available.",
                    isAutoClose: false,
                    isClosable: true,
                  })
                );
                return false;
              }
            }
          } else {
            if (listenSlot <= 0) {
              dispatch(
                showAlert({
                  type: "warning",
                  title: "Listen Slot Exceeded",
                  description:
                    "You have used up all your free listen slots. Please wait for more slots to become available.",
                  isAutoClose: false,
                  isClosable: true,
                })
              );
              return false;
            }
          }

          const res = await listenToEpisode({
            PodcastEpisodeId: audioId,
            SourceType: "SavedEpisodes",
            CurrentPodcastSubscriptionRegistrationBenefitList: benefitsList,
          }).unwrap();

          const session = res.ListenSession as ListenSessionEpisodes;
          const procedure =
            res.ListenSessionProcedure as ListenSessionProcedure;

          dispatch(setListenSession(session));
          dispatch(setListenSessionProcedure(procedure));

          await controller.playFromExistingSession({
            session,
            procedure,
            sourceType: "SavedEpisodes",
            seekTo: 0,
            isSeekThenPlay: true,
          });
          return true;
        } catch (error) {
          console.error(
            `Error in playEpisodeFromSavedEpisodes (attempt ${
              retryCount + 1
            }):`,
            error
          );
          return false;
        }
      };

      try {
        while (retryCount < MAX_RETRIES) {
          const success = await attemptLoad();
          if (success) break;

          retryCount++;
          if (retryCount < MAX_RETRIES) {
            console.log(`Retrying... (${retryCount}/${MAX_RETRIES})`);
            await new Promise((resolve) =>
              setTimeout(resolve, 1000 * retryCount)
            );
          }
        }

        if (retryCount >= MAX_RETRIES) {
          console.error("Max retries reached. Clearing state.");
          controller.stop();
          dispatch(setListenSession(null));
          dispatch(setListenSessionProcedure(null));
          dispatch(
            showAlert({
              type: "error",
              title: "Playback Failed",
              description:
                "Unable to load audio after multiple attempts. Please try again later.",
              isAutoClose: true,
              isClosable: true,
            })
          );
        }
      } finally {
        controller.setLoadingSession(false);
      }
    },
    [listenToEpisode, controller, dispatch, getPodcastListenSlotCount]
  );

  // Hàm Listen To Booking Track
  // Gọi API xong là play luôn, và luôn play từ đầu (seekTo = 0)
  const playBookingTrack = useCallback(
    async (opts: { bookingId: number; bookingTrackId: string }) => {
      if (controller.isCurrentlyLoadingSession()) {
        console.warn("Already loading a session, please wait");
        return;
      }

      const { bookingId, bookingTrackId } = opts;

      controller.setLoadingSession(true, bookingTrackId);

      let retryCount = 0;
      const MAX_RETRIES = 3;

      const attemptLoad = async (): Promise<boolean> => {
        try {
          const res = await listenToBookingTrack({
            BookingId: bookingId,
            BookingPodcastTrackId: bookingTrackId,
          }).unwrap();

          const session = res.ListenSession as ListenSessionBookingTracks;
          const procedure =
            res.ListenSessionProcedure as ListenSessionProcedure;

          dispatch(setListenSession(session));
          dispatch(setListenSessionProcedure(procedure));

          await controller.playFromExistingSession({
            session,
            procedure,
            sourceType: "BookingProducingTracks",
            seekTo: 0,
            isSeekThenPlay: true,
          });
          return true;
        } catch (error) {
          console.error(
            `Error in playBookingTrack (attempt ${retryCount + 1}):`,
            error
          );
          return false;
        }
      };

      try {
        while (retryCount < MAX_RETRIES) {
          const success = await attemptLoad();
          if (success) break;

          retryCount++;
          if (retryCount < MAX_RETRIES) {
            console.log(`Retrying... (${retryCount}/${MAX_RETRIES})`);
            await new Promise((resolve) =>
              setTimeout(resolve, 1000 * retryCount)
            );
          }
        }

        if (retryCount >= MAX_RETRIES) {
          console.error("Max retries reached. Clearing state.");
          controller.stop();
          dispatch(setListenSession(null));
          dispatch(setListenSessionProcedure(null));
          dispatch(
            showAlert({
              type: "error",
              title: "Playback Failed",
              description:
                "Unable to load audio after multiple attempts. Please try again later.",
              isAutoClose: true,
              isClosable: true,
            })
          );
        }
      } finally {
        controller.setLoadingSession(false);
      }
    },
    [listenToBookingTrack, controller, dispatch]
  );

  // Hàm Play From Latest
  const playFromLatest = useCallback(async () => {
    try {
      // Gọi tuần tự thay vì song song để tránh conflict/cancel
      console.log("FETCHING EPISODE NÈ...");
      // const resEpisode = await triggerEpisodeSession(undefined).unwrap();
      const resEpisode = await getEpisodeLatestSession();
      console.log("EPISODE LATEST REPONSE:", resEpisode);

      console.log("FETCHING BOOKING NÈ ...");
      // const resBooking = await triggerBookingSession(undefined).unwrap();
      const resBooking = await getBookingLatestSession();
      console.log("BOOKING LATEST RESPONSE:", resBooking);
      // if (!resEpisode.ListenSession && !resBooking.ListenSession) return;
      if (resEpisode.ListenSession && !resBooking.ListenSession) {
        const session = resEpisode.ListenSession as ListenSessionEpisodes;
        const latestPosition =
          session.PodcastEpisodeListenSession.LastListenDurationSeconds || 0;
        const procedure =
          resEpisode.ListenSessionProcedure as ListenSessionProcedure;

        dispatch(setListenSession(session));
        console.log("Latest Listen Session:", session);
        dispatch(setListenSessionProcedure(procedure));

        await controller.playFromExistingSession({
          session,
          procedure,
          sourceType: procedure?.SourceDetail.Type || "SpecifyShowEpisodes",
          seekTo: latestPosition,
          isSeekThenPlay: false,
        });
        return;
      }
      if (!resEpisode.ListenSession && resBooking.ListenSession) {
        // Handle Với Booking Track
        const session = resBooking.ListenSession as ListenSessionBookingTracks;
        const latestPosition =
          session.BookingPodcastTrackListenSession.LastListenDurationSeconds ||
          0;
        const procedure =
          resBooking.ListenSessionProcedure as ListenSessionProcedure;

        dispatch(setListenSession(session));
        console.log("Latest Listen Session:", session);
        dispatch(setListenSessionProcedure(procedure));

        await controller.playFromExistingSession({
          session,
          procedure,
          sourceType: "BookingProducingTracks",
          seekTo: latestPosition,
          isSeekThenPlay: false,
        });
        return;
      }

      if (resEpisode.ListenSession && resBooking.ListenSession) {
        // Lỗi =>  trả về luôn
        return;
      }
    } catch (error) {
      console.error("playFromLatest error:", error);
    } finally {
      console.log("playFromLatest finally done.");
    }
  }, [triggerEpisodeSession, triggerBookingSession, controller, dispatch]);

  // Hàm Navigate dành riêng cho Specify Show, có thể truyền benefitList
  const navigateInSpecifyShow = useCallback(
    async (opts?: {
      benefitList: SubscriptionBenefit[];
      navigateType: "Next" | "Previous";
    }) => {
      const { benefitList, navigateType } = opts || {};
      const session = (controller as any)[
        "currentSession"
      ] as ListenSessionEpisodes | null;
      const procedure = (controller as any)[
        "currentProcedure"
      ] as ListenSessionProcedure | null;
      if (!session || !procedure) return;

      // Bắt đầu gọi API để lấy session, procedure mới
      const res = await navigateEpisode({
        ListenSessionNavigateType: navigateType ? navigateType : "Next",
        ListenSessionId: session.PodcastEpisodeListenSession.Id,
        ListenSessionProcedureId: procedure.Id,
        CurrentPodcastSubscriptionRegistrationBenefitList: benefitList
          ? benefitList
          : [],
      }).unwrap();

      const newProc = res.ListenSessionProcedure as ListenSessionProcedure;
      if (!res.ListenSession) {
        dispatch(setListenSessionProcedure(newProc));
        controller.pause.bind(controller)();
        return;
      }
      const newSession = res.ListenSession as ListenSessionEpisodes;

      dispatch(setListenSession(newSession));
      dispatch(setListenSessionProcedure(newProc));

      await controller.switchToSession({
        session: newSession,
        procedure: newProc,
        sourceType: "SpecifyShowEpisodes",
        seekTo: 0,
        isSeekThenPlay: true,
      });
    },
    [navigateEpisode, controller, dispatch]
  );

  // Hàm Navigate dành riêng cho Saved Episodes
  const navigateInSavedEpisodes = useCallback(
    async (opts?: { navigateType: "Next" | "Previous" }) => {
      const { navigateType } = opts || {};
      const session = (controller as any)[
        "currentSession"
      ] as ListenSessionEpisodes | null;
      const procedure = (controller as any)[
        "currentProcedure"
      ] as ListenSessionProcedure | null;
      if (!session || !procedure) return;

      // Bắt đầu gọi API để lấy session, procedure mới
      const res = await navigateEpisode({
        ListenSessionNavigateType: navigateType ? navigateType : "Next",
        ListenSessionId: session.PodcastEpisodeListenSession.Id,
        ListenSessionProcedureId: procedure.Id,
        CurrentPodcastSubscriptionRegistrationBenefitList: null,
      }).unwrap();
      const newProc = res.ListenSessionProcedure as ListenSessionProcedure;

      if (!res.ListenSession) {
        dispatch(setListenSessionProcedure(newProc));
        controller.pause.bind(controller)();
        return;
      }

      const newSession = res.ListenSession as ListenSessionEpisodes;

      dispatch(setListenSession(newSession));
      dispatch(setListenSessionProcedure(newProc));

      await controller.switchToSession({
        session: newSession,
        procedure: newProc,
        sourceType: "SavedEpisodes",
        seekTo: 0,
        isSeekThenPlay: true,
      });
    },
    [navigateEpisode, controller, dispatch]
  );

  // Hàm Navigate dành riêng cho Booking Tracks
  const navigateInBookingTracks = useCallback(
    async (opts?: { navigateType: "Next" | "Previous" }) => {
      const { navigateType } = opts || {};
      const session = (controller as any)[
        "currentSession"
      ] as ListenSessionBookingTracks | null;
      const procedure = (controller as any)[
        "currentProcedure"
      ] as ListenSessionProcedure | null;
      if (!session || !procedure) return;
      // Bắt đầu gọi API để lấy session, procedure mới
      const res = await navigateBookingTrack({
        ListenSessionNavigateType: navigateType ? navigateType : "Next",
        ListenSessionId: session.BookingPodcastTrackListenSession.Id,
        ListenSessionProcedureId: procedure.Id,
        CurrentPodcastSubscriptionRegistrationBenefitList: null,
      }).unwrap();

      const newProc = res.ListenSessionProcedure as ListenSessionProcedure;

      if (!res.ListenSession) {
        dispatch(setListenSessionProcedure(newProc));
        controller.pause.bind(controller)();
        return;
      }
      const newSession = res.ListenSession as ListenSessionBookingTracks;

      dispatch(setListenSession(newSession));
      dispatch(setListenSessionProcedure(newProc));

      await controller.switchToSession({
        session: newSession,
        procedure: newProc,
        sourceType: "BookingProducingTracks",
        seekTo: 0,
        isSeekThenPlay: true,
      });
    },
    [navigateBookingTrack, controller, dispatch]
  );

  // Hàm update play mode
  const handleUpdatePlayMode = useCallback(
    async (otps: {
      change: "AutoPlay" | "OrderMode";
      PlayOrderMode?: "Sequential" | "Random";
      IsAutoPlay?: boolean;
    }) => {
      console.log("handleUpdatePlayMode called with:", otps);
      const { change, PlayOrderMode, IsAutoPlay } = otps;
      if (!player.listenSessionProcedure) {
        console.log("No listenSessionProcedure available");
        console.log("Current player state:", player);
        return;
      }
      if (change === "AutoPlay") {
        if (IsAutoPlay === undefined) {
          return;
        } else {
          // Call API to update AutoPlay mode
          await updatePlayMode({
            CustomerListenSessionProcedureId: player.listenSessionProcedure?.Id,
            IsAutoPlay: IsAutoPlay,
            PlayOrderMode: player.listenSessionProcedure?.PlayOrderMode,
          }).unwrap();
        }
      } else if (change === "OrderMode") {
        if (!PlayOrderMode) {
          return;
        } else {
          // Call API to update Play Order Mode
          await updatePlayMode({
            CustomerListenSessionProcedureId: player.listenSessionProcedure?.Id,
            IsAutoPlay: player.listenSessionProcedure?.IsAutoPlay,
            PlayOrderMode: PlayOrderMode,
          }).unwrap();
        }
      }
    },
    [player, updatePlayMode]
  );

  return {
    state,
    playEpisodeFromSpecifyShow,
    playEpisodeFromSavedEpisodes,
    playBookingTrack,
    playContinueListening,
    playFromLatest,
    navigateInSavedEpisodes,
    navigateInSpecifyShow,
    navigateInBookingTracks,
    handleUpdatePlayMode,
    play: controller.play.bind(controller),
    pause: controller.pause.bind(controller),
    stop: controller.stop.bind(controller),
    seek: controller.seek.bind(controller),
    setVolume: controller.setVolume.bind(controller),
  };
}

// usePlayer.ts
import { useEffect, useState, useCallback } from "react";
import {
  getPlayerController,
  type PlayerUiState,
  type SourceType,
} from "@/core/services/player/playerController";
import {
  useListenToEpisodeMutation,
  useListenToBookingTrackMutation,
  useNavigateEpisodeInProcedureMutation,
  useNavigateBookingTrackInProcedureMutation,
  useLazyGetEpisodeLatestSessionQuery,
  useLazyGetBookingLatestSessionQuery,
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
  // Lazy query vì chỉ gọi khi cần
  const [triggerBookingSession] = useLazyGetBookingLatestSessionQuery();
  const [triggerEpisodeSession] = useLazyGetEpisodeLatestSessionQuery();

  // Navigate
  const [navigateEpisode] = useNavigateEpisodeInProcedureMutation();
  const [navigateBookingTrack] = useNavigateBookingTrackInProcedureMutation();

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

    return unsubscribe;
  }, [controller]);

  // ====== Các hàm public gọi từ UI ======

  // Hàm Listen To Episode From Specify Show
  // Gọi API xong là play luôn, và luôn play từ đầu (seekTo = 0)
  const playEpisodeFromSpecifyShow = useCallback(
    async (opts: { audioId: string; benefitsList: SubscriptionBenefit[] }) => {
      const { audioId, benefitsList = [] } = opts;

      const res = await listenToEpisode({
        PodcastEpisodeId: audioId,
        SourceType: "SpecifyShowEpisodes",
        CurrentPodcastSubscriptionRegistrationBenefitList: benefitsList,
      }).unwrap();

      const session = res.ListenSession as ListenSessionEpisodes;
      const procedure = res.ListenSessionProcedure as ListenSessionProcedure;

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
    },
    [listenToEpisode, controller, dispatch]
  );

  // Hàm continue listening, đây là type của Specify Show Episodes
  const playContinueListening = useCallback(
    async (opts: {
      audioId: string;
      continueSessionId?: string;
      benefitsList: SubscriptionBenefit[];
      seekTo: number;
    }) => {
      const { audioId, continueSessionId, benefitsList = [], seekTo } = opts;

      const res = await listenToEpisode({
        PodcastEpisodeId: audioId,
        SourceType: "SpecifyShowEpisodes",
        CurrentPodcastSubscriptionRegistrationBenefitList: benefitsList,
        continue_listen_session_id: continueSessionId,
      }).unwrap();

      const session = res.ListenSession as ListenSessionEpisodes;
      const procedure = res.ListenSessionProcedure as ListenSessionProcedure;

      dispatch(setListenSession(session));
      dispatch(setListenSessionProcedure(procedure));

      await controller.playFromExistingSession({
        session,
        procedure,
        sourceType: "SpecifyShowEpisodes",
        seekTo: seekTo,
        isSeekThenPlay: true,
      });
    },
    [listenToEpisode, controller, dispatch]
  );

  // Hàm Listen To Episode From Saved Episodes
  // Gọi API xong là play luôn, và luôn play từ đầu (seekTo = 0)
  const playEpisodeFromSavedEpisodes = useCallback(
    async (opts: { audioId: string; benefitsList: SubscriptionBenefit[] }) => {
      const { audioId, benefitsList } = opts;

      const res = await listenToEpisode({
        PodcastEpisodeId: audioId,
        SourceType: "SavedEpisodes",
        CurrentPodcastSubscriptionRegistrationBenefitList: benefitsList,
      }).unwrap();

      const session = res.ListenSession as ListenSessionEpisodes;
      const procedure = res.ListenSessionProcedure as ListenSessionProcedure;

      dispatch(setListenSession(session));
      dispatch(setListenSessionProcedure(procedure));

      await controller.playFromExistingSession({
        session,
        procedure,
        sourceType: "SavedEpisodes",
        seekTo: 0,
        isSeekThenPlay: true,
      });
    },
    [listenToEpisode, controller, dispatch]
  );

  // Hàm Listen To Booking Track
  // Gọi API xong là play luôn, và luôn play từ đầu (seekTo = 0)
  const playBookingTrack = useCallback(
    async (opts: { bookingId: number; bookingTrackId: string }) => {
      const { bookingId, bookingTrackId } = opts;

      const res = await listenToBookingTrack({
        BookingId: bookingId,
        BookingPodcastTrackId: bookingTrackId,
      }).unwrap();

      const session = res.ListenSession as ListenSessionBookingTracks;
      const procedure = res.ListenSessionProcedure as ListenSessionProcedure;

      dispatch(setListenSession(session));
      dispatch(setListenSessionProcedure(procedure));

      await controller.playFromExistingSession({
        session,
        procedure,
        sourceType: "BookingProducingTracks",
        seekTo: 0,
        isSeekThenPlay: true,
      });
    },
    [listenToBookingTrack, controller, dispatch]
  );

  // Hàm Play From Latest
  const playFromLatest = useCallback(async () => {
    try {
      // Gọi lazy query, dùng unwrap cho gọn
      const [resEpisode, resBooking] = await Promise.all([
        triggerEpisodeSession().unwrap(),
        triggerBookingSession().unwrap(),
      ]);

      if (!resEpisode.ListenSession && !resBooking.ListenSession) return;
      if (resEpisode.ListenSession && !resBooking.ListenSession) {
        // Handle Với Episode
        const session = resEpisode.ListenSession as ListenSessionEpisodes;
        const latestPosition =
          session.PodcastEpisodeListenSession.LastListenDurationSeconds || 0;
        const procedure =
          resEpisode.ListenSessionProcedure as ListenSessionProcedure;

        dispatch(setListenSession(session));
        console.log("procedure in playFromLatest:", procedure);
        console.log("Dispatch setListenSessionProcedure with:", procedure);
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
        console.log("procedure in playFromLatest (booking):", procedure);
        console.log(
          "Dispatch setListenSessionProcedure with (booking):",
          procedure
        );
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
          const response = await updatePlayMode({
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
          const response = await updatePlayMode({
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

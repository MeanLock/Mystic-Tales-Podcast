// src/core/player/usePlayerNavigate.ts
import { useCallback, useMemo } from "react";

import {
  pauseAudio,
  stopAudio,
  setListenSession,
  setListenSessionProcedure,
  setCurrentAudio,
  setIsNextSessionNull,
} from "@/src/features/mediaPlayer/playerSlice";
import {
  ListenSessionBookingTracks,
  ListenSessionEpisodes,
  ListenSessionProcedure,
} from "@/src/core/types/audio.type";
import {
  useNavigateEpisodeInProcedureMutation,
  useNavigateBookingTrackInProcedureMutation,
} from "@/src/core/services/player/playerService";
import { useGetSubscriptionBenefitsMapListFromEpisodeIdQuery } from "@/src/core/services/subscription/subscription.service";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "@/src/store/store";

export const usePlayerNavigate = () => {
  const dispatch = useDispatch();
  const playerState = useSelector((s: RootState) => s.player);

  const [navigateEpisodeInProcedure] = useNavigateEpisodeInProcedureMutation();
  const [navigateBookingTrackInProcedure] =
    useNavigateBookingTrackInProcedureMutation();

  const listenSession = playerState.listenSession;
  const listenSessionProcedure =
    playerState.listenSessionProcedure as ListenSessionProcedure | null;

  const sourceType = listenSessionProcedure?.SourceDetail.Type;

  // EpisodeId hiện tại để lấy benefit list (chỉ áp dụng cho episode)
  const currentEpisodeId =
    sourceType !== "BookingProducingTracks" &&
    listenSession &&
    "PodcastEpisode" in listenSession
      ? (listenSession as ListenSessionEpisodes).PodcastEpisode.Id
      : undefined;

  const { data: benefitData } =
    useGetSubscriptionBenefitsMapListFromEpisodeIdQuery(
      { PodcastEpisodeId: currentEpisodeId! },
      { skip: !currentEpisodeId }
    );

  /** === canNavigate: copy y chang logic bạn đưa === */
  const canNavigate = useMemo(() => {
    if (!listenSessionProcedure || !listenSession) {
      return false;
    }

    if (!playerState.playMode.isNextSessionNull) {
      return true;
    }

    let availableAudioCount = 0;

    if (
      listenSessionProcedure.ListenObjectsRandomOrder &&
      listenSessionProcedure.ListenObjectsRandomOrder.length > 0
    ) {
      const availableAudio =
        listenSessionProcedure.ListenObjectsRandomOrder.filter(
          (item) => item.IsListenable
        );
      availableAudioCount = availableAudio.length;
    } else if (
      listenSessionProcedure.ListenObjectsSequentialOrder &&
      listenSessionProcedure.ListenObjectsSequentialOrder.length > 0
    ) {
      const availableAudio =
        listenSessionProcedure.ListenObjectsSequentialOrder.filter(
          (item) => item.IsListenable
        );
      availableAudioCount = availableAudio.length;
    }

    return availableAudioCount > 0;
  }, [
    listenSession,
    listenSessionProcedure,
    playerState.playMode.isNextSessionNull,
  ]);

  /** === Hàm navigate core: dùng chung cho Next / Previous === */
  const navigate = useCallback(
    async (navigateType: "Next" | "Previous") => {
      if (!listenSession || !listenSessionProcedure) {
        dispatch(pauseAudio());
        return;
      }

      // Nếu UI / auto-call mà thực ra không navigate được nữa → pause luôn
      if (!canNavigate) {
        dispatch(pauseAudio());
        return;
      }

      try {
        if (
          listenSessionProcedure.SourceDetail.Type === "BookingProducingTracks"
        ) {
          // ==== Navigate booking track ====
          const s = listenSession as ListenSessionBookingTracks;

          const res = await navigateBookingTrackInProcedure({
            ListenSessionNavigateType: navigateType,
            ListenSessionId: s.BookingPodcastTrackListenSession.Id,
            ListenSessionProcedureId: listenSessionProcedure.Id,
            CurrentPodcastSubscriptionRegistrationBenefitList: null,
          }).unwrap();

          const newSession = res.ListenSession;

          if (newSession) {
            const ls = newSession as ListenSessionBookingTracks;
            dispatch(setListenSession(ls));
            dispatch(setListenSessionProcedure(res.ListenSessionProcedure));
            dispatch(
              setCurrentAudio({
                Id: ls.BookingPodcastTrack.Id,
                Name: ls.BookingPodcastTrack.BookingRequirementName,
                PodcasterName: ls.Booking.Title,
                MainImageFileKey: "",
                AudioLength: 0,
              })
            );
            dispatch(setIsNextSessionNull(false));
          } else {
            // Không còn track theo hướng navigate đó
            dispatch(setIsNextSessionNull(true));
            dispatch(pauseAudio());
          }
        } else {
          // ==== Navigate episode ====
          const s = listenSession as ListenSessionEpisodes;

          const benefits =
            listenSessionProcedure.SourceDetail.Type === "SpecifyShowEpisodes"
              ? benefitData?.CurrentPodcastSubscriptionRegistrationBenefitList ??
                []
              : null;

          const res = await navigateEpisodeInProcedure({
            ListenSessionNavigateType: navigateType,
            ListenSessionId: s.PodcastEpisodeListenSession.Id,
            ListenSessionProcedureId: listenSessionProcedure.Id,
            CurrentPodcastSubscriptionRegistrationBenefitList: benefits,
          }).unwrap();

          const newSession = res.ListenSession;

          if (newSession) {
            const ls = newSession as ListenSessionEpisodes;
            dispatch(setListenSession(ls));
            dispatch(setListenSessionProcedure(res.ListenSessionProcedure));
            dispatch(
              setCurrentAudio({
                Id: ls.PodcastEpisode.Id,
                Name: ls.PodcastEpisode.Name,
                PodcasterName: ls.Podcaster.FullName,
                MainImageFileKey: ls.PodcastEpisode.MainImageFileKey,
                AudioLength: ls.PodcastEpisode.AudioLength,
              })
            );
            dispatch(setIsNextSessionNull(false));
          } else {
            dispatch(setIsNextSessionNull(true));
            dispatch(pauseAudio());
          }
        }
      } catch (e) {
        // Navigate lỗi -> stop player
        dispatch(stopAudio());
      }
    },
    [
      listenSession,
      listenSessionProcedure,
      canNavigate,
      benefitData,
      dispatch,
      navigateBookingTrackInProcedure,
      navigateEpisodeInProcedure,
    ]
  );

  return {
    canNavigate, // dùng chung cho Next & Previous
    navigateNext: () => navigate("Next"),
    navigatePrevious: () => navigate("Previous"),
  };
};

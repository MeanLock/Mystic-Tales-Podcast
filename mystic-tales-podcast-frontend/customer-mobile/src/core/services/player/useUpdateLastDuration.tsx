import { useDispatch, useSelector } from "react-redux";
import {
  useUpdateBookingTrackLastDurationMutation,
  useUpdateEpisodeLastDurationMutation,
} from "./playerService";
import { useEffect } from "react";
import { usePlayer } from "./usePlayer";
import { playerEngine } from "./playerEngine";
import type {
  ListenSessionEpisodes,
  ListenSessionBookingTracks,
} from "@/src/core/types/audio.type";
import { setDataAndShowAlert } from "@/src/features/alert/alertSlice";
import { useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery } from "../subscription/subscription.service";
import { RootState } from "@/src/store/store";

const useUpdateLastDurationListener = () => {
  // REDUX
  const dispatch = useDispatch();
  const listenSession = useSelector(
    (state: RootState) => state.player.listenSession
  );

  // PLAYER STATE
  const {
    state,
    navigateInBookingTracks,
    navigateInSavedEpisodes,
    navigateInSpecifyShows,
  } = usePlayer();

  // RTK QUERY HOOKS
  const [updateEpisodeLastDuration] = useUpdateEpisodeLastDurationMutation();
  const [updateBookingTrackLastDuration] =
    useUpdateBookingTrackLastDurationMutation();
  const [triggerGetBenefitList] =
    useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery();

  // UPDATE LAST DURATION EFFECT
  useEffect(() => {
    if (!state.isPlaying || !state.currentAudio || !listenSession) {
      console.log(
        "⏸️ Player is not playing or no current audio/listen session. Skipping last duration update."
      );
      return;
    }

    console.log("🎵 Setting up last duration update interval...");
    console.log("🎵 Current Audio ID:", state.currentAudio.id);
    console.log("📂 Source Type:", state.sourceType);

    const intervalId = setInterval(async () => {
      // Get fresh state from playerEngine
      const currentTimeSeconds = Math.floor(state.currentTime);

      console.log(
        "⏱️ Updating last duration for source type:",
        state.sourceType,
        "at",
        currentTimeSeconds,
        "seconds"
      );

      try {
        if (
          state.sourceType === "SavedEpisodes" ||
          state.sourceType === "SpecifyShowEpisodes"
        ) {
          // Update Episode Last Duration
          const episodeListenSession = listenSession as ListenSessionEpisodes;

          let benefitList: any[] = [];
          const benefitData = await triggerGetBenefitList({
            PodcastEpisodeId: episodeListenSession.PodcastEpisode.Id,
          }).unwrap();

          if (
            benefitData &&
            benefitData.CurrentPodcastSubscriptionRegistrationBenefitList
          ) {
            benefitList =
              benefitData.CurrentPodcastSubscriptionRegistrationBenefitList;
          }

          await updateEpisodeLastDuration({
            PodcastEpisodeListenSessionId:
              episodeListenSession.PodcastEpisodeListenSession.Id,
            LastListenDurationSeconds: currentTimeSeconds,
            CurrentPodcastSubscriptionRegistrationBenefitList: benefitList,
          }).unwrap();

          console.log("✅ Episode last duration updated successfully");
        } else if (state.sourceType === "BookingProducingTracks") {
          // Update Booking Track Last Duration
          const bookingListenSession =
            listenSession as ListenSessionBookingTracks;

          await updateBookingTrackLastDuration({
            BookingPodcastTrackListenSessionId:
              bookingListenSession.BookingPodcastTrackListenSession.Id,
            LastListenDurationSeconds: currentTimeSeconds,
          }).unwrap();

          console.log("✅ Booking track last duration updated successfully");
        }
      } catch (error) {
        console.error("❌ Error updating last duration:", error);
        // Stop playback and show alert on error (session expired)
        playerEngine.stopAndUnload();
        dispatch(
          setDataAndShowAlert({
            title: "Session Expired",
            description:
              "Your listening session has expired. Please start listening again.",
            type: "error",
            isCloseable: true,
            isFunctional: false,
            autoCloseDuration: 5,
          })
        );
      }
    }, 2000); // Update every 2 seconds

    return () => {
      console.log("🧹 Cleaning up last duration update interval");
      clearInterval(intervalId);
    };
  }, [state.isPlaying, state.currentAudio?.id]);

  // Handle Audio End Event
  useEffect(() => {
    const handleAudioEnd = () => {
      console.log("🎵 Audio has ended!");

      // Get fresh state from playerEngine to avoid stale closure
      const currentState = playerEngine.getState();
      console.log("🎵 isAutoPlay:", currentState.isAutoPlay);
      console.log("🎵 Source Type:", currentState.sourceType);
      console.log("🎵 listenSession:", currentState.listenSession !== null);
      console.log(
        "🎵 listenSessionProcedure:",
        currentState.listenSessionProcedure !== null
      );

      // Implement auto-play logic here if isAutoPlay is true
      if (currentState.isAutoPlay) {
        console.log("🎵 Auto-play is enabled - should play next track");

        if (
          !currentState.listenSessionProcedure ||
          !currentState.listenSession
        ) {
          console.log("🎵 No session procedure or session - cannot auto-play");
          return;
        }

        if (currentState.sourceType === "SpecifyShowEpisodes") {
          const ls = currentState.listenSession as ListenSessionEpisodes;
          console.log("🎵 Navigating to next in SpecifyShowEpisodes");
          navigateInSpecifyShows(
            "Next",
            ls,
            currentState.listenSessionProcedure
          );
        } else if (currentState.sourceType === "SavedEpisodes") {
          const ls = currentState.listenSession as ListenSessionEpisodes;
          console.log("🎵 Navigating to next in SavedEpisodes");
          navigateInSavedEpisodes(
            "Next",
            ls,
            currentState.listenSessionProcedure
          );
        } else if (currentState.sourceType === "BookingProducingTracks") {
          const ls = currentState.listenSession as ListenSessionBookingTracks;
          console.log("🎵 Navigating to next in BookingProducingTracks");
          navigateInBookingTracks(
            "Next",
            ls,
            currentState.listenSessionProcedure
          );
        }
      } else {
        console.log("🎵 Auto-play is disabled - not playing next track");
      }
    };

    playerEngine.setOnAudioEndCallback(handleAudioEnd);

    return () => {
      playerEngine.setOnAudioEndCallback(null);
    };
  }, [
    navigateInSpecifyShows,
    navigateInSavedEpisodes,
    navigateInBookingTracks,
  ]);

  return null;
};

export default useUpdateLastDurationListener;

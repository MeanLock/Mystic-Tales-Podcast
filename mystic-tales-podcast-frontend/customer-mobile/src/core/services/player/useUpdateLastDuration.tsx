import { useDispatch, useSelector } from "react-redux";
import {
  useUpdateBookingTrackLastDurationMutation,
  useUpdateEpisodeLastDurationMutation,
} from "./playerService";
import { useEffect, useRef } from "react";
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
  // const listenSession = useSelector(
  //   (state: RootState) => state.player.listenSession
  // );

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

  // Prevent concurrent updates
  const isUpdating = useRef(false);

  // UPDATE LAST DURATION EFFECT
  useEffect(() => {
    if (!state.isPlaying || !state.currentAudio || !state.listenSession) {
      if (__DEV__) {
        console.log("[UpdateDuration] Skipping - not playing or no session");
      }
      return;
    }

    if (__DEV__) {
      console.log("[UpdateDuration] Setting up interval");
      console.log("[UpdateDuration] Audio ID:", state.currentAudio.id);
      console.log("[UpdateDuration] Source:", state.sourceType);
    }

    const intervalId = setInterval(async () => {
      // Prevent concurrent updates
      if (isUpdating.current) {
        if (__DEV__) {
          console.warn("[UpdateDuration] Already updating, skipping");
        }
        return;
      }

      isUpdating.current = true;

      // CRITICAL: Get FRESH state from playerEngine to avoid stale closure
      // Reading from `state` would give stale value because dependencies don't include state.currentTime
      const freshState = playerEngine.getState();
      const currentTimeSeconds = Math.floor(freshState.currentTime);

      if (__DEV__) {
        console.log(
          `[UpdateDuration] Updating ${freshState.sourceType} at ${currentTimeSeconds}s`
        );
      }

      try {
        if (
          freshState.sourceType === "SavedEpisodes" ||
          freshState.sourceType === "SpecifyShowEpisodes"
        ) {
          // Update Episode Last Duration
          const episodeListenSession =
            freshState.listenSession as ListenSessionEpisodes;

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

          if (__DEV__) {
            console.log("[UpdateDuration] ✅ Episode updated");
          }
        } else if (freshState.sourceType === "BookingProducingTracks") {
          // Update Booking Track Last Duration
          const bookingListenSession =
            freshState.listenSession as ListenSessionBookingTracks;

          // Safety check for nested properties
          if (!bookingListenSession?.BookingPodcastTrackListenSession?.Id) {
            if (__DEV__) {
              console.warn(
                "[UpdateDuration] Booking session structure invalid, skipping"
              );
            }
            return;
          }

          await updateBookingTrackLastDuration({
            BookingPodcastTrackListenSessionId:
              bookingListenSession.BookingPodcastTrackListenSession.Id,
            LastListenDurationSeconds: currentTimeSeconds,
          }).unwrap();

          if (__DEV__) {
            console.log("[UpdateDuration] ✅ Booking updated");
          }
        }
      } catch (error) {
        if (__DEV__) {
          console.error("[UpdateDuration] Error:", error);
        }

        // Stop playback and show alert on error (session expired)
        await stop();

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
      } finally {
        isUpdating.current = false;
      }
    }, 2000); // Update every 2 seconds

    return () => {
      if (__DEV__) {
        console.log("[UpdateDuration] Cleaning up interval");
      }
      isUpdating.current = false;
      clearInterval(intervalId);
    };
  }, [
    state.isPlaying,
    state.currentAudio?.id,
    state.sourceType,
    state.listenSession,
    updateEpisodeLastDuration,
    updateBookingTrackLastDuration,
    triggerGetBenefitList,
    dispatch,
  ]);

  // Handle Audio End Event
  useEffect(() => {
    const handleAudioEnd = async () => {
      if (__DEV__) {
        console.log("[AudioEnd] Audio has ended!");
      }

      // Get fresh state from playerEngine to avoid stale closure
      const currentState = playerEngine.getState();

      if (__DEV__) {
        console.log("[AudioEnd] isAutoPlay:", currentState.isAutoPlay);
        console.log("[AudioEnd] Source:", currentState.sourceType);
        console.log(
          "[AudioEnd] hasSession:",
          currentState.listenSession !== null
        );
        console.log(
          "[AudioEnd] hasProcedure:",
          currentState.listenSessionProcedure !== null
        );
      }

      // Update last duration to full duration when audio ends
      if (currentState.listenSession && currentState.duration > 0) {
        const finalDurationSeconds = Math.floor(currentState.duration);

        if (__DEV__) {
          console.log(
            "[AudioEnd] Updating final duration:",
            finalDurationSeconds
          );
        }

        try {
          if (
            currentState.sourceType === "SavedEpisodes" ||
            currentState.sourceType === "SpecifyShowEpisodes"
          ) {
            const episodeListenSession =
              currentState.listenSession as ListenSessionEpisodes;

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
              LastListenDurationSeconds: finalDurationSeconds,
              CurrentPodcastSubscriptionRegistrationBenefitList: benefitList,
            }).unwrap();

            if (__DEV__) {
              console.log("[AudioEnd] ✅ Episode final duration updated");
            }
          } else if (currentState.sourceType === "BookingProducingTracks") {
            const bookingListenSession =
              currentState.listenSession as ListenSessionBookingTracks;

            if (bookingListenSession?.BookingPodcastTrackListenSession?.Id) {
              await updateBookingTrackLastDuration({
                BookingPodcastTrackListenSessionId:
                  bookingListenSession.BookingPodcastTrackListenSession.Id,
                LastListenDurationSeconds: finalDurationSeconds,
              }).unwrap();

              if (__DEV__) {
                console.log("[AudioEnd] ✅ Booking final duration updated");
              }
            }
          }
        } catch (error) {
          if (__DEV__) {
            console.error("[AudioEnd] Error updating final duration:", error);
          }
        }
      }

      // Implement auto-play logic here if isAutoPlay is true
      if (currentState.isAutoPlay) {
        if (__DEV__) {
          console.log("[AudioEnd] Auto-play enabled - playing next");
        }

        if (
          !currentState.listenSessionProcedure ||
          !currentState.listenSession
        ) {
          if (__DEV__) {
            console.warn("[AudioEnd] No session/procedure - cannot auto-play");
          }
          return;
        }

        if (currentState.sourceType === "SpecifyShowEpisodes") {
          const ls = currentState.listenSession as ListenSessionEpisodes;
          if (__DEV__) {
            console.log("[AudioEnd] → Next in SpecifyShowEpisodes");
          }
          navigateInSpecifyShows(
            "Next",
            ls,
            currentState.listenSessionProcedure
          );
        } else if (currentState.sourceType === "SavedEpisodes") {
          const ls = currentState.listenSession as ListenSessionEpisodes;
          if (__DEV__) {
            console.log("[AudioEnd] → Next in SavedEpisodes");
          }
          navigateInSavedEpisodes(
            "Next",
            ls,
            currentState.listenSessionProcedure
          );
        } else if (currentState.sourceType === "BookingProducingTracks") {
          const ls = currentState.listenSession as ListenSessionBookingTracks;
          if (__DEV__) {
            console.log("[AudioEnd] → Next in BookingProducingTracks");
          }
          navigateInBookingTracks(
            "Next",
            ls,
            currentState.listenSessionProcedure
          );
        }
      } else {
        if (__DEV__) {
          console.log("[AudioEnd] Auto-play disabled");
        }
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

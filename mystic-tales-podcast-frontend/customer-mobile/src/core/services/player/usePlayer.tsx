import { RootState } from "@/src/store/store";
import { useDispatch, useSelector } from "react-redux";
import { useCallback, useEffect, useState } from "react";
import {
  SubscriptionBenefit,
  useLazyGetIsHasNonQuotaAccessQuery,
  useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery,
} from "../subscription/subscription.service";
import { useLazyGetCustomerPodcastListenSlotQuery } from "../account/account.service";
import {
  useLazyGetBookingLatestSessionQuery,
  useLazyGetEpisodeLatestSessionQuery,
  useListenToBookingTrackMutation,
  useListenToEpisodeMutation,
  useNavigateBookingTrackInProcedureMutation,
  useNavigateEpisodeInProcedureMutation,
  useUpdateBookingTrackLastDurationMutation,
  useUpdateEpisodeLastDurationMutation,
} from "./playerService";
import { playerEngine, PlayerTrack, PlayerUiState } from "./playerEngine";
import { useRouter } from "expo-router";
import { setDataAndShowAlert } from "@/src/features/alert/alertSlice";
import {
  registerAlertAction,
  unregisterAlertAction,
} from "@/src/components/alert/GlobalAlert";
import {
  ListenSession,
  ListenSessionBookingTracks,
  ListenSessionEpisodes,
  ListenSessionProcedure,
} from "../../types/audio.type";
import {
  setListenSession,
  setListenSessionProcedure,
} from "@/src/features/mediaPlayer/playerSlice";

export function usePlayer() {
  // REDUX STATE AND DISPATCH
  const player = useSelector((state: RootState) => state.player);
  const user = useSelector((state: RootState) => state.auth.user);

  const dispatch = useDispatch();
  const router = useRouter();

  const actionId = "login-required-booking";
  registerAlertAction(actionId, () => {
    router.push("/(auth)/login");
    unregisterAlertAction(actionId);
  });

  // RTK Query HOOKS
  const [triggerGetBenefitList] =
    useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery();
  const [triggerGetListenSlot] = useLazyGetCustomerPodcastListenSlotQuery();
  const [triggerCheckNonQuota] = useLazyGetIsHasNonQuotaAccessQuery();

  const [listenToEpisode] = useListenToEpisodeMutation();
  const [listenToBookingTrack] = useListenToBookingTrackMutation();

  const [navigateEpisode] = useNavigateEpisodeInProcedureMutation();
  const [navigateBookingTrack] = useNavigateBookingTrackInProcedureMutation();

  const [getLatestEpisodeListenSession] = useLazyGetEpisodeLatestSessionQuery();
  const [getLatestBookingListenSession] = useLazyGetBookingLatestSessionQuery();

  const [updateEpisodeLastDuration] = useUpdateEpisodeLastDurationMutation();
  const [updateBookingTrackLastDuration] =
    useUpdateBookingTrackLastDurationMutation();

  // Player UI State
  const [state, setState] = useState<PlayerUiState>({
    isPlaying: false,
    buffering: false,
    listenSession: null,
    listenSessionProcedure: null,
    seeking: false,
    currentTime: 0,
    duration: 0,
    currentAudio: null,
    sourceType: null,
    volume: 1.0,
    isAutoPlay: false,
  });

  useEffect(() => {
    const unsubscribe = playerEngine.addUiStateListener((newState) => {
      setState(newState);
    });
    return () => {
      unsubscribe();
    };
  }, []);

  // FUNCTIONS

  // LISTEN:
  // 1. Listen From Episode
  const listenFromEpisode = useCallback(
    async (
      episodeId: string,
      sourceType: "SavedEpisodes" | "SpecifyShowEpisodes"
    ) => {
      if (!user) {
        dispatch(
          setDataAndShowAlert({
            title: "Login Required",
            description: "Please log in to listen to podcasts.",
            type: "warning",
            isCloseable: true,
            isFunctional: true,
            functionalButtonText: "Log In",
            autoCloseDuration: 5,
            actionId,
          })
        );
        return;
      }

      try {
        // Check Subscription Benefits
        let benefitList: SubscriptionBenefit[] = [];

        // Check Non-Quota Access
        const isNonQuota = await triggerCheckNonQuota({
          PodcastEpisodeId: episodeId,
        }).unwrap();

        // If not Non-Quota, check listen slots
        // If listen slots are 0, show alert and return
        if (!isNonQuota) {
          const listenSlot = (await triggerGetListenSlot().unwrap())
            .PodcastListenSlot;
          if (listenSlot <= 0) {
            dispatch(
              setDataAndShowAlert({
                title: "No Remaining Listen Slots",
                description:
                  "You have no remaining podcast listen slots. Please subscribe to a podcast plan to continue listening.",
                type: "error",
                isCloseable: true,
                isFunctional: false,
                autoCloseDuration: 10,
              })
            );
            return;
          }
        }

        const benefitData = await triggerGetBenefitList({
          PodcastEpisodeId: episodeId,
        }).unwrap();

        if (
          benefitData &&
          benefitData.CurrentPodcastSubscriptionRegistrationBenefitList
        ) {
          benefitList =
            benefitData.CurrentPodcastSubscriptionRegistrationBenefitList;
        }

        const listenResponse = await listenToEpisode({
          PodcastEpisodeId: episodeId,
          SourceType: sourceType,
          CurrentPodcastSubscriptionRegistrationBenefitList: benefitList,
        }).unwrap();

        if (!listenResponse) {
          dispatch(
            setDataAndShowAlert({
              title: "Listen Error",
              description:
                "An error occurred while trying to play the episode. Please try again later.",
              type: "error",
              isCloseable: true,
              isFunctional: false,
              autoCloseDuration: 5,
            })
          );
          return;
        }

        // Always set ListenSessionProcedure (never null)
        dispatch(
          setListenSessionProcedure(listenResponse.ListenSessionProcedure)
        );
        // Only play if ListenSession exists
        if (listenResponse.ListenSession) {
          const ls = listenResponse.ListenSession as ListenSessionEpisodes;
          const track: PlayerTrack = {
            id: ls.PodcastEpisode.Id,
            url: ls.AudioFileUrl,
            artist: ls.Podcaster.FullName,
            title: ls.PodcastEpisode.Name,
            artwork: ls.PodcastEpisode.MainImageFileKey,
          };

          playerEngine.setSourceType(sourceType);
          dispatch(setListenSession(listenResponse.ListenSession));
          await playerEngine.loadAndPlay(
            track,
            listenResponse.ListenSession,
            listenResponse.ListenSessionProcedure,
            true,
            ls.PodcastEpisodeListenSession.LastListenDurationSeconds,
            listenResponse.ListenSessionProcedure?.IsAutoPlay
          );
        }
      } catch (error) {
        console.error("Error in listenFromEpisode:", error);
        dispatch(
          setDataAndShowAlert({
            title: "Listen Error",
            description: `${error}`,
            type: "error",
            isCloseable: true,
            isFunctional: false,
            autoCloseDuration: 5,
          })
        );
      }
    },
    [
      user,
      dispatch,
      router,
      triggerCheckNonQuota,
      triggerGetListenSlot,
      triggerGetBenefitList,
      listenToEpisode,
    ]
  );

  // 2. Continue Listen From Episode
  const continueListenFromEpisode = useCallback(
    async (episodeId: string, continue_listen_session_id: string) => {
      if (!user) {
        const actionId = "login-required-continue-episode";
        registerAlertAction(actionId, () => {
          router.push("/(auth)/login");
          unregisterAlertAction(actionId);
        });

        dispatch(
          setDataAndShowAlert({
            title: "Login Required",
            description: "Please log in to listen to podcasts.",
            type: "warning",
            isCloseable: true,
            isFunctional: true,
            functionalButtonText: "Log In",
            autoCloseDuration: 5,
            actionId,
          })
        );
        return;
      }

      try {
        // Check Subscription Benefits
        let benefitList: SubscriptionBenefit[] = [];

        // Check Non-Quota Access
        const isNonQuota = await triggerCheckNonQuota({
          PodcastEpisodeId: episodeId,
        }).unwrap();

        // If not Non-Quota, check listen slots
        // If listen slots are 0, show alert and return
        if (!isNonQuota) {
          const listenSlot = (await triggerGetListenSlot().unwrap())
            .PodcastListenSlot;
          if (listenSlot <= 0) {
            dispatch(
              setDataAndShowAlert({
                title: "No Remaining Listen Slots",
                description:
                  "You have no remaining podcast listen slots. Please subscribe to a podcast plan to continue listening.",
                type: "error",
                isCloseable: true,
                isFunctional: false,
                autoCloseDuration: 10,
              })
            );
            return;
          }
        }

        const benefitData = await triggerGetBenefitList({
          PodcastEpisodeId: episodeId,
        }).unwrap();

        if (
          benefitData &&
          benefitData.CurrentPodcastSubscriptionRegistrationBenefitList
        ) {
          benefitList =
            benefitData.CurrentPodcastSubscriptionRegistrationBenefitList;
        }

        const listenResponse = await listenToEpisode({
          PodcastEpisodeId: episodeId,
          SourceType: "SpecifyShowEpisodes",
          CurrentPodcastSubscriptionRegistrationBenefitList: benefitList,
          continue_listen_session_id: continue_listen_session_id,
        }).unwrap();

        if (!listenResponse) {
          dispatch(
            setDataAndShowAlert({
              title: "Listen Error",
              description:
                "An error occurred while trying to play the episode. Please try again later.",
              type: "error",
              isCloseable: true,
              isFunctional: false,
              autoCloseDuration: 5,
            })
          );
          return;
        }

        // Always set ListenSessionProcedure (never null)
        dispatch(
          setListenSessionProcedure(listenResponse.ListenSessionProcedure)
        );

        // Only play if ListenSession exists
        if (listenResponse.ListenSession) {
          const ls = listenResponse.ListenSession as ListenSessionEpisodes;
          const track: PlayerTrack = {
            id: ls.PodcastEpisode.Id,
            url: ls.AudioFileUrl,
            artist: ls.Podcaster.FullName,
            title: ls.PodcastEpisode.Name,
            artwork: ls.PodcastEpisode.MainImageFileKey,
          };

          playerEngine.setSourceType("SpecifyShowEpisodes");
          dispatch(setListenSession(listenResponse.ListenSession));
          await playerEngine.loadAndPlay(
            track,
            listenResponse.ListenSession,
            listenResponse.ListenSessionProcedure,
            true,
            ls.PodcastEpisodeListenSession.LastListenDurationSeconds,
            listenResponse.ListenSessionProcedure?.IsAutoPlay
          );
        }
      } catch (error) {
        console.error("Error in listenFromEpisode:", error);
        dispatch(
          setDataAndShowAlert({
            title: "Listen Error",
            description: `${error}`,
            type: "error",
            isCloseable: true,
            isFunctional: false,
            autoCloseDuration: 5,
          })
        );
      }
    },
    [
      user,
      dispatch,
      router,
      triggerCheckNonQuota,
      triggerGetListenSlot,
      triggerGetBenefitList,
      listenToEpisode,
    ]
  );

  // 3. Load From Latest Listen Session and Play
  const loadFromLatestListenSessionAndPlay = useCallback(async () => {
    if (!user) {
      return;
    } else {
      const episodeListenSessionResponse =
        await getLatestEpisodeListenSession().unwrap();
      const bookingListenSessionResponse =
        await getLatestBookingListenSession().unwrap();

      const isNoEpisodeListenSession =
        !episodeListenSessionResponse.ListenSession ||
        episodeListenSessionResponse.ListenSession === null;
      const isNoBookingListenSession =
        !bookingListenSessionResponse.ListenSession ||
        bookingListenSessionResponse.ListenSession === null;

      console.log("Episode Listen Session:", episodeListenSessionResponse);
      console.log("Booking Listen Session:", bookingListenSessionResponse);
      console.log("isNoEpisodeListenSession:", isNoEpisodeListenSession);
      console.log("isNoBookingListenSession:", isNoBookingListenSession);

      if (isNoEpisodeListenSession && isNoBookingListenSession) {
        console.log("No listen sessions - returning");
        return;
      } else if (!isNoEpisodeListenSession && isNoBookingListenSession) {
        console.log("Loading Episode Listen Session");
        const ls =
          episodeListenSessionResponse.ListenSession as ListenSessionEpisodes;
        const lsp =
          episodeListenSessionResponse.ListenSessionProcedure as ListenSessionProcedure;
        // Không có listen session procedure => Chắc chắn lỗi
        if (!lsp) {
          return;
        }

        // Listen Session thì tùy
        if (ls) {
          const track: PlayerTrack = {
            id: ls.PodcastEpisode.Id,
            url: ls.AudioFileUrl,
            artist: ls.Podcaster.FullName,
            title: ls.PodcastEpisode.Name,
            artwork: ls.PodcastEpisode.MainImageFileKey,
          };
          playerEngine.setSourceType(lsp?.SourceDetail.Type);
          dispatch(
            setListenSession(episodeListenSessionResponse.ListenSession)
          );
          await playerEngine.loadAndPlay(
            track,
            episodeListenSessionResponse.ListenSession,
            episodeListenSessionResponse.ListenSessionProcedure,
            false,
            ls.PodcastEpisodeListenSession.LastListenDurationSeconds,
            episodeListenSessionResponse.ListenSessionProcedure?.IsAutoPlay
          );
        }

        dispatch(setListenSessionProcedure(lsp));
      } else if (isNoEpisodeListenSession && !isNoBookingListenSession) {
        console.log("Loading Booking Listen Session");
        const ls =
          bookingListenSessionResponse.ListenSession as ListenSessionBookingTracks;
        const lsp =
          bookingListenSessionResponse.ListenSessionProcedure as ListenSessionProcedure;
        // Không có listen session procedure => Chắc chắn lỗi
        if (!lsp) {
          return;
        }
        // Listen Session thì tùy
        if (ls) {
          const track: PlayerTrack = {
            id: ls.BookingPodcastTrack.Id,
            url: ls.AudioFileUrl,
            artist: ls.Booking.Title,
            title: ls.BookingPodcastTrack.BookingRequirementName,
            artwork: "", // Booking track không có artwork
          };
          playerEngine.setSourceType("BookingProducingTracks");
          dispatch(
            setListenSession(bookingListenSessionResponse.ListenSession)
          );
          await playerEngine.loadAndPlay(
            track,
            bookingListenSessionResponse.ListenSession,
            bookingListenSessionResponse.ListenSessionProcedure,
            false,
            ls.BookingPodcastTrackListenSession.LastListenDurationSeconds,
            bookingListenSessionResponse.ListenSessionProcedure?.IsAutoPlay
          );
        }
        dispatch(setListenSessionProcedure(lsp));
      } else {
        // Both exist - error case
        console.log("Episode Listen Session:", episodeListenSessionResponse);
        console.log("Booking Listen Session:", bookingListenSessionResponse);
        console.error(
          "Both episode and booking listen sessions exist - cannot determine which to load."
        );
        return;
      }
      try {
      } catch (error) {
        console.error("Error in loadFromLatestListenSessionAndPlay:", error);
        playerEngine.stopAndUnload();
      }
    }
  }, [
    user,
    dispatch,
    getLatestEpisodeListenSession,
    getLatestBookingListenSession,
  ]);

  // 4. Listen From Booking Track
  const listenFromBookingTrack = useCallback(
    async (bookingTrackId: string, bookingId: number) => {
      if (!user) {
        dispatch(
          setDataAndShowAlert({
            title: "Login Required",
            description: "Please log in to listen to podcasts.",
            type: "warning",
            isCloseable: true,
            isFunctional: true,
            functionalButtonText: "Log In",
            autoCloseDuration: 5,
            actionId,
          })
        );
        return;
      } else {
        try {
          const listenResponse = await listenToBookingTrack({
            BookingId: bookingId,
            BookingPodcastTrackId: bookingTrackId,
          }).unwrap();
          if (!listenResponse) {
            return;
          } else {
            // Always set ListenSessionProcedure (never null)
            dispatch(
              setListenSessionProcedure(listenResponse.ListenSessionProcedure)
            );
            // Only play if ListenSession exists
            if (listenResponse.ListenSession) {
              const ls =
                listenResponse.ListenSession as ListenSessionBookingTracks;
              const track: PlayerTrack = {
                id: ls.BookingPodcastTrack.Id,
                url: ls.AudioFileUrl,
                artist: ls.Booking.Title,
                title: ls.BookingPodcastTrack.BookingRequirementName,
                artwork: "", // Booking track không có artwork
              };
              playerEngine.setSourceType("BookingProducingTracks");
              dispatch(setListenSession(listenResponse.ListenSession));
              await playerEngine.loadAndPlay(
                track,
                listenResponse.ListenSession,
                listenResponse.ListenSessionProcedure,
                true,
                ls.BookingPodcastTrackListenSession.LastListenDurationSeconds,
                listenResponse.ListenSessionProcedure?.IsAutoPlay
              );
            } else {
              console.error(
                "No ListenSession returned from listenToBookingTrack"
              );
            }
          }
        } catch (error) {
          console.error("Error in listenFromBookingTrack:", error);
        }
      }
    },
    [dispatch, listenToBookingTrack, router, user]
  );

  // NAVIGATE
  // 1. Navigate Episode In Procedure: Source Type === SpecifyShowEpisodes
  const navigateInSpecifyShows = useCallback(
    async (
      navigateType: "Next" | "Previous",
      listenSession: ListenSessionEpisodes,
      listenSessionProcedure: ListenSessionProcedure
    ) => {
      if (!listenSessionProcedure) {
        return;
      }
      const episodeId = listenSession.PodcastEpisode.Id;
      const isHasNonQuota = await triggerCheckNonQuota({
        PodcastEpisodeId: episodeId,
      }).unwrap();
      let benefitList: SubscriptionBenefit[] = [];
      const listenSlot = await triggerGetListenSlot().unwrap();

      if (!isHasNonQuota && listenSlot.PodcastListenSlot <= 0) {
        dispatch(
          setDataAndShowAlert({
            title: "No Remaining Listen Slots",
            description:
              "You have no remaining podcast listen slots. Cannot navigate to next/previous episodes.",
            type: "error",
            isCloseable: true,
            isFunctional: false,
            autoCloseDuration: 10,
          })
        );
        return;
      }
      const benefitData = await triggerGetBenefitList({
        PodcastEpisodeId: episodeId,
      }).unwrap();
      if (
        benefitData &&
        benefitData.CurrentPodcastSubscriptionRegistrationBenefitList
      ) {
        benefitList =
          benefitData.CurrentPodcastSubscriptionRegistrationBenefitList;
      }

      try {
        const navigateResponse = await navigateEpisode({
          ListenSessionNavigateType: navigateType,
          ListenSessionId: listenSession.PodcastEpisodeListenSession.Id,
          ListenSessionProcedureId: listenSessionProcedure.Id,
          CurrentPodcastSubscriptionRegistrationBenefitList: benefitList,
        }).unwrap();
        if (!navigateResponse) {
          return;
        } else {
          // Always set ListenSessionProcedure (never null)
          dispatch(
            setListenSessionProcedure(navigateResponse.ListenSessionProcedure)
          );
          // Only play if ListenSession exists
          if (navigateResponse.ListenSession) {
            const ls = navigateResponse.ListenSession as ListenSessionEpisodes;
            const track: PlayerTrack = {
              id: ls.PodcastEpisode.Id,
              url: ls.AudioFileUrl,
              artist: ls.Podcaster.FullName,
              title: ls.PodcastEpisode.Name,
              artwork: ls.PodcastEpisode.MainImageFileKey,
            };
            playerEngine.setSourceType("SpecifyShowEpisodes");
            dispatch(setListenSession(navigateResponse.ListenSession));
            await playerEngine.loadAndPlay(
              track,
              navigateResponse.ListenSession,
              navigateResponse.ListenSessionProcedure,
              true,
              ls.PodcastEpisodeListenSession.LastListenDurationSeconds,
              navigateResponse.ListenSessionProcedure?.IsAutoPlay
            );
          } else {
            console.error("No ListenSession returned from navigateEpisode");
          }
        }
      } catch (error) {
        console.error("Error in navigateInSpecifyShows:", error);
      }
    },
    [navigateEpisode]
  );

  // 2. Navigate Episode In Procedure: Source Type === SavedEpisodes
  const navigateInSavedEpisodes = useCallback(
    async (
      navigateType: "Next" | "Previous",
      listenSession: ListenSessionEpisodes,
      listenSessionProcedure: ListenSessionProcedure
    ) => {
      if (!listenSessionProcedure) {
        return;
      }
      try {
        const navigateResponse = await navigateEpisode({
          ListenSessionNavigateType: navigateType,
          ListenSessionId: listenSession.PodcastEpisodeListenSession.Id,
          ListenSessionProcedureId: listenSessionProcedure.Id,
          CurrentPodcastSubscriptionRegistrationBenefitList: [], // SAVED EPISODES ON NAVIGATE DOESN'T CHECK BENEFITS
        }).unwrap();
        if (!navigateResponse) {
          return;
        } else {
          // Always set ListenSessionProcedure (never null)
          dispatch(
            setListenSessionProcedure(navigateResponse.ListenSessionProcedure)
          );
          // Only play if ListenSession exists
          if (navigateResponse.ListenSession) {
            const ls = navigateResponse.ListenSession as ListenSessionEpisodes;
            const track: PlayerTrack = {
              id: ls.PodcastEpisode.Id,
              url: ls.AudioFileUrl,
              artist: ls.Podcaster.FullName,
              title: ls.PodcastEpisode.Name,
              artwork: ls.PodcastEpisode.MainImageFileKey,
            };
            playerEngine.setSourceType("SpecifyShowEpisodes");
            dispatch(setListenSession(navigateResponse.ListenSession));
            await playerEngine.loadAndPlay(
              track,
              navigateResponse.ListenSession,
              navigateResponse.ListenSessionProcedure,
              true,
              ls.PodcastEpisodeListenSession.LastListenDurationSeconds,
              navigateResponse.ListenSessionProcedure?.IsAutoPlay
            );
          } else {
            console.error("No ListenSession returned from navigateEpisode");
          }
        }
      } catch (error) {
        console.error("Error in navigateInSpecifyShows:", error);
      }
    },
    [navigateEpisode]
  );

  // 3. Navigate Booking Track In Procedure
  const navigateInBookingTracks = useCallback(
    async (
      navigateType: "Next" | "Previous",
      listenSession: ListenSessionBookingTracks,
      listenSessionProcedure: ListenSessionProcedure
    ) => {
      if (!listenSessionProcedure) {
        return;
      }
      try {
        const navigateResponse = await navigateBookingTrack({
          ListenSessionNavigateType: navigateType,
          ListenSessionId: listenSession.BookingPodcastTrackListenSession.Id,
          ListenSessionProcedureId: listenSessionProcedure.Id,
          CurrentPodcastSubscriptionRegistrationBenefitList: [],
        }).unwrap();
        if (!navigateResponse) {
          return;
        } else {
          // Always set ListenSessionProcedure (never null)
          dispatch(
            setListenSessionProcedure(navigateResponse.ListenSessionProcedure)
          );
          // Only play if ListenSession exists
          if (navigateResponse.ListenSession) {
            const ls =
              navigateResponse.ListenSession as ListenSessionBookingTracks;
            const track: PlayerTrack = {
              id: ls.BookingPodcastTrack.Id,
              url: ls.AudioFileUrl,
              artist: ls.Booking.Title,
              title: ls.BookingPodcastTrack.BookingRequirementName,
              artwork: "",
            };
            playerEngine.setSourceType("SpecifyShowEpisodes");
            dispatch(setListenSession(navigateResponse.ListenSession));
            await playerEngine.loadAndPlay(
              track,
              navigateResponse.ListenSession,
              navigateResponse.ListenSessionProcedure,
              true,
              ls.BookingPodcastTrackListenSession.LastListenDurationSeconds,
              navigateResponse.ListenSessionProcedure?.IsAutoPlay
            );
          } else {
            console.error("No ListenSession returned from navigateEpisode");
          }
        }
      } catch (error) {
        console.error("Error in navigateInSpecifyShows:", error);
      }
    },
    [navigateEpisode]
  );

  const play = async () => {
    await playerEngine.play();
  };

  const pause = async () => {
    await playerEngine.pause();
  };

  const togglePlayPause = async () => {
    await playerEngine.togglePlayPause();
  };

  const seekTo = async (timeInSeconds: number) => {
    await playerEngine.seekTo(timeInSeconds * 1000);
  };

  const setVolume = async (volume: number) => {
    await playerEngine.setVolume(volume);
  };

  const stop = async () => {
    await playerEngine.stopAndUnload();
  };

  const checkIsCurrentPlay = useCallback(
    (audioId: string): boolean => {
      return state.isPlaying && state.currentAudio?.id === audioId;
    },
    [state.isPlaying, state.currentAudio]
  );

  const checkIsNaviableInProcedure = useCallback((): boolean => {
    if (!state.currentAudio || !player.listenSessionProcedure) {
      return false;
    }

    const playOrderMode = player.listenSessionProcedure?.PlayOrderMode;
    if (
      playOrderMode === "Sequential" &&
      player.listenSessionProcedure &&
      player.listenSessionProcedure.ListenObjectsSequentialOrder
    ) {
      const listenableList =
        player.listenSessionProcedure.ListenObjectsSequentialOrder.filter(
          (a) => a.IsListenable === true
        );
      if (listenableList.length > 1) {
        return true;
      } else {
        return false;
      }
    } else if (
      playOrderMode === "Random" &&
      player.listenSessionProcedure &&
      player.listenSessionProcedure.ListenObjectsRandomOrder
    ) {
      const listenableList =
        player.listenSessionProcedure.ListenObjectsRandomOrder.filter(
          (a) => a.IsListenable === true
        );
      if (listenableList.length > 1) {
        return true;
      } else {
        return false;
      }
    }
    return false;
  }, [state.currentAudio, player.listenSessionProcedure]);

  return {
    state,
    play,
    pause,
    togglePlayPause,
    seekTo,
    setVolume,
    stop,
    checkIsCurrentPlay,
    checkIsNaviableInProcedure,
    // LISTEN
    listenFromEpisode,
    continueListenFromEpisode,
    loadFromLatestListenSessionAndPlay,
    listenFromBookingTrack,
    // NAVIGATE
    navigateInSpecifyShows,
    navigateInSavedEpisodes,
    navigateInBookingTracks,
    // UPDATE LAST DURATION
  };
}

import { RootState } from "@/src/store/store";
import { useDispatch, useSelector } from "react-redux";
import { useCallback, useEffect, useRef, useState } from "react";
import {
  SubscriptionBenefit,
  useLazyGetActiveSubscriptionFromEpisodeIdQuery,
  useLazyGetIsHasNonQuotaAccessQuery,
  useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery,
} from "../subscription/subscription.service";
import { useLazyGetCustomerPodcastListenSlotQuery } from "../account/account.service";
import { playerEngine, PlayerTrack, PlayerUiState } from "./playerEngine";
import { useRouter } from "expo-router";
import { setDataAndShowAlert } from "@/src/features/alert/alertSlice";
import {
  registerAlertAction,
  unregisterAlertAction,
} from "@/src/components/alert/GlobalAlert";
import {
  ListenSessionBookingTracks,
  ListenSessionEpisodes,
  ListenSessionProcedure,
} from "../../types/audio.type";
import {
  setListenSession,
  setListenSessionProcedure,
} from "@/src/features/mediaPlayer/playerSlice";
import {
  getLatestBookingListenSessionV2,
  getLatestEpisodeListenSessionV2,
  listenToBookingTrackV2,
  listenToEpisodeV2,
  navigateBookingTrackInProcedureV2,
  navigateEpisodeInProcedureV2,
} from "./playerService-v2";
import {
  alertMessages,
  benefitTransformDescriptions,
} from "@/src/data/alert-messages";

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

  const [getActiveSubscriptionFromEpisodeId] =
    useLazyGetActiveSubscriptionFromEpisodeIdQuery();

  // Prevent concurrent loadLatest calls
  const isLoadingLatest = useRef(false);

  // Prevent concurrent listen/navigate calls
  const isProcessingAudio = useRef(false);

  // Global debounce timer for listen functions
  const listenDebounceTimer = useRef<number | null>(null);

  // Player UI State - Initialize with current engine state to prevent flash of empty state
  const [state, setState] = useState<PlayerUiState>(() => {
    // Get current state from engine on mount to avoid initial render with duration = 0
    try {
      return playerEngine.getState();
    } catch {
      // Fallback to default if getState fails
      return {
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
        isAudioLoading: false,
        loadingAudioId: null,
      };
    }
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
      // Clear any pending debounce timer
      if (listenDebounceTimer.current) {
        clearTimeout(listenDebounceTimer.current);
        listenDebounceTimer.current = null;
      }

      // Prevent concurrent calls
      if (isProcessingAudio.current) {
        if (__DEV__) {
          console.warn("[ListenFromEpisode] Already processing, skipping");
        }
        return;
      }

      // Set debounce timer - only the last click will execute
      listenDebounceTimer.current = setTimeout(async () => {
        listenDebounceTimer.current = null;

        // Double-check flag after debounce
        if (isProcessingAudio.current) {
          if (__DEV__) {
            console.warn(
              "[ListenFromEpisode] Already processing after debounce, skipping"
            );
          }
          return;
        }

        // Set flag IMMEDIATELY to block spam
        isProcessingAudio.current = true;
        playerEngine.setLoadingState(true, episodeId);

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
          isProcessingAudio.current = false;
          playerEngine.setLoadingState(false, null);
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
              isProcessingAudio.current = false;
              playerEngine.setLoadingState(false, null);
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

          const listenResponse = await listenToEpisodeV2({
            CurrentPodcastSubscriptionRegistrationBenefitList: benefitList,
            PodcastEpisodeId: episodeId,
            SourceType: sourceType,
          });

          if (listenResponse.isError || !listenResponse.data) {
            const messageId = listenResponse.messageId;
            if (messageId === "listen-failed-2") {
              const messageData = alertMessages["listen-failed-2"];
              if (messageData) {
                dispatch(
                  setDataAndShowAlert({
                    title: messageData.title,
                    description: messageData.description,
                    isCloseable: true,
                    isFunctional: false,
                    type: messageData.type,
                    autoCloseDuration: 10,
                  })
                );
                return;
              }
            } else if (messageId === "listen-failed-3/4") {
              const activeSubscription = (
                await getActiveSubscriptionFromEpisodeId({
                  PodcastEpisodeId: episodeId,
                }).unwrap()
              ).PodcastSubscription;
              if (activeSubscription) {
                const messageData = alertMessages["listen-failed-3"];
                if (messageData) {
                  dispatch(
                    setDataAndShowAlert({
                      title: messageData.title,
                      description: messageData.description,
                      isCloseable: true,
                      isFunctional: false,
                      type: messageData.type,
                      autoCloseDuration: 10,
                    })
                  );
                  return;
                }
              } else {
                const messageData = alertMessages["listen-failed-4"];
                if (messageData) {
                  dispatch(
                    setDataAndShowAlert({
                      title: messageData.title,
                      description: messageData.description,
                      isCloseable: true,
                      isFunctional: false,
                      type: messageData.type,
                      autoCloseDuration: 10,
                    })
                  );
                  return;
                }
              }
            } else if (
              messageId === "listen-failed-5" &&
              listenResponse.missingBenefits
            ) {
              const messageData = alertMessages["listen-failed-5"];
              const formatDescription =
                `${messageData.description}` +
                listenResponse.missingBenefits
                  .map((key) => benefitTransformDescriptions[key])
                  .filter(Boolean)
                  .map((text) => `• ${text}`)
                  .join("\n");

              if (messageData) {
                dispatch(
                  setDataAndShowAlert({
                    title: messageData.title,
                    description: formatDescription,
                    isCloseable: true,
                    isFunctional: false,
                    type: messageData.type,
                    autoCloseDuration: 10,
                  })
                );
                return;
              }
            } else {
              const messageData = alertMessages["listen-failed-1"];
              if (messageData) {
                dispatch(
                  setDataAndShowAlert({
                    title: messageData.title,
                    description: messageData.description,
                    isCloseable: true,
                    isFunctional: false,
                    type: messageData.type,
                    autoCloseDuration: 10,
                  })
                );
                return;
              }
            }
          } else {
            // Success: Always set ListenSessionProcedure (never null)
            dispatch(
              setListenSessionProcedure(
                listenResponse.data.ListenSessionProcedure
              )
            );
            // Only play if ListenSession exists
            if (listenResponse.data.ListenSession) {
              const ls = listenResponse.data
                .ListenSession as ListenSessionEpisodes;
              const track: PlayerTrack = {
                id: ls.PodcastEpisode.Id,
                url: ls.AudioFileUrl,
                artist: ls.Podcaster.FullName,
                title: ls.PodcastEpisode.Name,
                artwork: ls.PodcastEpisode.MainImageFileKey,
              };

              playerEngine.setSourceType(sourceType);
              dispatch(setListenSession(listenResponse.data.ListenSession));
              await playerEngine.loadAndPlay(
                track,
                listenResponse.data.ListenSession,
                listenResponse.data.ListenSessionProcedure,
                true,
                ls.PodcastEpisodeListenSession.LastListenDurationSeconds,
                listenResponse.data.ListenSessionProcedure?.IsAutoPlay
              );
            }
          }
        } catch (error) {
          playerEngine.setLoadingState(false, null);
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
        } finally {
          isProcessingAudio.current = false;
          playerEngine.setLoadingState(false, null);
        }
      }, 300); // 300ms debounce - quick enough for UX, long enough to prevent double clicks
    },
    [
      user,
      dispatch,
      router,
      triggerCheckNonQuota,
      triggerGetListenSlot,
      triggerGetBenefitList,
      getActiveSubscriptionFromEpisodeId,
    ]
  );

  // 2. Continue Listen From Episode
  const continueListenFromEpisode = useCallback(
    async (episodeId: string, continue_listen_session_id: string) => {
      if (isProcessingAudio.current) {
        if (__DEV__) {
          console.warn("[ListenFromEpisode] Already processing, skipping");
        }
        return;
      }

      // Set flag IMMEDIATELY to block spam
      isProcessingAudio.current = true;
      playerEngine.setLoadingState(true, episodeId);

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
        isProcessingAudio.current = false;
        playerEngine.setLoadingState(false, null);
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
            isProcessingAudio.current = false;
            playerEngine.setLoadingState(false, null);
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

        const listenResponse = await listenToEpisodeV2({
          PodcastEpisodeId: episodeId,
          SourceType: "SpecifyShowEpisodes",
          CurrentPodcastSubscriptionRegistrationBenefitList: benefitList,
          continue_listen_session_id: continue_listen_session_id,
        });

        if (listenResponse.isError || !listenResponse.data) {
          const messageId = listenResponse.messageId;
          if (messageId === "listen-failed-2") {
            const messageData = alertMessages["listen-failed-2"];
            if (messageData) {
              dispatch(
                setDataAndShowAlert({
                  title: messageData.title,
                  description: messageData.description,
                  isCloseable: true,
                  isFunctional: false,
                  type: messageData.type,
                  autoCloseDuration: 10,
                })
              );
              return;
            }
          } else if (messageId === "listen-failed-3/4") {
            const activeSubscription = (
              await getActiveSubscriptionFromEpisodeId({
                PodcastEpisodeId: episodeId,
              }).unwrap()
            ).PodcastSubscription;
            if (activeSubscription) {
              const messageData = alertMessages["listen-failed-3"];
              if (messageData) {
                dispatch(
                  setDataAndShowAlert({
                    title: messageData.title,
                    description: messageData.description,
                    isCloseable: true,
                    isFunctional: false,
                    type: messageData.type,
                    autoCloseDuration: 10,
                  })
                );
                return;
              }
            } else {
              const messageData = alertMessages["listen-failed-4"];
              if (messageData) {
                dispatch(
                  setDataAndShowAlert({
                    title: messageData.title,
                    description: messageData.description,
                    isCloseable: true,
                    isFunctional: false,
                    type: messageData.type,
                    autoCloseDuration: 10,
                  })
                );
                return;
              }
            }
          } else if (
            messageId === "listen-failed-5" &&
            listenResponse.missingBenefits
          ) {
            const messageData = alertMessages["listen-failed-5"];
            const formatDescription =
              `${messageData.description}` +
              listenResponse.missingBenefits
                .map((key) => benefitTransformDescriptions[key])
                .filter(Boolean)
                .map((text) => `• ${text}`)
                .join("\n");

            if (messageData) {
              dispatch(
                setDataAndShowAlert({
                  title: messageData.title,
                  description: formatDescription,
                  isCloseable: true,
                  isFunctional: false,
                  type: messageData.type,
                  autoCloseDuration: 10,
                })
              );
              return;
            }
          } else {
            const messageData = alertMessages["listen-failed-1"];
            if (messageData) {
              dispatch(
                setDataAndShowAlert({
                  title: messageData.title,
                  description: messageData.description,
                  isCloseable: true,
                  isFunctional: false,
                  type: messageData.type,
                  autoCloseDuration: 10,
                })
              );
              return;
            }
          }
        } else {
          // Always set ListenSessionProcedure (never null)
          dispatch(
            setListenSessionProcedure(
              listenResponse.data.ListenSessionProcedure
            )
          );

          // Only play if ListenSession exists
          if (listenResponse.data.ListenSession) {
            const ls = listenResponse.data
              .ListenSession as ListenSessionEpisodes;
            const track: PlayerTrack = {
              id: ls.PodcastEpisode.Id,
              url: ls.AudioFileUrl,
              artist: ls.Podcaster.FullName,
              title: ls.PodcastEpisode.Name,
              artwork: ls.PodcastEpisode.MainImageFileKey,
            };

            playerEngine.setSourceType("SpecifyShowEpisodes");
            dispatch(setListenSession(listenResponse.data.ListenSession));
            await playerEngine.loadAndPlay(
              track,
              listenResponse.data.ListenSession,
              listenResponse.data.ListenSessionProcedure,
              true,
              ls.PodcastEpisodeListenSession.LastListenDurationSeconds,
              listenResponse.data.ListenSessionProcedure?.IsAutoPlay
            );
          }
        }
      } catch (error) {
        console.log("Error while listening continue: ", error);
        dispatch(
          setDataAndShowAlert({
            title: alertMessages["listen-failed-1"].title,
            description: alertMessages["listen-failed-1"].description,
            type: "error",
            isCloseable: true,
            isFunctional: false,
            autoCloseDuration: 5,
          })
        );
      } finally {
        isProcessingAudio.current = false;
        playerEngine.setLoadingState(false, null);
      }
    },
    [
      user,
      dispatch,
      router,
      triggerCheckNonQuota,
      triggerGetListenSlot,
      triggerGetBenefitList,
      getActiveSubscriptionFromEpisodeId,
    ]
  );

  // 3. Load From Latest Listen Session and Play
  const loadFromLatestListenSessionAndPlay = useCallback(async () => {
    if (!user) {
      return;
    }

    // Prevent concurrent calls
    if (isLoadingLatest.current) {
      if (__DEV__) {
        console.warn("[LoadLatest] Already loading, skipping duplicate call");
      }
      return;
    }

    isLoadingLatest.current = true;

    try {
      if (__DEV__) {
        console.log("[LoadLatest] Starting to load latest session");
      }

      const episodeListenSessionResponse =
        await getLatestEpisodeListenSessionV2();
      const bookingListenSessionResponse =
        await getLatestBookingListenSessionV2();

      const isNoEpisodeListenSession =
        !episodeListenSessionResponse.ListenSession ||
        episodeListenSessionResponse.ListenSession === null;
      const isNoBookingListenSession =
        !bookingListenSessionResponse.ListenSession ||
        bookingListenSessionResponse.ListenSession === null;

      if (__DEV__) {
        console.log("[LoadLatest] Episode:", episodeListenSessionResponse);
        console.log("[LoadLatest] Booking:", bookingListenSessionResponse);
      }

      if (isNoEpisodeListenSession && isNoBookingListenSession) {
        if (__DEV__) console.log("[LoadLatest] No sessions found");
        return;
      }

      if (!isNoEpisodeListenSession && isNoBookingListenSession) {
        if (__DEV__) console.log("[LoadLatest] Loading Episode session");
        const ls =
          episodeListenSessionResponse.ListenSession as ListenSessionEpisodes;
        const lsp =
          episodeListenSessionResponse.ListenSessionProcedure as ListenSessionProcedure;

        if (!lsp) {
          if (__DEV__) console.warn("[LoadLatest] No procedure for episode");
          return;
        }

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
        return;
      }

      if (isNoEpisodeListenSession && !isNoBookingListenSession) {
        if (__DEV__) console.log("[LoadLatest] Loading Booking session");
        const ls =
          bookingListenSessionResponse.ListenSession as ListenSessionBookingTracks;
        const lsp =
          bookingListenSessionResponse.ListenSessionProcedure as ListenSessionProcedure;

        if (!lsp) {
          if (__DEV__) console.warn("[LoadLatest] No procedure for booking");
          return;
        }

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
        return;
      }

      // Both exist - conflict
      if (__DEV__) {
        console.error("[LoadLatest] Both sessions exist - conflict!");
      }
    } catch (error) {
      if (__DEV__) {
        console.error("[LoadLatest] Error:", error);
      }
      playerEngine.stopAndUnload();
    } finally {
      isLoadingLatest.current = false;
      if (__DEV__) {
        console.log("[LoadLatest] Finished");
      }
    }
  }, [user, dispatch]);

  // 4. Listen From Booking Track
  const listenFromBookingTrack = useCallback(
    async (bookingTrackId: string, bookingId: number) => {
      if (isProcessingAudio.current) {
        if (__DEV__) {
          console.warn("[ListenFromEpisode] Already processing, skipping");
        }
        return;
      }

      // Set flag IMMEDIATELY to block spam
      isProcessingAudio.current = true;
      playerEngine.setLoadingState(true, bookingTrackId);

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
        isProcessingAudio.current = false;
        playerEngine.setLoadingState(false, null);
        return;
      } else {
        try {
          const listenResponse = await listenToBookingTrackV2({
            BookingId: bookingId,
            BookingPodcastTrackId: bookingTrackId,
          });
          if (
            !listenResponse ||
            (!listenResponse.ListenSession &&
              !listenResponse.ListenSessionProcedure)
          ) {
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
              console.log(
                ">>>>>>>>>>>LISTEN FROM BOOKING TRACKS RESPONSE: ",
                listenResponse
              );
              console.error(
                "No ListenSession returned from listenToBookingTrack"
              );
            }
          }
        } catch (error) {
          console.error("Error in listenFromBookingTrack:", error);
        } finally {
          isProcessingAudio.current = false;
          playerEngine.setLoadingState(false, null);
        }
      }
    },
    [dispatch, router, user]
  );

  // NAVIGATE
  // 1. Navigate Episode In Procedure: Source Type === SpecifyShowEpisodes
  const navigateInSpecifyShows = useCallback(
    async (
      navigateType: "Next" | "Previous",
      listenSession: ListenSessionEpisodes,
      listenSessionProcedure: ListenSessionProcedure
    ) => {
      if (isProcessingAudio.current) {
        if (__DEV__) {
          console.warn("[ListenFromEpisode] Already processing, skipping");
        }
        return;
      }
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

      isProcessingAudio.current = true;
      playerEngine.setLoadingState(true, "");

      try {
        const navigateResponse = await navigateEpisodeInProcedureV2({
          ListenSessionNavigateType: navigateType,
          ListenSessionId: listenSession.PodcastEpisodeListenSession.Id,
          ListenSessionProcedureId: listenSessionProcedure.Id,
          CurrentPodcastSubscriptionRegistrationBenefitList: benefitList,
        });

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
      } finally {
        isProcessingAudio.current = false;
        playerEngine.setLoadingState(false, null);
      }
    },
    [
      dispatch,
      triggerCheckNonQuota,
      triggerGetListenSlot,
      triggerGetBenefitList,
    ]
  );

  // 2. Navigate Episode In Procedure: Source Type === SavedEpisodes
  const navigateInSavedEpisodes = useCallback(
    async (
      navigateType: "Next" | "Previous",
      listenSession: ListenSessionEpisodes,
      listenSessionProcedure: ListenSessionProcedure
    ) => {
      if (isProcessingAudio.current) {
        if (__DEV__) {
          console.warn("[ListenFromEpisode] Already processing, skipping");
        }
        return;
      }
      if (!listenSessionProcedure) {
        return;
      }

      isProcessingAudio.current = true;
      playerEngine.setLoadingState(true, "");
      try {
        const navigateResponse = await navigateEpisodeInProcedureV2({
          ListenSessionNavigateType: navigateType,
          ListenSessionId: listenSession.PodcastEpisodeListenSession.Id,
          ListenSessionProcedureId: listenSessionProcedure.Id,
          CurrentPodcastSubscriptionRegistrationBenefitList: null, // SAVED EPISODES ON NAVIGATE DOESN'T CHECK BENEFITS
        });
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
        console.error("Error in navigateInSavedEpisodes:", error);
      } finally {
        isProcessingAudio.current = false;
        playerEngine.setLoadingState(false, null);
      }
    },
    [dispatch]
  );

  // 3. Navigate Booking Track In Procedure
  const navigateInBookingTracks = useCallback(
    async (
      navigateType: "Next" | "Previous",
      listenSession: ListenSessionBookingTracks,
      listenSessionProcedure: ListenSessionProcedure
    ) => {
      if (isProcessingAudio.current) {
        if (__DEV__) {
          console.warn("[ListenFromEpisode] Already processing, skipping");
        }
        return;
      }
      if (!listenSessionProcedure) {
        return;
      }
      isProcessingAudio.current = true;
      playerEngine.setLoadingState(true, "");
      try {
        const navigateResponse = await navigateBookingTrackInProcedureV2({
          ListenSessionNavigateType: navigateType,
          ListenSessionId: listenSession.BookingPodcastTrackListenSession.Id,
          ListenSessionProcedureId: listenSessionProcedure.Id,
        });
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
            playerEngine.setSourceType("BookingProducingTracks");
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
        console.error("Error in navigateInBookingTracks:", error);
      } finally {
        isProcessingAudio.current = false;
        playerEngine.setLoadingState(false, null);
      }
    },
    [dispatch]
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

// // src/core/player/PlayerCore.tsx
// import { useCallback, useEffect, useRef } from "react";
// import { playerEngine } from "./playerEngine";
// import { usePlayerNavigate } from "@/src/core/services/player/usePlayerNavigate";
// import {
//   setListenSession,
//   setListenSessionProcedure,
//   setCurrentAudio,
//   setIsNextSessionNull,
//   setBuffering,
//   stopAudio,
//   consumeSeek,
//   pauseAudio,
//   setPlaybackState,
// } from "@/src/features/mediaPlayer/playerSlice";

// import {
//   ListenSessionBookingTracks,
//   ListenSessionEpisodes,
//   ListenSessionProcedure,
// } from "@/src/core/types/audio.type";

// import {
//   useListenToEpisodeMutation,
//   useListenToBookingTrackMutation,
//   useUpdateEpisodeLastDurationMutation,
//   useUpdateBookingTrackLastDurationMutation,
//   useNavigateEpisodeInProcedureMutation,
//   useNavigateBookingTrackInProcedureMutation,
// } from "@/src/core/services/player/playerService";

// import {
//   useLazyGetIsHasNonQuotaAccessQuery,
//   useGetSubscriptionBenefitsMapListFromEpisodeIdQuery,
// } from "@/src/core/services/subscription/subscription.service";

// import { useLazyGetCustomerPodcastListenSlotQuery } from "@/src/core/services/account/account.service";
// import { useDispatch, useSelector } from "react-redux";
// import { RootState } from "@/src/store/store";
// import { Alert } from "react-native";

// const PlayerCore = () => {
//   const dispatch = useDispatch();
//   const { navigateNext } = usePlayerNavigate();
//   const player = useSelector((state: RootState) => state.player);

//   // RTK Query hooks
//   const [listenToEpisode] = useListenToEpisodeMutation();
//   const [listenToBookingTrack] = useListenToBookingTrackMutation();

//   const [updateEpisodeLastDuration] = useUpdateEpisodeLastDurationMutation();
//   const [updateBookingTrackLastDuration] =
//     useUpdateBookingTrackLastDurationMutation();

//   const [navigateEpisodeInProcedure] = useNavigateEpisodeInProcedureMutation();
//   const [navigateBookingTrackInProcedure] =
//     useNavigateBookingTrackInProcedureMutation();

//   const [triggerIsHasNonQuotaAccess] = useLazyGetIsHasNonQuotaAccessQuery();
//   const [triggerGetListenSlot] = useLazyGetCustomerPodcastListenSlotQuery();

//   // Benefit list: dùng query thường, điều khiển bằng episodeId state
//   const benefitEpisodeId =
//     player.playMode.sourceType === "SpecifyShowEpisodes" &&
//     player.playMode.audioId
//       ? player.playMode.audioId
//       : undefined;

//   const { data: benefitData, isFetching: isFetchingBenefits } =
//     useGetSubscriptionBenefitsMapListFromEpisodeIdQuery(
//       { PodcastEpisodeId: benefitEpisodeId! },
//       {
//         skip: !benefitEpisodeId,
//       }
//     );

//   // Ref để tránh race + lưu session đã load vào engine
//   const lastLoadedSessionIdRef = useRef<string | null>(null);
//   const lastPositionMsRef = useRef<number>(0);
//   const lastDurationMsRef = useRef<number>(0);
//   const navigatingRef = useRef(false);

//   // Auto-play: khi track hiện tại kết thúc
//   const handleTrackFinished = useCallback(async () => {
//     const { playMode, listenSession, listenSessionProcedure } = player;

//     // Không bật AutoPlay hoặc không có procedure thì dừng
//     if (!playMode.isAutoPlay || !listenSession || !listenSessionProcedure) {
//       dispatch(pauseAudio());
//       return;
//     }

//     // Nếu đã biết là Next session null rồi thì thôi
//     if (playMode.isNextSessionNull) {
//       dispatch(pauseAudio());
//       return;
//     }

//     // Auto-play chỉ navigate Next
//     await navigateNext();
//   }, [player, dispatch, navigateNext]);

//   /** 1) Bắt status từ playerEngine -> setBuffering + lưu positionMs */
//   useEffect(() => {
//     playerEngine.setListener((status) => {
//       lastPositionMsRef.current = status.positionMs;
//       lastDurationMsRef.current = status.durationMs ?? 0;

//       // buffering khi engine báo đang buffer và chưa playing
//       dispatch(setBuffering(!!status.isBuffering && !status.isPlaying));

//       // 2) Progress (ms -> giây)
//       const posSec = Math.floor(status.positionMs / 1000);
//       const durSec = Math.floor((status.durationMs ?? 0) / 1000);

//       dispatch(
//         setPlaybackState({
//           position: posSec,
//           duration: durSec,
//         })
//       );
//       if (status.didJustFinish) {
//         // Track hiện tại đã chạy xong
//         handleTrackFinished();
//       }
//     });

//     return () => {
//       playerEngine.setListener(null);
//     };
//   }, [dispatch, handleTrackFinished]);

//   /** 2) Sync volume từ Redux -> expo-av */
//   useEffect(() => {
//     const vol0to1 = (player.playMode.volume ?? 100) / 100;
//     playerEngine.setVolume(vol0to1);
//   }, [player.playMode.volume]);

//   /** Helper: kiểm tra xem listenSession hiện tại có đúng audioId + sourceType không */
//   const isSameSessionWithCurrentCommand = (
//     listenSession: any,
//     procedure: ListenSessionProcedure | null,
//     sourceType:
//       | "SpecifyShowEpisodes"
//       | "SavedEpisodes"
//       | "BookingProducingTracks"
//       | null,
//     audioId: string | null
//   ) => {
//     if (!listenSession || !procedure || !sourceType || !audioId) return false;

//     const procedureSourceType = procedure.SourceDetail.Type;

//     if (procedureSourceType !== sourceType) return false;

//     if (sourceType === "BookingProducingTracks") {
//       const ls = listenSession as ListenSessionBookingTracks;
//       return ls.BookingPodcastTrack.Id === audioId;
//     }

//     // Episodes (SpecifyShowEpisodes | SavedEpisodes)
//     const ls = listenSession as ListenSessionEpisodes;
//     return ls.PodcastEpisode.Id === audioId;
//   };

//   /** 3) Effect: khi playStatus === "play" ⇒ đảm bảo đã có ListenSession mới */
//   useEffect(() => {
//     const { playStatus, sourceType, audioId } = player.playMode;

//     if (playStatus !== "play") return;
//     if (!sourceType || !audioId) return;

//     // Nếu session hiện tại đã đúng audio rồi thì không gọi listen API nữa
//     if (
//       isSameSessionWithCurrentCommand(
//         player.listenSession,
//         player.listenSessionProcedure,
//         sourceType,
//         audioId
//       )
//     ) {
//       return;
//     }

//     let cancelled = false;

//     const run = async () => {
//       try {
//         // ==== B3: check Non-Quota & ListenSlot cho episode ====
//         if (sourceType !== "BookingProducingTracks") {
//           try {
//             // const [hasNonQuota, slotResult] = await Promise.all([
//             //   triggerIsHasNonQuotaAccess({
//             //     PodcastEpisodeId: audioId,
//             //   }).unwrap(),
//             //   triggerGetListenSlot().unwrap(),
//             // ]);
//             const hasNonQuota = await triggerIsHasNonQuotaAccess({
//               PodcastEpisodeId: audioId,
//             }).unwrap();
//             console.log("Has Non-Quota Access:", hasNonQuota);
//             const slotResult = await triggerGetListenSlot().unwrap();

//             const listenSlot = slotResult.PodcastListenSlot;
//             console.log("Listen Slot:", listenSlot, "Non-Quota:", hasNonQuota);
//             if (!hasNonQuota && listenSlot <= 0) {
//               if (!cancelled) {
//                 // TODO: Alert ở UI: "Bạn đã hết lượt nghe"
//                 Alert.alert(
//                   "Hết lượt nghe",
//                   "Bạn đã hết lượt nghe miễn phí. Vui lòng đăng ký để tiếp tục nghe các tập podcast.",
//                   [{ text: "OK" }]
//                 );
//                 dispatch(stopAudio());
//               }
//               return;
//             }
//           } catch (e) {
//             // Nếu check quota lỗi, stop luôn để an toàn
//             if (!cancelled) {
//               dispatch(stopAudio());
//             }
//             return;
//           }
//         }

//         // ==== Gọi listen API theo SourceType ====
//         if (sourceType === "BookingProducingTracks") {
//           if (!player.bookingId) {
//             if (!cancelled) dispatch(stopAudio());
//             return;
//           }

//           const res = await listenToBookingTrack({
//             BookingId: String(player.bookingId),
//             BookingPodcastTrackId: audioId,
//           }).unwrap();

//           if (cancelled) return;

//           const session = res.ListenSession;
//           if (session) {
//             dispatch(setListenSession(session as ListenSessionBookingTracks));
//             dispatch(
//               setCurrentAudio({
//                 Id: session.BookingPodcastTrack.Id,
//                 Name: session.BookingPodcastTrack.BookingRequirementName,
//                 PodcasterName: session.Booking.Title,
//                 MainImageFileKey: "",
//                 AudioLength:
//                   session.BookingPodcastTrackListenSession
//                     .LastListenDurationSeconds,
//               })
//             );
//             dispatch(setIsNextSessionNull(false));
//           } else {
//             dispatch(setIsNextSessionNull(true));
//           }

//           dispatch(setListenSessionProcedure(res.ListenSessionProcedure));
//         } else {
//           // Episodes: SpecifyShowEpisodes | SavedEpisodes
//           const benefitsList =
//             sourceType === "SpecifyShowEpisodes"
//               ? benefitData?.CurrentPodcastSubscriptionRegistrationBenefitList ??
//                 []
//               : [];

//           const res = await listenToEpisode({
//             PodcastEpisodeId: audioId,
//             SourceType: sourceType,
//             CurrentPodcastSubscriptionRegistrationBenefitList: benefitsList,
//             continue_listen_session_id:
//               player.continue_listen_session_id ?? undefined,
//           }).unwrap();

//           if (cancelled) return;

//           const session = res.ListenSession;
//           if (session) {
//             const epSession = session as ListenSessionEpisodes;
//             dispatch(setListenSession(epSession));
//             dispatch(
//               setCurrentAudio({
//                 Id: epSession.PodcastEpisode.Id,
//                 Name: epSession.PodcastEpisode.Name,
//                 PodcasterName: epSession.Podcaster.FullName,
//                 MainImageFileKey: epSession.PodcastEpisode.MainImageFileKey,
//                 AudioLength: epSession.PodcastEpisode.AudioLength,
//               })
//             );
//             dispatch(setIsNextSessionNull(false));
//           } else {
//             dispatch(setIsNextSessionNull(true));
//           }

//           dispatch(setListenSessionProcedure(res.ListenSessionProcedure));
//         }
//       } catch (e) {
//         if (!cancelled) {
//           dispatch(stopAudio());
//         }
//       }
//     };

//     // Chỉ run khi:
//     // - Không đang fetch benefit cho SpecifyShowEpisodes
//     if (sourceType === "SpecifyShowEpisodes" && isFetchingBenefits) {
//       // chờ benefit xong ở lần render sau
//       return;
//     }

//     run();

//     return () => {
//       cancelled = true;
//     };
//   }, [
//     player.playMode.playStatus,
//     player.playMode.sourceType,
//     player.playMode.audioId,
//     player.bookingId,
//     player.continue_listen_session_id,
//     player.listenSession,
//     player.listenSessionProcedure,
//     isFetchingBenefits,
//     benefitData,
//     dispatch,
//     listenToBookingTrack,
//     listenToEpisode,
//     triggerGetListenSlot,
//     triggerIsHasNonQuotaAccess,
//   ]);

//   /** 4) Effect: khi ListenSession thay đổi + đang play ⇒ load & play bằng expo-av */
//   useEffect(() => {
//     if (player.playMode.playStatus !== "play") return;

//     const ls = player.listenSession;
//     if (!ls) return;

//     const procedure = player.listenSessionProcedure;
//     const sourceType =
//       procedure?.SourceDetail.Type ?? player.playMode.sourceType;

//     let sessionKey: string | null = null;
//     let trackId: string;
//     let audioUrl: string;
//     let title: string;
//     let artist: string;
//     let lastSecondsForContinue: number | null = null;

//     const isBooking =
//       sourceType === "BookingProducingTracks" || "BookingPodcastTrack" in ls;

//     if (isBooking) {
//       const s = ls as ListenSessionBookingTracks;
//       sessionKey = s.BookingPodcastTrackListenSession.Id;
//       trackId = s.BookingPodcastTrack.Id;
//       audioUrl = s.AudioFileUrl;
//       title = s.BookingPodcastTrack.BookingRequirementName;
//       artist = s.Booking.Title;
//       // Booking không có continue-listen theo LastDurationSeconds kiểu episode
//     } else {
//       const s = ls as ListenSessionEpisodes;
//       sessionKey = s.PodcastEpisodeListenSession.Id;
//       trackId = s.PodcastEpisode.Id;
//       audioUrl = s.AudioFileUrl;
//       title = s.PodcastEpisode.Name;
//       artist = s.Podcaster.FullName;
//       lastSecondsForContinue =
//         s.PodcastEpisodeListenSession.LastListenDurationSeconds;
//     }

//     if (!audioUrl || !sessionKey) return;

//     let cancelled = false;

//     const run = async () => {
//       // Nếu đã load session này rồi thì chỉ cần play lại (resume)
//       if (lastLoadedSessionIdRef.current === sessionKey) {
//         await playerEngine.play();
//       } else {
//         lastLoadedSessionIdRef.current = sessionKey;

//         await playerEngine.loadAndPlay({
//           id: trackId,
//           url: audioUrl,
//           title,
//           artist,
//         });

//         // Flow continue listening: nếu có continue_listen_session_id
//         if (
//           !isBooking &&
//           player.continue_listen_session_id &&
//           lastSecondsForContinue &&
//           lastSecondsForContinue > 0
//         ) {
//           await playerEngine.seekTo(lastSecondsForContinue * 1000);
//           // TODO: reset continue_listen_session_id trong Redux nếu cần
//         }

//         // Nếu có seekTo (manual seek hoặc from latest episode)
//         if (player.seekTo != null) {
//           // GIẢ ĐỊNH seekTo là seconds -> convert ms
//           await playerEngine.seekTo(player.seekTo * 1000);
//         }
//       }
//     };

//     run();

//     return () => {
//       cancelled = true;
//     };
//   }, [
//     player.listenSession,
//     player.listenSessionProcedure,
//     player.playMode.playStatus,
//     player.continue_listen_session_id,
//     player.seekTo,
//   ]);

//   /** 5) Effect: pause / stop từ Redux -> expo-av */
//   useEffect(() => {
//     const status = player.playMode.playStatus;
//     if (status === "pause") {
//       playerEngine.pause();
//     } else if (status === "stop") {
//       lastLoadedSessionIdRef.current = null;
//       playerEngine.stopAndUnload();
//     }
//   }, [player.playMode.playStatus]);

//   /** 5.1) Effect: handle seekTo/seekBy requests from Redux while playing */
//   useEffect(() => {
//     // if (player.playMode.playStatus !== "play") return;
//     if (player.playMode.playStatus === "stop") return;
//     // need a loaded session to apply seek immediately
//     if (!player.listenSession) return;

//     const hasSeekTo = player.seekTo != null;
//     const hasSeekDelta = player.seekDelta != null;
//     if (!hasSeekTo && !hasSeekDelta) return;

//     const run = async () => {
//       try {
//         // let targetSeconds: number | null = null;
//         // if (hasSeekTo) {
//         //   targetSeconds = player.seekTo as number;
//         // } else if (hasSeekDelta) {
//         //   const currentSeconds = Math.floor(lastPositionMsRef.current / 1000);
//         //   targetSeconds = Math.max(
//         //     0,
//         //     currentSeconds + (player.seekDelta as number)
//         //   );
//         // }
//         // if (targetSeconds != null) {
//         //   await playerEngine.seekTo(targetSeconds * 1000);
//         // }
//         const durationSec =
//           Math.floor((lastDurationMsRef.current ?? 0) / 1000) ||
//           player.playbackDuration ||
//           player.currentAudio?.AudioLength ||
//           0;

//         // ...

//         let targetSeconds: number | null = null;
//         if (hasSeekTo) {
//           targetSeconds = player.seekTo as number;
//         } else if (hasSeekDelta) {
//           const currentSeconds = Math.floor(lastDurationMsRef.current / 1000);
//           targetSeconds = currentSeconds + (player.seekDelta as number);
//         }

//         if (targetSeconds != null) {
//           // clamp
//           targetSeconds = Math.max(0, Math.min(targetSeconds, durationSec));
//           await playerEngine.seekTo(targetSeconds * 1000);
//         }
//       } finally {
//         // clear seek requests to avoid repeated seeking
//         dispatch(consumeSeek());
//       }
//     };

//     run();
//     // eslint-disable-next-line react-hooks/exhaustive-deps
//   }, [
//     player.seekTo,
//     player.seekDelta,
//     player.playMode.playStatus,
//     player.listenSession,
//     dispatch,
//   ]);

//   /** 6) Effect: update latest position mỗi 5s khi đang play */
//   useEffect(() => {
//     const ls = player.listenSession;
//     const procedure = player.listenSessionProcedure;

//     if (!ls || !procedure) return;
//     if (player.playMode.playStatus !== "play") return;

//     const sourceType = procedure.SourceDetail.Type;

//     let isCancelled = false;

//     const interval = setInterval(async () => {
//       if (isCancelled) return;

//       const seconds = Math.floor(lastPositionMsRef.current / 1000);

//       try {
//         if (
//           sourceType === "SpecifyShowEpisodes" ||
//           sourceType === "SavedEpisodes"
//         ) {
//           const s = ls as ListenSessionEpisodes;

//           const benefitsList =
//             sourceType === "SpecifyShowEpisodes"
//               ? benefitData?.CurrentPodcastSubscriptionRegistrationBenefitList ??
//                 []
//               : null;

//           await updateEpisodeLastDuration({
//             PodcastEpisodeListenSessionId: s.PodcastEpisodeListenSession.Id,
//             LastListenDurationSeconds: seconds,
//             CurrentPodcastSubscriptionRegistrationBenefitList: benefitsList,
//           }).unwrap();
//         } else {
//           const s = ls as ListenSessionBookingTracks;
//           await updateBookingTrackLastDuration({
//             BookingPodcastTrackListenSessionId:
//               s.BookingPodcastTrackListenSession.Id,
//             LastListenDurationSeconds: seconds,
//           }).unwrap();
//         }
//       } catch (e) {
//         // Backend báo lỗi thì stop luôn
//         dispatch(stopAudio());
//       }
//     }, 5000);

//     return () => {
//       isCancelled = true;
//       clearInterval(interval);
//     };
//   }, [
//     player.listenSession,
//     player.listenSessionProcedure,
//     player.playMode.playStatus,
//     benefitData,
//     dispatch,
//     updateEpisodeLastDuration,
//     updateBookingTrackLastDuration,
//   ]);

//   // Component này không render gì, chỉ là "não" chạy ngầm
//   return null;
// };

// export default PlayerCore;

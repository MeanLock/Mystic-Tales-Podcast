// @ts-nocheck
import { Outlet } from "react-router-dom";
import MediaPlayerSidebar from "./components/MediaPlayerSidebar";
import MediaPlayerControl from "./components/MediaPlayerControlBar";
import { useUpdateAccountMeQuery } from "@/core/services/account/account.service";
import { useSelector, useDispatch } from "react-redux";
import type { RootState } from "@/redux/store";
import {
  useGetEpisodeLatestSessionQuery,
  useGetBookingLatestSessionQuery,
} from "@/core/services/player/player.service";
import {
  setListenSession,
  setListenSessionProcedure,
  setCurrentAudio,
  stopAudio,
  playAudio,
} from "@/redux/slices/mediaPlayerSlice/mediaPlayerSlice";
import { useEffect } from "react";
import type {
  ListenSessionEpisodes,
  ListenSessionBookingTracks,
} from "@/core/types/audio";

const MediaPlayerLayout = () => {
  const dispatch = useDispatch();
  const accessToken = useSelector((state: RootState) => state.auth.accessToken);

  // Chỉ polling khi user đã đăng nhập (có token)
  useUpdateAccountMeQuery(undefined, {
    pollingInterval: accessToken ? 30000 : 0, // 30 giây nếu có token, không poll nếu chưa login
    skip: !accessToken, // Skip query hoàn toàn nếu chưa login
  });

  // Lấy latest session khi mount
  const { data: episodeLatestData } = useGetEpisodeLatestSessionQuery(
    undefined,
    {
      skip: !accessToken,
    }
  );

  const { data: bookingLatestData } = useGetBookingLatestSessionQuery(
    undefined,
    {
      skip: !accessToken,
    }
  );

  // Xử lý latest session khi có data - chỉ set state, playerCore sẽ xử lý việc listen
  useEffect(() => {
    if (!accessToken) return;

    const hasEpisode =
      episodeLatestData?.ListenSession &&
      episodeLatestData?.ListenSessionProcedure;
    const hasBooking =
      bookingLatestData?.ListenSession &&
      bookingLatestData?.ListenSessionProcedure;

    // Nếu có cả 2 -> Lỗi backend
    if (hasEpisode && hasBooking) {
      console.error(
        "Backend error: Both episode and booking latest session exist!"
      );
      dispatch(stopAudio());
      return;
    }

    // Nếu có episode session -> Set vào Redux, playerCore sẽ tự động xử lý
    if (hasEpisode) {
      const episodeSession =
        episodeLatestData.ListenSession as ListenSessionEpisodes;
      const procedure = episodeLatestData.ListenSessionProcedure!;
      const sourceType = procedure.SourceDetail.Type as
        | "SpecifyShowEpisodes"
        | "SavedEpisodes";

      // Set session và procedure vào Redux
      dispatch(setListenSession(episodeSession));
      dispatch(setListenSessionProcedure(procedure));

      // Set current audio UI
      const currentAudioData = {
        Id: episodeSession.PodcastEpisode.Id,
        Name: episodeSession.PodcastEpisode.Name,
        MainFileKey: episodeSession.PodcastEpisode.MainImageFileKey || "",
        PodcasterName: episodeSession.Podcaster.FullName || "Unknown",
        AudioLength:
          episodeSession.PodcastEpisodeListenSession
            .LastListenDurationSeconds || 0,
      };
      dispatch(setCurrentAudio(currentAudioData));

      // Trigger play - playerCore sẽ tự gọi listen API và load HLS
      dispatch(
        playAudio({
          sourceType: sourceType,
          audioId: episodeSession.PodcastEpisode.Id,
        })
      );
      return;
    }

    // Nếu có booking session -> Set vào Redux, playerCore sẽ tự động xử lý
    if (hasBooking) {
      const bookingSession =
        bookingLatestData.ListenSession as ListenSessionBookingTracks;
      const procedure = bookingLatestData.ListenSessionProcedure!;

      // Set session và procedure vào Redux
      dispatch(setListenSession(bookingSession));
      dispatch(setListenSessionProcedure(procedure));

      // Set current audio UI
      const currentAudioData = {
        Id: bookingSession.BookingPodcastTrack.Id,
        Name: bookingSession.BookingPodcastTrack.BookingRequirementName,
        MainFileKey: "", // Booking track không có ảnh đại diện
        PodcasterName: "Booking Track",
        AudioLength:
          bookingSession.BookingPodcastTrackListenSession
            .LastListenDurationSeconds || 0,
      };
      dispatch(setCurrentAudio(currentAudioData));

      // Trigger play - playerCore sẽ tự gọi listen API và load HLS
      dispatch(
        playAudio({
          sourceType: "BookingProducingTracks",
          audioId: bookingSession.BookingPodcastTrack.Id,
        })
      );
      return;
    }

    // Nếu cả 2 đều null -> Stop
    if (!hasEpisode && !hasBooking) {
      dispatch(stopAudio());
    }
  }, [episodeLatestData, bookingLatestData, accessToken, dispatch]);

  return (
    <div className="relative w-full h-screen flex flex-col justify-between bg-[url(/background/mediaplayer2.jpg)] bg-cover object-cover gap-5 overflow-hidden">
      <div className="absolute inset-0 bg-black/40" />

      <div className="w-full flex gap-5 flex-1 px-5 pt-5">
        <MediaPlayerSidebar />
        <div
          className="
            flex-1 
            bg-white/10 backdrop-blur-[10px] shadow-2xl 
            rounded-3xl
            min-w-[500px]
            h-[734px]
            overflow-y-auto
            [&::-webkit-scrollbar]:hidden
            [-ms-overflow-style:none]
            [scrollbar-width:none]
          
          "
        >
          <Outlet />
        </div>
      </div>

      <div className="w-full bg-white/10 backdrop-blur-[5px] shadow-2xlitems-center justify-center h-[86px] ">
        <MediaPlayerControl />
      </div>
    </div>
  );
};

export default MediaPlayerLayout;

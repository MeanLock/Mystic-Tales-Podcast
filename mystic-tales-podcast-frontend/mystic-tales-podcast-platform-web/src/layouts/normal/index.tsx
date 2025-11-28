import { Outlet, useNavigate } from "react-router-dom";
import NormalHeader from "./components/NormalHeader";
import "./styles.css";
import { useUpdateAccountMeQuery } from "@/core/services/account/account.service";
import { useSelector, useDispatch } from "react-redux";
import type { RootState } from "@/redux/store";
import { useEffect } from "react";
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
import type {
  ListenSessionEpisodes,
  ListenSessionBookingTracks,
} from "@/core/types/audio";

const NormalLayout = () => {
  const dispatch = useDispatch();
  const accessToken = useSelector((state: RootState) => state.auth.accessToken);

  // Chỉ polling khi user đã đăng nhập (có token)
  useUpdateAccountMeQuery(undefined, {
    pollingInterval: accessToken ? 30000 : 0, // 5 giây nếu có token, không poll nếu chưa login
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
        ImageUrl: episodeSession.PodcastEpisode.MainImageFileKey || "",
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
        ImageUrl: "",
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

  const navigate = useNavigate();
  useEffect(() => {
    if (accessToken) {
      // Tạo ID riêng cho từng tab nếu chưa có
      if (!sessionStorage.getItem("tabSessionId")) {
        sessionStorage.setItem("tabSessionId", crypto.randomUUID());
      }

      const tabSessionId = sessionStorage.getItem("tabSessionId");
      const isInWebKey = `${tabSessionId}:isInWeb`;

      const isFirstInWeb = sessionStorage.getItem(isInWebKey);

      if (!isFirstInWeb) {
        sessionStorage.setItem(isInWebKey, "true");
        navigate("/media-player/discovery");
      }
    }
  }, [accessToken, navigate]);

  return (
    <div className="relative min-h-screen w-full overflow-hidden bg-black">
      {/* 🎥 Background Video */}
      <video
        className="fixed top-0 left-0 w-full h-full object-cover z-0"
        src="/background/2.mp4"
        autoPlay
        loop
        muted
        playsInline
      />

      {/* 🩸 Overlay mờ để chữ không bị chìm */}
      <div id="overlay-glasses" className="absolute inset-0 z-10" />

      <div className="fixed top-5 left-1/2 -translate-x-1/2 z-30">
        <NormalHeader />
      </div>
      {/* 🧱 Nội dung chính */}
      <div className="relative z-20 w-full">
        <div className="mt-36">
          <Outlet />
        </div>
      </div>
    </div>
  );
};

export default NormalLayout;

import { Outlet, useNavigate } from "react-router-dom";
import NormalHeader from "./components/NormalHeader";
import "./styles.css";
import { useUpdateAccountMeQuery } from "@/core/services/account/account.service";
import { useSelector, useDispatch } from "react-redux";
import type { RootState } from "@/redux/store";
import { useEffect } from "react";
// import {
//   useGetEpisodeLatestSessionQuery,
//   useGetBookingLatestSessionQuery,
// } from "@/core/services/player/player.service";
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
import { usePlayer } from "@/core/services/player/usePlayer";

const NormalLayout = () => {
  const dispatch = useDispatch();
  const accessToken = useSelector((state: RootState) => state.auth.accessToken);
  const user = useSelector((state: RootState) => state.auth.user);
  const { playFromLatest } = usePlayer();
  // Chỉ polling khi user đã đăng nhập (có token)
  useUpdateAccountMeQuery(undefined, {
    pollingInterval: accessToken ? 300000 : 0, // 5 giây nếu có token, không poll nếu chưa login
    skip: !accessToken, // Skip query hoàn toàn nếu chưa login
  });

  // Lấy latest session khi mount
  // const { data: episodeLatestData } = useGetEpisodeLatestSessionQuery(
  //   undefined,
  //   {
  //     skip: !accessToken,
  //   }
  // );

  // const { data: bookingLatestData } = useGetBookingLatestSessionQuery(
  //   undefined,
  //   {
  //     skip: !accessToken,
  //   }
  // );

  // Xử lý latest session khi có data - chỉ set state, playerCore sẽ xử lý việc listen
  useEffect(() => {
    if (user) {
      playFromLatest();
    }
  }, []);

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

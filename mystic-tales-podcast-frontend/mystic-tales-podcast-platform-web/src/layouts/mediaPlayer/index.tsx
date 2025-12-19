import { Outlet, useNavigate } from "react-router-dom";
import MediaPlayerSidebar from "./components/MediaPlayerSidebar";
import { useUpdateAccountMeQuery } from "@/core/services/account/account.service";
import { useSelector, useDispatch } from "react-redux";
import type { RootState } from "@/redux/store";
import { useEffect, useRef } from "react";
import NewMediaPlayerControlBar from "./components/NewMediaPlayerControlBar";
import { usePlayer } from "@/core/services/player/usePlayer";
import { setUser, clearAuth } from "@/redux/slices/authSlice/authSlice";

const MediaPlayerLayout = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const accessToken = useSelector((state: RootState) => state.auth.accessToken);
  const user = useSelector((state: RootState) => state.auth.user);
  const { playFromLatest, stop } = usePlayer();
  const hasCalledPlayFromLatest = useRef(false);

  // Chỉ polling khi user đã đăng nhập (có token)
  const { data, error } = useUpdateAccountMeQuery(undefined, {
    pollingInterval: accessToken ? 1000 * 60 * 5 : 0, // 5 phút nếu có token, không poll nếu chưa login
    skip: !accessToken, // Skip query hoàn toàn nếu chưa login
    refetchOnFocus: true,
    refetchOnReconnect: true,
    refetchOnMountOrArgChange: accessToken ? true : false,
  });

  // Xử lý data và error từ updateAccountMe
  useEffect(() => {
    if (error) {
      dispatch(clearAuth());
      stop();
      navigate("/auth/login");
    }
  }, [error, dispatch, navigate]);

  useEffect(() => {
    if (data?.Account) {
      dispatch(setUser(data.Account));
    }
  }, [data, dispatch]);

  // Xử lý latest session khi có data - chỉ set state, playerCore sẽ xử lý việc listen
  useEffect(() => {
    if (user && !hasCalledPlayFromLatest.current) {
      hasCalledPlayFromLatest.current = true;
      playFromLatest();
    }
  }, [user, playFromLatest]);

  return (
    <div className="relative w-full h-screen flex flex-col justify-between bg-[url(/background/mediaplayer2.jpg)] bg-cover object-cover gap-5 overflow-hidden">
      <div className="absolute inset-0 bg-black/40" />

      <div className="w-full h-[80%] flex gap-5 flex-1 px-5 pt-5">
        <MediaPlayerSidebar />
        <div
          className="
            flex-1 
            bg-white/10 backdrop-blur-[10px] shadow-2xl 
            rounded-3xl
            min-w-[500px]
            h-full
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
        {/* <MediaPlayerControl /> */}
        <NewMediaPlayerControlBar />
      </div>
    </div>
  );
};

export default MediaPlayerLayout;

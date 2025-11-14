import { Outlet } from "react-router-dom";
import MediaPlayerSidebar from "./components/MediaPlayerSidebar";
import MediaPlayerControl from "./components/MediaPlayerControlBar";
import { useUpdateAccountMeQuery } from "@/core/services/account/account.service";
import { useSelector } from "react-redux";
import type { RootState } from "@/redux/store";

const MediaPlayerLayout = () => {
  const accessToken = useSelector((state: RootState) => state.auth.accessToken);

  // Chỉ polling khi user đã đăng nhập (có token)
  useUpdateAccountMeQuery(undefined, {
    pollingInterval: accessToken ? 30000 : 0, // 30 giây nếu có token, không poll nếu chưa login
    skip: !accessToken, // Skip query hoàn toàn nếu chưa login
  });

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

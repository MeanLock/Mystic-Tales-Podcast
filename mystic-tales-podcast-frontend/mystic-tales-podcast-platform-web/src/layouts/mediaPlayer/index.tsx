import { Outlet } from "react-router-dom";
import MediaPlayerSidebar from "./components/MediaPlayerSidebar";
import MediaPlayerControl from "./components/MediaPlayerControlBar";

const MediaPlayerLayout = () => {
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
            p-8
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

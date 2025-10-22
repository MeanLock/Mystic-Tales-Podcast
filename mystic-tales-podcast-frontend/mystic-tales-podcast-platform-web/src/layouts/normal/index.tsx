import { Outlet } from "react-router-dom";
import NormalHeader from "./components/NormalHeader";
import "./styles.css";
import { BackgroundBeamsWithCollision } from "@/components/ui/shadcn-io/background-beams-with-collision";
import { BubbleBackground } from "@/components/ui/shadcn-io/bubble-background";

const NormalLayout = () => {
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

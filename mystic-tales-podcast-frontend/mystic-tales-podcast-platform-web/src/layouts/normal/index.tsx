import { Outlet } from "react-router-dom";
import NormalHeader from "./components/NormalHeader";
import "./styles.css";
import { useUpdateAccountMeQuery } from "@/core/services/account/account.service";
import { useSelector } from "react-redux";
import type { RootState } from "@/redux/store";

const NormalLayout = () => {
  const accessToken = useSelector((state: RootState) => state.auth.accessToken);

  // Chỉ polling khi user đã đăng nhập (có token)
  useUpdateAccountMeQuery(undefined, {
    pollingInterval: accessToken ? 30000 : 0, // 5 giây nếu có token, không poll nếu chưa login
    skip: !accessToken, // Skip query hoàn toàn nếu chưa login
  });

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

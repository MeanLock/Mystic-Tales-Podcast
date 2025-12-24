import { Navigate } from "react-router-dom";
import { useSelector } from "react-redux";
import type { RootState } from "@/redux/store";

const RootRedirect = () => {
  const accessToken = useSelector((state: RootState) => state.auth.accessToken);

  // Nếu có accessToken thì đẩy về media-player, không thì về home
  return (
    <Navigate to={accessToken ? "/media-player/discovery" : "/home"} replace />
  );
};

export default RootRedirect;

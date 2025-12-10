import { Navigate } from "react-router-dom";
import { useSelector } from "react-redux";
import type { RootState } from "../redux/rootReducer";
import type { JSX } from "react";

const ProtectedRoute = ({
  redirectUrl,
  element,
}: {
  redirectUrl: string;
  element: JSX.Element;
}) => {
  const token = useSelector((state: RootState) => state.auth.token);

  if (!token) {
    localStorage.setItem("redirectUrl", redirectUrl);
    return <Navigate to="/login" />;
  }

  return <div className="w-full mt-10">{element}</div>;
};

export default ProtectedRoute;

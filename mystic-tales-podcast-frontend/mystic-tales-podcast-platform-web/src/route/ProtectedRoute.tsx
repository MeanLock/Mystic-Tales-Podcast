import { useSelector } from "react-redux";
import { Navigate, Outlet } from "react-router-dom";
import type { RootState } from "@/redux/store";

const ProtectedRoute = () => {
  const user = useSelector((state: RootState) => state.auth.user);

  // If user is not logged in, redirect to login page
  if (!user) {
    return <Navigate to="/" replace />;
  }

  // If user is logged in, render the protected content
  return <Outlet />;
};

export default ProtectedRoute;

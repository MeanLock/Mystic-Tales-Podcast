import { Outlet } from "react-router-dom";

const AuthLayout = () => {
  return (
    <div className="w-full overflow-hidden">
      <Outlet />
    </div>
  );
};

export default AuthLayout;

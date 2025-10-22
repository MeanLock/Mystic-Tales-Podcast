import type { RouteObject } from "react-router-dom";
import AuthLayout from "../views/components/layouts/auth-layout";
import { lazy } from "react";


const LoginPage = lazy(() => import("../views/pages/login-page"));
const RegisterPage = lazy(() => import("../views/pages/register-page"));

export const authRoutes: RouteObject = {
  path: "/",
  element: <AuthLayout />,
  children: [
    { path: "login", element: <LoginPage /> },
    { path: "register", element: <RegisterPage /> },

  ],
};

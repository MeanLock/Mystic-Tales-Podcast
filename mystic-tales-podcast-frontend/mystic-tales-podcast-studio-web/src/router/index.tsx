import { createBrowserRouter } from 'react-router-dom';
import { authRoutes } from './auth.routes';
import React from 'react';

const DefaultLayout = React.lazy(() => import('./../views/components/layouts/default-layout/index'))
const Login = React.lazy(() => import('./../views/pages/login-page/index'))

export const appRouter = createBrowserRouter([
   {
    path: '/login',
    element: <Login />,
  },
  {
    path: '*',
    element: <DefaultLayout />,
  },
]);

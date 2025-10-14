import type { RouteObject } from 'react-router-dom';
import DefaultLayout from '../views/components/layouts/default-layout';
import { lazy } from 'react';

const HomePage = lazy(() => import('../views/pages/home-page'));

export const defaultRoutes: RouteObject = {
  path: '/',
  element: <DefaultLayout />,
  children: [
    { index: true, element: <HomePage /> },
    // route con khác...
  ],
};

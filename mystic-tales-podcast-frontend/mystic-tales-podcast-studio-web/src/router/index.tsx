import { createBrowserRouter } from 'react-router-dom';
import { authRoutes } from './auth.routes';
import { defaultRoutes } from './default.routes';
import { surveyRoutes } from './survey.routes';

export const appRouter = createBrowserRouter([
  authRoutes,
  defaultRoutes,
  surveyRoutes,
  {
    path: '*',
    element: <div>404 Not Found</div>,
  }
]);

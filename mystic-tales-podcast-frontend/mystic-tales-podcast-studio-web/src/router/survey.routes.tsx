
import type { RouteObject } from 'react-router-dom';
import SurveyLayout from '../views/components/layouts/survey-layout';
import { lazy } from 'react';

const SurveyPage = lazy(() => import('../views/pages/survey-page'));

export const surveyRoutes: RouteObject = {
  path: '/survey',
  element: <SurveyLayout />,
  children: [
    { index: true, element: <SurveyPage /> },
    {path: 'create', element: <SurveyPage /> },
  ],
};

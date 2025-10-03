import { lazy } from "react";

export const SurveyNew = lazy(() => import("../pages/SurveyNew/index"));
export const MySurvey = lazy(() => import("../pages/MySurvey/index"));
export const SurveyEdit = lazy(() => import("../pages/SurveyUpdate/index"));
export const SurveyShare = lazy(() => import("../pages/SurveyShare/index"));
export const SurveyCustomer = lazy(
  () => import("../pages/SurveyCustomer/index")
);
export const EndSurveyCustomer = lazy(
  () => import("../pages/EndSurveyCustomer/index")
);

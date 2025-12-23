import { store } from "@/src/store/store";
import {
  AlertType,
  setDataAndShowAlert,
} from "@/src/features/alert/alertSlice";

interface ShowAlertParams {
  title: string;
  description: string;
  type: AlertType;
  isCloseable: boolean;
  isFunctional: boolean;
  functionalButtonText?: string;
  autoCloseDuration?: number;
  actionId?: string;
}

/**
 * Utility function to show alert from anywhere (not just React components)
 * Can be used in axios interceptors, utility functions, etc.
 */
export const showAlertUtil = (params: ShowAlertParams) => {
  store.dispatch(setDataAndShowAlert(params));
};

/**
 * Pre-configured alert for login required scenarios
 */
export const showLoginRequiredAlert = () => {
  showAlertUtil({
    type: "warning",
    title: "Login Required",
    description: "Your session has expired or you need to login to continue.",
    isFunctional: true,
    isCloseable: true,
    functionalButtonText: "Login Now",
    actionId: "login-required",
  });
};

/**
 * Pre-configured alert for token expired
 */
export const showTokenExpiredAlert = () => {
  showAlertUtil({
    type: "error",
    title: "Session Expired",
    description:
      "Your login session has expired. Please login again to continue.",
    isFunctional: true,
    isCloseable: true,
    functionalButtonText: "Login Now",
    actionId: "token-expired",
  });
};

/**
 * Pre-configured alert for unauthorized access
 */
export const showUnauthorizedAlert = () => {
  showAlertUtil({
    type: "warning",
    title: "Unauthorized",
    description:
      "You don't have permission to access this resource. Please login first.",
    isFunctional: true,
    isCloseable: true,
    functionalButtonText: "Login Now",
    actionId: "unauthorized",
  });
};

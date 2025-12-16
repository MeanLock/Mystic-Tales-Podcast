import { store } from "@/redux/store";
import { showAlert } from "@/redux/slices/alertSlice/alertSlice";

interface ShowAlertParams {
  type: "success" | "error" | "info" | "warning";
  title: string;
  description: string;
  isAutoClose: boolean;
  autoCloseDuration?: number;
  isFunctional?: boolean;
  isClosable?: boolean;
  functionalButtonText?: string;
  onClickAction?: () => void;
}

/**
 * Utility function to show alert from anywhere (not just React components)
 * Can be used in axios interceptors, utility functions, etc.
 */
export const showAlertUtil = (params: ShowAlertParams) => {
  store.dispatch(showAlert(params));
};

/**
 * Pre-configured alert for login required scenarios
 */
export const showLoginRequiredAlert = () => {
  showAlertUtil({
    type: "warning",
    title: "Login Required",
    description: "Your session has expired or you need to login to continue.",
    isAutoClose: false,
    isFunctional: true,
    isClosable: true,
    functionalButtonText: "Login Now",
    onClickAction: () => {
      window.location.href = "/auth/login";
    },
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
    isAutoClose: false,
    isFunctional: true,
    isClosable: true,
    functionalButtonText: "Login Now",
    onClickAction: () => {
      window.location.href = "/auth/login";
    },
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
    isAutoClose: false,
    isFunctional: true,
    isClosable: true,
    functionalButtonText: "Login Now",
    onClickAction: () => {
      window.location.href = "/auth/login";
    },
  });
};

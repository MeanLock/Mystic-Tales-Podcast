export type AlertType = "success" | "error" | "info" | "warning";

export interface AlertState {
  isAlert: boolean;
  type: AlertType;
  title: string;
  description: string;
  isAutoClose: boolean;
  autoCloseDuration?: number;
  isFunctional?: boolean;
  isClosable?: boolean;
  functionalButtonText?: string;
  onClickAction?: () => void;
}

export type AlertMessage = {
  id: string;
  type: AlertType;
  title: string;
  description: string;
};

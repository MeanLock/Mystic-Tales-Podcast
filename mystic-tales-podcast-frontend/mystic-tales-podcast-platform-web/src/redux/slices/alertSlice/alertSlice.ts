import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

interface AlertState {
  isAlert: boolean;
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

interface ShowAlertPayload {
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

const initialState: AlertState = {
  isAlert: false,
  type: "info",
  title: "",
  description: "",
  isAutoClose: false,
  autoCloseDuration: 3000,
  isClosable: true,
};

// const initialState: AlertState = {
//   isAlert: true,
//   type: "info",
//   title: "Opps! Something went wrong.",
//   description: "You need to login to perform this action.",
//   isAutoClose: true,
//   autoCloseDuration: 30000,
//   isFunctional: true,
//   functionalButtonText: "Login Now",
//   onClickAction: () => {
//     window.location.href = "/auth/login";
//   },
// };

const alertSlice = createSlice({
  name: "alert",
  initialState,
  reducers: {
    showAlert(state, action: PayloadAction<ShowAlertPayload>) {
      state.isAlert = true;
      state.type = action.payload.type;
      state.title = action.payload.title;
      state.description = action.payload.description;
      state.isAutoClose = action.payload.isAutoClose;
      state.autoCloseDuration = action.payload.autoCloseDuration;
      state.isClosable = action.payload.isClosable;
      state.isFunctional = action.payload.isFunctional || false;
      state.functionalButtonText =
        action.payload.functionalButtonText || "Close";
      state.onClickAction = action.payload.onClickAction || undefined;
    },
    hideAlert(state) {
      state.isAlert = false;
      state.type = "info";
      state.title = "";
      state.description = "";
      state.isAutoClose = false;
      state.autoCloseDuration = 3000;
      state.isFunctional = false;
      state.functionalButtonText = "Close";
      state.onClickAction = undefined;
    },
  },
});

export const { showAlert, hideAlert } = alertSlice.actions;
export default alertSlice.reducer;

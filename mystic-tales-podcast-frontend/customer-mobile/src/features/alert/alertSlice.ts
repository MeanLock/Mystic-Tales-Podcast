// src/redux/slices/alertSlice.ts
import { createSlice, PayloadAction } from "@reduxjs/toolkit";

export type AlertType = "success" | "error" | "warning" | "info";

export interface AlertState {
  visible: boolean;
  title: string;
  description: string;
  type: AlertType;
  isCloseable: boolean;
  isFunctional: boolean;
  functionalButtonText?: string;
  autoCloseDuration?: number;
  actionId?: string;
}

const initialState: AlertState = {
  visible: false,
  title: "",
  description: "",
  type: "info",
  isCloseable: true,
  isFunctional: false,
  functionalButtonText: undefined,
  autoCloseDuration: undefined,
  actionId: undefined,
};

type ShowAlertPayload = {
  title: string;
  description: string;
  type: AlertType;
  isCloseable: boolean;
  isFunctional: boolean;
  functionalButtonText?: string;
  autoCloseDuration?: number;
  actionId?: string;
};

const alertSlice = createSlice({
  name: "alert",
  initialState,
  reducers: {
    setDataAndShowAlert(state, action: PayloadAction<{} & ShowAlertPayload>) {
      state.visible = true;
      state.title = action.payload.title;
      state.description = action.payload.description;
      state.type = action.payload.type;
      state.isCloseable = action.payload.isCloseable;
      state.isFunctional = action.payload.isFunctional;
      state.functionalButtonText =
        action.payload.functionalButtonText || "Close";
      state.autoCloseDuration = action.payload.autoCloseDuration || undefined;
      state.actionId = action.payload.actionId || undefined;
    },

    hideAlert(state) {
      state.visible = false;
      state.type = "info";
      state.title = "";
      state.description = "";
      state.isCloseable = true;
      state.isFunctional = false;
      state.functionalButtonText = undefined;
      state.autoCloseDuration = undefined;
      state.actionId = undefined;
    },
  },
});

export const { setDataAndShowAlert, hideAlert } = alertSlice.actions;
export default alertSlice.reducer;

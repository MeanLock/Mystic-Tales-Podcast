// src/redux/slices/alertSlice.ts
import { createSlice, PayloadAction } from "@reduxjs/toolkit";

export type AlertType = "success" | "error";

export interface AlertState {
  visible: boolean;
  type: AlertType | null;
  message: string;
  seconds?: number;
}

const initialState: AlertState = {
  visible: false,
  type: null,
  message: "",
  seconds: undefined,
};

type ShowAlertPayload = {
  message: string;
  seconds?: number;
};

const alertSlice = createSlice({
  name: "alert",
  initialState,
  reducers: {
    showSuccess(state, action: PayloadAction<ShowAlertPayload>) {
      state.visible = true;
      state.type = "success";
      state.message = action.payload.message;
      state.seconds = action.payload.seconds;
    },
    showError(state, action: PayloadAction<ShowAlertPayload>) {
      state.visible = true;
      state.type = "error";
      state.message = action.payload.message;
      state.seconds = action.payload.seconds;
    },
    hideAlert(state) {
      state.visible = false;
      state.type = null;
      state.message = "";
      state.seconds = undefined;
    },
  },
});

export const { showSuccess, showError, hideAlert } = alertSlice.actions;
export default alertSlice.reducer;

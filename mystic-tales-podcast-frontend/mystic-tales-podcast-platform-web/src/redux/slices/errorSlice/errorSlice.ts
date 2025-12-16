// src/redux/slices/errorSlice.ts
import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

interface ErrorState {
  isError: boolean;
  message: string;
  autoClose: number | null;
}

const initialState: ErrorState = {
  isError: false,
  message: "",
  autoClose: null,
};

const errorSlice = createSlice({
  name: "error",
  initialState,
  reducers: {
    setError(
      state,
      action: PayloadAction<{ message: string; autoClose: number | null }>
    ) {
      state.isError = true;
      state.message = action.payload.message;
      state.autoClose = action.payload.autoClose;
    },
    endError(state) {
      state.isError = false;
      state.message = "";
      state.autoClose = null;
    },
  },
});

export const { setError, endError } = errorSlice.actions;
export default errorSlice.reducer;

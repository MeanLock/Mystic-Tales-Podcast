// src/redux/slices/authSlice.ts
import type { AccountUI } from "@/core/types/account";
import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

interface AuthState {
  accessToken?: string | null;
  user?: AccountUI | null;
}

const initialState: AuthState = { accessToken: null, user: null };

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    setAuthToken(state, action: PayloadAction<string>) {
      state.accessToken = action.payload;
    },
    setUser(state, action: PayloadAction<AccountUI>) {
      state.user = action.payload;
    },
    clearAuth(state) {
      state.accessToken = null;
      state.user = null;
    },
  },
});

export const { setAuthToken, setUser, clearAuth } = authSlice.actions;
export default authSlice.reducer;

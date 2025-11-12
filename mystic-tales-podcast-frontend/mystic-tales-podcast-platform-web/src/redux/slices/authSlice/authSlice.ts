// src/redux/slices/authSlice.ts
import type { AccountMeFromApi } from "@/core/types/account";
import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

interface AuthState {
  accessToken?: string | null;
  user?: AccountMeFromApi | null;
}

const initialState: AuthState = { accessToken: null, user: null };

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    setAuthToken(state, action: PayloadAction<string>) {
      state.accessToken = action.payload;
    },
    setUser(state, action: PayloadAction< AccountMeFromApi>) {
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

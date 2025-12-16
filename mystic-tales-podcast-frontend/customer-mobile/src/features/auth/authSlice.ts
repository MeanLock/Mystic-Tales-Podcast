// ...existing code...
import { User } from "@/src/core/types/account.type";
import { createSlice, PayloadAction } from "@reduxjs/toolkit";


type AuthType = { user: User | null; accessToken: string | null };

const initialState: AuthType = { user: null, accessToken: null };

const slice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    setCredentials: (
      s,
      a: PayloadAction<{ user: User | null; accessToken?: string | null }>
    ) => {
      s.user = a.payload.user ?? null;
      // set accessToken if provided (keeps existing if undefined)
      if (typeof a.payload.accessToken !== "undefined") {
        s.accessToken = a.payload.accessToken ?? null;
      }
    },
    logoutLocal: (s) => {
      s.user = null;
      s.accessToken = null;
    },
  },
});
export const { setCredentials, logoutLocal } = slice.actions;
export default slice.reducer;
export type AuthState = ReturnType<typeof slice.getInitialState>;

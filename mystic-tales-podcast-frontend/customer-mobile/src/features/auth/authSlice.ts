import { createSlice, PayloadAction } from "@reduxjs/toolkit";

type AuthState = { user: any | null; isLoggedIn: boolean };
const initialState: AuthState = { user: null, isLoggedIn: false };

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    setLoggedIn(state, action: PayloadAction<any>) {
      state.user = action.payload; // object user
      state.isLoggedIn = true;
    },
    setLoggedOut(state) {
      state.user = null;
      state.isLoggedIn = false;
    },
  },
});

export const { setLoggedIn, setLoggedOut } = authSlice.actions;
export default authSlice.reducer;

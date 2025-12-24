import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface AuthState {
  token: string | null;
  user: any | null;
  imageUrl?: string | null;
}

const initialState: AuthState = {
  token: null,
  user: null,
  imageUrl: null,
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    setAuthToken: (state, action ) => {
      state.token = action.payload.token;
      state.user = action.payload.user; 
      state.imageUrl = action.payload.imageUrl || null; 
    },
    clearAuthToken: (state) => {
      state.token = null;
      state.user = null;
      state.imageUrl = null;
    },
  },
});

export const { setAuthToken, clearAuthToken } = authSlice.actions;
export default authSlice.reducer;
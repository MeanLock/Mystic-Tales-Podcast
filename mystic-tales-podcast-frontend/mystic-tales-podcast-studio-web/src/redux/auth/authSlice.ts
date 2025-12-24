import { createSlice } from '@reduxjs/toolkit';

export interface AuthState {
  token: string | null;
  user: any | null;
}

const initialState: AuthState = {
  token: null,
  user: null,
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    setAuthToken: (state, action ) => {
      state.token = action.payload.token;
      state.user = action.payload.user; 
    },
    clearAuthToken: (state) => {
        
      state.token = null;
      state.user = null;
    },
  },
});

export const { setAuthToken, clearAuthToken } = authSlice.actions;
export default authSlice.reducer;
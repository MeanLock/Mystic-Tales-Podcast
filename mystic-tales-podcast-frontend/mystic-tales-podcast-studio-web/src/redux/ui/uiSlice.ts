
import { createSlice } from '@reduxjs/toolkit';

export interface UiState {
  sidebarNarrow: boolean;
  sidebarMobileOpen: boolean;
  theme: string;
}

const initialState: UiState = {
  sidebarNarrow: false,
  sidebarMobileOpen: false,
  theme: 'light',
} ;

const uiSlice = createSlice({
  name: 'ui',
  initialState,
  reducers: {
    set: (state, action) => {
      return { ...state, ...action.payload };
    },
  },
});

export const { set } = uiSlice.actions;
export default uiSlice.reducer;

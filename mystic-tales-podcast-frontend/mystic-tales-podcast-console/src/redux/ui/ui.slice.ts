
import { createSlice } from '@reduxjs/toolkit';

export interface UiState {
  sidebarNarrow: boolean;
  theme: string;
}

const initialState: UiState = {
  sidebarNarrow: false,
  theme: '123',
};

const uiSlice = createSlice({
  name: 'ui',
  initialState,
  reducers: {
    setUi: (state, action) => {
      return { ...state, ...action.payload };
    },
  },
});

export const { setUi } = uiSlice.actions;
export default uiSlice.reducer;

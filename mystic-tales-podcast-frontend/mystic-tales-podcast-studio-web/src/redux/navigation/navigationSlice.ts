import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface NavigationState {
  contextType: 'user' | 'show';
  currentContext: {
    id: string;
    name: string;
    avatar?: string;
    email?: string;
    subtitle?: string;
  } | null;
  navItems: Array<{
    label: string;
    icon: string;
    path: string;
  }>;
}

const initialState: NavigationState = {
  contextType: 'user',
  currentContext: null,
  navItems: []
};

const navigationSlice = createSlice({
  name: 'navigation',
  initialState,
  reducers: {
    setUserContext: (state, action: PayloadAction<{
      user: {
        id: string;
        name: string;
        email: string;
        avatar?: string;
      };
      navItems: Array<{ label: string; icon: string; path: string; }>;
    }>) => {
      state.contextType = 'user';
      state.currentContext = {
        ...action.payload.user,
        subtitle: 'Podcaster'
      };
      state.navItems = action.payload.navItems;
    },
    setShowContext: (state, action: PayloadAction<{
      show: {
        id: string;
        name: string;
        avatar?: string;
      };
      navItems: Array<{ label: string; icon: string; path: string; }>;
    }>) => {
      state.contextType = 'show';
      state.currentContext = {
        ...action.payload.show,
        subtitle: 'Show'
      };
      state.navItems = action.payload.navItems;
    },
    clearContext: (state) => {
      state.currentContext = null;
      state.navItems = [];
    }
  }
});

export const { setUserContext, setShowContext, clearContext } = navigationSlice.actions;
export default navigationSlice.reducer;
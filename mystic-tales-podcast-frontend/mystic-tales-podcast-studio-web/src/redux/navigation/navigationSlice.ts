import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { JSX } from 'react';

export interface NavigationState {
  contextType: 'user' | 'channel' | 'show';
  currentContext: {
    id: string;
    name: string;
    avatar?: string;
    email?: string;
    type?: string;
  } | null;
  navItems: Array<{
    label: string;
    icon: JSX.Element;
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
      navItems: Array<{ label: string; icon: JSX.Element; path: string; }>;
    }>) => {
      state.contextType = 'user';
      state.currentContext = {
        ...action.payload.user,
      };
      state.navItems = action.payload.navItems;
    },

    setChannelContext: (state, action: PayloadAction<{
      channel: {
        id: string;
        name: string;
        avatar?: string;
      };
      navItems: Array<{ label: string; icon: JSX.Element; path: string; }>;
    }>) => {
      state.contextType = 'channel';
      state.currentContext = {
        ...action.payload.channel,
        type: 'Channel'
      };
      state.navItems = action.payload.navItems;
    },

     setShowContext: (state, action: PayloadAction<{
      show: {
        id: string;
        name: string;
        avatar?: string;
      };
      navItems: Array<{ label: string; icon: JSX.Element; path: string; }>;
    }>) => {
      state.contextType = 'show';
      state.currentContext = {
        ...action.payload.show,
        type: 'Show'
      };
      state.navItems = action.payload.navItems;
    },

    clearContext: (state) => {
      state.currentContext = null;
      state.navItems = [];
    }
  }
});

export const { setUserContext, setChannelContext, setShowContext, clearContext } = navigationSlice.actions;
export default navigationSlice.reducer;
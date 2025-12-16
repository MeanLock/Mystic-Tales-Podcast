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
}

const initialState: NavigationState = {
  contextType: 'user',
  currentContext: null,
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
    }>) => {
      // replace state to drop any stray keys (e.g., legacy navItems)
      return {
        contextType: 'user',
        currentContext: {
          ...action.payload.user,
        }
      } as NavigationState as any;
    },

    setChannelContext: (state, action: PayloadAction<{
      channel: {
        id: string;
        name: string;
        avatar?: string;
      };
    }>) => {
      return {
        contextType: 'channel',
        currentContext: {
          ...action.payload.channel,
          type: 'Channel'
        }
      } as NavigationState as any;
    },

     setShowContext: (state, action: PayloadAction<{
      show: {
        id: string;
        name: string;
        avatar?: string;
      };
    }>) => {
      return {
        contextType: 'show',
        currentContext: {
          ...action.payload.show,
          type: 'Show'
        }
      } as NavigationState as any;
    },

    clearContext: () => {
      // reset to initialState (ensures removal of any non-serializable legacy fields)
      return initialState;
    }
  }
});

export const { setUserContext, setChannelContext, setShowContext, clearContext } = navigationSlice.actions;
export default navigationSlice.reducer;
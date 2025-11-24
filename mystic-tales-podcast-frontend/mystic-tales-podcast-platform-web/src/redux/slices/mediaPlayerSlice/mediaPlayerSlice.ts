import type {
  ListenSession,
  ListenSessionProcedure,
  PlayerControl,
  CurrentAudioUI,
} from "@/core/types/audio";
import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

interface MediaPlayerSlice {
  playMode: PlayerControl;
  currentAudio: CurrentAudioUI;
  listenSession: ListenSession;
  listenSessionProcedure: ListenSessionProcedure;
}

const initialState: MediaPlayerSlice = {
  playMode: {
    playStatus: "stop",
    isAutoPlay: false,
    isNextSessionNull: false,
    nextMode: "Sequential",
    sourceType: null,
    audioId: null,
    volume: 100,
  },
  currentAudio: null,
  listenSession: null,
  listenSessionProcedure: null,
};

function clamp(value: number, min: number, max: number): number {
  return Math.min(Math.max(value, min), max);
}

const mediaPlayerSlice = createSlice({
  name: "player",
  initialState: initialState,
  reducers: {
    setListenSession: (state, action: PayloadAction<ListenSession>) => {
      state.listenSession = action.payload;
    },
    setListenSessionProcedure: (
      state,
      action: PayloadAction<ListenSessionProcedure>
    ) => {
      state.listenSessionProcedure = action.payload;
    },
    setCurrentAudio: (state, action: PayloadAction<CurrentAudioUI>) => {
      state.currentAudio = action.payload;
    },
    playAudio: (
      state,
      action: PayloadAction<{
        sourceType:
          | "SpecifyShowEpisodes"
          | "SavedEpisodes"
          | "BookingProducingTracks";
        audioId: string;
      } | null>
    ) => {
      state.playMode.playStatus = "play";
      if (action.payload) {
        state.playMode.sourceType = action.payload.sourceType;
        state.playMode.audioId = action.payload.audioId;
      }
    },
    pauseAudio: (state) => {
      state.playMode.playStatus = "pause";
    },
    stopAudio: (state) => {
      state.playMode.playStatus = "stop";
    },
    setVolume: (state, action: PayloadAction<number>) => {
      state.playMode.volume = clamp(action.payload, 0, 100);
    },
    setNextMode: (state, action: PayloadAction<"Sequential" | "Random">) => {
      state.playMode.nextMode = action.payload;
    },
    nextAudio: () => {
      // This reducer is just a placeholder to trigger next audio logic in middleware
    },
    setIsNextSessionNull: (state, action: PayloadAction<boolean>) => {
      state.playMode.isNextSessionNull = action.payload;
    },
    setUIIsAutoPlay: (state, action: PayloadAction<boolean>) => {
      state.playMode.isAutoPlay = action.payload;
    },
    setUIPlayOrderMode: (
      state,
      action: PayloadAction<"Sequential" | "Random">
    ) => {
      state.playMode.nextMode = action.payload;
    },
  },
});

export const {
  setListenSession,
  setListenSessionProcedure,
  setCurrentAudio,
  playAudio,
  pauseAudio,
  stopAudio,
  setVolume,
  setNextMode,
  nextAudio,
  setIsNextSessionNull,
  setUIIsAutoPlay,
  setUIPlayOrderMode
} = mediaPlayerSlice.actions;

export default mediaPlayerSlice.reducer;

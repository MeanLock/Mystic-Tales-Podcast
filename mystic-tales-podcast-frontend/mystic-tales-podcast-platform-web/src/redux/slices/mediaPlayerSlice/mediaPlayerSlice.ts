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
  bookingId: number | null;
  seekTo: number | null;
  continue_listen_session_id: string | null;
  isBuffering: boolean;
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
  bookingId: null,
  seekTo: null,
  continue_listen_session_id: null,
  isBuffering: false,
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
        bookingId?: number;
        seekTo?: number;
        continue_listen_session_id?: string;
      } | null>
    ) => {
      state.playMode.playStatus = "play";
      if (action.payload) {
        state.playMode.sourceType = action.payload.sourceType;
        state.playMode.audioId = action.payload.audioId;
        state.seekTo = action.payload.seekTo ?? null;
        state.continue_listen_session_id =
          action.payload.continue_listen_session_id ?? null;
        state.bookingId = action.payload.bookingId ?? null;
      }
    },
    pauseAudio: (state) => {
      state.playMode.playStatus = "pause";
      state.seekTo = null;
      state.continue_listen_session_id = null;
    },
    stopAudio: (state) => {
      state.playMode.playStatus = "stop";
      state.seekTo = null;
      state.continue_listen_session_id = null;
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
    setBuffering: (state, action: PayloadAction<boolean>) => {
      state.isBuffering = action.payload;
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
  setUIPlayOrderMode,
  setBuffering,
} = mediaPlayerSlice.actions;

export default mediaPlayerSlice.reducer;

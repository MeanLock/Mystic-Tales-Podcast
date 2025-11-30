import {
  CurrentAudio,
  ListenSession,
  ListenSessionProcedure,
  PlayerControl,
} from "@/src/core/types/audio.type";
import { createSlice, PayloadAction } from "@reduxjs/toolkit";

/** ===== Types ===== */
export type PlayerType = {
  playMode: PlayerControl;
  currentAudio: CurrentAudio;
  listenSession: ListenSession;
  listenSessionProcedure: ListenSessionProcedure;
  bookingId: number | null;
  seekTo: number | null;
  seekDelta: number | null;
  continue_listen_session_id: string | null;
  isBuffering: boolean;
  playbackPosition: number; // giây
  playbackDuration: number; // giây
};

/** ===== Initial ===== */
const initialState: PlayerType = {
  playMode: {
    playStatus: "stop",
    nextMode: "Sequential",
    sourceType: null,
    isNextSessionNull: false,
    audioId: null,
    isAutoPlay: false,
    volume: 100,
  },
  currentAudio: null,
  bookingId: null,
  listenSession: null,
  listenSessionProcedure: null,
  continue_listen_session_id: null,
  isBuffering: false,
  seekTo: null,
  seekDelta: null,
  playbackPosition: 0,
  playbackDuration: 0,
};

function clamp(value: number, min: number, max: number): number {
  return Math.min(Math.max(value, min), max);
}

/** ===== Slice ===== */
const playerSlice = createSlice({
  name: "player",
  initialState,
  reducers: {
    // ACTION 1 //
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

    // ACTION 2 //
    pauseAudio: (state) => {
      state.playMode.playStatus = "pause";
      state.seekTo = null;
      state.continue_listen_session_id = null;
    },

    // ACTION 3 //
    stopAudio: (state) => {
      state.playMode.playStatus = "stop";
      state.seekTo = null;
      state.continue_listen_session_id = null;
    },

    // ACTION 4 //
    setVolume: (state, action: PayloadAction<number>) => {
      state.playMode.volume = clamp(action.payload, 0, 100);
    },

    // ACTION 5 //
    setListenSession: (state, action: PayloadAction<ListenSession>) => {
      state.listenSession = action.payload;
    },

    // ACTION 6 //
    setListenSessionProcedure: (
      state,
      action: PayloadAction<ListenSessionProcedure>
    ) => {
      state.listenSessionProcedure = action.payload;
    },

    // ACTION 7 //
    setCurrentAudio: (state, action: PayloadAction<CurrentAudio>) => {
      state.currentAudio = action.payload;
    },

    // ACTION 8 //
    setNextMode: (state, action: PayloadAction<"Sequential" | "Random">) => {
      state.playMode.nextMode = action.payload;
    },

    // ACTION 9 //
    setIsNextSessionNull: (state, action: PayloadAction<boolean>) => {
      state.playMode.isNextSessionNull = action.payload;
    },

    // ACTION 10 //
    setUIIsAutoPlay: (state, action: PayloadAction<boolean>) => {
      state.playMode.isAutoPlay = action.payload;
    },

    // ACTION 11 //
    setUIPlayOrderMode: (
      state,
      action: PayloadAction<"Sequential" | "Random">
    ) => {
      state.playMode.nextMode = action.payload;
    },

    // ACTION 12 //
    setBuffering: (state, action: PayloadAction<boolean>) => {
      state.isBuffering = action.payload;
    },

    // ACTION 13 //
    seekTo: (state, action: PayloadAction<{ position: number }>) => {
      state.seekTo = action.payload.position;
    },

    // ACTION 14 //
    seekBy: (state, action: PayloadAction<{ delta: number }>) => {
      state.seekDelta = action.payload.delta;
    },

    // ACTION 15 //
    consumeSeek: (state) => {
      state.seekTo = null;
      state.seekDelta = null;
    },

    // ACTION 16 //
    setPlaybackState: (
      state,
      action: PayloadAction<{ position: number; duration: number }>
    ) => {
      state.playbackPosition = action.payload.position; // giây
      state.playbackDuration = action.payload.duration; // giây
    },
  },
});

export const {
  playAudio,
  pauseAudio,
  stopAudio,
  setVolume,
  setListenSession,
  setListenSessionProcedure,
  setCurrentAudio,
  setNextMode,
  setIsNextSessionNull,
  setUIIsAutoPlay,
  setUIPlayOrderMode,
  setBuffering,
  seekTo,
  seekBy,
  consumeSeek,
  setPlaybackState
} = playerSlice.actions;

export default playerSlice.reducer;

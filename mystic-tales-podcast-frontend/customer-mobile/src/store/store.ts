// store.ts
import { configureStore, combineReducers } from "@reduxjs/toolkit";
import { baseApi } from "../services/baseApi";
import authReducer from "../features/auth/authSlice";
import downloadsReducer from "../features/download/downloadSlice";

// persist
import AsyncStorage from "@react-native-async-storage/async-storage";
import {
  persistStore,
  persistReducer,
  FLUSH,
  REHYDRATE,
  PAUSE,
  PERSIST,
  PURGE,
  REGISTER,
} from "redux-persist";

import playerReducer, {
  updatePosition,
  onEnded,
} from "../features/mediaPlayer/playerSlice";
import { playerMiddleware } from "../features/mediaPlayer/playerMiddleware";
import { playerEngine } from "../services/audio/playerEngine";

// ✅ root reducer
const rootReducer = combineReducers({
  auth: authReducer,
  downloads: downloadsReducer,
  player: playerReducer,
  [baseApi.reducerPath]: baseApi.reducer,
});

// ✅ persist config
const persistConfig = {
  key: "root",
  storage: AsyncStorage,
  whitelist: ["auth"],
};

const persistedReducer = persistReducer(persistConfig, rootReducer);

// ✅ thêm logging middleware
const rtkQueryLogger = () => (next: any) => (action: any) => {
  if (action.type.endsWith("/pending")) {
    console.log("➡️ [RTK Query] Request:", {
      type: action.type,
      endpoint: action.meta?.arg?.endpointName,
      params: action.meta?.arg?.originalArgs,
    });
  } else if (action.type.endsWith("/fulfilled")) {
    console.log("⬅️ [RTK Query] Response:", {
      type: action.type,
      endpoint: action.meta?.arg?.endpointName,
      payload: action.payload,
    });
  } else if (action.type.endsWith("/rejected")) {
    console.log("❌ [RTK Query] Error:", {
      type: action.type,
      endpoint: action.meta?.arg?.endpointName,
      error: action.error,
    });
  }
  return next(action);
};

// ✅ store
export const store = configureStore({
  reducer: persistedReducer,
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        ignoredActions: [FLUSH, REHYDRATE, PAUSE, PERSIST, PURGE, REGISTER],
      },
    })
      .concat(baseApi.middleware)
      .concat(rtkQueryLogger) // 👈 thêm dòng này
      .concat(playerMiddleware),
  devTools: __DEV__,
});

export const persistor = persistStore(store);

// Types
export type RootState = ReturnType<typeof rootReducer>;
export type AppDispatch = typeof store.dispatch;

playerEngine.onStatus = (s) => {
  if ((s as any).didJustFinish) console.log("[status] didJustFinish true");

  if ("isLoaded" in s && s.isLoaded) {
    // chỉ cập nhật tiến độ nếu ĐANG PHÁT
    if (s.isPlaying) {
      const seconds = Math.floor((s.positionMillis ?? 0) / 1000);
      store.dispatch(updatePosition({ position: seconds }));
    }

    // hết bài chuẩn
    if ((s as any).didJustFinish) {
      store.dispatch(onEnded());
      return;
    }

    // fallback: một vài thiết bị không bắn didJustFinish ổn định
    const pos = s.positionMillis ?? 0;
    const dur = (s as any).durationMillis ?? 0;
    if (!s.isPlaying && dur > 0 && pos >= dur - 150) {
      store.dispatch(onEnded());
    }
  }
};

// === NEW: EndGuard – fallback chắc chắn auto-next ===
const END_TOLERANCE_SEC = 0.7;
let lastEndedId: string | null = null;

setInterval(() => {
  const st = store.getState().player;
  const cur = st.currentAudio;
  if (!cur) return;

  // Reset guard nếu đã chuyển bài
  if (lastEndedId && lastEndedId !== cur.Id) {
    lastEndedId = null;
  }

  if (st.playerMode.playStatus !== "playing") return;
  if (cur.AudioLength <= 0) return;

  const nearTheEnd = cur.LatestPosition >= cur.AudioLength - END_TOLERANCE_SEC;
  if (nearTheEnd && lastEndedId !== cur.Id) {
    lastEndedId = cur.Id;
    store.dispatch(onEnded());
  }
}, 400);

// src/store/store.ts (MOBILE)
import { configureStore, combineReducers } from "@reduxjs/toolkit";
import { appApi } from "@/src/core/api/appApi/index"; // <-- thay path đúng với project mobile
import authReducer from "../features/auth/authSlice";
import downloadsReducer from "../features/download/downloadSlice";
import alertReducer from "../features/alert/alertSlice";

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
// import { playerEngine } from "../services/audio/playerEngine";

import { configureTokenGetter } from "@/src/core/api/appApi/token"; // <-- mới

// rootReducer
const rootReducer = combineReducers({
  auth: authReducer,
  downloads: downloadsReducer,
  player: playerReducer,
  alert: alertReducer,
  [appApi.reducerPath]: appApi.reducer, // <-- thay baseApi bằng appApi
});

// persist config
const persistConfig = {
  key: "root",
  storage: AsyncStorage,
  whitelist: ["auth"], // chỉ cần persist auth là đủ để lấy token
};

const persistedReducer = persistReducer(persistConfig, rootReducer);

// store
export const store = configureStore({
  reducer: persistedReducer,
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        ignoredActions: [FLUSH, REHYDRATE, PAUSE, PERSIST, PURGE, REGISTER],
      },
    })
      .concat(appApi.middleware) // <-- thay baseApi.middleware
      .concat(playerMiddleware),
  devTools: __DEV__,
});

export const persistor = persistStore(store);

// Types
export type RootState = ReturnType<typeof rootReducer>;
export type AppDispatch = typeof store.dispatch;

/** Liên kết tokenGetter với auth slice */
configureTokenGetter(() => {
  const state = store.getState() as RootState;
  return state.auth.accessToken ?? undefined;
});

/// ===== phần playerEngine.onStatus & setInterval giữ nguyên như cũ =====

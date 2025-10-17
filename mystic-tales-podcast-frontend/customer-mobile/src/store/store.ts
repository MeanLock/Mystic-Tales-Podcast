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
} from "../features/mediaPlayer/playerSlice";
import { playerMiddleware } from "../features/mediaPlayer/playerMiddleware"; // ✅
import { playerEngine } from "../services/audio/playerEngine";

const rootReducer = combineReducers({
  auth: authReducer,
  downloads: downloadsReducer,
  player: playerReducer,
  [baseApi.reducerPath]: baseApi.reducer,
});

const persistConfig = {
  key: "root",
  storage: AsyncStorage,
  whitelist: [
    // "player", // đang tắt persist player để test
    "auth",
  ],
};

const persistedReducer = persistReducer(persistConfig, rootReducer);

export const store = configureStore({
  reducer: persistedReducer,
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        ignoredActions: [FLUSH, REHYDRATE, PAUSE, PERSIST, PURGE, REGISTER],
      },
    })
      .concat(baseApi.middleware) // RTK Query middleware
      .concat(playerMiddleware), // ✅ Thêm playerEngine middleware
  devTools: __DEV__,
});

export const persistor = persistStore(store);

// Types
export type RootState = ReturnType<typeof rootReducer>;
export type AppDispatch = typeof store.dispatch;

playerEngine.onStatus = (s) => {
  // đẩy tiến độ phát (giây) về Redux
  if ("isLoaded" in s && s.isLoaded) {
    const seconds = Math.floor((s.positionMillis ?? 0) / 1000);
    store.dispatch(updatePosition({ position: seconds }));
  }
};

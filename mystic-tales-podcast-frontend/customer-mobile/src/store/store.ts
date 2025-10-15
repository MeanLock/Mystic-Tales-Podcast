// store.ts
import { configureStore, combineReducers } from "@reduxjs/toolkit";
import { baseApi } from "../services/baseApi";
import authReducer from "../features/auth/authSlice";
import downloadsReducer from "../features/download/downloadSlice";

// ⬇️ redux-persist + AsyncStorage
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

import playerReducer from "../features/mediaPlayer/playerSlice";

// Gộp reducer gốc
const rootReducer = combineReducers({
  auth: authReducer,
  downloads: downloadsReducer,
  player: playerReducer,
  [baseApi.reducerPath]: baseApi.reducer, // KHÔNG persist RTK Query
});

// Cấu hình persist (chỉ whitelist các slice bạn muốn giữ)
const persistConfig = {
  key: "root",
  storage: AsyncStorage,
  whitelist: [
    // Muốn giữ cái gì thì thêm ở đây
    "player",   
    "auth", 
  ],
  // blacklist: [baseApi.reducerPath], // không cần vì ta persist root, whitelist đủ rồi
};

const persistedReducer = persistReducer(persistConfig, rootReducer);

export const store = configureStore({
  reducer: persistedReducer,
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      // Bỏ qua các action đặc thù của redux-persist để khỏi warning serializable
      serializableCheck: {
        ignoredActions: [FLUSH, REHYDRATE, PAUSE, PERSIST, PURGE, REGISTER],
      },
    }).concat(baseApi.middleware),
});

export const persistor = persistStore(store);

// Types
export type RootState = ReturnType<typeof rootReducer>;
export type AppDispatch = typeof store.dispatch;

// src/redux/store.ts
import { configureStore, combineReducers } from "@reduxjs/toolkit";
import { appApi } from "@/core/api/appApi";
import authReducer from "./slices/authSlice/authSlice";
import mediaPlayerReducer from "./slices/mediaPlayerSlice/mediaPlayerSlice";

// ⬇️ redux-persist
import storage from "redux-persist/lib/storage"; // web: localStorage
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

// Gộp reducers trước khi persist
const rootReducer = combineReducers({
  [appApi.reducerPath]: appApi.reducer,
  auth: authReducer,
  player: mediaPlayerReducer,
  // ...reducers khác
});

// Cấu hình persist: chỉ lưu auth
const persistConfig = {
  key: "root",
  storage,
  whitelist: ["auth"], // ✅ chỉ persist auth
  // blacklist: [appApi.reducerPath], // mặc định không whitelist nên RTKQ không bị lưu
};

const persistedReducer = persistReducer(persistConfig, rootReducer);

export const store = configureStore({
  reducer: persistedReducer,
  middleware: (gDM) =>
    gDM({
      // Bỏ qua các action không tuần tự của redux-persist để tránh warning
      serializableCheck: {
        ignoredActions: [FLUSH, REHYDRATE, PAUSE, PERSIST, PURGE, REGISTER],
      },
    }).concat(appApi.middleware),
});

export const persistor = persistStore(store);

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

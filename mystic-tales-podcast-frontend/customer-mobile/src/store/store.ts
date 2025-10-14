import { configureStore } from "@reduxjs/toolkit";
import { exampleApi } from "../services/testApi";
import authReducer from "../features/auth/authSlice";
import downloadsReducer from "../features/download/downloadSlice";
import { baseApi } from "../services/baseApi";

export const store = configureStore({
  reducer: {
    auth: authReducer,
    downloads: downloadsReducer,
    [exampleApi.reducerPath]: exampleApi.reducer,
    [baseApi.reducerPath]: baseApi.reducer,
  },
  middleware: (gDM) => gDM().concat(exampleApi.middleware, baseApi.middleware),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

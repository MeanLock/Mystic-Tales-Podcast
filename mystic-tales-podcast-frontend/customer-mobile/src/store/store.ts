import { configureStore } from "@reduxjs/toolkit";
import { exampleApi } from "../services/testApi";
import authReducer from "../features/auth/authSlice";
export const store = configureStore({
  reducer: {
    auth: authReducer,
    [exampleApi.reducerPath]: exampleApi.reducer,
  },
  middleware: (gDM) => gDM().concat(exampleApi.middleware),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

import { combineReducers } from 'redux';
import authReducer from './auth/auth.slice';
import uiReducer from './ui/ui.slice';
import appSliceReducer from "@/app/appSlice";

// Import các reducer khác nếu cần

export type RootState = ReturnType<typeof rootReducer>;

const rootReducer = combineReducers({
  auth: authReducer,
  ui: uiReducer,
  appSlice: appSliceReducer,

  // Các reducer khác
});

export default rootReducer;
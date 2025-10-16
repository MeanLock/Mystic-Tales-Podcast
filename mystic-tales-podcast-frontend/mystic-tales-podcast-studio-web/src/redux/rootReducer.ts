import { combineReducers } from 'redux';
import authReducer from './auth/authSlice';
import uiReducer from './ui/uiSlice';
import navigationReducer from './navigation/navigationSlice';
// Import các reducer khác nếu cần

export type RootState = ReturnType<typeof rootReducer>;

const rootReducer = combineReducers({
  auth: authReducer,
  ui: uiReducer,
  navigation: navigationReducer
  // Các reducer khác
});

export default rootReducer;
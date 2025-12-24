import storage from 'redux-persist/lib/storage'; // sử dụng sessionStorage

const persistConfig = {
  key: 'root',
  storage,
  whitelist: ['auth'], // Slice bạn muốn lưu trữ, ví dụ như auth chứa JWT
};

export default persistConfig;
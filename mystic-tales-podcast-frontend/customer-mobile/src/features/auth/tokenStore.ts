import * as SecureStore from "expo-secure-store";

const ACCESS = "access_token";
const ACCESS_EXP = "access_token_expiry";
const REFRESH = "refresh_token";

const DEFAULT_TTL_MS = 90 * 24 * 60 * 60 * 1000; // 90 days

export const tokenStore = {
  // access
  getAccess: () => SecureStore.getItemAsync(ACCESS),
  // returns { token, expiresAt } useful for bootstrap
  getAccessWithExpiry: async () => {
    const token = await SecureStore.getItemAsync(ACCESS);
    const exp = await SecureStore.getItemAsync(ACCESS_EXP);
    return { token, expiresAt: exp ? parseInt(exp, 10) : null };
  },
  // set access and save expiry (ttlMs optional)
  setAccess: async (v: string, ttlMs: number = DEFAULT_TTL_MS) => {
    await SecureStore.setItemAsync(ACCESS, v);
    await SecureStore.setItemAsync(ACCESS_EXP, (Date.now() + ttlMs).toString());
  },
  delAccess: () => SecureStore.deleteItemAsync(ACCESS),
  // refresh
  getRefresh: () => SecureStore.getItemAsync(REFRESH),
  setRefresh: (v: string) => SecureStore.setItemAsync(REFRESH, v),
  delRefresh: () => SecureStore.deleteItemAsync(REFRESH),
  // clear all including expiry
  clearAll: async () => {
    await Promise.all([
      SecureStore.deleteItemAsync(ACCESS),
      SecureStore.deleteItemAsync(REFRESH),
      SecureStore.deleteItemAsync(ACCESS_EXP),
    ]);
  },
};

import axios from "axios";
import { getAccessToken } from "@/core/api/appApi/token";
import { BASE_URL } from "@/core/api/appApi";
import { JwtUtil } from "@/core/utils/token";
import {
  showTokenExpiredAlert,
  showUnauthorizedAlert,
} from "@/core/utils/alert";

const loginRequiredAxiosInstance = axios.create({
  baseURL: BASE_URL,
  timeout: 1000000,
});

loginRequiredAxiosInstance.interceptors.request.use(
  async (config) => {
    const token = getAccessToken();
    if (token) {
      if (JwtUtil.isTokenValid(token) === false) {
        // Show alert for invalid token
        showUnauthorizedAlert();
        return Promise.reject(new Error("Unauthorized"));
      }
      if (JwtUtil.isTokenNotExpired(token) === false) {
        // Show alert for expired token
        showTokenExpiredAlert();
        return Promise.reject(new Error("Login expired"));
      }
      config.headers.Authorization = `Bearer ${token}`;
    } else {
      // Show alert when no token found
      showUnauthorizedAlert();
      return Promise.reject(new Error("Unauthorized"));
    }
    config.headers["ngrok-skip-browser-warning"] = "69420";
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

export { loginRequiredAxiosInstance };

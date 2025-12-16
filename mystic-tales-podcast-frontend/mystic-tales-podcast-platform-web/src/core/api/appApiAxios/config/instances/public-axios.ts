import axios from "axios";
import { getAccessToken } from "@/core/api/appApi/token";
import { BASE_URL } from "@/core/api/appApi";

const publicAxiosInstance = axios.create({
  baseURL: BASE_URL,
  timeout: 10000,
});

publicAxiosInstance.interceptors.request.use(
  (config) => {
    const token = getAccessToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    config.headers["ngrok-skip-browser-warning"] = "8041";
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

export { publicAxiosInstance };

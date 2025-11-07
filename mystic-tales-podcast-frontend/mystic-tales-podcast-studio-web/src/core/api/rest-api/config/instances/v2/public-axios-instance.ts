import axios from "axios";
import { LocalStorageUtil } from "../../../../../utils/storage.util";
import { API_CONFIG } from "../../../../../../config";



const publicAxiosInstance = axios.create({
  baseURL: API_CONFIG.REST_API_BASE_URL,
  timeout: 10000,
});

publicAxiosInstance.interceptors.request.use(
  (config) => {
    const token = LocalStorageUtil.getAuthTokenFromPersistLocalStorage();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    config.headers["ngrok-skip-browser-warning"] = '8041';
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);



export {
  publicAxiosInstance
}




import axios from "axios";
import { jwtDecode } from "jwt-decode";
import { LocalStorageUtil } from "../../../../../utils/storage.util";
import { JwtUtil } from "../../../../../utils/jwt.util";
import { loginRequiredAlert } from "../../../../../utils/alert.util";
import { AppError, AppErrorType } from "../../../main/errors";
import { API_CONFIG } from "../../../../../../config";



const loginRequiredAxiosInstance = axios.create({
  baseURL: API_CONFIG.REST_API_BASE_URL,
  timeout: 1000000,
});

loginRequiredAxiosInstance.interceptors.request.use(
  async (config) => {
    const token = LocalStorageUtil.getAuthTokenFromPersistLocalStorage();
    console.log("Token in interceptor:", token);
    if (token) {
      if (JwtUtil.isTokenValid(token) === false) {

        //** CHO HIỆN THÔNG BÁO YÊU CẦU ĐĂNG NHẬP
        // await loginRequiredAlert();

        // return Promise.reject(new Error('Unauthorized'));
        return Promise.reject(new AppError(AppErrorType.Unauthorized, "Unauthorized"));
      }
      if (JwtUtil.isTokenNotExpired(token) === false) {
        //** CHO HIỆN THÔNG BÁO YÊU CẦU ĐĂNG NHẬP
        // await loginRequiredAlert();

        // return Promise.reject(new Error('Login expired'));
        return Promise.reject(new AppError(AppErrorType.TokenExpired, "Login expired"));
      }
      config.headers.Authorization = `Bearer ${token}`;
    } else {

      //** CHO HIỆN THÔNG BÁO YÊU CẦU ĐĂNG NHẬP
      // await loginRequiredAlert();

      // window.location.replace('/login');
      return Promise.reject(new AppError(AppErrorType.LoginRequired, "Login required"));
    }
    config.headers["ngrok-skip-browser-warning"] = '69420';
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);


export {
  loginRequiredAxiosInstance
}




import axios from "axios";
import { LocalStorageUtil } from "../../../../../utils/storage.util";
import { JwtUtil } from "../../../../../utils/jwt.util";
import { AppError, AppErrorType } from "../../../main/errors";
import { API_CONFIG } from "../../../../../../config";



const loginRequiredApi = axios.create({
  baseURL: API_CONFIG.REST_API_BASE_URL,
  timeout: 10000,
});

loginRequiredApi.interceptors.request.use(
  async (config) => {
    const token = LocalStorageUtil.getAuthTokenFromPersistLocalStorage();

    if (token) {
      if (JwtUtil.isTokenValid(token) === false) {

        //** CHO HIỆN THÔNG BÁO YÊU CẦU ĐĂNG NHẬP
        // await loginRequiredAlert();

        // return Promise.reject(new Error('Unauthorized'));
        return Promise.reject(new AppError(AppErrorType.Unauthorized, "Unauthorized"));
      }
      if(JwtUtil.isTokenNotExpired(token) === false){
        //** CHO HIỆN THÔNG BÁO YÊU CẦU ĐĂNG NHẬP
        // await loginRequiredAlert();

        // return Promise.reject(new Error('Login expired'));
        return Promise.reject(new AppError(AppErrorType.TokenExpired, "Login expired"));
      }
      config.headers.Authorization = `Bearer ${token}`;
    } else {

      //** CHO HIỆN THÔNG BÁO YÊU CẦU ĐĂNG NHẬP
      // await loginRequiredAlert();

      // return Promise.reject(new Error('Login required'));
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
  loginRequiredApi
}




import axios from "axios";
import { jwtDecode } from "jwt-decode";
import { LocalStorageUtil } from "../../../../../utils/storage.util";
import { JwtUtil } from "../../../../../utils/jwt.util";
import { loginRequiredAlert } from "../../../../../utils/alert.util";
import { AppError, AppErrorType } from "../../../main/errors";
import { API_CONFIG } from "../../../../../../config";



const staffAxiosInstance = axios.create({
  baseURL: API_CONFIG.REST_API_BASE_URL,
  timeout: 200000,
});


staffAxiosInstance.interceptors.request.use(
  async (config) => {
    const token = LocalStorageUtil.getAuthTokenFromPersistLocalStorage();

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
      const user = JwtUtil.decodeToken(token);

      //if (user.role.id != 2 && user.role.id != 3 && user.role.id != 4) {
      if (!user || ![2].includes(Number(user.role_id))) {
        //** CHO HIỆN THÔNG BÁO YÊU CẦU ĐĂNG NHẬP
        // await loginRequiredAlert();
        
        // return Promise.reject(new Error('No permission'));
        return Promise.reject(new AppError(AppErrorType.Forbidden, "No permission"));
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
  staffAxiosInstance
}




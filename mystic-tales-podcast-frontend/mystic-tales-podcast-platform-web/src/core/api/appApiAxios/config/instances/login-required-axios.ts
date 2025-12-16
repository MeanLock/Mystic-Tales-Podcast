import axios from "axios";
import { jwtDecode } from "jwt-decode";
import { getAccessToken } from "@/core/api/appApi/token";
import { BASE_URL } from "@/core/api/appApi";
import { JwtUtil } from "@/core/utils/token";



const loginRequiredAxiosInstance = axios.create({
  baseURL: BASE_URL.REST_API_BASE_URL,
  timeout: 1000000,
});

loginRequiredAxiosInstance.interceptors.request.use(
  async (config) => {
    const token = getAccessToken();
    if (token) {
      if (JwtUtil.isTokenValid(token) === false) {

        //** CHO HIỆN THÔNG BÁO YÊU CẦU ĐĂNG NHẬP
        // await loginRequiredAlert();

        return Promise.reject(new Error('Unauthorized'));
      }
      if (JwtUtil.isTokenNotExpired(token) === false) {
        //** CHO HIỆN THÔNG BÁO YÊU CẦU ĐĂNG NHẬP
        // await loginRequiredAlert();

        return Promise.reject(new Error('Login expired'));
      }
      config.headers.Authorization = `Bearer ${token}`;
    } else {

      //** CHO HIỆN THÔNG BÁO YÊU CẦU ĐĂNG NHẬP
      // await loginRequiredAlert();

      // window.location.replace('/login');
      return Promise.reject(new Error('Unauthorized'));
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




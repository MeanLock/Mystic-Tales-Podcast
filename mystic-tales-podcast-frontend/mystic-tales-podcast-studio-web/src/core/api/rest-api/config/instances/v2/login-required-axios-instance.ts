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
    // const token = LocalStorageUtil.getAuthTokenFromPersistLocalStorage();
    const token ='eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjE3IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkpvaG4gRG9lIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoiaGFvaG5zZTE4MTUyNUBmcHQuZWR1LnZuIiwiaWQiOiIxNyIsInJvbGVfaWQiOiIxIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQ3VzdG9tZXIiLCJiYWxhbmNlIjoiMC4wMCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvc2VyaWFsbnVtYmVyIjoiOGRjNGMxMDItZWRlMC00ZjJlLWJhNTktZTVjMTM5MjUzYWUwIiwiZXhwIjo3NzYwMzI4NTUyLCJpc3MiOiJsb2NhbGhvc3QiLCJhdWQiOiJsb2NhbGhvc3QifQ.jVA-GOP_vRpTpMd8hrPCPdEzUpyCLa37aSjC5fQCblnN1gPOBUfkMG3QyFHmZG8KPAo7oe5c3MvGQagOuQIbtEtl2rVOUjDHyT7mRj62R7IqtbZt67NfkiVDVuMeaWcU97zrV_5hSjIkOTrFl1Uhc5loBX7QRNkPIc8XwKuzTg3b9nAbaMeBTqKrOThwofoHKro0eTk1UGQhWp1c5tthXtiNfs43B81Gb9j43XScyPh01HqzyO8XuqfQbpHV0tEIkatpCwB7C74F-BegVw0diMKXNEdWqldKI7jndl6UB0OFOvgXpgM1E78JiAcorMyhNYE415I2Nd30OhUTZHELUg';

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
  loginRequiredAxiosInstance
}



